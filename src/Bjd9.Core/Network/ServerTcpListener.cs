using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Bjd9.Core.Network;

/// <summary>
/// TCPリスナー（System.Net.Sockets.TcpListenerとの名前衝突を回避）
/// </summary>
public class ServerTcpListener : IDisposable
{
    private readonly ILogger _logger;
    private readonly int _port;
    private readonly IPAddress _bindAddress;
    private Socket? _listenerSocket;
    private bool _disposed;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="port">リスニングポート</param>
    /// <param name="bindAddress">バインドアドレス（nullの場合はAny）</param>
    /// <param name="logger">ロガー</param>
    public ServerTcpListener(int port, IPAddress? bindAddress, ILogger logger)
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

        _listenerSocket = new Socket(
            _bindAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        _listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _listenerSocket.Bind(new IPEndPoint(_bindAddress, _port));
        _listenerSocket.Listen(100); // バックログ100

        IsListening = true;
        _logger.LogDebug("TCP Listener started on {Address}:{Port}", _bindAddress, _port);

        await Task.CompletedTask;
    }

    /// <summary>
    /// クライアント接続を受け入れる
    /// </summary>
    public async Task<Socket> AcceptAsync(CancellationToken cancellationToken)
    {
        if (!IsListening || _listenerSocket == null)
            throw new InvalidOperationException("Not listening");

        var clientSocket = await _listenerSocket.AcceptAsync(cancellationToken);
        _logger.LogDebug("Accepted TCP connection from {RemoteEndPoint}", clientSocket.RemoteEndPoint);

        return clientSocket;
    }

    /// <summary>
    /// リスニング停止
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!IsListening)
            return Task.CompletedTask;

        IsListening = false;
        _listenerSocket?.Close();
        _listenerSocket?.Dispose();
        _listenerSocket = null;

        _logger.LogDebug("TCP Listener stopped on port {Port}", _port);

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
