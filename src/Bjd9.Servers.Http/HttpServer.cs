using System.Net;
using System.Net.Sockets;
using System.Text;
using Bjd9.Core.Abstractions;
using Bjd9.Core.Network;
using Microsoft.Extensions.Logging;

namespace Bjd9.Servers.Http;

/// <summary>
/// HTTPサーバー
/// </summary>
public class HttpServer : ServerBase
{
    private readonly int _port;
    private ServerTcpListener? _listener;

    public HttpServer(ILogger<HttpServer> logger, int port = 8080) : base(logger)
    {
        _port = port;
    }

    public override string Name => "HttpServer";
    public override ServerType Type => ServerType.Http;
    public override int Port => _port;

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

        _listener = new ServerTcpListener(_port, null, Logger);
        await _listener.StartAsync(cancellationToken);

        Logger.LogInformation("HTTP Server listening on http://localhost:{Port}", _port);

        // クライアント接続を受け入れるループ
        _ = Task.Run(async () =>
        {
            while (!StopCts.Token.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await _listener.AcceptAsync(StopCts.Token);
                    Statistics.TotalConnections++;
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

        try
        {
            // リクエストを読み取る
            var buffer = new byte[8192];
            var bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

            if (bytesRead == 0)
            {
                return;
            }

            var requestText = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var lines = requestText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
            {
                return;
            }

            // リクエスト行をパース
            var request = HttpRequest.Parse(lines[0]);
            Statistics.TotalRequests++;

            Logger.LogInformation("HTTP {Method} {Path} from {RemoteEndPoint}",
                request.Method, request.Path, remoteEndPoint);

            // レスポンスを生成
            var response = GenerateResponse(request);

            // レスポンスを送信
            var responseBytes = response.ToBytes();
            await clientSocket.SendAsync(responseBytes, SocketFlags.None, cancellationToken);

            Statistics.TotalBytesSent += responseBytes.Length;

            Logger.LogInformation("HTTP {StatusCode} {Method} {Path}",
                response.StatusCode, request.Method, request.Path);
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

    private HttpResponse GenerateResponse(HttpRequest request)
    {
        // シンプルなルーティング
        if (request.Path == "/")
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>BJD9 HTTP Server</title>
</head>
<body>
    <h1>Welcome to BJD9 HTTP Server!</h1>
    <p>This is a simple HTTP server built with .NET 9</p>
    <p>Server Status: Running</p>
    <hr>
    <p><a href=""/stats"">View Statistics</a></p>
</body>
</html>";
            return HttpResponse.Ok(html);
        }
        else if (request.Path == "/stats")
        {
            var stats = GetStatistics();
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>BJD9 Server Statistics</title>
</head>
<body>
    <h1>Server Statistics</h1>
    <ul>
        <li>Active Connections: {stats.ActiveConnections}</li>
        <li>Total Connections: {stats.TotalConnections}</li>
        <li>Total Requests: {stats.TotalRequests}</li>
        <li>Total Bytes Sent: {stats.TotalBytesSent:N0} bytes</li>
        <li>Total Errors: {stats.TotalErrors}</li>
        <li>Uptime: {stats.Uptime:hh\:mm\:ss}</li>
    </ul>
    <hr>
    <p><a href=""/"">Back to Home</a></p>
</body>
</html>";
            return HttpResponse.Ok(html);
        }
        else
        {
            return HttpResponse.NotFound();
        }
    }
}
