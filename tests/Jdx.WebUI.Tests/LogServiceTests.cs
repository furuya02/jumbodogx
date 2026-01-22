using Jdx.WebUI.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jdx.WebUI.Tests;

/// <summary>
/// LogServiceのユニットテスト
/// </summary>
public class LogServiceTests
{
    #region AddLog Tests

    [Fact]
    public void AddLog_WithValidEntry_AddsToLogs()
    {
        // Arrange
        var service = new LogService();

        // Act
        service.AddLog(LogLevel.Information, "TestCategory", "Test message");

        // Assert
        var logs = service.GetRecentLogs(10).ToList();
        Assert.Single(logs);
        Assert.Equal(LogLevel.Information, logs[0].Level);
        Assert.Equal("TestCategory", logs[0].Category);
        Assert.Equal("Test message", logs[0].Message);
    }

    [Fact]
    public void AddLog_MultipleTimes_AddsAllEntries()
    {
        // Arrange
        var service = new LogService();

        // Act
        service.AddLog(LogLevel.Information, "Cat1", "Message 1");
        service.AddLog(LogLevel.Warning, "Cat2", "Message 2");
        service.AddLog(LogLevel.Error, "Cat3", "Message 3");

        // Assert
        var logs = service.GetRecentLogs(10).ToList();
        Assert.Equal(3, logs.Count);
    }

    [Fact]
    public void AddLog_TriggersLogAddedEvent()
    {
        // Arrange
        var service = new LogService();
        LogEntry? receivedEntry = null;
        service.LogAdded += (sender, entry) => receivedEntry = entry;

        // Act
        service.AddLog(LogLevel.Information, "TestCategory", "Test message");

        // Assert
        Assert.NotNull(receivedEntry);
        Assert.Equal("TestCategory", receivedEntry.Category);
        Assert.Equal("Test message", receivedEntry.Message);
    }

    [Fact]
    public void AddLog_SetsTimestamp()
    {
        // Arrange
        var service = new LogService();
        var beforeAdd = DateTime.Now;

        // Act
        service.AddLog(LogLevel.Information, "TestCategory", "Test message");

        // Assert
        var logs = service.GetRecentLogs(1).ToList();
        Assert.True(logs[0].Timestamp >= beforeAdd);
        Assert.True(logs[0].Timestamp <= DateTime.Now);
    }

    [Fact]
    public void AddLog_ExceedingMaxEntries_RemovesOldest()
    {
        // Arrange
        var service = new LogService();

        // Act - MaxLogEntries is 1000
        for (int i = 0; i < 1100; i++)
        {
            service.AddLog(LogLevel.Information, "Test", $"Message {i}");
        }

        // Assert
        var logs = service.GetRecentLogs(2000).ToList();
        Assert.Equal(1000, logs.Count);
        // First message should be removed, so oldest should be around 100
        Assert.Contains("Message 100", logs.First().Message);
    }

    #endregion

    #region GetRecentLogs Tests

    [Fact]
    public void GetRecentLogs_WithEmptyLogs_ReturnsEmptyList()
    {
        // Arrange
        var service = new LogService();

        // Act
        var logs = service.GetRecentLogs(10);

        // Assert
        Assert.Empty(logs);
    }

    [Fact]
    public void GetRecentLogs_WithCountLessThanTotal_ReturnsRequestedCount()
    {
        // Arrange
        var service = new LogService();
        for (int i = 0; i < 20; i++)
        {
            service.AddLog(LogLevel.Information, "Test", $"Message {i}");
        }

        // Act
        var logs = service.GetRecentLogs(5).ToList();

        // Assert
        Assert.Equal(5, logs.Count);
        // Should be the last 5 messages
        Assert.Equal("Message 15", logs[0].Message);
        Assert.Equal("Message 19", logs[4].Message);
    }

    [Fact]
    public void GetRecentLogs_WithCountGreaterThanTotal_ReturnsAllLogs()
    {
        // Arrange
        var service = new LogService();
        service.AddLog(LogLevel.Information, "Test", "Message 1");
        service.AddLog(LogLevel.Information, "Test", "Message 2");

        // Act
        var logs = service.GetRecentLogs(100).ToList();

        // Assert
        Assert.Equal(2, logs.Count);
    }

    [Fact]
    public void GetRecentLogs_DefaultCount_Returns100()
    {
        // Arrange
        var service = new LogService();
        for (int i = 0; i < 150; i++)
        {
            service.AddLog(LogLevel.Information, "Test", $"Message {i}");
        }

        // Act
        var logs = service.GetRecentLogs().ToList();

        // Assert
        Assert.Equal(100, logs.Count);
    }

    #endregion

    #region GetLogsByLevel Tests

    [Fact]
    public void GetLogsByLevel_ReturnsOnlyMatchingLevel()
    {
        // Arrange
        var service = new LogService();
        service.AddLog(LogLevel.Information, "Test", "Info 1");
        service.AddLog(LogLevel.Warning, "Test", "Warning 1");
        service.AddLog(LogLevel.Information, "Test", "Info 2");
        service.AddLog(LogLevel.Error, "Test", "Error 1");
        service.AddLog(LogLevel.Information, "Test", "Info 3");

        // Act
        var infoLogs = service.GetLogsByLevel(LogLevel.Information).ToList();
        var warningLogs = service.GetLogsByLevel(LogLevel.Warning).ToList();
        var errorLogs = service.GetLogsByLevel(LogLevel.Error).ToList();

        // Assert
        Assert.Equal(3, infoLogs.Count);
        Assert.Single(warningLogs);
        Assert.Single(errorLogs);
    }

    [Fact]
    public void GetLogsByLevel_WithNoMatchingLevel_ReturnsEmpty()
    {
        // Arrange
        var service = new LogService();
        service.AddLog(LogLevel.Information, "Test", "Info message");

        // Act
        var debugLogs = service.GetLogsByLevel(LogLevel.Debug);

        // Assert
        Assert.Empty(debugLogs);
    }

    #endregion

    #region ClearLogs Tests

    [Fact]
    public void ClearLogs_RemovesAllLogs()
    {
        // Arrange
        var service = new LogService();
        service.AddLog(LogLevel.Information, "Test", "Message 1");
        service.AddLog(LogLevel.Information, "Test", "Message 2");
        service.AddLog(LogLevel.Information, "Test", "Message 3");

        // Act
        service.ClearLogs();

        // Assert
        var logs = service.GetRecentLogs(100);
        Assert.Empty(logs);
    }

    [Fact]
    public void ClearLogs_CanAddLogsAfterClear()
    {
        // Arrange
        var service = new LogService();
        service.AddLog(LogLevel.Information, "Test", "Message 1");
        service.ClearLogs();

        // Act
        service.AddLog(LogLevel.Warning, "NewCat", "New message");

        // Assert
        var logs = service.GetRecentLogs(10).ToList();
        Assert.Single(logs);
        Assert.Equal("New message", logs[0].Message);
    }

    #endregion
}

/// <summary>
/// LogEntryのユニットテスト
/// </summary>
public class LogEntryTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new LogEntry();

        // Assert
        Assert.Equal("", entry.Category);
        Assert.Equal("", entry.Message);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var entry = new LogEntry();
        var now = DateTime.Now;

        // Act
        entry.Timestamp = now;
        entry.Level = LogLevel.Warning;
        entry.Category = "TestCategory";
        entry.Message = "Test message";

        // Assert
        Assert.Equal(now, entry.Timestamp);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal("TestCategory", entry.Category);
        Assert.Equal("Test message", entry.Message);
    }

    #endregion

    #region LevelClass Tests

    [Theory]
    [InlineData(LogLevel.Error, "text-danger")]
    [InlineData(LogLevel.Critical, "text-danger")]
    [InlineData(LogLevel.Warning, "text-warning")]
    [InlineData(LogLevel.Information, "text-info")]
    [InlineData(LogLevel.Debug, "text-secondary")]
    [InlineData(LogLevel.Trace, "text-secondary")]
    public void LevelClass_ReturnsCorrectClass(LogLevel level, string expectedClass)
    {
        // Arrange
        var entry = new LogEntry { Level = level };

        // Act
        var levelClass = entry.LevelClass;

        // Assert
        Assert.Equal(expectedClass, levelClass);
    }

    [Fact]
    public void LevelClass_WithNone_ReturnsEmpty()
    {
        // Arrange
        var entry = new LogEntry { Level = LogLevel.None };

        // Act
        var levelClass = entry.LevelClass;

        // Assert
        Assert.Equal("", levelClass);
    }

    #endregion

    #region LevelIcon Tests

    [Theory]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void LevelIcon_ForErrorAndCritical_ReturnsErrorIcon(LogLevel level)
    {
        // Arrange
        var entry = new LogEntry { Level = level };

        // Act & Assert
        Assert.NotEmpty(entry.LevelIcon);
    }

    [Fact]
    public void LevelIcon_ForWarning_ReturnsWarningIcon()
    {
        // Arrange
        var entry = new LogEntry { Level = LogLevel.Warning };

        // Act & Assert
        Assert.NotEmpty(entry.LevelIcon);
    }

    [Fact]
    public void LevelIcon_ForInformation_ReturnsInfoIcon()
    {
        // Arrange
        var entry = new LogEntry { Level = LogLevel.Information };

        // Act & Assert
        Assert.NotEmpty(entry.LevelIcon);
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Trace)]
    public void LevelIcon_ForDebugAndTrace_ReturnsDebugIcon(LogLevel level)
    {
        // Arrange
        var entry = new LogEntry { Level = level };

        // Act & Assert
        Assert.NotEmpty(entry.LevelIcon);
    }

    [Fact]
    public void LevelIcon_ForNone_ReturnsDefaultIcon()
    {
        // Arrange
        var entry = new LogEntry { Level = LogLevel.None };

        // Act & Assert
        Assert.NotEmpty(entry.LevelIcon);
    }

    #endregion
}

/// <summary>
/// ServerEventArgsのユニットテスト
/// </summary>
public class ServerEventArgsTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange & Act
        var args = new ServerEventArgs("http:localhost:8080", Jdx.Core.Abstractions.ServerStatus.Running);

        // Assert
        Assert.Equal("http:localhost:8080", args.ServerId);
        Assert.Equal(Jdx.Core.Abstractions.ServerStatus.Running, args.Status);
    }

    [Theory]
    [InlineData("dns", Jdx.Core.Abstractions.ServerStatus.Stopped)]
    [InlineData("ftp", Jdx.Core.Abstractions.ServerStatus.Running)]
    [InlineData("http:example.com:80", Jdx.Core.Abstractions.ServerStatus.Starting)]
    public void Constructor_WithDifferentValues_SetsCorrectly(string serverId, Jdx.Core.Abstractions.ServerStatus status)
    {
        // Arrange & Act
        var args = new ServerEventArgs(serverId, status);

        // Assert
        Assert.Equal(serverId, args.ServerId);
        Assert.Equal(status, args.Status);
    }
}

/// <summary>
/// VirtualHostInfoのユニットテスト
/// </summary>
public class VirtualHostInfoTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange & Act
        var info = new VirtualHostInfo("example.com:8080", "192.168.1.100");

        // Assert
        Assert.Equal("example.com:8080", info.Host);
        Assert.Equal("192.168.1.100", info.BindAddress);
    }

    [Theory]
    [InlineData("localhost:80", "0.0.0.0")]
    [InlineData("test.local:8443", "127.0.0.1")]
    [InlineData("api.example.com:3000", "10.0.0.1")]
    public void Constructor_WithDifferentValues_SetsCorrectly(string host, string bindAddress)
    {
        // Arrange & Act
        var info = new VirtualHostInfo(host, bindAddress);

        // Assert
        Assert.Equal(host, info.Host);
        Assert.Equal(bindAddress, info.BindAddress);
    }
}
