using System.Net;
using Jdx.Core.Settings;
using Jdx.Servers.Dns;
using Microsoft.Extensions.Logging;
using Moq;

namespace Jdx.Tests.Servers.Dns;

/// <summary>
/// Resource Record Database Tests
/// Tests for RrDb (DNS record storage and retrieval)
/// </summary>
public class RrDbTests
{
    private readonly Mock<ILogger> _mockLogger;

    public RrDbTests()
    {
        _mockLogger = new Mock<ILogger>();
    }

    [Fact]
    public void RrDb_DefaultConstructor_ShouldInitializeWithExampleDomain()
    {
        // Act
        var db = new RrDb();

        // Assert
        Assert.Equal("example.com.", db.GetDomainName());
        Assert.True(db.Authority);
    }

    [Fact]
    public void RrDb_Add_ShouldAddRecordSuccessfully()
    {
        // Arrange
        var db = new RrDb();
        var record = new RrA("test.example.com.", 3600, IPAddress.Parse("192.0.2.1"));

        // Act
        var result = db.Add(record);

        // Assert
        Assert.True(result);
        Assert.Equal(1, db.Count);
    }

    [Fact]
    public void RrDb_Add_ShouldPreventDuplicates()
    {
        // Arrange
        var db = new RrDb();
        var record1 = new RrA("test.example.com.", 3600, IPAddress.Parse("192.0.2.1"));
        var record2 = new RrA("test.example.com.", 3600, IPAddress.Parse("192.0.2.1"));

        // Act
        var result1 = db.Add(record1);
        var result2 = db.Add(record2);

        // Assert
        Assert.True(result1);
        Assert.False(result2); // Duplicate should be rejected
        Assert.Equal(1, db.Count);
    }

    [Fact]
    public void RrDb_Find_ShouldReturnTrueForExistingRecord()
    {
        // Arrange
        var db = new RrDb();
        var record = new RrA("test.example.com.", 3600, IPAddress.Parse("192.0.2.1"));
        db.Add(record);

        // Act
        var result = db.Find("test.example.com", DnsType.A);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RrDb_Find_ShouldReturnFalseForNonExistingRecord()
    {
        // Arrange
        var db = new RrDb();

        // Act
        var result = db.Find("nonexistent.example.com", DnsType.A);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RrDb_GetList_ShouldReturnMatchingRecords()
    {
        // Arrange
        var db = new RrDb();
        var record1 = new RrA("test.example.com.", 3600, IPAddress.Parse("192.0.2.1"));
        var record2 = new RrA("test.example.com.", 3600, IPAddress.Parse("192.0.2.2"));
        var record3 = new RrA("other.example.com.", 3600, IPAddress.Parse("192.0.2.3"));
        db.Add(record1);
        db.Add(record2);
        db.Add(record3);

        // Act
        var results = db.GetList("test.example.com", DnsType.A);

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void RrDb_GetList_ShouldReturnEmptyForNonExistingRecord()
    {
        // Arrange
        var db = new RrDb();

        // Act
        var results = db.GetList("nonexistent.example.com", DnsType.A);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void RrDb_GetList_ShouldCloneRecordsWithZeroTTL()
    {
        // Arrange
        var db = new RrDb();
        var record = new RrA("test.example.com.", 0, IPAddress.Parse("192.0.2.1"));
        db.Add(record);

        // Act
        var results = db.GetList("test.example.com", DnsType.A);

        // Assert
        Assert.Single(results);
        Assert.NotEqual(0u, results[0].Ttl); // Should be cloned with non-zero TTL
    }

    [Fact]
    public void RrDb_GetList_ShouldHandleDotSuffix()
    {
        // Arrange
        var db = new RrDb();
        var record = new RrA("test.example.com.", 3600, IPAddress.Parse("192.0.2.1"));
        db.Add(record);

        // Act
        var results1 = db.GetList("test.example.com", DnsType.A); // Without dot
        var results2 = db.GetList("test.example.com.", DnsType.A); // With dot

        // Assert
        Assert.Single(results1);
        Assert.Single(results2);
    }

    [Fact]
    public void RrDb_ResourceConstructor_ShouldLoadResourcesFromSettings()
    {
        // Arrange
        var settings = new DnsServerSettings
        {
            SoaExpire = 360000,
            SoaMail = "postmaster",
            SoaSerial = 1,
            SoaRefresh = 3600,
            SoaRetry = 300,
            SoaMinimum = 3600
        };

        var resources = new List<DnsResourceEntry>
        {
            new() { Type = DnsType.A, Name = "test.example.com.", Address = "192.0.2.1" },
            new() { Type = DnsType.Aaaa, Name = "test.example.com.", Address = "2001:db8::1" },
            new() { Type = DnsType.Ns, Name = "example.com.", Address = "ns1.example.com." }
        };

        // Act
        var db = new RrDb(_mockLogger.Object, resources, "example.com", true, settings);

        // Assert
        Assert.Equal("example.com.", db.GetDomainName());
        Assert.True(db.Authority);
        Assert.True(db.Count > 0);
    }

    [Fact]
    public void RrDb_ResourceConstructor_ShouldInitializeSOARecordWhenNSExists()
    {
        // Arrange
        var settings = new DnsServerSettings
        {
            SoaExpire = 360000,
            SoaMail = "postmaster",
            SoaSerial = 1,
            SoaRefresh = 3600,
            SoaRetry = 300,
            SoaMinimum = 3600
        };

        var resources = new List<DnsResourceEntry>
        {
            new() { Type = DnsType.Ns, Name = "example.com.", Address = "ns1.example.com." }
        };

        // Act
        var db = new RrDb(_mockLogger.Object, resources, "example.com", true, settings);

        // Assert
        var soaRecords = db.GetList("example.com", DnsType.Soa);
        Assert.NotEmpty(soaRecords);
    }

    [Fact]
    public void RrDb_CaseInsensitive_ShouldMatchRecordsRegardlessOfCase()
    {
        // Arrange
        var db = new RrDb();
        var record = new RrA("Test.Example.Com.", 3600, IPAddress.Parse("192.0.2.1"));
        db.Add(record);

        // Act
        var result1 = db.Find("test.example.com", DnsType.A);
        var result2 = db.Find("TEST.EXAMPLE.COM", DnsType.A);
        var result3 = db.Find("TeSt.ExAmPlE.cOm", DnsType.A);

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
    }

    [Fact]
    public void RrDb_MultipleRecordTypes_ShouldStoreAndRetrieveCorrectly()
    {
        // Arrange
        var db = new RrDb();
        var recordA = new RrA("test.example.com.", 3600, IPAddress.Parse("192.0.2.1"));
        var recordAAAA = new RrAaaa("test.example.com.", 3600, IPAddress.Parse("2001:db8::1"));
        var recordCNAME = new RrCname("www.example.com.", 3600, "test.example.com.");
        db.Add(recordA);
        db.Add(recordAAAA);
        db.Add(recordCNAME);

        // Act
        var aRecords = db.GetList("test.example.com", DnsType.A);
        var aaaaRecords = db.GetList("test.example.com", DnsType.Aaaa);
        var cnameRecords = db.GetList("www.example.com", DnsType.Cname);

        // Assert
        Assert.Single(aRecords);
        Assert.Single(aaaaRecords);
        Assert.Single(cnameRecords);
        Assert.Equal(3, db.Count);
    }
}
