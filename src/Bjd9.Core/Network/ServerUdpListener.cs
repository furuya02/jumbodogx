using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Bjd9.Core.Network;

/// <summary>
/// UDPリスナー（System.Net.Sockets.UdpClientとの名前衝突を回避）
/// </summary>
public class ServerUdpListener : IDisposable
{
    private readonly ILogger _logger;
    private readonly int _port;
    private readonly IPAddress _bindAddress;
    private Socket? _socket;
    private bool _disposed;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="port">リスニングポート</param>
    /// <param name="bindAddress">バインドアドレス（nullの場合はAny）</param>
    /// <param name="logger">ロガー</param>
    public ServerUdpListener(int port, IPAddress? bindAddress, ILogger logger)
    {
        if (port <= 0 || port > 65535)
            throw new ArgumentException("Port must be between 1 and 65535", nameof(port));

        _port = port;
        _bindAddress = bindAddress ?? IPAddress.Any;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>リスニング中かどうか</summary>
    public bool IsListening { get; private set; }

    /// <summary>
    /// リスニング開始
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (IsListening)
            throw new InvalidOperationException("Already listening");

        _socket = new Socket(
            _bindAddress.AddressFamily,
            SocketType.Dgram,
            ProtocolType.Udp);

        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _socket.Bind(new IPEndPoint(_bindAddress, _port));

        IsListening = true;
        _logger.LogDebug("UDP Listener started on {Address}:{Port}", _bindAddress, _port);

        await Task.CompletedTask;
    }

    /// <summary>
    /// UDPデータグラムを受信
    /// </summary>
    public async Task<(byte[] data, EndPoint remoteEndPoint)> ReceiveAsync(CancellationToken cancellationToken)
    {
        if (!IsListening || _socket == null)
            throw new InvalidOperationException("Not listening");

        var buffer = new byte[65535]; // UDPの最大サイズ
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        var result = await _socket.ReceiveFromAsync(buffer, SocketFlags.None, remoteEndPoint, cancellationToken);

        var data = new byte[result.ReceivedBytes];
        Array.Copy(buffer, data, result.ReceivedBytes);

        _logger.LogDebug("Received {Bytes} bytes from {RemoteEndPoint}", result.ReceivedBytes, result.RemoteEndPoint);

        return (data, result.RemoteEndPoint);
    }

    /// <summary>
    /// UDPデータグラムを送信
    /// </summary>
    public async Task SendAsync(byte[] data, EndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        if (!IsListening || _socket == null)
            throw new InvalidOperationException("Not listening");

        var bytesSent = await _socket.SendToAsync(data, SocketFlags.None, remoteEndPoint, cancellationToken);

        _logger.LogDebug("Sent {Bytes} bytes to {RemoteEndPoint}", bytesSent, remoteEndPoint);
    }

    /// <summary>
    /// リスニング停止
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!IsListening)
            return Task.CompletedTask;

        IsListening = false;
        _socket?.Close();
        _socket?.Dispose();
        _socket = null;

        _logger.LogDebug("UDP Listener stopped on port {Port}", _port);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        StopAsync(CancellationToken.None).GetAwaiter().GetResult();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
