using System.Net;
using System.Net.Sockets;
using System.Text;
using Jdx.Core.Abstractions;
using Jdx.Core.Constants;
using Jdx.Core.Helpers;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Http;

/// <summary>
/// HTTPサーバー
/// </summary>
public class HttpServer : ServerBase
{
    private readonly ISettingsService _settingsService;
    private readonly ReaderWriterLockSlim _settingsLock = new ReaderWriterLockSlim();
    private int _port;
    private ServerTcpListener? _listener;
    private HttpTarget? _target;
    private HttpContentType? _contentType;
    private HttpFileHandler? _fileHandler;
    private HttpAuthenticator? _authenticator;
    private HttpAclFilter? _aclFilter;
    private HttpCgiHandler? _cgiHandler;
    private HttpWebDavHandler? _webDavHandler;
    private HttpAttackDb? _attackDb;
    private HttpVirtualHostManager? _virtualHostManager;
    private HttpSslManager? _sslManager;

    // デフォルトタイムアウト（タイムアウト設定が0の場合に使用）
    private const int DefaultTimeoutSeconds = 30;

    public HttpServer(ILogger<HttpServer> logger, ISettingsService settingsService) : base(logger)
    {
        _settingsService = settingsService;

        // 初期設定を取得
        var settings = _settingsService.GetSettings().HttpServer;
        _port = settings.Port;

        // コンポーネントを初期化
        InitializeComponents(settings);

        // 設定変更イベントを購読
        _settingsService.SettingsChanged += OnSettingsChanged;
    }

    public override string Name => "HttpServer";
    public override ServerType Type => ServerType.Http;
    public override int Port => _port;

    private void InitializeComponents(HttpServerSettings settings)
    {
        _target = new HttpTarget(settings, Logger);
        _contentType = new HttpContentType(settings);
        _fileHandler = new HttpFileHandler(Logger, _contentType);
        _authenticator = new HttpAuthenticator(settings, Logger);
        _aclFilter = new HttpAclFilter(settings, Logger);
        _cgiHandler = new HttpCgiHandler(settings, Logger);
        _webDavHandler = new HttpWebDavHandler(settings, Logger);

        // Virtual Host初期化
        _virtualHostManager = new HttpVirtualHostManager(settings, Logger);
        if (settings.VirtualHosts != null && settings.VirtualHosts.Count > 0)
        {
            Logger.LogInformation("Virtual Host enabled: {Count} hosts configured", settings.VirtualHosts.Count);
        }

        // SSL/TLS初期化
        _sslManager = new HttpSslManager(settings.CertificateFile, settings.CertificatePassword, Logger);
        if (_sslManager.IsEnabled)
        {
            Logger.LogInformation("HTTPS/SSL enabled");
            // TODO: ServerTcpListenerをSSL対応に変更する必要があります
            // 現在の実装では、HTTPのみサポートしています
            Logger.LogWarning("SSL/TLS is initialized but not yet fully integrated. HTTP-only mode.");
        }

        // AttackDb初期化（UseAutoAclが有効な場合）
        if (settings.UseAutoAcl)
        {
            _attackDb = new HttpAttackDb(Logger, timeWindowSeconds: 120, maxAttempts: 1);
            Logger.LogInformation("AttackDb enabled: timeWindow=120s, maxAttempts=1");
        }
        else
        {
            _attackDb = null;
        }

        // SSIプロセッサを初期化してFileHandlerに設定
        var ssiProcessor = new HttpSsiProcessor(settings, Logger);
        _fileHandler.SetSsiProcessor(ssiProcessor);

        // AttackDb攻撃検出コールバックを設定
        if (_attackDb != null)
        {
            _fileHandler.SetAttackDetectedCallback((remoteIp) =>
            {
                // Apache Killer攻撃検出時の処理
                if (_attackDb.IsInjustice(false, remoteIp))
                {
                    Logger.LogWarning("Attack detected from {RemoteIp}", remoteIp);
                    // TODO: ACL自動追加機能
                    // ISettingsService.GetSettings()でACL設定を取得し、
                    // AclListに新しいAclEntryを追加してSaveSettingsAsync()で保存する。
                    // EnableAcl設定がDenyMode(1)であることを確認する必要がある。
                }
            });
        }
    }

    private void OnSettingsChanged(object? sender, ApplicationSettings settings)
    {
        _settingsLock.EnterWriteLock();
        try
        {
            var httpSettings = settings.HttpServer;

            // ポート変更があればサーバーを再起動
            if (_port != httpSettings.Port)
            {
                _port = httpSettings.Port;
                Logger.LogInformation("HTTP Server port changed to {Port}", _port);
            }

            // コンポーネントを再初期化
            InitializeComponents(httpSettings);
            Logger.LogInformation("HTTP Server settings updated");
        }
        finally
        {
            _settingsLock.ExitWriteLock();
        }
    }

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        HttpServerSettings settings;
        _settingsLock.EnterReadLock();
        try
        {
            settings = _settingsService.GetSettings().HttpServer;
        }
        finally
        {
            _settingsLock.ExitReadLock();
        }

        _listener = await CreateTcpListenerAsync(_port, settings.BindAddress, cancellationToken);

        var displayAddress = settings.BindAddress ?? "0.0.0.0";
        Logger.LogInformation("HTTP Server listening on http://{Address}:{Port}", displayAddress, _port);

        // クライアント接続を受け入れるループ
        // 注: ServerBase.RunTcpAcceptLoopAsync()は使用していません。
        // 理由: 最大接続数到達時に503 Service Unavailableレスポンスを送信する必要があるため。
        // ConnectionLimiterではレスポンス送信前に接続を拒否してしまうため、
        // カスタムループで接続受け入れ後に503を返す実装が必要です。
        _ = Task.Run(async () =>
        {
            while (!StopCts.Token.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await _listener.AcceptAsync(StopCts.Token);
                    Statistics.TotalConnections++;

                    // 最大接続数チェック
                    HttpServerSettings currentSettings;
                    _settingsLock.EnterReadLock();
                    try
                    {
                        currentSettings = _settingsService.GetSettings().HttpServer;
                    }
                    finally
                    {
                        _settingsLock.ExitReadLock();
                    }

                    if (currentSettings.MaxConnections > 0 && Statistics.ActiveConnections >= currentSettings.MaxConnections)
                    {
                        Logger.LogWarning("Max connections ({MaxConnections}) reached, rejecting connection from {RemoteEndPoint}",
                            currentSettings.MaxConnections, clientSocket.RemoteEndPoint);

                        // 503 Service Unavailable を送信してクローズ
                        try
                        {
                            var errorResponse = HttpResponseBuilder.BuildErrorResponse(503, "Service Unavailable", currentSettings);
                            var errorBytes = errorResponse.ToBytes();
                            await clientSocket.SendAsync(errorBytes, SocketFlags.None, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            // エラーレスポンス送信に失敗してもログのみ出力
                            Logger.LogDebug(ex, "Failed to send error response to client");
                        }
                        finally
                        {
                            clientSocket.Close();
                            clientSocket.Dispose();
                        }

                        Statistics.TotalErrors++;
                        continue;
                    }

                    Statistics.ActiveConnections++;

                    // クライアント処理を非同期で実行
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await HandleClientAsync(clientSocket, StopCts.Token);
                        }
                        finally
                        {
                            Statistics.ActiveConnections--;
                        }
                    }, StopCts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    NetworkExceptionHandler.LogNetworkException(ex, Logger, "HTTP client accept");
                }
            }
        }, StopCts.Token);
    }

    protected override Task StopListeningAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("HTTP Server stopped");
        return Task.CompletedTask;
    }

    protected override async Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        var remoteEndPoint = clientSocket.RemoteEndPoint?.ToString() ?? "unknown";
        Logger.LogInformation("HTTP connection from {RemoteEndPoint}", remoteEndPoint);

        HttpServerSettings settings;
        _settingsLock.EnterReadLock();
        try
        {
            settings = _settingsService.GetSettings().HttpServer;
        }
        finally
        {
            _settingsLock.ExitReadLock();
        }

        // ACLチェック（接続元IPアドレス）
        var remoteIp = clientSocket.RemoteEndPoint?.ToString()?.Split(':')[0] ?? "";
        if (!string.IsNullOrEmpty(remoteIp) && !_aclFilter!.IsAllowed(remoteIp))
        {
            Logger.LogWarning("Connection from {RemoteIP} rejected by ACL", remoteIp);

            // 403 Forbidden を送信してクローズ
            try
            {
                var forbiddenResponse = HttpResponseBuilder.BuildErrorResponse(403, "Forbidden", settings);
                var forbiddenBytes = forbiddenResponse.ToBytes();
                await clientSocket.SendAsync(forbiddenBytes, SocketFlags.None, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // エラーレスポンス送信に失敗してもログのみ出力
                Logger.LogDebug(ex, "Failed to send 403 Forbidden response");
            }
            finally
            {
                clientSocket.Close();
                clientSocket.Dispose();
            }

            Statistics.TotalErrors++;
            return;
        }

        // Keep-Alive用の変数
        var keepAlive = settings.UseKeepAlive;
        var requestCount = 0;

        try
        {
            // Keep-Aliveループ：接続を維持しながら複数のリクエストを処理
            while (keepAlive && !cancellationToken.IsCancellationRequested)
            {
                requestCount++;

                // 最大リクエスト数チェック
                if (settings.MaxKeepAliveRequests > 0 && requestCount > settings.MaxKeepAliveRequests)
                {
                    Logger.LogDebug("Max keep-alive requests ({Max}) reached for {RemoteEndPoint}",
                        settings.MaxKeepAliveRequests, remoteEndPoint);
                    break;
                }

                // Keep-Aliveタイムアウト設定
                using var keepAliveCts = new CancellationTokenSource();
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, keepAliveCts.Token);

                var timeout = requestCount == 1 ? settings.TimeOut : settings.KeepAliveTimeout;
                // タイムアウトが0の場合はデフォルト値を使用（無限待機を防ぐ）
                if (timeout <= 0)
                {
                    timeout = DefaultTimeoutSeconds;
                }
                keepAliveCts.CancelAfter(TimeSpan.FromSeconds(timeout));

                try
                {
                    // リクエストを読み取る
                    var buffer = new byte[8192];
                    var bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None, linkedCts.Token);

                    if (bytesRead == 0)
                    {
                        // クライアントが接続を閉じた
                        Logger.LogDebug("Client closed connection: {RemoteEndPoint}", remoteEndPoint);
                        break;
                    }

                    var requestText = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var lines = requestText.Split("\r\n");

                    if (lines.Length == 0)
                    {
                        break;
                    }

                    // リクエスト全体をパース（ヘッダー含む）
                    var request = HttpRequest.ParseFull(lines);
                    Statistics.TotalRequests++;

                    // リクエストボディサイズチェック（DoS対策、RFC 7230準拠）
                    if (request.Headers.TryGetValue("Content-Length", out var contentLengthStr))
                    {
                        if (long.TryParse(contentLengthStr, out var contentLength))
                        {
                            if (contentLength > NetworkConstants.Http.MaxRequestBodySize)
                            {
                                Logger.LogWarning("Request body size {Size} exceeds limit {Max} from {RemoteEndPoint}",
                                    contentLength, NetworkConstants.Http.MaxRequestBodySize, remoteEndPoint);

                                // 413 Payload Too Large を返す
                                var errorResponse = HttpResponseBuilder.BuildErrorResponse(413, "Payload Too Large", settings);
                                var errorBytes = errorResponse.ToBytes();
                                await clientSocket.SendAsync(errorBytes, SocketFlags.None, linkedCts.Token);
                                Statistics.TotalErrors++;
                                keepAlive = false;
                                break;
                            }
                        }
                    }

                    Logger.LogInformation("HTTP {Method} {Path} from {RemoteEndPoint} (request #{Count})",
                        request.Method, request.Path, remoteEndPoint, requestCount);

                    // Keep-Aliveの判定
                    keepAlive = ShouldKeepAlive(request, settings);

                    // レスポンスを生成
                    var response = await GenerateResponseAsync(request, linkedCts.Token, remoteIp);

                    // Connection ヘッダーを設定
                    if (keepAlive && requestCount < settings.MaxKeepAliveRequests)
                    {
                        response.Headers["Connection"] = "keep-alive";
                        response.Headers["Keep-Alive"] = $"timeout={settings.KeepAliveTimeout}, max={settings.MaxKeepAliveRequests - requestCount}";
                    }
                    else
                    {
                        response.Headers["Connection"] = "close";
                        keepAlive = false;
                    }

                    // レスポンスを送信（ストリーム対応）
                    if (response.BodyStream != null)
                    {
                        await response.SendAsync(clientSocket, linkedCts.Token);
                        Statistics.TotalBytesSent += response.ContentLength ?? 0;
                    }
                    else
                    {
                        var responseBytes = response.ToBytes();
                        await clientSocket.SendAsync(responseBytes, SocketFlags.None, linkedCts.Token);
                        Statistics.TotalBytesSent += responseBytes.Length;
                    }

                    Logger.LogInformation("HTTP {StatusCode} {Method} {Path} - Keep-Alive: {KeepAlive}",
                        response.StatusCode, request.Method, request.Path, keepAlive);
                }
                catch (OperationCanceledException) when (keepAliveCts.Token.IsCancellationRequested)
                {
                    // Keep-Aliveタイムアウト
                    if (requestCount == 1)
                    {
                        Logger.LogWarning("HTTP request from {RemoteEndPoint} timed out after {TimeOut} seconds",
                            remoteEndPoint, timeout);
                        Statistics.TotalErrors++;

                        // タイムアウトエラーレスポンスを送信（ベストエフォート）
                        try
                        {
                            var timeoutResponse = HttpResponseBuilder.BuildErrorResponse(408, "Request Timeout", settings);
                            var timeoutBytes = timeoutResponse.ToBytes();
                            await clientSocket.SendAsync(timeoutBytes, SocketFlags.None, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            // エラーレスポンス送信に失敗してもログのみ出力
                            Logger.LogDebug(ex, "Failed to send error response to client");
                        }
                    }
                    else
                    {
                        Logger.LogDebug("Keep-alive timeout for {RemoteEndPoint} after {Count} requests",
                            remoteEndPoint, requestCount - 1);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    NetworkExceptionHandler.LogNetworkException(ex, Logger, "HTTP request handling");
                    Statistics.TotalErrors++;
                    break;
                }
            }

            Logger.LogDebug("HTTP connection closed: {RemoteEndPoint} ({Count} requests processed)",
                remoteEndPoint, requestCount - 1);
        }
        finally
        {
            clientSocket.Close();
            clientSocket.Dispose();
        }
    }

    /// <summary>
    /// Keep-Aliveを維持するかどうかを判定
    /// </summary>
    private bool ShouldKeepAlive(HttpRequest request, HttpServerSettings settings)
    {
        if (!settings.UseKeepAlive)
        {
            return false;
        }

        // HTTP/1.0 の場合、Connection: Keep-Alive ヘッダーが必要
        if (request.Version == "HTTP/1.0")
        {
            if (request.Headers.TryGetValue("Connection", out var connection))
            {
                return connection.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        // HTTP/1.1 の場合、デフォルトでKeep-Alive（Connection: close で明示的に無効化）
        if (request.Headers.TryGetValue("Connection", out var connectionHeader))
        {
            return !connectionHeader.Equals("close", StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    private async Task<HttpResponse> GenerateResponseAsync(HttpRequest request, CancellationToken cancellationToken, string? remoteIp = null)
    {
        HttpServerSettings settings;
        _settingsLock.EnterReadLock();
        try
        {
            settings = _settingsService.GetSettings().HttpServer;
        }
        finally
        {
            _settingsLock.ExitReadLock();
        }

        // Virtual Host解決
        string? documentRoot = null;
        if (_virtualHostManager != null && request.Headers.TryGetValue("Host", out var hostHeader))
        {
            // ローカルアドレス・ポートの取得（TODO: 実際の値を取得）
            var localAddress = settings.BindAddress;
            var localPort = _port;

            documentRoot = _virtualHostManager.ResolveDocumentRoot(hostHeader, localAddress, localPort);
            if (documentRoot != settings.DocumentRoot)
            {
                Logger.LogDebug("Virtual host resolved: {Host} -> {DocumentRoot}", hostHeader, documentRoot);
            }
        }

        // 組み込みルート（/stats）
        if (request.Path == "/stats")
        {
            return GenerateStatsResponse();
        }

        // 認証チェック
        var authResult = _authenticator!.Authenticate(request.Path, request);

        if (authResult.IsUnauthorized)
        {
            // 401 Unauthorized
            return new HttpResponse
            {
                StatusCode = 401,
                StatusText = "Unauthorized",
                Body = "<html><body><h1>401 Unauthorized</h1><p>Authentication required</p></body></html>",
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "text/html; charset=utf-8",
                    ["WWW-Authenticate"] = $"Basic realm=\"{authResult.Realm}\"",
                    ["Server"] = settings.ServerHeader.Replace("$v", GetVersion()),
                    ["Date"] = DateTime.UtcNow.ToString("R")
                }
            };
        }

        if (authResult.IsForbidden)
        {
            // 403 Forbidden
            return HttpResponseBuilder.BuildErrorResponse(403, "Forbidden", settings);
        }

        // WebDAVチェック
        if (_webDavHandler!.IsWebDavRequest(request.Path, request.Method, out var webDavPhysicalPath, out var allowWrite))
        {
            return await _webDavHandler.HandleWebDavAsync(request.Method, request.Path, webDavPhysicalPath, allowWrite, request, cancellationToken);
        }

        // CGIチェック
        if (_cgiHandler!.IsCgiRequest(request.Path, out var scriptPath, out var interpreter))
        {
            return await _cgiHandler.ExecuteCgiAsync(scriptPath, interpreter, request, remoteIp ?? "127.0.0.1", cancellationToken);
        }

        // ターゲット解決（Virtual Host対応）
        var targetInfo = _target!.ResolveTarget(request.Path, documentRoot);

        if (!targetInfo.IsValid)
        {
            // エラーレスポンス
            var statusCode = targetInfo.ErrorMessage.Contains("Not Found") ? 404 :
                           targetInfo.ErrorMessage.Contains("Forbidden") || targetInfo.ErrorMessage.Contains("Access denied") ? 403 : 500;
            var statusText = targetInfo.ErrorMessage.Contains("Not Found") ? "Not Found" :
                            targetInfo.ErrorMessage.Contains("Forbidden") || targetInfo.ErrorMessage.Contains("Access denied") ? "Forbidden" : "Internal Server Error";

            return HttpResponseBuilder.BuildErrorResponse(statusCode, statusText, settings);
        }

        // ファイルハンドラで処理
        if (targetInfo.Type == TargetType.StaticFile || targetInfo.Type == TargetType.Directory)
        {
            return await _fileHandler!.HandleFileAsync(targetInfo, request, settings, cancellationToken, remoteIp);
        }

        // その他（本来ここには来ないはず）
        return HttpResponseBuilder.BuildErrorResponse(500, "Internal Server Error", settings);
    }

    private HttpResponse GenerateStatsResponse()
    {
        var stats = GetStatistics();
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>JumboDogX Server Statistics</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        h1 {{ color: #333; border-bottom: 2px solid #333; padding-bottom: 10px; }}
        ul {{ list-style: none; padding: 0; }}
        li {{ padding: 8px; margin: 5px 0; background-color: #f5f5f5; border-left: 4px solid #4CAF50; }}
        .value {{ font-weight: bold; color: #4CAF50; }}
    </style>
</head>
<body>
    <h1>JumboDogX HTTP Server Statistics</h1>
    <ul>
        <li>Active Connections: <span class=""value"">{stats.ActiveConnections}</span></li>
        <li>Total Connections: <span class=""value"">{stats.TotalConnections}</span></li>
        <li>Total Requests: <span class=""value"">{stats.TotalRequests}</span></li>
        <li>Total Bytes Sent: <span class=""value"">{stats.TotalBytesSent:N0}</span> bytes</li>
        <li>Total Errors: <span class=""value"">{stats.TotalErrors}</span></li>
        <li>Uptime: <span class=""value"">{stats.Uptime:hh\:mm\:ss}</span></li>
    </ul>
    <hr>
    <p><a href=""/"">Back to Home</a></p>
</body>
</html>";
        return HttpResponse.Ok(html);
    }

    private string GetVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "9.0.0";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _settingsLock?.Dispose();
        }
        base.Dispose(disposing);
    }
}
