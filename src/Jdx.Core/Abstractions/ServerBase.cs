using System.Net;
using System.Net.Sockets;
using Jdx.Core.Helpers;
using Jdx.Core.Metrics;
using Jdx.Core.Network;
using Microsoft.Extensions.Logging;

namespace Jdx.Core.Abstractions;

/// <summary>
/// サーバーの基底抽象クラス
/// 共通機能を提供し、派生クラスは個別の実装のみを行う
/// </summary>
public abstract class ServerBase : IServer
{
    protected readonly ILogger Logger;
    protected CancellationTokenSource StopCts;
    protected readonly ServerStatistics Statistics;
    protected readonly ServerMetrics Metrics;

    private ServerStatus _status;
    private bool _disposed;
    private ServerTcpListener? _tcpListener;
    private ServerUdpListener? _udpListener;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    protected ServerBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        StopCts = new CancellationTokenSource();
        Statistics = new ServerStatistics();
        Metrics = new ServerMetrics(Name, Type.ToString());
        _status = ServerStatus.Stopped;
    }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract ServerType Type { get; }

    /// <inheritdoc/>
    public ServerStatus Status
    {
        get => _status;
        protected set => _status = value;
    }

    /// <inheritdoc/>
    public abstract int Port { get; }

    /// <inheritdoc/>
    public abstract string BindAddress { get; }

    /// <inheritdoc/>
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_status != ServerStatus.Stopped)
        {
            throw new InvalidOperationException($"Server {Name} is already running or starting.");
        }

        Logger.LogInformation("Starting server {ServerName} on port {Port}", Name, Port);
        _status = ServerStatus.Starting;

        try
        {
            // 新しい CancellationTokenSource を作成（再起動対応）
            StopCts?.Dispose();
            StopCts = new CancellationTokenSource();

            await StartListeningAsync(cancellationToken);
            Statistics.StartTime = DateTime.UtcNow;

            // Register metrics with global collector
            MetricsCollector.Instance.RegisterServer(Metrics);

            _status = ServerStatus.Running;
            Logger.LogInformation("Server {ServerName} started successfully", Name);
        }
        catch (Exception ex)
        {
            _status = ServerStatus.Error;
            Logger.LogError(ex, "Failed to start server {ServerName}", Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_status != ServerStatus.Running)
        {
            Logger.LogWarning("Server {ServerName} is not running", Name);
            return;
        }

        Logger.LogInformation("Stopping server {ServerName}", Name);
        _status = ServerStatus.Stopping;

        try
        {
            StopCts.Cancel();
            await StopListeningAsync(cancellationToken);

            // Unregister metrics from global collector
            MetricsCollector.Instance.UnregisterServer(Name);

            _status = ServerStatus.Stopped;
            Logger.LogInformation("Server {ServerName} stopped successfully", Name);
        }
        catch (Exception ex)
        {
            _status = ServerStatus.Error;
            Logger.LogError(ex, "Error stopping server {ServerName}", Name);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual ServerStatistics GetStatistics()
    {
        return Statistics;
    }

    /// <inheritdoc/>
    public virtual Task<bool> CheckHealthAsync(CancellationToken cancellationToken)
    {
        var isHealthy = _status == ServerStatus.Running;
        return Task.FromResult(isHealthy);
    }

    /// <summary>
    /// リスニング開始処理（派生クラスで実装）
    /// </summary>
    protected abstract Task StartListeningAsync(CancellationToken cancellationToken);

    /// <summary>
    /// リスニング停止処理（派生クラスで実装）
    /// </summary>
    protected abstract Task StopListeningAsync(CancellationToken cancellationToken);

    /// <summary>
    /// クライアント接続処理（派生クラスで実装）
    /// </summary>
    protected abstract Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken);

    #region リスナー管理メソッド（Phase 2で追加）

    /// <summary>
    /// TCPリスナーを作成・起動し、既存のリスナーは停止する
    /// </summary>
    protected async Task<ServerTcpListener> CreateTcpListenerAsync(
        int port,
        string? bindAddress,
        CancellationToken cancellationToken)
    {
        await StopExistingListenerAsync(_tcpListener);

        var ipAddress = NetworkHelper.ParseBindAddress(bindAddress, Logger);
        var listener = new ServerTcpListener(port, ipAddress, Logger);
        await listener.StartAsync(cancellationToken);

        _tcpListener = listener;
        return listener;
    }

    /// <summary>
    /// UDPリスナーを作成・起動し、既存のリスナーは停止する
    /// </summary>
    protected async Task<ServerUdpListener> CreateUdpListenerAsync(
        int port,
        string? bindAddress,
        CancellationToken cancellationToken)
    {
        await StopExistingListenerAsync(_udpListener);

        var ipAddress = NetworkHelper.ParseBindAddress(bindAddress, Logger);
        var listener = new ServerUdpListener(port, ipAddress, Logger);
        await listener.StartAsync(cancellationToken);

        _udpListener = listener;
        return listener;
    }

    /// <summary>
    /// 既存のリスナーを停止する（タイムアウト付き）
    /// </summary>
    private async Task StopExistingListenerAsync(IDisposable? listener)
    {
        if (listener == null)
        {
            return;
        }

        try
        {
            // タイムアウト付きで停止処理を実行（デッドロック防止）
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (listener is ServerTcpListener tcp)
            {
                await tcp.StopAsync(cts.Token);
            }
            else if (listener is ServerUdpListener udp)
            {
                await udp.StopAsync(cts.Token);
            }

            listener.Dispose();
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Listener stop timed out (5s), forcing disposal");
            // タイムアウト時も Dispose を試みる
            try
            {
                listener.Dispose();
            }
            catch (Exception disposeEx)
            {
                Logger.LogDebug(disposeEx, "Error disposing listener after stop timeout");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error stopping existing listener");
            // エラー時も Dispose を試みる
            try
            {
                listener.Dispose();
            }
            catch (Exception disposeEx)
            {
                Logger.LogDebug(disposeEx, "Error disposing listener after stop failure");
            }
        }
    }

    /// <summary>
    /// 現在のTCPリスナーを停止する（派生クラスから呼び出し可能）
    /// </summary>
    protected async Task StopTcpListenerAsync()
    {
        await StopExistingListenerAsync(_tcpListener);
        _tcpListener = null;
    }

    /// <summary>
    /// 現在のUDPリスナーを停止する（派生クラスから呼び出し可能）
    /// </summary>
    protected async Task StopUdpListenerAsync()
    {
        await StopExistingListenerAsync(_udpListener);
        _udpListener = null;
    }

    #endregion

    #region Accept/Receiveループメソッド（Phase 2で追加）

    /// <summary>
    /// TCP Accept ループを実行
    /// </summary>
    protected async Task RunTcpAcceptLoopAsync(
        ServerTcpListener listener,
        Func<TcpClient, CancellationToken, Task> handler,
        ConnectionLimiter? limiter,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var clientSocket = await listener.AcceptAsync(cancellationToken);
                // SocketをTcpClientにラップ（既存のサーバー実装との互換性維持）
                var client = new TcpClient { Client = clientSocket };

                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (limiter != null)
                        {
                            await limiter.ExecuteWithLimitAsync(
                                async ct => await handler(client, ct),
                                cancellationToken);
                        }
                        else
                        {
                            await handler(client, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        NetworkExceptionHandler.LogNetworkException(ex, Logger, "Client handling");
                    }
                    finally
                    {
                        client.Dispose();
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error accepting client");
            }
        }
    }

    /// <summary>
    /// UDP Receive ループを実行
    /// </summary>
    protected async Task RunUdpReceiveLoopAsync(
        ServerUdpListener listener,
        Func<byte[], EndPoint, CancellationToken, Task> handler,
        ConnectionLimiter? limiter,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var (data, remoteEndPoint) = await listener.ReceiveAsync(cancellationToken);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (limiter != null)
                        {
                            await limiter.ExecuteWithLimitAsync(
                                async ct => await handler(data, remoteEndPoint, ct),
                                cancellationToken);
                        }
                        else
                        {
                            await handler(data, remoteEndPoint, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        NetworkExceptionHandler.LogNetworkException(ex, Logger, "Request handling");
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error receiving request");
            }
        }
    }

    #endregion

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// リソース解放
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Unregister metrics from global collector
            MetricsCollector.Instance.UnregisterServer(Name);

            StopCts?.Dispose();
        }

        _disposed = true;
    }
}
