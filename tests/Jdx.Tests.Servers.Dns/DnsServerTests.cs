using System.Net;
using Jdx.Core.Settings;
using Jdx.Servers.Dns;
using Microsoft.Extensions.Logging;
using Moq;

namespace Jdx.Tests.Servers.Dns;

/// <summary>
/// DNS Server Integration Tests
/// Tests for DnsServer with resource record database integration
/// </summary>
public class DnsServerTests
{
    private readonly Mock<ILogger<DnsServer>> _mockLogger;

    public DnsServerTests()
    {
        _mockLogger = new Mock<ILogger<DnsServer>>();
    }

    private DnsServerSettings CreateTestSettings(int port = 5353)
    {
        return new DnsServerSettings
        {
            Enabled = true,
            Port = port,
            MaxConnections = 10,
            TimeOut = 30,
            RootCache = "named.ca",
            UseRecursion = false,
            SoaMail = "postmaster",
            SoaSerial = 1,
            SoaRefresh = 3600,
            SoaRetry = 300,
            SoaExpire = 360000,
            SoaMinimum = 3600,
            DomainList = new List<DnsDomainEntry>
            {
                new() { Name = "test.local", IsAuthority = true }
            },
            ResourceList = new List<DnsResourceEntry>
            {
                new() { Type = DnsType.A, Name = "test.local", Address = "192.168.1.100" },
                new() { Type = DnsType.Aaaa, Name = "test.local", Address = "2001:db8::1" },
                new() { Type = DnsType.Ns, Name = "test.local", Address = "ns1.test.local" }
            }
        };
    }

    [Fact]
    public void DnsServer_Creation_ShouldInitializeSuccessfully()
    {
        // Arrange
        var settings = CreateTestSettings();

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal("DnsServer", server.Name);
        Assert.Equal(settings.Port, server.Port);
        Assert.NotNull(server);
    }

    [Fact]
    public void DnsServer_Initialization_ShouldLoadDomainsAndResources()
    {
        // Arrange
        var settings = CreateTestSettings();

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        // Verify logger was called with initialization message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("DNS Server initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void DnsServer_AddRecord_ShouldAddRecordToCorrectDomain()
    {
        // Arrange
        var settings = CreateTestSettings();
        var server = new DnsServer(_mockLogger.Object, settings);

        // Act
        server.AddRecord("www.test.local", "192.168.1.200");

        // Assert
        // Verify logger was called with add record message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added A record")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void DnsServer_AddRecord_NonExistentDomain_ShouldNotAdd()
    {
        // Arrange
        var settings = CreateTestSettings();
        var server = new DnsServer(_mockLogger.Object, settings);

        // Act
        server.AddRecord("example.com", "192.0.2.1");

        // Assert
        // Verify logger was NOT called with add record message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added A record")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void DnsServer_MultipleDomainsAndResources_ShouldOrganizeCorrectly()
    {
        // Arrange
        var settings = new DnsServerSettings
        {
            Enabled = true,
            Port = 5353,
            MaxConnections = 10,
            TimeOut = 30,
            RootCache = "named.ca",
            UseRecursion = false,
            SoaMail = "postmaster",
            SoaSerial = 1,
            SoaRefresh = 3600,
            SoaRetry = 300,
            SoaExpire = 360000,
            SoaMinimum = 3600,
            DomainList = new List<DnsDomainEntry>
            {
                new() { Name = "domain1.local", IsAuthority = true },
                new() { Name = "domain2.local", IsAuthority = false }
            },
            ResourceList = new List<DnsResourceEntry>
            {
                new() { Type = DnsType.A, Name = "www.domain1.local", Address = "192.168.1.1" },
                new() { Type = DnsType.A, Name = "www.domain2.local", Address = "192.168.2.1" },
                new() { Type = DnsType.Ns, Name = "domain1.local", Address = "ns1.domain1.local" },
                new() { Type = DnsType.Ns, Name = "domain2.local", Address = "ns1.domain2.local" }
            }
        };

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("2 domains") && v.ToString()!.Contains("4 resources")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void DnsServer_WithRootCache_ShouldLoadIfFileExists()
    {
        // Arrange
        var settings = CreateTestSettings();
        settings.RootCache = "nonexistent_named.ca";

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        // Should not throw, just log warning if file doesn't exist
        Assert.NotNull(server);
    }

    [Fact]
    public void DnsServer_EmptySettings_ShouldInitializeWithoutError()
    {
        // Arrange
        var settings = new DnsServerSettings
        {
            Enabled = true,
            Port = 5353,
            MaxConnections = 10,
            TimeOut = 30,
            RootCache = "named.ca",
            UseRecursion = false,
            SoaMail = "postmaster",
            SoaSerial = 1,
            SoaRefresh = 3600,
            SoaRetry = 300,
            SoaExpire = 360000,
            SoaMinimum = 3600,
            DomainList = new List<DnsDomainEntry>(),
            ResourceList = new List<DnsResourceEntry>()
        };

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("0 domains") && v.ToString()!.Contains("0 resources")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void DnsServer_MixedRecordTypes_ShouldLoadAllCorrectly()
    {
        // Arrange
        var settings = new DnsServerSettings
        {
            Enabled = true,
            Port = 5353,
            MaxConnections = 10,
            TimeOut = 30,
            RootCache = "named.ca",
            UseRecursion = false,
            SoaMail = "postmaster",
            SoaSerial = 1,
            SoaRefresh = 3600,
            SoaRetry = 300,
            SoaExpire = 360000,
            SoaMinimum = 3600,
            DomainList = new List<DnsDomainEntry>
            {
                new() { Name = "example.local", IsAuthority = true }
            },
            ResourceList = new List<DnsResourceEntry>
            {
                new() { Type = DnsType.A, Name = "www.example.local", Address = "192.168.1.1" },
                new() { Type = DnsType.Aaaa, Name = "www.example.local", Address = "2001:db8::1" },
                new() { Type = DnsType.Cname, Name = "alias.example.local", Alias = "www.example.local" },
                new() { Type = DnsType.Mx, Name = "example.local", Address = "mail.example.local", Priority = 10 },
                new() { Type = DnsType.Ns, Name = "example.local", Address = "ns1.example.local" },
                new() { Type = DnsType.Ptr, Name = "1.1.168.192.in-addr.arpa", Address = "www.example.local" }
            }
        };

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("1 domains") && v.ToString()!.Contains("6 resources")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void DnsServer_Dispose_ShouldNotThrow()
    {
        // Arrange
        var settings = CreateTestSettings();
        var server = new DnsServer(_mockLogger.Object, settings);

        // Act & Assert
        var exception = Record.Exception(() => server.Dispose());
        Assert.Null(exception);
    }
}
