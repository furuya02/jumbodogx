using System.Net;
using System.Net.Sockets;
using Jdx.Core.Abstractions;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Tftp;

/// <summary>
/// TFTP Server Implementation (RFC 1350)
/// Trivial File Transfer Protocol - Simple file transfer over UDP
/// </summary>
public class TftpServer : ServerBase
{
    // 定数定義
    private const int BlockSize = 512; // RFC 1350: TFTP標準ブロックサイズ
    private const int MaxFileSize = 100 * 1024 * 1024; // 最大ファイルサイズ: 100MB（DoS対策）

    private readonly TftpServerSettings _settings;
    private ServerUdpListener? _udpListener;
    private readonly SemaphoreSlim _connectionSemaphore;

    public TftpServer(ILogger<TftpServer> logger, TftpServerSettings settings)
        : base(logger)
    {
        _settings = settings;
        _connectionSemaphore = new SemaphoreSlim(settings.MaxConnections, settings.MaxConnections);
    }

    public override string Name => "TftpServer";
    public override ServerType Type => ServerType.Tftp;
    public override int Port => _settings.Port;

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        // Validate working directory
        if (string.IsNullOrWhiteSpace(_settings.WorkDir))
        {
            throw new InvalidOperationException("WorkDir is not configured");
        }

        if (!Directory.Exists(_settings.WorkDir))
        {
            Logger.LogWarning("WorkDir does not exist, creating: {WorkDir}", _settings.WorkDir);
            Directory.CreateDirectory(_settings.WorkDir);
        }

        // Create UDP listener
        IPAddress bindAddress;
        if (string.IsNullOrWhiteSpace(_settings.BindAddress) || _settings.BindAddress == "0.0.0.0")
        {
            bindAddress = IPAddress.Any;
        }
        else if (!IPAddress.TryParse(_settings.BindAddress, out bindAddress))
        {
            Logger.LogWarning("Invalid bind address '{Address}', using Any", _settings.BindAddress);
            bindAddress = IPAddress.Any;
        }

        _udpListener = new ServerUdpListener(_settings.Port, bindAddress, Logger);
        await _udpListener.StartAsync(cancellationToken);

        Logger.LogInformation(
            "TFTP Server started on {Address}:{Port} (WorkDir: {WorkDir}, Read: {Read}, Write: {Write})",
            _settings.BindAddress, _settings.Port, _settings.WorkDir, _settings.Read, _settings.Write);

        // Start listening loop
        _ = Task.Run(() => ListenLoopAsync(StopCts.Token), StopCts.Token);
    }

    protected override async Task StopListeningAsync(CancellationToken cancellationToken)
    {
        if (_udpListener != null)
        {
            await _udpListener.StopAsync(cancellationToken);
            _udpListener = null;
        }

        Logger.LogInformation("TFTP Server stopped");
    }

    protected override Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        // TFTP uses UDP, not TCP, so this method is not used
        // Client handling is done in ListenLoopAsync -> HandleRequestAsync
        return Task.CompletedTask;
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        if (_udpListener == null)
            return;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var (data, remoteEndPoint) = await _udpListener.ReceiveAsync(cancellationToken);

                if (data.Length == 0)
                    continue;

                // Handle request in background
                _ = Task.Run(async () =>
                {
                    await _connectionSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        await HandleRequestAsync(data, (IPEndPoint)remoteEndPoint, cancellationToken);
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
            catch (SocketException ex)
            {
                Logger.LogDebug(ex, "Socket error in TFTP listen loop");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Unexpected error in TFTP listen loop");
            }
        }
    }

    private async Task HandleRequestAsync(byte[] data, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        try
        {
            var opcode = TftpPacket.GetOpcode(data);

            Logger.LogDebug("TFTP request from {RemoteEndPoint}: Opcode={Opcode}", remoteEndPoint, opcode);

            switch (opcode)
            {
                case TftpOpcode.RRQ:
                    await HandleReadRequestAsync(data, remoteEndPoint, cancellationToken);
                    break;

                case TftpOpcode.WRQ:
                    await HandleWriteRequestAsync(data, remoteEndPoint, cancellationToken);
                    break;

                default:
                    Logger.LogWarning("Invalid TFTP opcode from {RemoteEndPoint}: {Opcode}", remoteEndPoint, opcode);
                    await SendErrorAsync(remoteEndPoint, TftpErrorCode.IllegalOperation, "Invalid opcode", cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling TFTP request from {RemoteEndPoint}", remoteEndPoint);
        }
    }

    private async Task HandleReadRequestAsync(byte[] data, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        try
        {
            if (!_settings.Read)
            {
                Logger.LogWarning("Read request denied (read disabled): {RemoteEndPoint}", remoteEndPoint);
                await SendErrorAsync(remoteEndPoint, TftpErrorCode.AccessViolation, "Read not allowed", cancellationToken);
                return;
            }

            var (filename, mode) = TftpPacket.ParseRequest(data);
            var filePath = GetSafePath(filename);

            if (filePath == null)
            {
                Logger.LogWarning("Invalid file path in read request: {Filename}", filename);
                await SendErrorAsync(remoteEndPoint, TftpErrorCode.AccessViolation, "Invalid file path", cancellationToken);
                return;
            }

            if (!File.Exists(filePath))
            {
                Logger.LogWarning("File not found: {FilePath}", filePath);
                await SendErrorAsync(remoteEndPoint, TftpErrorCode.FileNotFound, "File not found", cancellationToken);
                return;
            }

            Logger.LogInformation("RRQ: {Filename} ({Mode}) from {RemoteEndPoint}", filename, mode, remoteEndPoint);

            await SendFileAsync(filePath, remoteEndPoint, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling read request");
            await SendErrorAsync(remoteEndPoint, TftpErrorCode.NotDefined, "Internal error", cancellationToken);
        }
    }

    private async Task HandleWriteRequestAsync(byte[] data, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        try
        {
            if (!_settings.Write)
            {
                Logger.LogWarning("Write request denied (write disabled): {RemoteEndPoint}", remoteEndPoint);
                await SendErrorAsync(remoteEndPoint, TftpErrorCode.AccessViolation, "Write not allowed", cancellationToken);
                return;
            }

            var (filename, mode) = TftpPacket.ParseRequest(data);
            var filePath = GetSafePath(filename);

            if (filePath == null)
            {
                Logger.LogWarning("Invalid file path in write request: {Filename}", filename);
                await SendErrorAsync(remoteEndPoint, TftpErrorCode.AccessViolation, "Invalid file path", cancellationToken);
                return;
            }

            if (File.Exists(filePath) && !_settings.Override)
            {
                Logger.LogWarning("File already exists and override disabled: {FilePath}", filePath);
                await SendErrorAsync(remoteEndPoint, TftpErrorCode.FileAlreadyExists, "File exists", cancellationToken);
                return;
            }

            Logger.LogInformation("WRQ: {Filename} ({Mode}) from {RemoteEndPoint}", filename, mode, remoteEndPoint);

            await ReceiveFileAsync(filePath, remoteEndPoint, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling write request");
            await SendErrorAsync(remoteEndPoint, TftpErrorCode.NotDefined, "Internal error", cancellationToken);
        }
    }

    private async Task SendFileAsync(string filePath, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        using var udpClient = new UdpClient();
        udpClient.Connect(remoteEndPoint);

        try
        {
            await using var fileStream = File.OpenRead(filePath);
            ushort blockNumber = 1;
            var buffer = new byte[BlockSize];

            while (true)
            {
                // Read block from file
                var bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, BlockSize), cancellationToken);
                var blockData = buffer[0..bytesRead];

                // Send DATA packet
                var dataPacket = TftpPacket.BuildData(blockNumber, blockData);
                await udpClient.SendAsync(dataPacket, cancellationToken);

                // Wait for ACK with timeout
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeOut));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                try
                {
                    var ackResult = await udpClient.ReceiveAsync(linkedCts.Token);
                    var ackBlockNumber = TftpPacket.GetBlockNumber(ackResult.Buffer);

                    if (ackBlockNumber != blockNumber)
                    {
                        Logger.LogWarning("Block number mismatch: expected {Expected}, got {Actual}", blockNumber, ackBlockNumber);
                        return;
                    }
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    Logger.LogWarning("Timeout waiting for ACK for block {BlockNumber}", blockNumber);
                    return;
                }

                // Last block
                if (bytesRead < BlockSize)
                {
                    Logger.LogInformation("File sent successfully: {FilePath} ({Blocks} blocks)", filePath, blockNumber);
                    break;
                }

                blockNumber++;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending file: {FilePath}", filePath);
        }
    }

    private async Task ReceiveFileAsync(string filePath, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        using var udpClient = new UdpClient();
        udpClient.Connect(remoteEndPoint);

        try
        {
            // Send initial ACK(0)
            var ackPacket = TftpPacket.BuildAck(0);
            await udpClient.SendAsync(ackPacket, cancellationToken);

            await using var fileStream = File.Create(filePath);
            ushort expectedBlock = 1;
            long totalBytesReceived = 0;

            while (true)
            {
                // Wait for DATA with timeout
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.TimeOut));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                UdpReceiveResult dataResult;
                try
                {
                    dataResult = await udpClient.ReceiveAsync(linkedCts.Token);
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    Logger.LogWarning("Timeout waiting for DATA block {BlockNumber}", expectedBlock);
                    return;
                }

                var blockNumber = TftpPacket.GetBlockNumber(dataResult.Buffer);
                if (blockNumber != expectedBlock)
                {
                    Logger.LogWarning("Block number mismatch: expected {Expected}, got {Actual}", expectedBlock, blockNumber);
                    continue;
                }

                // Extract and write data
                var blockData = TftpPacket.ExtractData(dataResult.Buffer);

                // ファイルサイズ制限チェック（DoS対策）
                totalBytesReceived += blockData.Length;
                if (totalBytesReceived > MaxFileSize)
                {
                    Logger.LogWarning("File size exceeds limit: {Size} bytes (max {MaxSize})", totalBytesReceived, MaxFileSize);
                    await SendErrorAsync(remoteEndPoint, TftpErrorCode.DiskFull, "File too large", cancellationToken);
                    throw new InvalidOperationException("File size exceeds limit");
                }

                await fileStream.WriteAsync(blockData, cancellationToken);

                // Send ACK
                ackPacket = TftpPacket.BuildAck(blockNumber);
                await udpClient.SendAsync(ackPacket, cancellationToken);

                // Last block
                if (blockData.Length < BlockSize)
                {
                    Logger.LogInformation("File received successfully: {FilePath} ({Blocks} blocks)", filePath, expectedBlock);
                    break;
                }

                expectedBlock++;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error receiving file: {FilePath}", filePath);

            // Clean up partial file on error
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { }
        }
    }

    private async Task SendErrorAsync(IPEndPoint remoteEndPoint, TftpErrorCode errorCode, string message, CancellationToken cancellationToken)
    {
        try
        {
            using var udpClient = new UdpClient();
            udpClient.Connect(remoteEndPoint);

            var errorPacket = TftpPacket.BuildError(errorCode, message);
            await udpClient.SendAsync(errorPacket, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending TFTP error packet");
        }
    }

    private string? GetSafePath(string filename)
    {
        try
        {
            // ファイル名検証の強化（DoS対策）
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            // ファイル名長制限チェック
            if (filename.Length > 255)
                return null;

            // パストラバーサル対策: Path.GetFileName()でファイル名部分のみを抽出
            // パス区切り文字を含む場合、GetFileName()は最後のセグメントのみを返す
            // これにより、すべての形式のパストラバーサル攻撃を防ぐ
            var safeFilename = Path.GetFileName(filename);

            // 元のfilenameと異なる場合、パス成分が含まれていたため拒否
            if (string.IsNullOrWhiteSpace(safeFilename) || safeFilename != filename)
                return null;

            // URL-encodedパス区切り文字のチェック（%2F, %5C）
            if (filename.Contains("%2F", StringComparison.OrdinalIgnoreCase) ||
                filename.Contains("%5C", StringComparison.OrdinalIgnoreCase))
                return null;

            // システム固有のパス区切り文字のチェック
            if (filename.Contains(Path.DirectorySeparatorChar) ||
                filename.Contains(Path.AltDirectorySeparatorChar))
                return null;

            // 不正な文字のチェック（制御文字等）
            if (filename.Any(c => char.IsControl(c) || c == '<' || c == '>' || c == '|' || c == '*' || c == '?'))
                return null;

            var fullPath = Path.Combine(_settings.WorkDir, safeFilename);
            var normalizedPath = Path.GetFullPath(fullPath);

            // 正規化後のパスがWorkDir内にあることを確認（二重チェック）
            if (!normalizedPath.StartsWith(Path.GetFullPath(_settings.WorkDir), StringComparison.OrdinalIgnoreCase))
                return null;

            return normalizedPath;
        }
        catch
        {
            return null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _udpListener?.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
            _connectionSemaphore?.Dispose();
        }
        base.Dispose(disposing);
    }
}
