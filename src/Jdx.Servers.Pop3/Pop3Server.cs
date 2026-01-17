using System.Net;
using System.Net.Sockets;
using System.Text;
using Jdx.Core.Abstractions;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Pop3;

/// <summary>
/// POP3 Server Implementation (RFC 1939)
/// Post Office Protocol Version 3
/// </summary>
public class Pop3Server : ServerBase
{
    private readonly Pop3ServerSettings _settings;
    private ServerTcpListener? _tcpListener;
    private readonly SemaphoreSlim _connectionSemaphore;

    public Pop3Server(ILogger<Pop3Server> logger, Pop3ServerSettings settings)
        : base(logger)
    {
        _settings = settings;
        _connectionSemaphore = new SemaphoreSlim(settings.MaxConnections, settings.MaxConnections);
    }

    public override string Name => "Pop3Server";
    public override ServerType Type => ServerType.Pop3;
    public override int Port => _settings.Port;

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        var bindAddress = string.IsNullOrWhiteSpace(_settings.BindAddress) || _settings.BindAddress == "0.0.0.0"
            ? IPAddress.Any
            : IPAddress.Parse(_settings.BindAddress);

        _tcpListener = new ServerTcpListener(_settings.Port, bindAddress, Logger);
        await _tcpListener.StartAsync(cancellationToken);

        Logger.LogInformation("POP3 Server started on {Address}:{Port}", _settings.BindAddress, _settings.Port);

        // Start accept loop
        _ = Task.Run(() => AcceptLoopAsync(StopCts.Token), StopCts.Token);
    }

    protected override async Task StopListeningAsync(CancellationToken cancellationToken)
    {
        if (_tcpListener != null)
        {
            await _tcpListener.StopAsync(cancellationToken);
            _tcpListener = null;
        }

        Logger.LogInformation("POP3 Server stopped");
    }

    protected override Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        // Convert Socket to TcpClient for existing handler
        var tcpClient = new TcpClient { Client = clientSocket };
        return HandleClientInternalAsync(tcpClient, cancellationToken);
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        if (_tcpListener == null)
            return;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var clientSocket = await _tcpListener.AcceptAsync(cancellationToken);
                var client = new TcpClient { Client = clientSocket };

                _ = Task.Run(async () =>
                {
                    await _connectionSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        await HandleClientInternalAsync(client, cancellationToken);
                    }
                    finally
                    {
                        _connectionSemaphore.Release();
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error accepting POP3 client");
            }
        }
    }

    private async Task HandleClientInternalAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        {
            try
            {
                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.ASCII);
                var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                // Send welcome banner
                var banner = _settings.BannerMessage
                    .Replace("$p", "JumboDogX POP3")
                    .Replace("$v", "1.0");
                await writer.WriteLineAsync($"+OK {banner}");

                Logger.LogInformation("POP3 client connected from {RemoteEndPoint}", client.Client.RemoteEndPoint);

                // Main command loop
                var authenticated = false;
                string? username = null;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (string.IsNullOrEmpty(line))
                        break;

                    Logger.LogDebug("POP3 << {Command}", line);

                    var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0)
                        continue;

                    var command = parts[0].ToUpper();
                    var args = parts.Length > 1 ? parts[1] : "";

                    switch (command)
                    {
                        case "USER":
                            username = args;
                            await writer.WriteLineAsync("+OK User accepted");
                            break;

                        case "PASS":
                            // TODO: Implement authentication
                            authenticated = true;
                            await writer.WriteLineAsync("+OK Mailbox open, 0 messages");
                            break;

                        case "STAT":
                            if (!authenticated)
                            {
                                await writer.WriteLineAsync("-ERR Not authenticated");
                                break;
                            }
                            await writer.WriteLineAsync("+OK 0 0");
                            break;

                        case "LIST":
                            if (!authenticated)
                            {
                                await writer.WriteLineAsync("-ERR Not authenticated");
                                break;
                            }
                            await writer.WriteLineAsync("+OK 0 messages");
                            await writer.WriteLineAsync(".");
                            break;

                        case "RETR":
                            if (!authenticated)
                            {
                                await writer.WriteLineAsync("-ERR Not authenticated");
                                break;
                            }
                            await writer.WriteLineAsync("-ERR No such message");
                            break;

                        case "DELE":
                            if (!authenticated)
                            {
                                await writer.WriteLineAsync("-ERR Not authenticated");
                                break;
                            }
                            await writer.WriteLineAsync("-ERR No such message");
                            break;

                        case "NOOP":
                            await writer.WriteLineAsync("+OK");
                            break;

                        case "RSET":
                            await writer.WriteLineAsync("+OK");
                            break;

                        case "QUIT":
                            await writer.WriteLineAsync("+OK Goodbye");
                            return;

                        default:
                            await writer.WriteLineAsync($"-ERR Unknown command: {command}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling POP3 client");
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tcpListener?.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
            _connectionSemaphore?.Dispose();
        }
        base.Dispose(disposing);
    }
}
