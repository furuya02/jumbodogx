using Jdx.Core.Abstractions;
using Jdx.Core.Settings;
using Jdx.Servers.Smtp;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Servers.Smtp.Tests;

/// <summary>
/// SmtpServerのユニットテスト
/// </summary>
public class SmtpServerTests
{
    private readonly Mock<ILogger<SmtpServer>> _mockLogger;

    public SmtpServerTests()
    {
        _mockLogger = new Mock<ILogger<SmtpServer>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldInitializeServer()
    {
        // Arrange
        var settings = new SmtpServerSettings
        {
            Port = 25,
            DomainName = "mail.example.com",
            UseEsmtp = true
        };

        // Act
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal("SmtpServer", server.Name);
        Assert.Equal(ServerType.Smtp, server.Type);
        Assert.Equal(25, server.Port);
    }

    [Fact]
    public void Constructor_WithDefaultSettings_InitializesCorrectly()
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal(25, server.Port);
        Assert.Equal("0.0.0.0", server.BindAddress);
    }

    [Fact]
    public void Constructor_WithCustomBindAddress_InitializesCorrectly()
    {
        // Arrange
        var settings = new SmtpServerSettings
        {
            Port = 587,
            BindAddress = "192.168.1.100",
            DomainName = "smtp.test.local"
        };

        // Act
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal("192.168.1.100", server.BindAddress);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_ShouldReturnCorrectValues()
    {
        // Arrange
        var settings = new SmtpServerSettings
        {
            Port = 587,
            DomainName = "smtp.test.local",
            BannerMessage = "Test SMTP Server"
        };

        // Act
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(587, server.Port);
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    [Theory]
    [InlineData(25, "mail.example.com")]
    [InlineData(587, "smtp.example.com")]
    [InlineData(465, "smtps.example.com")]
    public void Port_WithDifferentValues_ShouldReturnCorrectPort(int port, string domain)
    {
        // Arrange
        var settings = new SmtpServerSettings
        {
            Port = port,
            DomainName = domain
        };

        // Act
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(port, server.Port);
    }

    [Fact]
    public void ServerType_ShouldBeSmtp()
    {
        // Arrange
        var settings = new SmtpServerSettings { Port = 25 };

        // Act
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(ServerType.Smtp, server.Type);
    }

    [Fact]
    public void Name_ReturnsSmtpServer()
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal("SmtpServer", server.Name);
    }

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("127.0.0.1")]
    [InlineData("192.168.1.100")]
    public void BindAddress_WithDifferentValues_ReturnsCorrectValue(string address)
    {
        // Arrange
        var settings = new SmtpServerSettings { BindAddress = address };

        // Act
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(address, server.BindAddress);
    }

    #endregion

    #region Status Tests

    [Fact]
    public void Status_Initially_IsStopped()
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var settings = new SmtpServerSettings();
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Act & Assert - should not throw
        server.Dispose();
        server.Dispose();
    }

    [Fact]
    public void Dispose_DisposesConnectionLimiter()
    {
        // Arrange
        var settings = new SmtpServerSettings();
        var server = new SmtpServer(_mockLogger.Object, settings);

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
        var settings = new SmtpServerSettings();
        var server = new SmtpServer(_mockLogger.Object, settings);

        // Act
        var stats = server.GetStatistics();

        // Assert
        Assert.NotNull(stats);
    }

    #endregion
}

/// <summary>
/// SmtpServerSettingsのユニットテスト
/// </summary>
public class SmtpServerSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultSettings_HaveCorrectBasicValues()
    {
        // Act
        var settings = new SmtpServerSettings();

        // Assert
        Assert.False(settings.Enabled);
        Assert.Equal("0.0.0.0", settings.BindAddress);
        Assert.Equal(25, settings.Port);
        Assert.Equal(30, settings.TimeOut);
        Assert.Equal(10, settings.MaxConnections);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectMessageValues()
    {
        // Act
        var settings = new SmtpServerSettings();

        // Assert
        Assert.Equal("", settings.DomainName);
        Assert.Equal("$d SMTP Server Ready", settings.BannerMessage);
        Assert.Equal(5000, settings.SizeLimit);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectEsmtpValues()
    {
        // Act
        var settings = new SmtpServerSettings();

        // Assert
        Assert.False(settings.UseEsmtp);
        Assert.False(settings.UseAuthCramMd5);
        Assert.False(settings.UseAuthPlain);
        Assert.False(settings.UseAuthLogin);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectRelayValues()
    {
        // Act
        var settings = new SmtpServerSettings();

        // Assert
        Assert.Equal(0, settings.Order);
        Assert.Empty(settings.AllowList);
        Assert.Empty(settings.DenyList);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectQueueValues()
    {
        // Act
        var settings = new SmtpServerSettings();

        // Assert
        Assert.False(settings.Always);
        Assert.Equal(300, settings.ThreadSpan);
        Assert.Equal(5, settings.RetryMax);
        Assert.Equal(5, settings.ThreadMax);
    }

    #endregion

    #region Property Setter Tests

    [Theory]
    [InlineData(25)]
    [InlineData(587)]
    [InlineData(465)]
    [InlineData(2525)]
    public void Port_CanBeSet(int port)
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        settings.Port = port;

        // Assert
        Assert.Equal(port, settings.Port);
    }

    [Theory]
    [InlineData("mail.example.com")]
    [InlineData("smtp.local")]
    [InlineData("mx1.company.org,mx2.company.org")]
    public void DomainName_CanBeSet(string domain)
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        settings.DomainName = domain;

        // Assert
        Assert.Equal(domain, settings.DomainName);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(5000)]
    [InlineData(10000)]
    [InlineData(50000)]
    public void SizeLimit_CanBeSet(int limit)
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        settings.SizeLimit = limit;

        // Assert
        Assert.Equal(limit, settings.SizeLimit);
    }

    [Theory]
    [InlineData("Welcome to SMTP")]
    [InlineData("$d SMTP Server Ready")]
    [InlineData("Mail Server $v")]
    public void BannerMessage_CanBeSet(string banner)
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        settings.BannerMessage = banner;

        // Assert
        Assert.Equal(banner, settings.BannerMessage);
    }

    [Fact]
    public void EsmtpFlags_CanBeSet()
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        settings.UseEsmtp = true;
        settings.UseAuthCramMd5 = true;
        settings.UseAuthPlain = true;
        settings.UseAuthLogin = true;

        // Assert
        Assert.True(settings.UseEsmtp);
        Assert.True(settings.UseAuthCramMd5);
        Assert.True(settings.UseAuthPlain);
        Assert.True(settings.UseAuthLogin);
    }

    [Fact]
    public void PopBeforeSmtpSettings_CanBeSet()
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        settings.UsePopBeforeSmtp = true;
        settings.TimePopBeforeSmtp = 1200;

        // Assert
        Assert.True(settings.UsePopBeforeSmtp);
        Assert.Equal(1200, settings.TimePopBeforeSmtp);
    }

    [Fact]
    public void QueueSettings_CanBeSet()
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        settings.Always = true;
        settings.ThreadSpan = 600;
        settings.RetryMax = 10;
        settings.ThreadMax = 10;
        settings.MxOnly = true;

        // Assert
        Assert.True(settings.Always);
        Assert.Equal(600, settings.ThreadSpan);
        Assert.Equal(10, settings.RetryMax);
        Assert.Equal(10, settings.ThreadMax);
        Assert.True(settings.MxOnly);
    }

    [Fact]
    public void RelayLists_CanBeModified()
    {
        // Arrange
        var settings = new SmtpServerSettings();

        // Act
        settings.AllowList.Add("192.168.1.0/24");
        settings.AllowList.Add("10.0.0.0/8");
        settings.DenyList.Add("0.0.0.0/0");

        // Assert
        Assert.Equal(2, settings.AllowList.Count);
        Assert.Single(settings.DenyList);
        Assert.Contains("192.168.1.0/24", settings.AllowList);
    }

    #endregion

    #region ESMTP User List Tests

    [Fact]
    public void EsmtpUserList_CanBeModified()
    {
        // Arrange
        var settings = new SmtpServerSettings();
        var user = new SmtpUserEntry { UserName = "testuser", Password = "testpass" };

        // Act
        settings.EsmtpUserList.Add(user);

        // Assert
        Assert.Single(settings.EsmtpUserList);
        Assert.Equal("testuser", settings.EsmtpUserList[0].UserName);
    }

    #endregion
}
