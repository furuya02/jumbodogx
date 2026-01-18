namespace Jdx.Core.Network;

/// <summary>
/// 接続数制限を管理するクラス
/// SemaphoreSlimをラップし、Task.Run内での安全な使用を提供
/// </summary>
public class ConnectionLimiter : IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    public ConnectionLimiter(int maxConnections)
    {
        _semaphore = new SemaphoreSlim(maxConnections, maxConnections);
    }

    /// <summary>
    /// Task.Run内での非同期エラーハンドリングに対応した実行メソッド
    /// セマフォの取得・解放をtry/finallyで確実に行う
    /// </summary>
    public async Task ExecuteWithLimitAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
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
    public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        return new ReleaseHandle(_semaphore);
    }

    private class ReleaseHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public ReleaseHandle(SemaphoreSlim semaphore) => _semaphore = semaphore;
        public void Dispose() => _semaphore.Release();
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}
