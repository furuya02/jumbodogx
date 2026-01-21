using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Jdx.Core.Abstractions;
using Jdx.Core.Constants;
using Jdx.Core.Helpers;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Ftp;

/// <summary>
/// FTP Server implementation
/// Based on bjd5-master/FtpServer/Server.cs
/// </summary>
public class FtpServer : ServerBase
{
    private readonly ISettingsService _settingsService;
    private FtpUserManager? _userManager;
    private FtpMountManager? _mountManager;
    private FtpCommandHandler? _commandHandler;
    private FtpAclFilter? _aclFilter;
    private ServerTcpListener? _listener;
    private int _port;

    public FtpServer(ILogger<FtpServer> logger, ISettingsService settingsService)
        : base(logger)
    {
        _settingsService = settingsService;

        var settings = _settingsService.GetSettings();
        _port = settings.FtpServer.Port;
    }

    public override string Name => "FtpServer";
    public override ServerType Type => ServerType.Ftp;
    public override int Port => _port;

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        var settings = _settingsService.GetSettings().FtpServer;
        _port = settings.Port;

        // Initialize components
        _userManager = new FtpUserManager(settings.UserList, Logger);
        _mountManager = new FtpMountManager(settings.MountList);
        _commandHandler = new FtpCommandHandler(settings, _userManager, _mountManager, Logger);
        _aclFilter = new FtpAclFilter(settings, Logger);

        Logger.LogInformation("Starting FTP Server on {Address}:{Port}",
            settings.BindAddress, settings.Port);

        _listener = await CreateTcpListenerAsync(_port, settings.BindAddress, cancellationToken);

        Logger.LogInformation("FTP Server started successfully");
    }

    protected override async Task StopListeningAsync(CancellationToken cancellationToken)
    {
        await StopTcpListenerAsync();
        Logger.LogInformation("FTP Server stopped");
    }

    protected override async Task HandleClientAsync(Socket socket, CancellationToken cancellationToken)
    {
        var remoteAddress = socket.RemoteEndPoint?.ToString() ?? "unknown";
        Logger.LogDebug("FTP client connected: {RemoteAddress}", remoteAddress);

        var settings = _settingsService.GetSettings().FtpServer;

        // ACL check
        if (_aclFilter != null && !_aclFilter.IsAllowed(remoteAddress))
        {
            Logger.LogWarning("Connection rejected by ACL: {RemoteAddress}", remoteAddress);
            socket.Close();
            return;
        }

        var client = new TcpClient { Client = socket };
        var session = new FtpSession
        {
            RemoteAddress = remoteAddress,
            ControlStream = client.GetStream(),
        };

        // Validate control stream
        if (session.ControlStream == null)
        {
            Logger.LogError("Failed to get network stream from client: {RemoteAddress}", remoteAddress);
            socket.Close();
            return;
        }

        try
        {
            session.ControlReader = new StreamReader(session.ControlStream);
            session.ControlWriter = new StreamWriter(session.ControlStream) { AutoFlush = true };

            // Send banner
            await session.SendResponseAsync($"220 {settings.BannerMessage}");

            var continueSession = true;

            // Main command loop
            while (!cancellationToken.IsCancellationRequested && continueSession)
            {
                // Read command with timeout
                var readTask = session.ControlReader.ReadLineAsync(cancellationToken).AsTask();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(settings.TimeOut), cancellationToken);

                var completedTask = await Task.WhenAny(readTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Logger.LogWarning("Client timeout: {RemoteAddress}", remoteAddress);
                    await session.SendResponseAsync("421 Timeout.");
                    break;
                }

                var line = await readTask;

                if (line == null)
                {
                    // Client disconnected
                    Logger.LogDebug("Client disconnected: {RemoteAddress}", remoteAddress);
                    break;
                }

                // コマンドライン長制限チェック（DoS対策、RFC 959準拠）
                if (line.Length > NetworkConstants.Ftp.MaxCommandLineLength)
                {
                    await session.SendResponseAsync("500 Command line too long");
                    Logger.LogWarning("FTP command line too long from {RemoteAddress}: {Length} bytes (max: {Max})",
                        remoteAddress, line.Length, NetworkConstants.Ftp.MaxCommandLineLength);
                    break;
                }

                Logger.LogDebug("FTP command received from {RemoteAddress}: {Command}",
                    remoteAddress, line);

                // Execute command (session and line are guaranteed non-null, _commandHandler is initialized in StartAsync)
                continueSession = await _commandHandler!.ExecuteCommandAsync(session!, line!);
            }

            // Log logout if user was authenticated
            if (session.IsAuthenticated)
            {
                Logger.LogInformation("User logged out: {UserName} from {RemoteAddress}",
                    session.UserName, remoteAddress);
            }
        }
        catch (Exception ex)
        {
            NetworkExceptionHandler.LogNetworkException(ex, Logger, "FTP client handling");
        }
        finally
        {
            session.Dispose();
            socket.Close();
        }
    }
}
