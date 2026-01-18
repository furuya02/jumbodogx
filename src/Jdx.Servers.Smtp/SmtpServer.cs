using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Jdx.Core.Abstractions;
using Jdx.Core.Constants;
using Jdx.Core.Helpers;
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
    // Email address validation regex (RFC 5321 compliant)
    private static readonly Regex EmailRegex = new(
        @"^<?([a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*)>?$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(NetworkConstants.Timeouts.RegexTimeoutMilliseconds));

    private readonly SmtpServerSettings _settings;
    private readonly ConnectionLimiter _connectionLimiter;

    public SmtpServer(ILogger<SmtpServer> logger, SmtpServerSettings settings)
        : base(logger)
    {
        _settings = settings;
        _connectionLimiter = new ConnectionLimiter(settings.MaxConnections);
    }

    public override string Name => "SmtpServer";
    public override ServerType Type => ServerType.Smtp;
    public override int Port => _settings.Port;

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        var listener = await CreateTcpListenerAsync(
            _settings.Port,
            _settings.BindAddress,
            cancellationToken);

        Logger.LogInformation("SMTP Server started on {Address}:{Port} (Domain: {Domain})",
            _settings.BindAddress, _settings.Port, _settings.DomainName);

        // Start accept loop
        _ = Task.Run(() => RunTcpAcceptLoopAsync(
            listener,
            HandleClientInternalAsync,
            _connectionLimiter,
            StopCts.Token), StopCts.Token);
    }

    protected override Task StopListeningAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("SMTP Server stopped");
        return Task.CompletedTask;
    }

    protected override Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        // Convert Socket to TcpClient for existing handler
        var tcpClient = new TcpClient { Client = clientSocket };
        return HandleClientInternalAsync(tcpClient, cancellationToken);
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
                // メッセージをメモリに格納（最大100,000行 x 平均80文字 ≈ 8MB/接続）
                // MaxConnections制限により、総メモリ使用量は管理可能
                // TODO: 大規模運用時はストリーミング処理への変更を検討
                var messageLines = new List<string>();

                while (!cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (string.IsNullOrEmpty(line))
                        break;

                    // 行長制限チェック（DoS対策、RFC 5321準拠）
                    if (line.Length > NetworkConstants.Smtp.MaxLineLength)
                    {
                        await writer.WriteLineAsync("500 Line too long");
                        Logger.LogWarning("SMTP line too long: {Length} bytes (max: {Max})",
                            line.Length, NetworkConstants.Smtp.MaxLineLength);
                        break;
                    }

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
                                // 受信者数制限チェック（DoS対策）
                                if (rcptTo.Count >= NetworkConstants.Smtp.MaxRecipients)
                                {
                                    await writer.WriteLineAsync($"452 Too many recipients (max {NetworkConstants.Smtp.MaxRecipients})");
                                    break;
                                }

                                var recipient = args[3..].Trim();

                                // メールアドレス形式の検証（RFC 5321準拠、ReDoS保護付き）
                                try
                                {
                                    if (!EmailRegex.IsMatch(recipient))
                                    {
                                        await writer.WriteLineAsync("553 Invalid recipient address format");
                                        Logger.LogWarning("SMTP invalid recipient address: {Recipient}", recipient);
                                        break;
                                    }
                                }
                                catch (RegexMatchTimeoutException)
                                {
                                    await writer.WriteLineAsync("553 Invalid recipient address");
                                    Logger.LogWarning("SMTP regex timeout validating recipient: {Recipient}", recipient);
                                    break;
                                }

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

                            // Read message data with size limits
                            messageLines.Clear();
                            long totalBytes = 0;
                            var sizeLimitBytes = _settings.SizeLimit * 1024L;

                            while (true)
                            {
                                var dataLine = await reader.ReadLineAsync(cancellationToken);
                                if (dataLine == ".")
                                    break;
                                if (dataLine == null)
                                    break;

                                // 行長制限チェック（DoS対策、RFC 5321準拠）
                                if (dataLine.Length > NetworkConstants.Smtp.MaxLineLength)
                                {
                                    await writer.WriteLineAsync("552 Line too long in message body");
                                    Logger.LogWarning("SMTP message line too long: {Length} bytes (max: {Max})",
                                        dataLine.Length, NetworkConstants.Smtp.MaxLineLength);
                                    goto ResetSession;
                                }

                                // 行数制限チェック（DoS対策）
                                if (messageLines.Count >= NetworkConstants.Smtp.MaxMessageLines)
                                {
                                    await writer.WriteLineAsync($"552 Too many lines (max {NetworkConstants.Smtp.MaxMessageLines})");
                                    Logger.LogWarning("SMTP message exceeds max lines: {Count}", messageLines.Count);
                                    goto ResetSession;
                                }

                                // メッセージサイズ制限チェック（SizeLimit設定を実際に適用）
                                totalBytes += dataLine.Length + 2; // +2 for CRLF
                                if (totalBytes > sizeLimitBytes)
                                {
                                    await writer.WriteLineAsync($"552 Message size exceeds fixed maximum message size ({_settings.SizeLimit} KB)");
                                    Logger.LogWarning("SMTP message size exceeds limit: {Size} KB", totalBytes / 1024);
                                    goto ResetSession;
                                }

                                messageLines.Add(dataLine);
                            }

                            // TODO: Process message (save, relay, etc.)
                            Logger.LogInformation("SMTP mail from {From} to {Recipients} ({Lines} lines, {Size} KB)",
                                mailFrom, string.Join(", ", rcptTo), messageLines.Count, totalBytes / 1024);

                            await writer.WriteLineAsync("250 OK: Message accepted");

                        ResetSession:

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
                NetworkExceptionHandler.LogNetworkException(ex, Logger, "SMTP client handling");
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connectionLimiter?.Dispose();
        }
        base.Dispose(disposing);
    }
}
