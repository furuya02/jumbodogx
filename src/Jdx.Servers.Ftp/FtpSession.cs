using System;
using System.Net.Sockets;
using Jdx.Core.Settings;

namespace Jdx.Servers.Ftp;

/// <summary>
/// FTP session state for each client connection
/// Based on bjd5-master/FtpServer/Session.cs
/// </summary>
public class FtpSession
{
    // Control connection
    public NetworkStream? ControlStream { get; set; }
    public StreamReader? ControlReader { get; set; }
    public StreamWriter? ControlWriter { get; set; }

    // Data connection
    public NetworkStream? DataStream { get; set; }
    public Socket? DataSocket { get; set; }
    public TcpListener? PasvListener { get; set; }

    // Authentication state
    public string? UserName { get; set; }
    public FtpUserEntry? User { get; set; }
    public bool IsAuthenticated => User != null && CurrentDirectory != null;

    // Directory management
    public FtpCurrentDir? CurrentDirectory { get; set; }

    // Transfer state
    public FtpTransferType TransferType { get; set; } = FtpTransferType.ASCII;

    // Rename operation state (RNFR command)
    public string? RenameFromPath { get; set; }

    // PASV port range
    public int PasvPort { get; set; }

    // Remote address
    public string RemoteAddress { get; set; } = "";

    public FtpSession()
    {
        // Initialize PASV port to random value in range 2000-9900
        // Based on bjd5-master logic
        var rnd = new Random();
        PasvPort = (rnd.Next(79) + 20) * 100;
    }

    /// <summary>
    /// Send a response line to the client
    /// </summary>
    public async Task SendResponseAsync(string message)
    {
        if (ControlWriter != null)
        {
            await ControlWriter.WriteLineAsync(message);
            await ControlWriter.FlushAsync();
        }
    }

    /// <summary>
    /// Close data connection
    /// </summary>
    public void CloseDataConnection()
    {
        DataStream?.Close();
        DataSocket?.Close();
        PasvListener?.Stop();
        DataStream = null;
        DataSocket = null;
        PasvListener = null;
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        CloseDataConnection();
        ControlWriter?.Close();
        ControlReader?.Close();
        ControlStream?.Close();
    }
}
