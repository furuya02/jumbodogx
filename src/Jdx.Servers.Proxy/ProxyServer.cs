using System.Net;
using System.Net.Sockets;
using System.Text;
using Jdx.Core.Abstractions;
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

    // Proxyコンポーネント
    private ProxyLimitUrl? _limitUrl;
    private ProxyLimitString? _limitString;
    private ProxyCache? _cache;
    private List<string> _disableAddressList = new();

    // デフォルトタイムアウト（タイムアウト設定が0の場合に使用）
    private const int DefaultTimeoutSeconds = 60;

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
        // 上位プロキシを経由しないサーバのリスト
        _disableAddressList = settings.DisableAddressList
            .Select(e => e.Address)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToList();

        // URL制限
        _limitUrl = new ProxyLimitUrl(settings.LimitUrlAllowList, settings.LimitUrlDenyList, Logger);

        // コンテンツ制限
        if (settings.LimitStringList != null && settings.LimitStringList.Count > 0)
        {
            _limitString = new ProxyLimitString(settings.LimitStringList, Logger);
        }
        else
        {
            _limitString = null;
        }

        // キャッシュ
        if (settings.UseCache)
        {
            _cache = new ProxyCache(settings, Logger);
            Logger.LogInformation("Proxy cache enabled: Dir={CacheDir}", settings.CacheDir);
        }
        else
        {
            _cache = null;
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
        // 既存のリスナーがあれば停止
        if (_listener != null)
        {
            try
            {
                await _listener.StopAsync(CancellationToken.None);
                _listener.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error stopping existing listener");
            }
        }

        var settings = _settingsService.GetSettings().ProxyServer;

        // BindAddressをIPAddressに変換
        IPAddress? bindAddress = null;
        if (!string.IsNullOrWhiteSpace(settings.BindAddress))
        {
            if (IPAddress.TryParse(settings.BindAddress, out var parsedAddress))
            {
                bindAddress = parsedAddress;
            }
            else
            {
                Logger.LogWarning("Invalid bind address '{BindAddress}', using default (0.0.0.0)", settings.BindAddress);
                bindAddress = IPAddress.Any;
            }
        }
        else
        {
            bindAddress = IPAddress.Any;
        }

        // リスナーを作成して開始
        _listener = new ServerTcpListener(_port, bindAddress, Logger);

        await _listener.StartAsync(cancellationToken);

        Logger.LogInformation(
            "Proxy Server started: {BindAddress}:{Port}",
            bindAddress,
            _port
        );

        // クライアント接続を待機
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
                Logger.LogError(ex, "Error accepting client connection");
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

        // リスナーを停止
        if (_listener != null)
        {
            _listener.StopAsync(cancellationToken).Wait();
            _listener.Dispose();
            _listener = null;
        }

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

            // Proxy接続オブジェクトを作成
            var proxyConnection = new ProxyConnection(
                client,
                settings.TimeOut,
                upperProxy,
                Logger
            );

            try
            {
                // 1. クライアントからリクエストを読み取り
                var clientStream = client.GetStream();
                var request = new ProxyRequest();

                if (!await request.ReceiveAsync(clientStream, Logger, cancellationToken))
                {
                    Logger.LogWarning("Failed to receive request from {RemoteIp}", remoteIp);
                    return;
                }

                Logger.LogInformation("Proxy request: {Method} {Host}:{Port}{Uri}",
                    request.HttpMethod, request.HostName, request.Port, request.Uri);

                // 3. URL制限チェック
                var fullUrl = $"{request.Protocol.ToString().ToLower()}://{request.HostName}:{request.Port}{request.Uri}";
                if (_limitUrl != null && !_limitUrl.IsAllowed(fullUrl))
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
                proxyConnection.Dispose();
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

            // レスポンスを転送
            await RelayDataAsync(serverStream, clientStream, cancellationToken);
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
            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                await destination.FlushAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Relay connection closed");
        }
    }

    private bool CheckAcl(string remoteIp, ProxyServerSettings settings)
    {
        if (settings.AclList == null || settings.AclList.Count == 0)
        {
            // ACLが設定されていない場合
            // Allow Mode (0): すべて拒否
            // Deny Mode (1): すべて許可
            return settings.EnableAcl == 1;
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

        // CIDR表記のサポート（簡易版）
        if (pattern.Contains('/'))
        {
            // TODO: CIDR表記の実装
            return false;
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
