using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Proxy;

/// <summary>
/// Proxy接続管理
/// bjd5-master/ProxyHttpServer/Proxy.cs に対応
/// </summary>
public class ProxyConnection : IDisposable
{
    // 定数定義
    private const int SocketPollTimeoutMicroseconds = 1000; // ソケットPollのタイムアウト（マイクロ秒）

    private readonly ILogger _logger;
    private readonly Dictionary<ProxyConnectionSide, TcpClient?> _sockets;
    private readonly Dictionary<ProxyConnectionSide, NetworkStream?> _streams;

    public int OptionTimeout { get; private set; }
    public ProxyUpperProxy UpperProxy { get; private set; }
    public string HostName { get; private set; } = "";
    public int Port { get; private set; } = 0;
    public ProxyProtocol Protocol { get; private set; }

    public ProxyConnection(
        TcpClient clientSocket,
        int optionTimeout,
        ProxyUpperProxy upperProxy,
        ILogger logger)
    {
        _logger = logger;
        OptionTimeout = optionTimeout;
        UpperProxy = upperProxy;
        Protocol = ProxyProtocol.Unknown;

        _sockets = new Dictionary<ProxyConnectionSide, TcpClient?>
        {
            { ProxyConnectionSide.Client, clientSocket },
            { ProxyConnectionSide.Server, null }
        };

        _streams = new Dictionary<ProxyConnectionSide, NetworkStream?>
        {
            { ProxyConnectionSide.Client, clientSocket.GetStream() },
            { ProxyConnectionSide.Server, null }
        };
    }

    /// <summary>
    /// 指定された側のソケットを取得
    /// </summary>
    public TcpClient? GetSocket(ProxyConnectionSide side)
    {
        return _sockets[side];
    }

    /// <summary>
    /// 指定された側のストリームを取得
    /// </summary>
    public NetworkStream? GetStream(ProxyConnectionSide side)
    {
        return _streams[side];
    }

    /// <summary>
    /// 指定された側のソケットに到着しているデータ量
    /// </summary>
    public int GetAvailableData(ProxyConnectionSide side)
    {
        var socket = _sockets[side];
        return socket?.Available ?? 0;
    }

    /// <summary>
    /// キャッシュヒット時のダミーサーバーソケット作成
    /// </summary>
    public void CreateDummyServerSocket(string host, int port)
    {
        // ダミーのTcpClientを作成（実際には接続しない）
        _sockets[ProxyConnectionSide.Server] = new TcpClient();
        HostName = host;
        Port = port;
    }

    /// <summary>
    /// サーバーへ接続
    /// </summary>
    public async Task<bool> ConnectAsync(
        string host,
        int port,
        string requestStr,
        ProxyProtocol protocol,
        CancellationToken cancellationToken = default)
    {
        Protocol = protocol;

        // 既に同じホストに接続している場合は再利用（ヘルスチェック付き）
        if (_sockets[ProxyConnectionSide.Server] != null)
        {
            if (host == HostName && port == Port)
            {
                // 接続がまだ有効かチェック
                var socket = _sockets[ProxyConnectionSide.Server];
                if (socket != null && socket.Connected && IsSocketConnected(socket))
                {
                    _logger.LogDebug("Reusing existing connection to {Host}:{Port}", host, port);
                    return true;
                }
                else
                {
                    _logger.LogDebug("Existing connection to {Host}:{Port} is no longer valid, reconnecting", host, port);
                    CloseServerConnection();
                }
            }
            else
            {
                // 異なるホストの場合は既存の接続を閉じる
                CloseServerConnection();
            }
        }

        // 上位プロキシのチェック
        bool useUpperProxy = UpperProxy.Use;
        if (useUpperProxy)
        {
            // 上位プロキシを経由しないサーバの確認
            foreach (var address in UpperProxy.DisableAddressList)
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    continue;
                }

                if (protocol == ProxyProtocol.Ssl)
                {
                    if (host.StartsWith(address, StringComparison.OrdinalIgnoreCase))
                    {
                        useUpperProxy = false;
                        break;
                    }
                }
                else
                {
                    if (requestStr.Length > 11)
                    {
                        var urlPart = requestStr.Substring(11);
                        if (urlPart.StartsWith(address, StringComparison.OrdinalIgnoreCase))
                        {
                            useUpperProxy = false;
                            break;
                        }
                    }
                }
            }
        }

        // 接続先を決定
        string targetHost = useUpperProxy ? UpperProxy.Server : host;
        int targetPort = useUpperProxy ? UpperProxy.Port : port;

        // IPアドレスの解決
        IPAddress[] addresses;
        try
        {
            // IPアドレスとして直接パース可能かチェック
            if (IPAddress.TryParse(targetHost, out var ipAddress))
            {
                addresses = new[] { ipAddress };
            }
            else
            {
                // DNSルックアップ
                addresses = await Dns.GetHostAddressesAsync(targetHost, cancellationToken);
                if (addresses == null || addresses.Length == 0)
                {
                    _logger.LogError("Failed to resolve host: {Host}", targetHost);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve host: {Host}", targetHost);
            return false;
        }

        // 接続を試行
        TcpClient? serverSocket = null;
        foreach (var address in addresses)
        {
            try
            {
                serverSocket = new TcpClient();
                var connectTask = serverSocket.ConnectAsync(address, targetPort, cancellationToken).AsTask();
                // OptionTimeoutを使用（0の場合はデフォルト3秒）
                var timeoutSeconds = OptionTimeout > 0 ? OptionTimeout : 3;
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds), cancellationToken);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                if (completedTask == connectTask && serverSocket.Connected)
                {
                    break;
                }
                else
                {
                    serverSocket.Close();
                    serverSocket = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Connection attempt failed to {Address}:{Port}", address, targetPort);
                serverSocket?.Close();
                serverSocket = null;
            }
        }

        if (serverSocket == null || !serverSocket.Connected)
        {
            _logger.LogWarning("Failed to connect to {Host}:{Port}", targetHost, targetPort);
            return false;
        }

        _sockets[ProxyConnectionSide.Server] = serverSocket;
        _streams[ProxyConnectionSide.Server] = serverSocket.GetStream();
        HostName = host;
        Port = port;

        _logger.LogDebug("Connected to {Host}:{Port}", targetHost, targetPort);
        return true;
    }

    private void CloseServerConnection()
    {
        if (_streams[ProxyConnectionSide.Server] != null)
        {
            _streams[ProxyConnectionSide.Server]?.Close();
            _streams[ProxyConnectionSide.Server] = null;
        }

        if (_sockets[ProxyConnectionSide.Server] != null)
        {
            _sockets[ProxyConnectionSide.Server]?.Close();
            _sockets[ProxyConnectionSide.Server] = null;
        }
    }

    /// <summary>
    /// ソケットが実際に接続されているかチェック
    /// </summary>
    private bool IsSocketConnected(TcpClient client)
    {
        try
        {
            // TcpClient.Connectedは信頼できないので、Pollで確認
            var socket = client.Client;
            bool pollRead = socket.Poll(SocketPollTimeoutMicroseconds, System.Net.Sockets.SelectMode.SelectRead);
            bool dataAvailable = (socket.Available > 0);

            // データが読み取り可能だが利用可能なデータがない場合は切断されている
            if (pollRead && !dataAvailable)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        // クライアント接続を閉じる
        if (_streams[ProxyConnectionSide.Client] != null)
        {
            _streams[ProxyConnectionSide.Client]?.Close();
            _streams[ProxyConnectionSide.Client] = null;
        }

        if (_sockets[ProxyConnectionSide.Client] != null)
        {
            _sockets[ProxyConnectionSide.Client]?.Close();
            _sockets[ProxyConnectionSide.Client] = null;
        }

        // サーバー接続を閉じる
        CloseServerConnection();
    }
}
