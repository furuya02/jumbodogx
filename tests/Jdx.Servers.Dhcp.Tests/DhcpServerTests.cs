using Jdx.Core.Abstractions;
using Jdx.Core.Settings;
using Jdx.Servers.Dhcp;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Servers.Dhcp.Tests;

public class DhcpServerTests
{
    [Fact]
    public void Constructor_ShouldInitializeServer()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DhcpServer>>();
        var settings = new DhcpServerSettings
        {
            Port = 67,
            StartIp = "192.168.1.100",
            EndIp = "192.168.1.200",
            MaskIp = "255.255.255.0",
            GwIp = "192.168.1.1",
            LeaseTime = 86400
        };

        // Act
        var server = new DhcpServer(mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal("DhcpServer", server.Name);
        Assert.Equal(ServerType.Dhcp, server.Type);
        Assert.Equal(67, server.Port);
    }

    [Fact]
    public void Properties_ShouldReturnCorrectValues()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DhcpServer>>();
        var settings = new DhcpServerSettings
        {
            Port = 6767,
            StartIp = "10.0.0.10",
            EndIp = "10.0.0.50"
        };

        // Act
        var server = new DhcpServer(mockLogger.Object, settings);

        // Assert
        Assert.Equal(6767, server.Port);
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    [Fact]
    public async Task StartAsync_WithoutIpRange_ShouldThrowException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DhcpServer>>();
        var settings = new DhcpServerSettings
        {
            Port = 6767,
            StartIp = "", // Missing StartIp
            EndIp = ""    // Missing EndIp
        };
        var server = new DhcpServer(mockLogger.Object, settings);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => server.StartAsync(CancellationToken.None));
    }
}
