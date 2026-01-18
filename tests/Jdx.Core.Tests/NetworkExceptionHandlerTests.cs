using System.Net.Sockets;
using Jdx.Core.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Core.Tests;

public class NetworkExceptionHandlerTests
{
    [Fact]
    public void LogNetworkException_WithOperationCanceledException_LogsDebug()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var ex = new OperationCanceledException();

        // Act
        NetworkExceptionHandler.LogNetworkException(ex, mockLogger.Object, "Test operation");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("cancelled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogNetworkException_WithSocketException_LogsDebug()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var ex = new SocketException();

        // Act
        NetworkExceptionHandler.LogNetworkException(ex, mockLogger.Object, "Test operation");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("socket error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogNetworkException_WithIOExceptionContainingSocketException_LogsDebug()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var socketEx = new SocketException();
        var ioEx = new IOException("Connection closed", socketEx);

        // Act
        NetworkExceptionHandler.LogNetworkException(ioEx, mockLogger.Object, "Test operation");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("connection closed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogNetworkException_WithUnexpectedException_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var ex = new InvalidOperationException("Unexpected error");

        // Act
        NetworkExceptionHandler.LogNetworkException(ex, mockLogger.Object, "Test operation");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("unexpected error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void IsTerminalException_WithOperationCanceledException_ReturnsTrue()
    {
        // Arrange
        var ex = new OperationCanceledException();

        // Act
        var result = NetworkExceptionHandler.IsTerminalException(ex);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTerminalException_WithSocketException_ReturnsFalse()
    {
        // Arrange
        var ex = new SocketException();

        // Act
        var result = NetworkExceptionHandler.IsTerminalException(ex);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTerminalException_WithIOException_ReturnsFalse()
    {
        // Arrange
        var ex = new IOException();

        // Act
        var result = NetworkExceptionHandler.IsTerminalException(ex);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HandleOrRethrow_WithOperationCanceledException_Rethrows()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var ex = new OperationCanceledException();

        // Act & Assert
        Assert.Throws<OperationCanceledException>(
            () => NetworkExceptionHandler.HandleOrRethrow(ex, mockLogger.Object, "Test"));
    }

    [Fact]
    public void HandleOrRethrow_WithSocketException_DoesNotRethrow()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var ex = new SocketException();

        // Act
        NetworkExceptionHandler.HandleOrRethrow(ex, mockLogger.Object, "Test");

        // Assert - no exception thrown, just logs
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
