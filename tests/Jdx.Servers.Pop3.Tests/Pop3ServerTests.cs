using Jdx.Core.Abstractions;
using Jdx.Core.Settings;
using Jdx.Servers.Pop3;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Servers.Pop3.Tests;

public class Pop3ServerTests
{
    [Fact]
    public void Constructor_ShouldInitializeServer()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Pop3Server>>();
        var settings = new Pop3ServerSettings
        {
            Port = 110
        };

        // Act
        var server = new Pop3Server(mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal("Pop3Server", server.Name);
        Assert.Equal(ServerType.Pop3, server.Type);
        Assert.Equal(110, server.Port);
    }

    [Fact]
    public void Properties_ShouldReturnCorrectValues()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Pop3Server>>();
        var settings = new Pop3ServerSettings
        {
            Port = 1110,
            BannerMessage = "Test POP3 Server Ready"
        };

        // Act
        var server = new Pop3Server(mockLogger.Object, settings);

        // Assert
        Assert.Equal(1110, server.Port);
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    [Theory]
    [InlineData(110)]
    [InlineData(995)]
    [InlineData(11000)]
    public void Port_WithDifferentValues_ShouldReturnCorrectPort(int port)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Pop3Server>>();
        var settings = new Pop3ServerSettings { Port = port };

        // Act
        var server = new Pop3Server(mockLogger.Object, settings);

        // Assert
        Assert.Equal(port, server.Port);
    }
}
