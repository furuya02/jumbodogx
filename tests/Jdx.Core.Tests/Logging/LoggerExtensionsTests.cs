using Jdx.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;

namespace Jdx.Core.Tests.Logging;

/// <summary>
/// LoggerExtensionsのユニットテスト
/// </summary>
public class LoggerExtensionsTests
{
    private readonly Mock<ILogger> _mockLogger;

    public LoggerExtensionsTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }

    #region LogServerEvent

    [Fact]
    public void LogServerEvent_WithValidParameters_LogsInformation()
    {
        // Arrange
        var serverName = "TestServer";
        var eventType = "Started";
        var message = "Server initialized";

        // Act
        _mockLogger.Object.LogServerEvent(serverName, eventType, message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(serverName) && v.ToString()!.Contains(eventType)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogServerEvent_WithAdditionalArgs_LogsInformation()
    {
        // Arrange
        var serverName = "HttpServer";
        var eventType = "Configured";
        var message = "Port: {0}";
        var port = 8080;

        // Act
        _mockLogger.Object.LogServerEvent(serverName, eventType, message, port);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region LogClientConnection

    [Fact]
    public void LogClientConnection_WithDefaultAction_LogsConnected()
    {
        // Arrange
        var serverName = "FtpServer";
        var clientAddress = "192.168.1.100";

        // Act
        _mockLogger.Object.LogClientConnection(serverName, clientAddress);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(serverName) &&
                    v.ToString()!.Contains(clientAddress) &&
                    v.ToString()!.Contains("Connected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogClientConnection_WithCustomAction_LogsCustomAction()
    {
        // Arrange
        var serverName = "SmtpServer";
        var clientAddress = "10.0.0.50";
        var action = "Authenticated";

        // Act
        _mockLogger.Object.LogClientConnection(serverName, clientAddress, action);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(serverName) &&
                    v.ToString()!.Contains(clientAddress) &&
                    v.ToString()!.Contains(action)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region LogClientDisconnection

    [Fact]
    public void LogClientDisconnection_WithoutReason_LogsWithoutReason()
    {
        // Arrange
        var serverName = "Pop3Server";
        var clientAddress = "172.16.0.1";

        // Act
        _mockLogger.Object.LogClientDisconnection(serverName, clientAddress);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(serverName) &&
                    v.ToString()!.Contains(clientAddress) &&
                    v.ToString()!.Contains("disconnected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogClientDisconnection_WithNullReason_LogsWithoutReason()
    {
        // Arrange
        var serverName = "DnsServer";
        var clientAddress = "8.8.8.8";

        // Act
        _mockLogger.Object.LogClientDisconnection(serverName, clientAddress, null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogClientDisconnection_WithEmptyReason_LogsWithoutReason()
    {
        // Arrange
        var serverName = "TftpServer";
        var clientAddress = "192.168.0.1";

        // Act
        _mockLogger.Object.LogClientDisconnection(serverName, clientAddress, "");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogClientDisconnection_WithReason_LogsWithReason()
    {
        // Arrange
        var serverName = "HttpServer";
        var clientAddress = "192.168.1.1";
        var reason = "Timeout";

        // Act
        _mockLogger.Object.LogClientDisconnection(serverName, clientAddress, reason);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(serverName) &&
                    v.ToString()!.Contains(clientAddress) &&
                    v.ToString()!.Contains(reason)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region LogServerStatistics

    [Fact]
    public void LogServerStatistics_WithValidParameters_LogsStatistics()
    {
        // Arrange
        var serverName = "ProxyServer";
        var activeConnections = 10L;
        var totalRequests = 1000L;

        // Act
        _mockLogger.Object.LogServerStatistics(serverName, activeConnections, totalRequests);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(serverName) &&
                    v.ToString()!.Contains("Stats")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogServerStatistics_WithZeroValues_LogsStatistics()
    {
        // Arrange
        var serverName = "DhcpServer";
        var activeConnections = 0L;
        var totalRequests = 0L;

        // Act
        _mockLogger.Object.LogServerStatistics(serverName, activeConnections, totalRequests);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region LogRequestProcessed

    [Fact]
    public void LogRequestProcessed_WithValidParameters_LogsRequest()
    {
        // Arrange
        var serverName = "HttpServer";
        var method = "GET";
        var path = "/index.html";
        var statusCode = 200;
        var elapsedMs = 15.5;

        // Act
        _mockLogger.Object.LogRequestProcessed(serverName, method, path, statusCode, elapsedMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains(serverName) &&
                    v.ToString()!.Contains(method) &&
                    v.ToString()!.Contains(path)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("POST", "/api/data", 201)]
    [InlineData("PUT", "/api/update", 200)]
    [InlineData("DELETE", "/api/delete/1", 204)]
    [InlineData("GET", "/notfound", 404)]
    [InlineData("GET", "/error", 500)]
    public void LogRequestProcessed_WithDifferentMethods_LogsCorrectly(
        string method, string path, int statusCode)
    {
        // Arrange
        var serverName = "WebServer";
        var elapsedMs = 25.0;

        // Act
        _mockLogger.Object.LogRequestProcessed(serverName, method, path, statusCode, elapsedMs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}
