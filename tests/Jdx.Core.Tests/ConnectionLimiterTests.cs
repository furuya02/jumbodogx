using Jdx.Core.Network;
using Xunit;

namespace Jdx.Core.Tests;

public class ConnectionLimiterTests
{
    [Fact]
    public void Constructor_WithMaxConnections_CreatesLimiter()
    {
        // Act
        using var limiter = new ConnectionLimiter(5);

        // Assert
        Assert.NotNull(limiter);
    }

    [Fact]
    public async Task ExecuteWithLimitAsync_WithinLimit_ExecutesAction()
    {
        // Arrange
        using var limiter = new ConnectionLimiter(2);
        var executed = false;

        // Act
        await limiter.ExecuteWithLimitAsync(async ct =>
        {
            executed = true;
            await Task.Delay(10, ct);
        }, CancellationToken.None);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public async Task ExecuteWithLimitAsync_ConcurrentCalls_RespectsLimit()
    {
        // Arrange
        using var limiter = new ConnectionLimiter(2);
        var concurrentCount = 0;
        var maxConcurrent = 0;

        // Act
        var tasks = Enumerable.Range(0, 5).Select(async _ =>
        {
            await limiter.ExecuteWithLimitAsync(async ct =>
            {
                Interlocked.Increment(ref concurrentCount);
                var current = concurrentCount;
                if (current > maxConcurrent)
                {
                    maxConcurrent = current;
                }
                await Task.Delay(50, ct);
                Interlocked.Decrement(ref concurrentCount);
            }, CancellationToken.None);
        });

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(0, concurrentCount);
        Assert.True(maxConcurrent <= 2, $"Max concurrent was {maxConcurrent}, expected <= 2");
    }

    [Fact]
    public async Task AcquireAsync_ReturnsDisposableHandle()
    {
        // Arrange
        using var limiter = new ConnectionLimiter(1);

        // Act
        var handle = await limiter.AcquireAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(handle);
        handle.Dispose();
    }

    [Fact]
    public async Task AcquireAsync_WithDispose_ReleasesSlot()
    {
        // Arrange
        using var limiter = new ConnectionLimiter(1);
        var handle1 = await limiter.AcquireAsync(CancellationToken.None);

        // Act
        handle1.Dispose();
        var handle2 = await limiter.AcquireAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(handle2);
        handle2.Dispose();
    }

    [Fact]
    public async Task ExecuteWithLimitAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var limiter = new ConnectionLimiter(1);
        limiter.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => limiter.ExecuteWithLimitAsync(async ct => await Task.Delay(10, ct), CancellationToken.None));
    }

    [Fact]
    public async Task AcquireAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var limiter = new ConnectionLimiter(1);
        limiter.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => limiter.AcquireAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteWithLimitAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        using var limiter = new ConnectionLimiter(1);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => limiter.ExecuteWithLimitAsync(async ct => await Task.Delay(10, ct), cts.Token));
    }
}
