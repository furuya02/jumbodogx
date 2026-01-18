using System.Net;
using System.Net.Sockets;
using System.Text;
using Jdx.Core.Abstractions;
using Jdx.Core.Constants;
using Jdx.Core.Helpers;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Proxy;

/// <summary>
/// Proxyサーバー
/// bjd5-master/ProxyHttpServer/Server.cs に対応
/// </summary>
public class ProxyServer : ServerBase
{
    private readonly ISettingsService _settingsService;
    private int _port;
    private ServerTcpListener? _listener;

    // Proxyコンポーネント（スレッドセーフアクセスのためロック使用）
    private readonly object _componentsLock = new();
    private ProxyLimitUrl? _limitUrl;
    private ProxyLimitString? _limitString;
    private ProxyCache? _cache;
    private List<string> _disableAddressList = new();

    // 定数定義
    private const int DefaultTimeoutSeconds = 60; // デフォルトタイムアウト（秒）
    private const int MaxBufferedResponseSize = 1024 * 1024; // 完全バッファリングの上限（1MB）
    private const int CacheDisposeGracePeriodMs = 100; // キャッシュ破棄時の猶予期間（ミリ秒）

    public ProxyServer(ILogger<ProxyServer> logger, ISettingsService settingsService) : base(logger)
    {
        _settingsService = settingsService;

        // 初期設定を取得
        var settings = _settingsService.GetSettings().ProxyServer;
        _port = settings.Port;

        // コンポーネントを初期化
        InitializeComponents(settings);

        // 設定変更イベントを購読
        _settingsService.SettingsChanged += OnSettingsChanged;
    }

    public override string Name => "ProxyServer";
    public override ServerType Type => ServerType.Proxy;
    public override int Port => _port;

    private void InitializeComponents(ProxyServerSettings settings)
    {
        // 新しいコンポーネントを作成（ロック外で）
        var newDisableAddressList = settings.DisableAddressList
            .Select(e => e.Address)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToList();

        var newLimitUrl = new ProxyLimitUrl(settings.LimitUrlAllowList, settings.LimitUrlDenyList, Logger);

        ProxyLimitString? newLimitString = null;
        if (settings.LimitStringList != null && settings.LimitStringList.Count > 0)
        {
            newLimitString = new ProxyLimitString(settings.LimitStringList, Logger);
        }

        ProxyCache? newCache = null;
        if (settings.UseCache)
        {
            newCache = new ProxyCache(settings, Logger);
            newCache.Start();
            Logger.LogInformation("Proxy cache enabled: Dir={CacheDir}", settings.CacheDir);
        }

        // ロック内で参照を入れ替え、古いキャッシュを取得
        ProxyCache? oldCache;
        lock (_componentsLock)
        {
            _disableAddressList = newDisableAddressList;
            _limitUrl = newLimitUrl;
            _limitString = newLimitString;

            oldCache = _cache;
            _cache = newCache;
        }

        // 古いキャッシュを非同期で破棄（グレースピリオド付き）
        // 他のスレッドがまだ使用している可能性があるため、猶予期間を設ける
        if (oldCache != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // グレースピリオド: 進行中のリクエストが完了するのを待つ
                    await Task.Delay(CacheDisposeGracePeriodMs);

                    oldCache.Stop();
                    oldCache.Dispose();
                    Logger.LogDebug("Old cache disposed successfully");
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Error disposing old cache");
                }
            });
        }

        Logger.LogInformation("Proxy server components initialized");
    }

    private void OnSettingsChanged(object? sender, ApplicationSettings settings)
    {
        var proxySettings = settings.ProxyServer;

        // ポート変更があればサーバーを再起動
        if (_port != proxySettings.Port)
        {
            _port = proxySettings.Port;
            Logger.LogInformation("Proxy Server port changed to {Port}", _port);
        }

        // コンポーネントを再初期化
        InitializeComponents(proxySettings);
        Logger.LogInformation("Proxy Server settings updated");
    }

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        var settings = _settingsService.GetSettings().ProxyServer;

        _listener = await CreateTcpListenerAsync(_port, settings.BindAddress, cancellationToken);

        var displayAddress = settings.BindAddress ?? "0.0.0.0";
        Logger.LogInformation(
            "Proxy Server started: {BindAddress}:{Port}",
            displayAddress,
            _port
        );

        // クライアント接続を待機
        // 注: ServerBase.RunTcpAcceptLoopAsync()は使用していません。
        // 理由: StartListeningAsync()内でループを実装することで、
        // 設定変更時のリスナー再起動処理との整合性を保つため。
        // また、Proxyサーバー特有のキャッシュ管理ロジックと連携しやすくするため。
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var clientSocket = await _listener.AcceptAsync(cancellationToken);
                if (clientSocket != null)
                {
                    // クライアント接続を別タスクで処理
                    _ = Task.Run(async () => await HandleClientAsync(clientSocket, cancellationToken), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合は正常終了
                break;
            }
            catch (Exception ex)
            {
                NetworkExceptionHandler.LogNetworkException(ex, Logger, "Proxy client accept");
            }
        }
    }

    protected override Task StopListeningAsync(CancellationToken cancellationToken)
    {
        // キャッシュを停止
        if (_cache != null)
        {
            _cache.Dispose();
            _cache = null;
        }

        Logger.LogInformation("Proxy Server stopped");
        return Task.CompletedTask;
    }

    protected override async Task HandleClientAsync(Socket socket, CancellationToken cancellationToken)
    {
        // SocketからTcpClientを作成
        using var client = new TcpClient { Client = socket };
        await HandleProxyClientAsync(client, cancellationToken);
    }

    private async Task HandleProxyClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var settings = _settingsService.GetSettings().ProxyServer;
        var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        var remoteIp = remoteEndPoint?.Address.ToString() ?? "unknown";

        try
        {
            Logger.LogDebug("Proxy client connected from {RemoteIp}", remoteIp);

            // ACLチェック
            if (!CheckAcl(remoteIp, settings))
            {
                Logger.LogWarning("Access denied for {RemoteIp} (ACL)", remoteIp);
                client.Close();
                return;
            }

            // 上位プロキシ情報を作成
            var upperProxy = new ProxyUpperProxy(
                settings.UseUpperProxy,
                settings.UpperProxyServer,
                settings.UpperProxyPort,
                _disableAddressList,
                settings.UpperProxyUseAuth,
                settings.UpperProxyAuthName,
                settings.UpperProxyAuthPass
            );

            // Proxy接続オブジェクトを作成（リソースリーク防止のためtry内で作成）
            ProxyConnection? proxyConnection = null;
            try
            {
                proxyConnection = new ProxyConnection(
                    client,
                    settings.TimeOut,
                    upperProxy,
                    Logger
                );

                // 1. クライアントからリクエストを読み取り
                var clientStream = client.GetStream();
                var request = new ProxyRequest();

                if (!await request.ReceiveAsync(clientStream, Logger, cancellationToken))
                {
                    Logger.LogWarning("Failed to receive request from {RemoteIp}", remoteIp);
                    return;
                }

                Logger.LogDebug("Proxy request: {Method} {Host}:{Port}{Uri}",
                    request.HttpMethod, request.HostName, request.Port, request.Uri);

                // 3. URL制限チェック（スレッドセーフ）
                var fullUrl = $"{request.Protocol.ToString().ToLower()}://{request.HostName}:{request.Port}{request.Uri}";
                ProxyLimitUrl? limitUrl;
                lock (_componentsLock)
                {
                    limitUrl = _limitUrl;
                }

                if (limitUrl != null && !limitUrl.IsAllowed(fullUrl))
                {
                    Logger.LogWarning("URL blocked by limit: {Url}", fullUrl);
                    await SendErrorResponseAsync(clientStream, 403, "Forbidden", cancellationToken);
                    return;
                }

                // 2. プロトコル判別と処理
                if (request.Protocol == ProxyProtocol.Http)
                {
                    await HandleHttpRequestAsync(proxyConnection, request, clientStream, cancellationToken);
                }
                else if (request.Protocol == ProxyProtocol.Ssl)
                {
                    await HandleSslRequestAsync(proxyConnection, request, clientStream, cancellationToken);
                }
                else if (request.Protocol == ProxyProtocol.Ftp)
                {
                    await HandleFtpRequestAsync(proxyConnection, request, clientStream, cancellationToken);
                }
                else
                {
                    Logger.LogWarning("Unsupported protocol: {Protocol}", request.Protocol);
                    await SendErrorResponseAsync(clientStream, 400, "Bad Request", cancellationToken);
                }
            }
            finally
            {
                proxyConnection?.Dispose();
            }

            Logger.LogDebug("Proxy client disconnected from {RemoteIp}", remoteIp);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling proxy client from {RemoteIp}", remoteIp);
        }
        finally
        {
            client?.Close();
        }
    }

    private async Task HandleHttpRequestAsync(
        ProxyConnection connection,
        ProxyRequest request,
        NetworkStream clientStream,
        CancellationToken cancellationToken)
    {
        try
        {
            // サーバーへ接続
            if (!await connection.ConnectAsync(
                request.HostName,
                request.Port,
                request.RequestLine,
                ProxyProtocol.Http,
                cancellationToken))
            {
                await SendErrorResponseAsync(clientStream, 502, "Bad Gateway", cancellationToken);
                return;
            }

            var serverStream = connection.GetStream(ProxyConnectionSide.Server);
            if (serverStream == null)
            {
                await SendErrorResponseAsync(clientStream, 502, "Bad Gateway", cancellationToken);
                return;
            }

            // リクエストラインを送信
            var requestLine = request.GetRequestLineBytes(connection.UpperProxy.Use);
            await serverStream.WriteAsync(requestLine, 0, requestLine.Length, cancellationToken);

            // 上位プロキシ認証ヘッダーを追加
            if (connection.UpperProxy.Use && connection.UpperProxy.UseAuth)
            {
                var credentials = $"{connection.UpperProxy.AuthUser}:{connection.UpperProxy.AuthPass}";
                var credentialsBytes = Encoding.UTF8.GetBytes(credentials);
                var base64Credentials = Convert.ToBase64String(credentialsBytes);
                var authHeader = $"Proxy-Authorization: Basic {base64Credentials}\r\n";
                var authBytes = Encoding.ASCII.GetBytes(authHeader);
                await serverStream.WriteAsync(authBytes, 0, authBytes.Length, cancellationToken);
            }

            // ヘッダーを送信
            foreach (var header in request.Headers)
            {
                var headerLine = $"{header.Key}: {header.Value}\r\n";
                var headerBytes = System.Text.Encoding.ASCII.GetBytes(headerLine);
                await serverStream.WriteAsync(headerBytes, 0, headerBytes.Length, cancellationToken);
            }

            // ヘッダー終了
            await serverStream.WriteAsync(System.Text.Encoding.ASCII.GetBytes("\r\n"), cancellationToken);

            // ボディがあれば送信
            if (request.Body != null && request.Body.Length > 0)
            {
                await serverStream.WriteAsync(request.Body, 0, request.Body.Length, cancellationToken);
            }

            // レスポンスを転送（コンテンツフィルタリング対応、スレッドセーフ）
            ProxyLimitString? limitString;
            lock (_componentsLock)
            {
                limitString = _limitString;
            }

            if (limitString != null)
            {
                await RelayWithContentFilteringAsync(serverStream, clientStream, limitString, cancellationToken);
            }
            else
            {
                await RelayDataAsync(serverStream, clientStream, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling HTTP request");
            await SendErrorResponseAsync(clientStream, 500, "Internal Server Error", cancellationToken);
        }
    }

    private async Task HandleSslRequestAsync(
        ProxyConnection connection,
        ProxyRequest request,
        NetworkStream clientStream,
        CancellationToken cancellationToken)
    {
        try
        {
            // CONNECT メソッドの場合、SSL/TLSトンネルを確立
            if (!await connection.ConnectAsync(
                request.HostName,
                request.Port,
                request.RequestLine,
                ProxyProtocol.Ssl,
                cancellationToken))
            {
                await SendErrorResponseAsync(clientStream, 502, "Bad Gateway", cancellationToken);
                return;
            }

            // 接続成功を通知
            var response = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n");
            await clientStream.WriteAsync(response, 0, response.Length, cancellationToken);

            var serverStream = connection.GetStream(ProxyConnectionSide.Server);
            if (serverStream == null)
            {
                return;
            }

            // 双方向リレー
            var clientToServer = RelayDataAsync(clientStream, serverStream, cancellationToken);
            var serverToClient = RelayDataAsync(serverStream, clientStream, cancellationToken);

            await Task.WhenAny(clientToServer, serverToClient);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling SSL request");
        }
    }

    private async Task HandleFtpRequestAsync(
        ProxyConnection connection,
        ProxyRequest request,
        NetworkStream clientStream,
        CancellationToken cancellationToken)
    {
        // FTPプロキシは複雑なので、基本的なエラーレスポンスを返す
        Logger.LogWarning("FTP proxy not fully implemented");
        await SendErrorResponseAsync(clientStream, 501, "Not Implemented", cancellationToken);
    }

    private async Task SendErrorResponseAsync(
        NetworkStream stream,
        int statusCode,
        string statusText,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = $"HTTP/1.1 {statusCode} {statusText}\r\n" +
                          $"Content-Type: text/plain\r\n" +
                          $"Connection: close\r\n" +
                          $"\r\n" +
                          $"{statusCode} {statusText}\r\n";

            var responseBytes = System.Text.Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending error response");
        }
    }

    private async Task RelayDataAsync(
        NetworkStream source,
        NetworkStream destination,
        CancellationToken cancellationToken)
    {
        try
        {
            var buffer = new byte[NetworkConstants.Http.MaxLineLength];
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                await destination.FlushAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            NetworkExceptionHandler.LogNetworkException(ex, Logger, "Proxy relay");
        }
    }

    private async Task RelayWithContentFilteringAsync(
        NetworkStream source,
        NetworkStream destination,
        ProxyLimitString limitString,
        CancellationToken cancellationToken)
    {
        try
        {
            var buffer = new byte[NetworkConstants.Http.MaxLineLength];
            var responseBuffer = new List<byte>();
            var headerEndPosition = -1;
            var headersSent = false;

            // レスポンスをバッファリング（サイズ制限まで）
            while (true)
            {
                var bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0)
                    break;

                responseBuffer.AddRange(buffer.Take(bytesRead));

                // ヘッダー終了位置を検出（一度だけ）
                if (headerEndPosition == -1)
                {
                    for (int i = 0; i < responseBuffer.Count - 3; i++)
                    {
                        if (responseBuffer[i] == '\r' && responseBuffer[i + 1] == '\n' &&
                            responseBuffer[i + 2] == '\r' && responseBuffer[i + 3] == '\n')
                        {
                            headerEndPosition = i + 4;
                            break;
                        }
                    }
                }

                // サイズ制限チェック - バッファリング上限を超えた場合
                if (responseBuffer.Count > MaxBufferedResponseSize)
                {
                    Logger.LogWarning("Response size ({Size} bytes) exceeds buffer limit, switching to streaming filter mode", responseBuffer.Count);

                    // ここまでのデータをチェック
                    if (headerEndPosition > 0)
                    {
                        var bufferedBody = responseBuffer.Skip(headerEndPosition).ToArray();
                        if (limitString.Contains(bufferedBody))
                        {
                            Logger.LogWarning("Content blocked by filter in buffered portion");
                            // ヘッダー未送信なら403を返せる
                            if (!headersSent)
                            {
                                await SendErrorResponseAsync(destination, 403, "Forbidden - Content Blocked", cancellationToken);
                                return;
                            }
                            // ヘッダー送信済みなら接続を閉じる
                            return;
                        }
                    }

                    // バッファリング済みデータを送信
                    var bufferedData = responseBuffer.ToArray();
                    await destination.WriteAsync(bufferedData, 0, bufferedData.Length, cancellationToken);
                    await destination.FlushAsync(cancellationToken);
                    headersSent = true;
                    responseBuffer.Clear();

                    // 残りはストリーミングモードでフィルタリング
                    var streamBuffer = new List<byte>();
                    while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        streamBuffer.AddRange(buffer.Take(bytesRead));

                        // 定期的にフィルタリングチェック
                        if (streamBuffer.Count >= NetworkConstants.Http.MaxLineLength || bytesRead == 0)
                        {
                            var checkData = streamBuffer.ToArray();
                            if (limitString.Contains(checkData))
                            {
                                Logger.LogWarning("Content blocked by filter in streaming portion, closing connection");
                                // すでにヘッダー+一部データを送信済みなので、接続を閉じるしかない
                                return;
                            }

                            // チェックOKなら送信
                            await destination.WriteAsync(checkData, 0, checkData.Length, cancellationToken);
                            await destination.FlushAsync(cancellationToken);
                            streamBuffer.Clear();
                        }
                    }

                    // 最後のバッファをチェック
                    if (streamBuffer.Count > 0)
                    {
                        var checkData = streamBuffer.ToArray();
                        if (limitString.Contains(checkData))
                        {
                            Logger.LogWarning("Content blocked by filter in final portion, closing connection");
                            return;
                        }
                        await destination.WriteAsync(checkData, 0, checkData.Length, cancellationToken);
                        await destination.FlushAsync(cancellationToken);
                    }
                    return;
                }
            }

            // レスポンス全体をバッファリング完了（上限内）
            var responseData = responseBuffer.ToArray();

            // ヘッダーとボディを分離してフィルタリング
            if (headerEndPosition > 0 && headerEndPosition < responseData.Length)
            {
                var bodyData = responseData.Skip(headerEndPosition).ToArray();

                if (bodyData.Length > 0 && limitString.Contains(bodyData))
                {
                    Logger.LogWarning("Content blocked by filter (response size: {Size} bytes)", responseData.Length);
                    // ヘッダー未送信なので403エラーを送信可能
                    await SendErrorResponseAsync(destination, 403, "Forbidden - Content Blocked", cancellationToken);
                    return;
                }
            }

            // フィルタリングOK - レスポンス全体を転送
            if (responseData.Length > 0)
            {
                await destination.WriteAsync(responseData, 0, responseData.Length, cancellationToken);
                await destination.FlushAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            NetworkExceptionHandler.LogNetworkException(ex, Logger, "Proxy relay with filtering");
        }
    }

    private bool CheckAcl(string remoteIp, ProxyServerSettings settings)
    {
        if (settings.AclList == null || settings.AclList.Count == 0)
        {
            // ACLが設定されていない場合（fail-safe: すべて拒否）
            // Allow Mode (0): リストが空ならすべて拒否（明示的な許可がないため）
            // Deny Mode (1): リストが空ならすべて拒否（セキュリティ優先）
            Logger.LogDebug("ACL list is empty, denying access for {RemoteIp}", remoteIp);
            return false;
        }

        // IPアドレスがACLリストに含まれているかチェック
        var isInList = settings.AclList.Any(acl => MatchIpAddress(remoteIp, acl.Address));

        // Allow Mode (0): リストにあれば許可
        // Deny Mode (1): リストにあれば拒否
        if (settings.EnableAcl == 0)
        {
            return isInList;
        }
        else
        {
            return !isInList;
        }
    }

    private bool MatchIpAddress(string remoteIp, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        // 完全一致
        if (remoteIp == pattern)
            return true;

        // CIDR表記のサポート（192.168.1.0/24 形式）
        if (pattern.Contains('/'))
        {
            var parts = pattern.Split('/');
            if (parts.Length != 2)
                return false;

            if (!IPAddress.TryParse(parts[0], out var networkAddress))
                return false;

            if (!int.TryParse(parts[1], out var prefixLength))
                return false;

            if (!IPAddress.TryParse(remoteIp, out var remoteAddress))
                return false;

            // IPv4とIPv6の両方をサポート
            if (networkAddress.AddressFamily != remoteAddress.AddressFamily)
                return false;

            var networkBytes = networkAddress.GetAddressBytes();
            var remoteBytes = remoteAddress.GetAddressBytes();

            // プレフィックス長の妥当性チェック
            var maxPrefixLength = networkBytes.Length * 8;
            if (prefixLength < 0 || prefixLength > maxPrefixLength)
                return false;

            // ネットワーク部分を比較
            var fullBytes = prefixLength / 8;
            var remainingBits = prefixLength % 8;

            // 完全バイトの比較
            for (int i = 0; i < fullBytes; i++)
            {
                if (networkBytes[i] != remoteBytes[i])
                    return false;
            }

            // 残りのビットの比較
            if (remainingBits > 0)
            {
                var mask = (byte)(0xFF << (8 - remainingBits));
                if ((networkBytes[fullBytes] & mask) != (remoteBytes[fullBytes] & mask))
                    return false;
            }

            return true;
        }

        // ワイルドカード表記のサポート（簡易版）
        if (pattern.Contains('*'))
        {
            var regex = new System.Text.RegularExpressions.Regex(
                "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*") + "$"
            );
            return regex.IsMatch(remoteIp);
        }

        return false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 設定変更イベントの購読解除
            _settingsService.SettingsChanged -= OnSettingsChanged;

            // キャッシュを破棄
            _cache?.Dispose();

            // リスナーを破棄
            _listener?.Dispose();
        }

        base.Dispose(disposing);
    }
}
