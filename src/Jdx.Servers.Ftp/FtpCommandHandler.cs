using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Jdx.Core.Constants;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Ftp;

/// <summary>
/// FTP command execution logic
/// Based on bjd5-master/FtpServer/Server.cs command handling
/// </summary>
public class FtpCommandHandler
{
    private readonly FtpServerSettings _settings;
    private readonly FtpUserManager _userManager;
    private readonly FtpMountManager _mountManager;
    private readonly ILogger _logger;

    public FtpCommandHandler(
        FtpServerSettings settings,
        FtpUserManager userManager,
        FtpMountManager mountManager,
        ILogger logger)
    {
        _settings = settings;
        _userManager = userManager;
        _mountManager = mountManager;
        _logger = logger;
    }

    /// <summary>
    /// Parse and execute FTP command
    /// </summary>
    public async Task<bool> ExecuteCommandAsync(FtpSession session, string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            await session.SendResponseAsync(FtpResponseCodes.InvalidCommand);
            return true;
        }

        // Parse command and parameters
        var parts = line.Trim().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        var cmdStr = parts[0].ToUpper();
        var param = parts.Length > 1 ? parts[1] : "";

        // Parse command enum
        if (!Enum.TryParse<FtpCommand>(cmdStr, true, out var command))
        {
            command = FtpCommand.UNKNOWN;
        }

        // Check SYST command permission
        if (command == FtpCommand.SYST && !_settings.UseSyst)
        {
            command = FtpCommand.UNKNOWN;
        }

        // Handle unknown commands
        if (command == FtpCommand.UNKNOWN)
        {
            await session.SendResponseAsync(FtpResponseCodes.CommandNotUnderstood);
            return true;
        }

        // Handle QUIT/ABOR
        if (command == FtpCommand.QUIT)
        {
            await session.SendResponseAsync(FtpResponseCodes.ServiceClosing);
            return false;
        }

        if (command == FtpCommand.ABOR)
        {
            await session.SendResponseAsync(FtpResponseCodes.ActionCompleted);
            return false;
        }

        // Convert CDUP to CWD ..
        if (command == FtpCommand.CDUP)
        {
            param = "..";
            command = FtpCommand.CWD;
        }

        // Security: reject excessively long parameters
        if (param.Length > 128)
        {
            _logger.LogWarning("Excessive parameter length from {RemoteAddress}: {Length}",
                session.RemoteAddress, param.Length);
            return false;
        }

        // Pre-authentication commands
        if (!session.IsAuthenticated)
        {
            return await HandlePreAuthCommandAsync(session, command, param);
        }

        // Post-authentication commands
        return await HandleAuthenticatedCommandAsync(session, command, param);
    }

    /// <summary>
    /// Handle commands before authentication
    /// </summary>
    private async Task<bool> HandlePreAuthCommandAsync(FtpSession session, FtpCommand command, string param)
    {
        switch (command)
        {
            case FtpCommand.USER:
                if (string.IsNullOrEmpty(param))
                {
                    await session.SendResponseAsync($"500 {command}: command requires a parameter.");
                    return true;
                }
                return await HandleUserCommandAsync(session, param);

            case FtpCommand.PASS:
                return await HandlePassCommandAsync(session, param);

            default:
                await session.SendResponseAsync(FtpResponseCodes.NotLoggedIn);
                return true;
        }
    }

    /// <summary>
    /// Handle commands after authentication
    /// </summary>
    private async Task<bool> HandleAuthenticatedCommandAsync(FtpSession session, FtpCommand command, string param)
    {
        // Check if parameter is required
        var requiresParam = new[] { FtpCommand.CWD, FtpCommand.TYPE, FtpCommand.MKD, FtpCommand.RMD,
                                     FtpCommand.DELE, FtpCommand.PORT, FtpCommand.RNFR, FtpCommand.RNTO,
                                     FtpCommand.STOR, FtpCommand.RETR };

        if (requiresParam.Contains(command) && string.IsNullOrEmpty(param))
        {
            await session.SendResponseAsync($"500 {command}: command requires a parameter.");
            return true;
        }

        // Check data connection requirement
        var requiresData = new[] { FtpCommand.NLST, FtpCommand.LIST, FtpCommand.STOR, FtpCommand.RETR };
        if (requiresData.Contains(command) && session.DataStream == null)
        {
            await session.SendResponseAsync(FtpResponseCodes.DataConnectionFailed);
            return true;
        }

        // Check user permissions
        if (session.User != null)
        {
            var writeCommands = new[] { FtpCommand.STOR, FtpCommand.DELE, FtpCommand.RNFR,
                                         FtpCommand.RNTO, FtpCommand.RMD, FtpCommand.MKD };
            var readCommands = new[] { FtpCommand.RETR };

            if (session.User.AccessControl == FtpAccessControl.Down && writeCommands.Contains(command))
            {
                await session.SendResponseAsync(FtpResponseCodes.AccessDenied);
                return true;
            }

            if (session.User.AccessControl == FtpAccessControl.Up && readCommands.Contains(command))
            {
                await session.SendResponseAsync(FtpResponseCodes.AccessDenied);
                return true;
            }
        }

        // Reject USER/PASS after login
        if (command == FtpCommand.USER || command == FtpCommand.PASS)
        {
            await session.SendResponseAsync(FtpResponseCodes.NotLoggedIn);
            return true;
        }

        // Execute command
        return command switch
        {
            FtpCommand.NOOP => await HandleNoopAsync(session),
            FtpCommand.PWD or FtpCommand.XPWD => await HandlePwdAsync(session),
            FtpCommand.CWD => await HandleCwdAsync(session, param),
            FtpCommand.SYST => await HandleSystAsync(session),
            FtpCommand.TYPE => await HandleTypeAsync(session, param),
            FtpCommand.MKD => await HandleMkdAsync(session, param),
            FtpCommand.RMD => await HandleRmdAsync(session, param),
            FtpCommand.DELE => await HandleDeleAsync(session, param),
            FtpCommand.LIST or FtpCommand.NLST => await HandleListAsync(session, param, command),
            FtpCommand.PORT or FtpCommand.EPRT => await HandlePortAsync(session, param, command),
            FtpCommand.PASV or FtpCommand.EPSV => await HandlePasvAsync(session, command),
            FtpCommand.RNFR => await HandleRnfrAsync(session, param),
            FtpCommand.RNTO => await HandleRntoAsync(session, param),
            FtpCommand.STOR => await HandleStorAsync(session, param),
            FtpCommand.RETR => await HandleRetrAsync(session, param),
            _ => true
        };
    }

    private async Task<bool> HandleUserCommandAsync(FtpSession session, string userName)
    {
        session.UserName = userName;
        await session.SendResponseAsync(FtpResponseCodes.PasswordRequired);
        return true;
    }

    private async Task<bool> HandlePassCommandAsync(FtpSession session, string password)
    {
        if (string.IsNullOrEmpty(session.UserName))
        {
            await session.SendResponseAsync(FtpResponseCodes.BadSequence);
            return true;
        }

        if (_userManager.Authenticate(session.UserName, password))
        {
            var user = _userManager.GetUser(session.UserName);
            if (user != null)
            {
                session.User = user;
                session.CurrentDirectory = new FtpCurrentDir(user.HomeDirectory, _mountManager);
                await session.SendResponseAsync(FtpResponseCodes.UserLoggedIn);
                _logger.LogInformation("User logged in: {UserName} from {RemoteAddress}",
                    session.UserName, session.RemoteAddress);
                return true;
            }
        }

        await session.SendResponseAsync(FtpResponseCodes.NotLoggedIn);
        return true;
    }

    private async Task<bool> HandleNoopAsync(FtpSession session)
    {
        await session.SendResponseAsync(FtpResponseCodes.CommandOk);
        return true;
    }

    private async Task<bool> HandlePwdAsync(FtpSession session)
    {
        var pwd = session.CurrentDirectory?.GetPwd() ?? "/";
        await session.SendResponseAsync(FtpResponseCodes.CurrentDirectory(pwd));
        return true;
    }

    private async Task<bool> HandleCwdAsync(FtpSession session, string path)
    {
        if (session.CurrentDirectory?.ChangeDirectory(path) == true)
        {
            await session.SendResponseAsync(FtpResponseCodes.ActionCompleted);
            return true;
        }

        await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
        return true;
    }

    private async Task<bool> HandleSystAsync(FtpSession session)
    {
        var os = Environment.OSVersion;
        await session.SendResponseAsync($"215 {os.Platform}");
        return true;
    }

    private async Task<bool> HandleTypeAsync(FtpSession session, string param)
    {
        var type = param.ToUpper();
        if (type == "A" || type == "ASCII")
        {
            session.TransferType = FtpTransferType.ASCII;
            await session.SendResponseAsync(FtpResponseCodes.CommandOk);
        }
        else if (type == "I" || type == "BINARY")
        {
            session.TransferType = FtpTransferType.BINARY;
            await session.SendResponseAsync(FtpResponseCodes.CommandOk);
        }
        else
        {
            await session.SendResponseAsync(FtpResponseCodes.InvalidCommand);
        }
        return true;
    }

    private async Task<bool> HandleMkdAsync(FtpSession session, string path)
    {
        var fullPath = session.CurrentDirectory?.CreatePath(path, true);
        if (string.IsNullOrEmpty(fullPath))
        {
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
            return true;
        }

        try
        {
            Directory.CreateDirectory(fullPath);
            await session.SendResponseAsync(FtpResponseCodes.PathCreated(path));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create directory: {Path}", fullPath);
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
        }

        return true;
    }

    private async Task<bool> HandleRmdAsync(FtpSession session, string path)
    {
        var fullPath = session.CurrentDirectory?.CreatePath(path, true);
        if (string.IsNullOrEmpty(fullPath))
        {
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
            return true;
        }

        try
        {
            Directory.Delete(fullPath);
            await session.SendResponseAsync(FtpResponseCodes.ActionCompleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove directory: {Path}", fullPath);
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
        }

        return true;
    }

    private async Task<bool> HandleDeleAsync(FtpSession session, string path)
    {
        var fullPath = session.CurrentDirectory?.CreatePath(path, false);
        if (string.IsNullOrEmpty(fullPath))
        {
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
            return true;
        }

        try
        {
            File.Delete(fullPath);
            await session.SendResponseAsync(FtpResponseCodes.ActionCompleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {Path}", fullPath);
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
        }

        return true;
    }

    private async Task<bool> HandleListAsync(FtpSession session, string path, FtpCommand command)
    {
        // Wait for PASV connection if pending
        if (session.PasvConnectionReady != null)
        {
            try
            {
                // Wait up to 30 seconds for client to connect
                await session.PasvConnectionReady.Task.WaitAsync(TimeSpan.FromSeconds(30));
            }
            catch (TimeoutException)
            {
                await session.SendResponseAsync(FtpResponseCodes.DataConnectionFailed);
                session.CloseDataConnection();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish data connection");
                await session.SendResponseAsync(FtpResponseCodes.DataConnectionFailed);
                session.CloseDataConnection();
                return true;
            }
            finally
            {
                session.PasvConnectionReady = null;
            }
        }

        if (session.DataStream == null)
        {
            await session.SendResponseAsync(FtpResponseCodes.DataConnectionFailed);
            return true;
        }

        await session.SendResponseAsync(FtpResponseCodes.FileStatusOk);

        try
        {
            var files = session.CurrentDirectory?.ListDirectory(path) ?? Array.Empty<string>();
            var writer = new StreamWriter(session.DataStream, Encoding.UTF8);

            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var isDir = (info.Attributes & FileAttributes.Directory) != 0;

                if (command == FtpCommand.NLST)
                {
                    // Simple name list
                    await writer.WriteLineAsync(Path.GetFileName(file));
                }
                else
                {
                    // Detailed list (Unix-style)
                    var perms = isDir ? "drwxr-xr-x" : "-rw-r--r--";
                    var size = isDir ? 0 : info.Length;
                    var date = info.LastWriteTime.ToString("MMM dd HH:mm");
                    var name = Path.GetFileName(file);
                    await writer.WriteLineAsync($"{perms} 1 owner group {size,10} {date} {name}");
                }
            }

            await writer.FlushAsync();
            session.CloseDataConnection();
            await session.SendResponseAsync(FtpResponseCodes.TransferComplete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list directory");
            session.CloseDataConnection();
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
        }

        return true;
    }

    private async Task<bool> HandlePortAsync(FtpSession session, string param, FtpCommand command)
    {
        try
        {
            // Parse PORT command: h1,h2,h3,h4,p1,p2
            var parts = param.Split(',');
            if (parts.Length != 6)
            {
                await session.SendResponseAsync(FtpResponseCodes.InvalidCommand);
                return true;
            }

            // PORT/PASVコマンド入力検証（DoS対策）
            // 各パートが0-255の範囲内であることを確認
            var bytes = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                if (!byte.TryParse(parts[i], out bytes[i]))
                {
                    _logger.LogWarning("Invalid PORT parameter: {Param}", param);
                    await session.SendResponseAsync(FtpResponseCodes.InvalidCommand);
                    return true;
                }
            }

            var ip = $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}";
            var port = bytes[4] * 256 + bytes[5];

            // ポート番号の範囲チェック（1024-65535、ウェルノウンポート除外）
            if (port < NetworkConstants.Ftp.MinPort || port > NetworkConstants.Ftp.MaxPort)
            {
                _logger.LogWarning("Invalid PORT port number: {Port} (allowed: {Min}-{Max})",
                    port, NetworkConstants.Ftp.MinPort, NetworkConstants.Ftp.MaxPort);
                await session.SendResponseAsync(FtpResponseCodes.InvalidCommand);
                return true;
            }

            // IPアドレスの妥当性チェック
            if (!IPAddress.TryParse(ip, out var ipAddress))
            {
                _logger.LogWarning("Invalid PORT IP address: {IP}", ip);
                await session.SendResponseAsync(FtpResponseCodes.InvalidCommand);
                return true;
            }

            // FTPバウンス攻撃防止: クライアントの送信元IPアドレスと一致するか確認
            var clientIp = session.RemoteAddress?.Split(':')[0];
            if (clientIp != null && clientIp != ip)
            {
                _logger.LogWarning("FTP bounce attack attempt: client {ClientIP} tried to connect to {TargetIP}",
                    clientIp, ip);
                await session.SendResponseAsync(FtpResponseCodes.InvalidCommand);
                return true;
            }

            // Close existing data connection
            session.CloseDataConnection();

            // Connect to client with proper resource management
            TcpClient? client = null;
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ipAddress, port);
                session.DataClient = client;
                session.DataSocket = client.Client;
                session.DataStream = client.GetStream();

                await session.SendResponseAsync(FtpResponseCodes.CommandOk);
            }
            catch
            {
                // Clean up TcpClient if connection fails
                client?.Dispose();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to handle PORT command");
            await session.SendResponseAsync(FtpResponseCodes.InvalidCommand);
        }

        return true;
    }

    private async Task<bool> HandlePasvAsync(FtpSession session, FtpCommand command)
    {
        try
        {
            // Close existing data connection
            session.CloseDataConnection();

            // Create listener on ephemeral port
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            session.PasvListener = listener;

            var endpoint = (IPEndPoint)listener.LocalEndpoint;
            var port = endpoint.Port;

            // Get server IP from bind address setting or control connection
            var localIp = GetServerIpForPasv();

            // Create TaskCompletionSource for connection synchronization
            session.PasvConnectionReady = new TaskCompletionSource<bool>();

            await session.SendResponseAsync(FtpResponseCodes.EnteringPassiveMode(localIp.Replace(',', '.'), port));

            // Accept connection in background with synchronization
            _ = Task.Run(async () =>
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync();
                    session.DataClient = client;
                    session.DataSocket = client.Client;
                    session.DataStream = client.GetStream();
                    session.PasvConnectionReady.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to accept PASV connection");
                    session.PasvConnectionReady.TrySetException(ex);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle PASV command");
            await session.SendResponseAsync(FtpResponseCodes.InvalidCommand);
        }

        return true;
    }

    /// <summary>
    /// Get server IP for PASV response
    /// </summary>
    private string GetServerIpForPasv()
    {
        // Try to get from settings BindAddress
        if (!string.IsNullOrEmpty(_settings.BindAddress) && _settings.BindAddress != "0.0.0.0")
        {
            return _settings.BindAddress.Replace('.', ',');
        }

        // Fallback to localhost (for development)
        // In production, this should be the server's public IP
        return "127,0,0,1";
    }

    /// <summary>
    /// Wait for PASV data connection to be established
    /// Returns true if connection is ready, false if failed/timeout
    /// </summary>
    private async Task<bool> WaitForDataConnectionAsync(FtpSession session)
    {
        // Wait for PASV connection if pending
        if (session.PasvConnectionReady != null)
        {
            try
            {
                // Wait up to 30 seconds for client to connect
                await session.PasvConnectionReady.Task.WaitAsync(TimeSpan.FromSeconds(30));
            }
            catch (TimeoutException)
            {
                await session.SendResponseAsync(FtpResponseCodes.DataConnectionFailed);
                session.CloseDataConnection();
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish data connection");
                await session.SendResponseAsync(FtpResponseCodes.DataConnectionFailed);
                session.CloseDataConnection();
                return false;
            }
            finally
            {
                session.PasvConnectionReady = null;
            }
        }

        if (session.DataStream == null)
        {
            await session.SendResponseAsync(FtpResponseCodes.DataConnectionFailed);
            return false;
        }

        return true;
    }

    private async Task<bool> HandleRnfrAsync(FtpSession session, string path)
    {
        var fullPath = session.CurrentDirectory?.CreatePath(path, false);
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
        {
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
            return true;
        }

        session.RenameFromPath = fullPath;
        await session.SendResponseAsync(FtpResponseCodes.ActionPending);
        return true;
    }

    private async Task<bool> HandleRntoAsync(FtpSession session, string path)
    {
        if (string.IsNullOrEmpty(session.RenameFromPath))
        {
            await session.SendResponseAsync(FtpResponseCodes.BadSequence);
            return true;
        }

        var fullPath = session.CurrentDirectory?.CreatePath(path, false);
        if (string.IsNullOrEmpty(fullPath))
        {
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
            session.RenameFromPath = null;
            return true;
        }

        try
        {
            File.Move(session.RenameFromPath, fullPath);
            await session.SendResponseAsync(FtpResponseCodes.ActionCompleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rename file");
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
        }

        session.RenameFromPath = null;
        return true;
    }

    private async Task<bool> HandleStorAsync(FtpSession session, string path)
    {
        // Wait for PASV data connection if needed
        if (!await WaitForDataConnectionAsync(session))
        {
            return true;
        }

        var fullPath = session.CurrentDirectory?.CreatePath(path, false);
        if (string.IsNullOrEmpty(fullPath))
        {
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
            return true;
        }

        await session.SendResponseAsync(FtpResponseCodes.FileStatusOk);

        try
        {
            using var fileStream = File.Create(fullPath);
            await session.DataStream.CopyToAsync(fileStream);
            session.CloseDataConnection();
            await session.SendResponseAsync(FtpResponseCodes.TransferComplete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store file: {Path}", fullPath);
            session.CloseDataConnection();

            // Delete partial file on error
            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogDebug("Deleted partial file after failed transfer: {Path}", fullPath);
                }
            }
            catch (Exception deleteEx)
            {
                _logger.LogWarning(deleteEx, "Failed to delete partial file: {Path}", fullPath);
            }

            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
        }

        return true;
    }

    private async Task<bool> HandleRetrAsync(FtpSession session, string path)
    {
        // Wait for PASV data connection if needed
        if (!await WaitForDataConnectionAsync(session))
        {
            return true;
        }

        var fullPath = session.CurrentDirectory?.CreatePath(path, false);
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
        {
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
            return true;
        }

        await session.SendResponseAsync(FtpResponseCodes.FileStatusOk);

        try
        {
            using var fileStream = File.OpenRead(fullPath);
            await fileStream.CopyToAsync(session.DataStream);
            session.CloseDataConnection();
            await session.SendResponseAsync(FtpResponseCodes.TransferComplete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve file: {Path}", fullPath);
            session.CloseDataConnection();
            await session.SendResponseAsync(FtpResponseCodes.FileActionFailed);
        }

        return true;
    }
}
