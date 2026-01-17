using Jdx.Core.Abstractions;
using Jdx.Core.Settings;
using Jdx.Servers.Tftp;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Servers.Tftp.Tests;

public class TftpServerTests
{
    [Fact]
    public void Constructor_ShouldInitializeServer()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TftpServer>>();
        var settings = new TftpServerSettings
        {
            Port = 69,
            WorkDir = Path.GetTempPath(),
            Read = true,
            Write = true
        };

        // Act
        var server = new TftpServer(mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal("TftpServer", server.Name);
        Assert.Equal(ServerType.Tftp, server.Type);
        Assert.Equal(69, server.Port);
    }

    [Fact]
    public void Properties_ShouldReturnCorrectValues()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TftpServer>>();
        var settings = new TftpServerSettings
        {
            Port = 6969,
            WorkDir = Path.GetTempPath()
        };

        // Act
        var server = new TftpServer(mockLogger.Object, settings);

        // Assert
        Assert.Equal(6969, server.Port);
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    [Fact]
    public async Task StartAsync_WithInvalidWorkDir_ShouldThrowException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TftpServer>>();
        var settings = new TftpServerSettings
        {
            Port = 6969,
            WorkDir = "" // Invalid empty WorkDir
        };
        var server = new TftpServer(mockLogger.Object, settings);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => server.StartAsync(CancellationToken.None));
    }
}
