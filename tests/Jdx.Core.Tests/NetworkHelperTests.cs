using System.Net;
using Jdx.Core.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Core.Tests;

public class NetworkHelperTests
{
    [Fact]
    public void ParseBindAddress_WithNull_ReturnsIpAddressAny()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var result = NetworkHelper.ParseBindAddress(null, mockLogger.Object);

        // Assert
        Assert.Equal(IPAddress.Any, result);
    }

    [Fact]
    public void ParseBindAddress_WithEmptyString_ReturnsIpAddressAny()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var result = NetworkHelper.ParseBindAddress("", mockLogger.Object);

        // Assert
        Assert.Equal(IPAddress.Any, result);
    }

    [Fact]
    public void ParseBindAddress_WithZeroAddress_ReturnsIpAddressAny()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var result = NetworkHelper.ParseBindAddress("0.0.0.0", mockLogger.Object);

        // Assert
        Assert.Equal(IPAddress.Any, result);
    }

    [Fact]
    public void ParseBindAddress_WithValidIpAddress_ReturnsParsedAddress()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var expectedAddress = IPAddress.Parse("192.168.1.1");

        // Act
        var result = NetworkHelper.ParseBindAddress("192.168.1.1", mockLogger.Object);

        // Assert
        Assert.Equal(expectedAddress, result);
    }

    [Fact]
    public void ParseBindAddress_WithLocalhost_ReturnsLoopbackAddress()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var expectedAddress = IPAddress.Parse("127.0.0.1");

        // Act
        var result = NetworkHelper.ParseBindAddress("127.0.0.1", mockLogger.Object);

        // Assert
        Assert.Equal(expectedAddress, result);
    }

    [Fact]
    public void ParseBindAddress_WithInvalidAddress_ReturnsIpAddressAnyAndLogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();

        // Act
        var result = NetworkHelper.ParseBindAddress("invalid-address", mockLogger.Object);

        // Assert
        Assert.Equal(IPAddress.Any, result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid bind address")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
