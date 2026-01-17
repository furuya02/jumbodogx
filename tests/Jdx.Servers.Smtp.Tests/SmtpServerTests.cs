using Jdx.Core.Abstractions;
using Jdx.Core.Settings;
using Jdx.Servers.Smtp;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Servers.Smtp.Tests;

public class SmtpServerTests
{
    [Fact]
    public void Constructor_ShouldInitializeServer()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SmtpServer>>();
        var settings = new SmtpServerSettings
        {
            Port = 25,
            DomainName = "mail.example.com",
            UseEsmtp = true
        };

        // Act
        var server = new SmtpServer(mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal("SmtpServer", server.Name);
        Assert.Equal(ServerType.Smtp, server.Type);
        Assert.Equal(25, server.Port);
    }

    [Fact]
    public void Properties_ShouldReturnCorrectValues()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SmtpServer>>();
        var settings = new SmtpServerSettings
        {
            Port = 587,
            DomainName = "smtp.test.local",
            BannerMessage = "Test SMTP Server"
        };

        // Act
        var server = new SmtpServer(mockLogger.Object, settings);

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
        var mockLogger = new Mock<ILogger<SmtpServer>>();
        var settings = new SmtpServerSettings
        {
            Port = port,
            DomainName = domain
        };

        // Act
        var server = new SmtpServer(mockLogger.Object, settings);

        // Assert
        Assert.Equal(port, server.Port);
    }

    [Fact]
    public void ServerType_ShouldBeSmtp()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SmtpServer>>();
        var settings = new SmtpServerSettings { Port = 25 };

        // Act
        var server = new SmtpServer(mockLogger.Object, settings);

        // Assert
        Assert.Equal(ServerType.Smtp, server.Type);
    }
}
