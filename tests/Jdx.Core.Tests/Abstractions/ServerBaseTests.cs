using System.Net.Sockets;
using Jdx.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Jdx.Core.Tests.Abstractions;

/// <summary>
/// ServerBaseのユニットテスト
/// </summary>
public class ServerBaseTests
{
    private readonly Mock<ILogger> _mockLogger;

    public ServerBaseTests()
    {
        _mockLogger = new Mock<ILogger>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidLogger_InitializesCorrectly()
    {
        // Arrange & Act
        using var server = new TestServer(_mockLogger.Object);

        // Assert
        Assert.Equal(ServerStatus.Stopped, server.Status);
        Assert.Equal("TestServer", server.Name);
        Assert.Equal(ServerType.Http, server.Type);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TestServer(null!));
    }

    #endregion

    #region StartAsync Tests

    [Fact]
    public async Task StartAsync_WhenStopped_StartsServer()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object);

        // Act
        await server.StartAsync(CancellationToken.None);

        // Assert
        Assert.Equal(ServerStatus.Running, server.Status);
    }

    [Fact]
    public async Task StartAsync_WhenAlreadyRunning_ThrowsInvalidOperationException()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object);
        await server.StartAsync(CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => server.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsync_WhenStartFails_SetsErrorStatus()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object)
        {
            ShouldFailOnStart = true
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => server.StartAsync(CancellationToken.None));
        Assert.Equal(ServerStatus.Error, server.Status);
    }

    #endregion

    #region StopAsync Tests

    [Fact]
    public async Task StopAsync_WhenRunning_StopsServer()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object);
        await server.StartAsync(CancellationToken.None);

        // Act
        await server.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    [Fact]
    public async Task StopAsync_WhenNotRunning_DoesNothing()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object);

        // Act
        await server.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    [Fact]
    public async Task StopAsync_WhenStopFails_SetsErrorStatus()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object);
        await server.StartAsync(CancellationToken.None);
        server.ShouldFailOnStop = true;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => server.StopAsync(CancellationToken.None));
        Assert.Equal(ServerStatus.Error, server.Status);
    }

    #endregion

    #region CheckHealthAsync Tests

    [Fact]
    public async Task CheckHealthAsync_WhenRunning_ReturnsTrue()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object);
        await server.StartAsync(CancellationToken.None);

        // Act
        var isHealthy = await server.CheckHealthAsync(CancellationToken.None);

        // Assert
        Assert.True(isHealthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenStopped_ReturnsFalse()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object);

        // Act
        var isHealthy = await server.CheckHealthAsync(CancellationToken.None);

        // Assert
        Assert.False(isHealthy);
    }

    #endregion

    #region GetStatistics Tests

    [Fact]
    public void GetStatistics_ReturnsStatisticsObject()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object);

        // Act
        var stats = server.GetStatistics();

        // Assert
        Assert.NotNull(stats);
    }

    [Fact]
    public async Task GetStatistics_AfterStart_HasStartTime()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object);
        var beforeStart = DateTime.UtcNow;

        // Act
        await server.StartAsync(CancellationToken.None);
        var stats = server.GetStatistics();
        var afterStart = DateTime.UtcNow;

        // Assert
        Assert.True(stats.StartTime >= beforeStart);
        Assert.True(stats.StartTime <= afterStart);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var server = new TestServer(_mockLogger.Object);

        // Act & Assert - should not throw
        server.Dispose();
        server.Dispose();
    }

    [Fact]
    public async Task Dispose_AfterStart_CleansUpResources()
    {
        // Arrange
        var server = new TestServer(_mockLogger.Object);
        await server.StartAsync(CancellationToken.None);

        // Act
        server.Dispose();

        // Assert - should not throw, resources should be cleaned up
        Assert.True(server.IsDisposed);
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public async Task Lifecycle_StartStopRestart_WorksCorrectly()
    {
        // Arrange
        using var server = new TestServer(_mockLogger.Object);

        // Act & Assert - Start
        await server.StartAsync(CancellationToken.None);
        Assert.Equal(ServerStatus.Running, server.Status);

        // Act & Assert - Stop
        await server.StopAsync(CancellationToken.None);
        Assert.Equal(ServerStatus.Stopped, server.Status);

        // Act & Assert - Restart
        await server.StartAsync(CancellationToken.None);
        Assert.Equal(ServerStatus.Running, server.Status);
    }

    #endregion

    /// <summary>
    /// テスト用のServerBase実装
    /// </summary>
    private class TestServer : ServerBase
    {
        public bool ShouldFailOnStart { get; set; }
        public bool ShouldFailOnStop { get; set; }
        public bool IsDisposed { get; private set; }

        public TestServer(ILogger logger) : base(logger)
        {
        }

        public override string Name => "TestServer";
        public override ServerType Type => ServerType.Http;
        public override int Port => 8080;
        public override string BindAddress => "127.0.0.1";

        protected override Task StartListeningAsync(CancellationToken cancellationToken)
        {
            if (ShouldFailOnStart)
            {
                throw new InvalidOperationException("Simulated start failure");
            }
            return Task.CompletedTask;
        }

        protected override Task StopListeningAsync(CancellationToken cancellationToken)
        {
            if (ShouldFailOnStop)
            {
                throw new InvalidOperationException("Simulated stop failure");
            }
            return Task.CompletedTask;
        }

        protected override Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
