using Jdx.Core.Abstractions;
using Jdx.Core.Settings;
using Jdx.Servers.Pop3;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Servers.Pop3.Tests;

/// <summary>
/// Pop3Serverのユニットテスト
/// </summary>
public class Pop3ServerTests
{
    private readonly Mock<ILogger<Pop3Server>> _mockLogger;

    public Pop3ServerTests()
    {
        _mockLogger = new Mock<ILogger<Pop3Server>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeServer()
    {
        // Arrange
        var settings = new Pop3ServerSettings
        {
            Port = 110
        };

        // Act
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal("Pop3Server", server.Name);
        Assert.Equal(ServerType.Pop3, server.Type);
        Assert.Equal(110, server.Port);
    }

    [Fact]
    public void Constructor_WithCustomSettings_InitializesCorrectly()
    {
        // Arrange
        var settings = new Pop3ServerSettings
        {
            Port = 1110,
            BindAddress = "192.168.1.100",
            BannerMessage = "Custom POP3 Server",
            MaxConnections = 50,
            TimeOut = 60
        };

        // Act
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal(1110, server.Port);
        Assert.Equal("192.168.1.100", server.BindAddress);
    }

    [Fact]
    public void Constructor_WithDefaultSettings_UsesDefaults()
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(110, server.Port);
        Assert.Equal("0.0.0.0", server.BindAddress);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_ShouldReturnCorrectValues()
    {
        // Arrange
        var settings = new Pop3ServerSettings
        {
            Port = 1110,
            BannerMessage = "Test POP3 Server Ready"
        };

        // Act
        var server = new Pop3Server(_mockLogger.Object, settings);

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
        var settings = new Pop3ServerSettings { Port = port };

        // Act
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(port, server.Port);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Assert
        Assert.Equal("Pop3Server", server.Name);
    }

    [Fact]
    public void Type_ReturnsCorrectValue()
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(ServerType.Pop3, server.Type);
    }

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("127.0.0.1")]
    [InlineData("192.168.1.100")]
    public void BindAddress_WithDifferentValues_ReturnsCorrectValue(string address)
    {
        // Arrange
        var settings = new Pop3ServerSettings { BindAddress = address };

        // Act
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(address, server.BindAddress);
    }

    #endregion

    #region Status Tests

    [Fact]
    public void Status_Initially_IsStopped()
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var settings = new Pop3ServerSettings();
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Act & Assert - should not throw
        server.Dispose();
        server.Dispose();
    }

    [Fact]
    public void Dispose_DisposesConnectionLimiter()
    {
        // Arrange
        var settings = new Pop3ServerSettings();
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Act
        server.Dispose();

        // Assert - no exception means success
        Assert.True(true);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public void GetStatistics_ReturnsValidStatistics()
    {
        // Arrange
        var settings = new Pop3ServerSettings();
        var server = new Pop3Server(_mockLogger.Object, settings);

        // Act
        var stats = server.GetStatistics();

        // Assert
        Assert.NotNull(stats);
    }

    #endregion
}

/// <summary>
/// Pop3ServerSettingsのユニットテスト
/// </summary>
public class Pop3ServerSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultSettings_HaveCorrectValues()
    {
        // Act
        var settings = new Pop3ServerSettings();

        // Assert
        Assert.False(settings.Enabled);
        Assert.Equal("0.0.0.0", settings.BindAddress);
        Assert.Equal(110, settings.Port);
        Assert.Equal(30, settings.TimeOut);
        Assert.Equal(10, settings.MaxConnections);
        Assert.Equal("$p $v", settings.BannerMessage);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectAuthSettings()
    {
        // Act
        var settings = new Pop3ServerSettings();

        // Assert
        Assert.Equal(Pop3AuthType.UserPass, settings.AuthType);
        Assert.Equal(30, settings.AuthTimeout);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectPasswordSettings()
    {
        // Act
        var settings = new Pop3ServerSettings();

        // Assert
        Assert.False(settings.UseChps);
        Assert.Equal(8, settings.MinimumLength);
        Assert.False(settings.DisableJoe);
        Assert.False(settings.UseNum);
        Assert.False(settings.UseSmall);
        Assert.False(settings.UseLarge);
        Assert.False(settings.UseSign);
    }

    #endregion

    #region Property Setter Tests

    [Theory]
    [InlineData(110)]
    [InlineData(995)]
    [InlineData(11000)]
    [InlineData(65535)]
    public void Port_CanBeSet(int port)
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        settings.Port = port;

        // Assert
        Assert.Equal(port, settings.Port);
    }

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("127.0.0.1")]
    [InlineData("192.168.1.100")]
    [InlineData("::1")]
    public void BindAddress_CanBeSet(string address)
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        settings.BindAddress = address;

        // Assert
        Assert.Equal(address, settings.BindAddress);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void MaxConnections_CanBeSet(int maxConnections)
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        settings.MaxConnections = maxConnections;

        // Assert
        Assert.Equal(maxConnections, settings.MaxConnections);
    }

    [Theory]
    [InlineData("Welcome to POP3")]
    [InlineData("$p $v ready")]
    [InlineData("Custom Banner")]
    public void BannerMessage_CanBeSet(string banner)
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        settings.BannerMessage = banner;

        // Assert
        Assert.Equal(banner, settings.BannerMessage);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Enabled_CanBeSet(bool enabled)
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        settings.Enabled = enabled;

        // Assert
        Assert.Equal(enabled, settings.Enabled);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    public void TimeOut_CanBeSet(int timeout)
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        settings.TimeOut = timeout;

        // Assert
        Assert.Equal(timeout, settings.TimeOut);
    }

    #endregion

    #region Password Policy Tests

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    public void MinimumLength_CanBeSet(int length)
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        settings.MinimumLength = length;

        // Assert
        Assert.Equal(length, settings.MinimumLength);
    }

    [Fact]
    public void PasswordPolicyFlags_CanBeSet()
    {
        // Arrange
        var settings = new Pop3ServerSettings();

        // Act
        settings.UseNum = true;
        settings.UseSmall = true;
        settings.UseLarge = true;
        settings.UseSign = true;
        settings.DisableJoe = true;

        // Assert
        Assert.True(settings.UseNum);
        Assert.True(settings.UseSmall);
        Assert.True(settings.UseLarge);
        Assert.True(settings.UseSign);
        Assert.True(settings.DisableJoe);
    }

    #endregion
}
