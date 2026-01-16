using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Bjd9.Core.Abstractions;

/// <summary>
/// サーバーの基底抽象クラス
/// 共通機能を提供し、派生クラスは個別の実装のみを行う
/// </summary>
public abstract class ServerBase : IServer
{
    protected readonly ILogger Logger;
    protected CancellationTokenSource StopCts;
    protected readonly ServerStatistics Statistics;

    private ServerStatus _status;
    private bool _disposed;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    protected ServerBase(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        StopCts = new CancellationTokenSource();
        Statistics = new ServerStatistics();
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
            return;

        if (disposing)
        {
            StopCts?.Dispose();
        }

        _disposed = true;
    }
}
