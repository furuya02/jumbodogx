using Jdx.Core.Abstractions;
using Jdx.Core.Settings;
using Jdx.Servers.Dns;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Servers.Dns.Tests;

public class DnsServerTests
{
    [Fact]
    public void Constructor_InitializesServerWithBasicSettings()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DnsServer>>();
        var settings = new DnsServerSettings
        {
            Port = 53,
            DomainList = new List<DnsDomainEntry>(),
            ResourceList = new List<DnsResourceEntry>()
        };

        // Act
        var server = new DnsServer(mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal("DnsServer", server.Name);
        Assert.Equal(ServerType.Dns, server.Type);
        Assert.Equal(53, server.Port);
    }

    [Fact]
    public void Constructor_WithDomains_InitializesCaches()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DnsServer>>();
        var settings = new DnsServerSettings
        {
            Port = 53,
            DomainList = new List<DnsDomainEntry>
            {
                new DnsDomainEntry { Name = "example.com.", IsAuthority = true }
            },
            ResourceList = new List<DnsResourceEntry>
            {
                new DnsResourceEntry { Name = "www.example.com.", Type = DnsType.A, Address = "192.168.1.1" }
            }
        };

        // Act
        var server = new DnsServer(mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    [Fact]
    public void AddRecord_AddsRecordToCache()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DnsServer>>();
        var settings = new DnsServerSettings
        {
            Port = 53,
            DomainList = new List<DnsDomainEntry>
            {
                new DnsDomainEntry { Name = "test.local.", IsAuthority = true }
            },
            ResourceList = new List<DnsResourceEntry>()
        };
        var server = new DnsServer(mockLogger.Object, settings);

        // Act
        server.AddRecord("host.test.local.", "10.0.0.1");

        // Assert - no exception thrown
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added A record")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void AddRecord_WithInvalidIp_LogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DnsServer>>();
        var settings = new DnsServerSettings
        {
            Port = 53,
            DomainList = new List<DnsDomainEntry>
            {
                new DnsDomainEntry { Name = "test.local.", IsAuthority = true }
            },
            ResourceList = new List<DnsResourceEntry>()
        };
        var server = new DnsServer(mockLogger.Object, settings);

        // Act
        server.AddRecord("host.test.local.", "invalid-ip");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid IP address")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Properties_ReturnCorrectValues()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DnsServer>>();
        var settings = new DnsServerSettings
        {
            Port = 5353,
            DomainList = new List<DnsDomainEntry>(),
            ResourceList = new List<DnsResourceEntry>()
        };

        // Act
        var server = new DnsServer(mockLogger.Object, settings);

        // Assert
        Assert.Equal(5353, server.Port);
        Assert.Equal(ServerStatus.Stopped, server.Status);
        Assert.Equal(ServerType.Dns, server.Type);
    }

    [Fact]
    public void Constructor_WithMultipleDomains_InitializesAll()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DnsServer>>();
        var settings = new DnsServerSettings
        {
            Port = 53,
            DomainList = new List<DnsDomainEntry>
            {
                new DnsDomainEntry { Name = "example.com.", IsAuthority = true },
                new DnsDomainEntry { Name = "test.local.", IsAuthority = false }
            },
            ResourceList = new List<DnsResourceEntry>()
        };

        // Act
        var server = new DnsServer(mockLogger.Object, settings);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("2 domains")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
