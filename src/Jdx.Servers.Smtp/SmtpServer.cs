using System.Net;
using System.Net.Sockets;
using System.Text;
using Jdx.Core.Abstractions;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Smtp;

/// <summary>
/// SMTP Server Implementation (RFC 5321)
/// Simple Mail Transfer Protocol
/// </summary>
public class SmtpServer : ServerBase
{
    private readonly SmtpServerSettings _settings;
    private ServerTcpListener? _tcpListener;
    private readonly SemaphoreSlim _connectionSemaphore;

    public SmtpServer(ILogger<SmtpServer> logger, SmtpServerSettings settings)
        : base(logger)
    {
        _settings = settings;
        _connectionSemaphore = new SemaphoreSlim(settings.MaxConnections, settings.MaxConnections);
    }

    public override string Name => "SmtpServer";
    public override ServerType Type => ServerType.Smtp;
    public override int Port => _settings.Port;

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        var bindAddress = string.IsNullOrWhiteSpace(_settings.BindAddress) || _settings.BindAddress == "0.0.0.0"
            ? IPAddress.Any
            : IPAddress.Parse(_settings.BindAddress);

        _tcpListener = new ServerTcpListener(_settings.Port, bindAddress, Logger);
        await _tcpListener.StartAsync(cancellationToken);

        Logger.LogInformation("SMTP Server started on {Address}:{Port} (Domain: {Domain})",
            _settings.BindAddress, _settings.Port, _settings.DomainName);

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

        Logger.LogInformation("SMTP Server stopped");
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
                Logger.LogError(ex, "Error accepting SMTP client");
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

                // Send greeting
                var banner = _settings.BannerMessage
                    .Replace("$d", _settings.DomainName)
                    .Replace("$v", "1.0");
                await writer.WriteLineAsync($"220 {banner}");

                Logger.LogInformation("SMTP client connected from {RemoteEndPoint}", client.Client.RemoteEndPoint);

                // Session state
                string? mailFrom = null;
                var rcptTo = new List<string>();
                var messageLines = new List<string>();

                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (string.IsNullOrEmpty(line))
                        break;

                    Logger.LogDebug("SMTP << {Command}", line);

                    var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0)
                        continue;

                    var command = parts[0].ToUpper();
                    var args = parts.Length > 1 ? parts[1] : "";

                    switch (command)
                    {
                        case "HELO":
                        case "EHLO":
                            await writer.WriteLineAsync($"250 {_settings.DomainName}");
                            if (command == "EHLO" && _settings.UseEsmtp)
                            {
                                // ESMTP extensions
                                if (_settings.UseAuthCramMd5)
                                    await writer.WriteLineAsync("250-AUTH CRAM-MD5");
                                if (_settings.UseAuthPlain)
                                    await writer.WriteLineAsync("250-AUTH PLAIN");
                                if (_settings.UseAuthLogin)
                                    await writer.WriteLineAsync("250-AUTH LOGIN");
                                await writer.WriteLineAsync($"250 SIZE {_settings.SizeLimit * 1024}");
                            }
                            break;

                        case "MAIL":
                            if (args.StartsWith("FROM:", StringComparison.OrdinalIgnoreCase))
                            {
                                mailFrom = args[5..].Trim();
                                rcptTo.Clear();
                                await writer.WriteLineAsync("250 OK");
                            }
                            else
                            {
                                await writer.WriteLineAsync("501 Syntax error");
                            }
                            break;

                        case "RCPT":
                            if (args.StartsWith("TO:", StringComparison.OrdinalIgnoreCase))
                            {
                                var recipient = args[3..].Trim();
                                rcptTo.Add(recipient);
                                await writer.WriteLineAsync("250 OK");
                            }
                            else
                            {
                                await writer.WriteLineAsync("501 Syntax error");
                            }
                            break;

                        case "DATA":
                            if (string.IsNullOrEmpty(mailFrom) || rcptTo.Count == 0)
                            {
                                await writer.WriteLineAsync("503 Bad sequence of commands");
                                break;
                            }

                            await writer.WriteLineAsync("354 Start mail input; end with <CRLF>.<CRLF>");

                            // Read message data
                            messageLines.Clear();
                            while (true)
                            {
                                var dataLine = await reader.ReadLineAsync(cancellationToken);
                                if (dataLine == ".")
                                    break;
                                if (dataLine == null)
                                    break;
                                messageLines.Add(dataLine);
                            }

                            // TODO: Process message (save, relay, etc.)
                            Logger.LogInformation("SMTP mail from {From} to {Recipients} ({Lines} lines)",
                                mailFrom, string.Join(", ", rcptTo), messageLines.Count);

                            await writer.WriteLineAsync("250 OK: Message accepted");

                            // Reset session
                            mailFrom = null;
                            rcptTo.Clear();
                            messageLines.Clear();
                            break;

                        case "RSET":
                            mailFrom = null;
                            rcptTo.Clear();
                            messageLines.Clear();
                            await writer.WriteLineAsync("250 OK");
                            break;

                        case "NOOP":
                            await writer.WriteLineAsync("250 OK");
                            break;

                        case "QUIT":
                            await writer.WriteLineAsync($"221 {_settings.DomainName} closing connection");
                            return;

                        default:
                            await writer.WriteLineAsync($"500 Unknown command: {command}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling SMTP client");
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
