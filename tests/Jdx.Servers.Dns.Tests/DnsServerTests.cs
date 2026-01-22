using Jdx.Core.Abstractions;
using Jdx.Core.Settings;
using Jdx.Servers.Dns;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Servers.Dns.Tests;

/// <summary>
/// DnsServerのユニットテスト
/// </summary>
public class DnsServerTests
{
    private readonly Mock<ILogger<DnsServer>> _mockLogger;

    public DnsServerTests()
    {
        _mockLogger = new Mock<ILogger<DnsServer>>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesServerWithBasicSettings()
    {
        // Arrange
        var settings = new DnsServerSettings
        {
            Port = 53,
            DomainList = new List<DnsDomainEntry>(),
            ResourceList = new List<DnsResourceEntry>()
        };

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

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
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    [Fact]
    public void Constructor_WithDefaultSettings_InitializesCorrectly()
    {
        // Arrange
        var settings = new DnsServerSettings();

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        Assert.NotNull(server);
        Assert.Equal(53, server.Port);
        Assert.Equal("0.0.0.0", server.BindAddress);
    }

    [Fact]
    public void Constructor_WithMultipleDomains_InitializesAll()
    {
        // Arrange
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
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("2 domains")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_ReturnCorrectValues()
    {
        // Arrange
        var settings = new DnsServerSettings
        {
            Port = 5353,
            DomainList = new List<DnsDomainEntry>(),
            ResourceList = new List<DnsResourceEntry>()
        };

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(5353, server.Port);
        Assert.Equal(ServerStatus.Stopped, server.Status);
        Assert.Equal(ServerType.Dns, server.Type);
    }

    [Theory]
    [InlineData(53)]
    [InlineData(5353)]
    [InlineData(10053)]
    public void Port_WithDifferentValues_ReturnsCorrectValue(int port)
    {
        // Arrange
        var settings = new DnsServerSettings { Port = port };

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(port, server.Port);
    }

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("127.0.0.1")]
    [InlineData("192.168.1.100")]
    public void BindAddress_WithDifferentValues_ReturnsCorrectValue(string address)
    {
        // Arrange
        var settings = new DnsServerSettings { BindAddress = address };

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(address, server.BindAddress);
    }

    #endregion

    #region AddRecord Tests

    [Fact]
    public void AddRecord_AddsRecordToCache()
    {
        // Arrange
        var settings = new DnsServerSettings
        {
            Port = 53,
            DomainList = new List<DnsDomainEntry>
            {
                new DnsDomainEntry { Name = "test.local.", IsAuthority = true }
            },
            ResourceList = new List<DnsResourceEntry>()
        };
        var server = new DnsServer(_mockLogger.Object, settings);

        // Act
        server.AddRecord("host.test.local.", "10.0.0.1");

        // Assert - no exception thrown
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
    public void AddRecord_WithInvalidIp_LogsWarning()
    {
        // Arrange
        var settings = new DnsServerSettings
        {
            Port = 53,
            DomainList = new List<DnsDomainEntry>
            {
                new DnsDomainEntry { Name = "test.local.", IsAuthority = true }
            },
            ResourceList = new List<DnsResourceEntry>()
        };
        var server = new DnsServer(_mockLogger.Object, settings);

        // Act
        server.AddRecord("host.test.local.", "invalid-ip");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid IP address")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Status Tests

    [Fact]
    public void Status_Initially_IsStopped()
    {
        // Arrange
        var settings = new DnsServerSettings();

        // Act
        var server = new DnsServer(_mockLogger.Object, settings);

        // Assert
        Assert.Equal(ServerStatus.Stopped, server.Status);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public void GetStatistics_ReturnsValidStatistics()
    {
        // Arrange
        var settings = new DnsServerSettings();
        var server = new DnsServer(_mockLogger.Object, settings);

        // Act
        var stats = server.GetStatistics();

        // Assert
        Assert.NotNull(stats);
    }

    #endregion
}

/// <summary>
/// DnsServerSettingsのユニットテスト
/// </summary>
public class DnsServerSettingsTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultSettings_HaveCorrectBasicValues()
    {
        // Act
        var settings = new DnsServerSettings();

        // Assert
        Assert.False(settings.Enabled);
        Assert.Equal(53, settings.Port);
        Assert.Equal("0.0.0.0", settings.BindAddress);
        Assert.Equal(10, settings.MaxConnections);
        Assert.Equal(30, settings.TimeOut);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectSoaValues()
    {
        // Act
        var settings = new DnsServerSettings();

        // Assert
        Assert.Equal("postmaster", settings.SoaMail);
        Assert.Equal(1, settings.SoaSerial);
        Assert.Equal(3600, settings.SoaRefresh);
        Assert.Equal(300, settings.SoaRetry);
        Assert.Equal(360000, settings.SoaExpire);
        Assert.Equal(3600, settings.SoaMinimum);
    }

    [Fact]
    public void DefaultSettings_HaveEmptyLists()
    {
        // Act
        var settings = new DnsServerSettings();

        // Assert
        Assert.Empty(settings.DomainList);
        Assert.Empty(settings.ResourceList);
    }

    [Fact]
    public void DefaultSettings_HaveCorrectCacheSettings()
    {
        // Act
        var settings = new DnsServerSettings();

        // Assert
        Assert.Equal("named.ca", settings.RootCache);
        Assert.True(settings.UseRecursion);
    }

    #endregion

    #region Property Setter Tests

    [Theory]
    [InlineData(53)]
    [InlineData(5353)]
    [InlineData(10053)]
    public void Port_CanBeSet(int port)
    {
        // Arrange
        var settings = new DnsServerSettings();

        // Act
        settings.Port = port;

        // Assert
        Assert.Equal(port, settings.Port);
    }

    [Theory]
    [InlineData("admin@example.com")]
    [InlineData("hostmaster")]
    [InlineData("postmaster")]
    public void SoaMail_CanBeSet(string mail)
    {
        // Arrange
        var settings = new DnsServerSettings();

        // Act
        settings.SoaMail = mail;

        // Assert
        Assert.Equal(mail, settings.SoaMail);
    }

    [Fact]
    public void SoaValues_CanBeSet()
    {
        // Arrange
        var settings = new DnsServerSettings();

        // Act
        settings.SoaSerial = 100;
        settings.SoaRefresh = 7200;
        settings.SoaRetry = 600;
        settings.SoaExpire = 720000;
        settings.SoaMinimum = 7200;

        // Assert
        Assert.Equal(100, settings.SoaSerial);
        Assert.Equal(7200, settings.SoaRefresh);
        Assert.Equal(600, settings.SoaRetry);
        Assert.Equal(720000, settings.SoaExpire);
        Assert.Equal(7200, settings.SoaMinimum);
    }

    [Fact]
    public void RecursionFlag_CanBeSet()
    {
        // Arrange
        var settings = new DnsServerSettings();

        // Act
        settings.UseRecursion = false;

        // Assert
        Assert.False(settings.UseRecursion);
    }

    #endregion

    #region Domain List Tests

    [Fact]
    public void DomainList_CanBeModified()
    {
        // Arrange
        var settings = new DnsServerSettings();
        var domain = new DnsDomainEntry { Name = "example.com.", IsAuthority = true };

        // Act
        settings.DomainList.Add(domain);

        // Assert
        Assert.Single(settings.DomainList);
        Assert.Equal("example.com.", settings.DomainList[0].Name);
        Assert.True(settings.DomainList[0].IsAuthority);
    }

    [Fact]
    public void DomainList_CanHaveMultipleDomains()
    {
        // Arrange
        var settings = new DnsServerSettings();

        // Act
        settings.DomainList.Add(new DnsDomainEntry { Name = "example.com.", IsAuthority = true });
        settings.DomainList.Add(new DnsDomainEntry { Name = "test.local.", IsAuthority = false });
        settings.DomainList.Add(new DnsDomainEntry { Name = "internal.net.", IsAuthority = true });

        // Assert
        Assert.Equal(3, settings.DomainList.Count);
    }

    #endregion

    #region Resource List Tests

    [Fact]
    public void ResourceList_CanAddARecord()
    {
        // Arrange
        var settings = new DnsServerSettings();
        var record = new DnsResourceEntry
        {
            Type = DnsType.A,
            Name = "www.example.com.",
            Address = "192.168.1.100"
        };

        // Act
        settings.ResourceList.Add(record);

        // Assert
        Assert.Single(settings.ResourceList);
        Assert.Equal(DnsType.A, settings.ResourceList[0].Type);
        Assert.Equal("192.168.1.100", settings.ResourceList[0].Address);
    }

    [Fact]
    public void ResourceList_CanAddMxRecord()
    {
        // Arrange
        var settings = new DnsServerSettings();
        var record = new DnsResourceEntry
        {
            Type = DnsType.Mx,
            Name = "example.com.",
            Address = "mail.example.com.",
            Priority = 10
        };

        // Act
        settings.ResourceList.Add(record);

        // Assert
        Assert.Single(settings.ResourceList);
        Assert.Equal(DnsType.Mx, settings.ResourceList[0].Type);
        Assert.Equal(10, settings.ResourceList[0].Priority);
    }

    [Fact]
    public void ResourceList_CanAddCnameRecord()
    {
        // Arrange
        var settings = new DnsServerSettings();
        var record = new DnsResourceEntry
        {
            Type = DnsType.Cname,
            Name = "www.example.com.",
            Alias = "webserver.example.com."
        };

        // Act
        settings.ResourceList.Add(record);

        // Assert
        Assert.Single(settings.ResourceList);
        Assert.Equal(DnsType.Cname, settings.ResourceList[0].Type);
        Assert.Equal("webserver.example.com.", settings.ResourceList[0].Alias);
    }

    [Fact]
    public void ResourceList_CanAddAaaaRecord()
    {
        // Arrange
        var settings = new DnsServerSettings();
        var record = new DnsResourceEntry
        {
            Type = DnsType.Aaaa,
            Name = "ipv6.example.com.",
            Address = "2001:db8::1"
        };

        // Act
        settings.ResourceList.Add(record);

        // Assert
        Assert.Single(settings.ResourceList);
        Assert.Equal(DnsType.Aaaa, settings.ResourceList[0].Type);
    }

    #endregion
}

/// <summary>
/// DnsDomainEntryのユニットテスト
/// </summary>
public class DnsDomainEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new DnsDomainEntry();

        // Assert
        Assert.Equal("", entry.Name);
        Assert.False(entry.IsAuthority);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var entry = new DnsDomainEntry();

        // Act
        entry.Name = "example.com.";
        entry.IsAuthority = true;

        // Assert
        Assert.Equal("example.com.", entry.Name);
        Assert.True(entry.IsAuthority);
    }
}

/// <summary>
/// DnsResourceEntryのユニットテスト
/// </summary>
public class DnsResourceEntryTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Act
        var entry = new DnsResourceEntry();

        // Assert
        Assert.Equal(DnsType.A, entry.Type);
        Assert.Equal("", entry.Name);
        Assert.Equal("", entry.Alias);
        Assert.Equal("", entry.Address);
        Assert.Equal(0, entry.Priority);
    }

    [Theory]
    [InlineData(DnsType.A)]
    [InlineData(DnsType.Aaaa)]
    [InlineData(DnsType.Cname)]
    [InlineData(DnsType.Mx)]
    [InlineData(DnsType.Ns)]
    [InlineData(DnsType.Ptr)]
    [InlineData(DnsType.Soa)]
    public void Type_CanBeSetToAnyDnsType(DnsType type)
    {
        // Arrange
        var entry = new DnsResourceEntry();

        // Act
        entry.Type = type;

        // Assert
        Assert.Equal(type, entry.Type);
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        // Arrange
        var entry = new DnsResourceEntry();

        // Act
        entry.Type = DnsType.Mx;
        entry.Name = "example.com.";
        entry.Alias = "alias.example.com.";
        entry.Address = "mail.example.com.";
        entry.Priority = 10;

        // Assert
        Assert.Equal(DnsType.Mx, entry.Type);
        Assert.Equal("example.com.", entry.Name);
        Assert.Equal("alias.example.com.", entry.Alias);
        Assert.Equal("mail.example.com.", entry.Address);
        Assert.Equal(10, entry.Priority);
    }
}

/// <summary>
/// DnsTypeのユニットテスト
/// </summary>
public class DnsTypeTests
{
    [Fact]
    public void DnsType_HasCorrectValues()
    {
        // Assert
        Assert.Equal(1, (int)DnsType.A);
        Assert.Equal(2, (int)DnsType.Ns);
        Assert.Equal(5, (int)DnsType.Cname);
        Assert.Equal(6, (int)DnsType.Soa);
        Assert.Equal(12, (int)DnsType.Ptr);
        Assert.Equal(15, (int)DnsType.Mx);
        Assert.Equal(28, (int)DnsType.Aaaa);
    }
}
