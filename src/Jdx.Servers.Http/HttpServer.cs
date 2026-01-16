using System.Net;
using System.Net.Sockets;
using System.Text;
using Jdx.Core.Abstractions;
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
    private int _port;
    private ServerTcpListener? _listener;
    private HttpTarget? _target;
    private HttpContentType? _contentType;
    private HttpFileHandler? _fileHandler;
    private HttpAuthenticator? _authenticator;
    private HttpAclFilter? _aclFilter;
    private HttpCgiHandler? _cgiHandler;
    private HttpWebDavHandler? _webDavHandler;

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

        // SSIプロセッサを初期化してFileHandlerに設定
        var ssiProcessor = new HttpSsiProcessor(settings, Logger);
        _fileHandler.SetSsiProcessor(ssiProcessor);
    }

    private void OnSettingsChanged(object? sender, ApplicationSettings settings)
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

        var settings = _settingsService.GetSettings().HttpServer;

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
                Logger.LogWarning("Invalid BindAddress: {BindAddress}, using default (Any)", settings.BindAddress);
            }
        }

        _listener = new ServerTcpListener(_port, bindAddress, Logger);
        await _listener.StartAsync(cancellationToken);

        var displayAddress = bindAddress?.ToString() ?? "0.0.0.0";
        Logger.LogInformation("HTTP Server listening on http://{Address}:{Port}", displayAddress, _port);

        // クライアント接続を受け入れるループ
        _ = Task.Run(async () =>
        {
            while (!StopCts.Token.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await _listener.AcceptAsync(StopCts.Token);
                    Statistics.TotalConnections++;

                    // 最大接続数チェック
                    var currentSettings = _settingsService.GetSettings().HttpServer;
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
                        catch
                        {
                            // エラーレスポンス送信に失敗しても無視
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
                    Logger.LogError(ex, "Error accepting client connection");
                }
            }
        }, StopCts.Token);
    }

    protected override async Task StopListeningAsync(CancellationToken cancellationToken)
    {
        if (_listener != null)
        {
            await _listener.StopAsync(cancellationToken);
            _listener.Dispose();
            _listener = null;
        }
    }

    protected override async Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        var remoteEndPoint = clientSocket.RemoteEndPoint?.ToString() ?? "unknown";
        Logger.LogInformation("HTTP request from {RemoteEndPoint}", remoteEndPoint);

        var settings = _settingsService.GetSettings().HttpServer;

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
            catch
            {
                // エラーレスポンス送信に失敗しても無視
            }
            finally
            {
                clientSocket.Close();
                clientSocket.Dispose();
            }

            Statistics.TotalErrors++;
            return;
        }

        // タイムアウト設定
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        if (settings.TimeOut > 0)
        {
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(settings.TimeOut));
        }

        try
        {
            // リクエストを読み取る
            var buffer = new byte[8192];
            var bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None, linkedCts.Token);

            if (bytesRead == 0)
            {
                return;
            }

            var requestText = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var lines = requestText.Split("\r\n");

            if (lines.Length == 0)
            {
                return;
            }

            // リクエスト全体をパース（ヘッダー含む）
            var request = HttpRequest.ParseFull(lines);
            Statistics.TotalRequests++;

            Logger.LogInformation("HTTP {Method} {Path} from {RemoteEndPoint}",
                request.Method, request.Path, remoteEndPoint);

            // レスポンスを生成
            var response = await GenerateResponseAsync(request, linkedCts.Token);

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

            Logger.LogInformation("HTTP {StatusCode} {Method} {Path}",
                response.StatusCode, request.Method, request.Path);
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            Logger.LogWarning("HTTP request from {RemoteEndPoint} timed out after {TimeOut} seconds",
                remoteEndPoint, settings.TimeOut);
            Statistics.TotalErrors++;

            // タイムアウトエラーレスポンスを送信（ベストエフォート）
            try
            {
                var timeoutResponse = HttpResponseBuilder.BuildErrorResponse(408, "Request Timeout", settings);
                var timeoutBytes = timeoutResponse.ToBytes();
                await clientSocket.SendAsync(timeoutBytes, SocketFlags.None, CancellationToken.None);
            }
            catch
            {
                // タイムアウトレスポンス送信に失敗しても無視
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling HTTP request from {RemoteEndPoint}", remoteEndPoint);
            Statistics.TotalErrors++;
        }
        finally
        {
            clientSocket.Close();
            clientSocket.Dispose();
        }
    }

    private async Task<HttpResponse> GenerateResponseAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        var settings = _settingsService.GetSettings().HttpServer;

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
        var remoteIp = "127.0.0.1"; // TODO: 実際のリモートIPアドレス
        if (_cgiHandler!.IsCgiRequest(request.Path, out var scriptPath, out var interpreter))
        {
            return await _cgiHandler.ExecuteCgiAsync(scriptPath, interpreter, request, remoteIp, cancellationToken);
        }

        // ターゲット解決
        var targetInfo = _target!.ResolveTarget(request.Path);

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
            return await _fileHandler!.HandleFileAsync(targetInfo, request, settings, cancellationToken);
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
}
