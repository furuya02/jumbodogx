namespace Jdx.Core.Network;

/// <summary>
/// 接続数制限を管理するクラス
/// SemaphoreSlimをラップし、Task.Run内での安全な使用を提供
/// </summary>
public class ConnectionLimiter : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;

    /// <summary>
    /// ConnectionLimiterの新しいインスタンスを初期化します
    /// </summary>
    /// <param name="maxConnections">最大同時接続数</param>
    public ConnectionLimiter(int maxConnections)
    {
        _semaphore = new SemaphoreSlim(maxConnections, maxConnections);
    }

    /// <summary>
    /// Task.Run内での非同期エラーハンドリングに対応した実行メソッド
    /// セマフォの取得・解放をtry/finallyで確実に行う
    /// </summary>
    /// <param name="action">実行するアクション</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <exception cref="ObjectDisposedException">既に破棄されている場合</exception>
    /// <exception cref="OperationCanceledException">キャンセルされた場合</exception>
    public async Task ExecuteWithLimitAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ConnectionLimiter));
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await action(cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// using パターン用（単純なケース向け）
    /// Task.Run内で使用する場合はExecuteWithLimitAsyncを推奨
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>破棄時にセマフォを解放するハンドル</returns>
    /// <exception cref="ObjectDisposedException">既に破棄されている場合</exception>
    /// <exception cref="OperationCanceledException">キャンセルされた場合</exception>
    public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ConnectionLimiter));
        }

        await _semaphore.WaitAsync(cancellationToken);
        return new ReleaseHandle(_semaphore);
    }

    private class ReleaseHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public ReleaseHandle(SemaphoreSlim semaphore) => _semaphore = semaphore;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _semaphore.Release();
        }
    }

    /// <summary>
    /// リソースを解放します
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _semaphore?.Dispose();
    }
}
