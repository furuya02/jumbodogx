using System.Net;
using Jdx.Core.Settings;
using Jdx.Servers.Dns;

namespace Jdx.Tests.Servers.Dns;

/// <summary>
/// DNS Resource Record Tests
/// Tests for individual record types (A, AAAA, NS, CNAME, MX, PTR, SOA)
/// </summary>
public class DnsRecordTests
{
    [Fact]
    public void RrA_Creation_ShouldStoreIPv4Address()
    {
        // Arrange
        var name = "example.com.";
        var ip = IPAddress.Parse("192.0.2.1");
        uint ttl = 3600;

        // Act
        var record = new RrA(name, ttl, ip);

        // Assert
        Assert.Equal(name, record.Name);
        Assert.Equal(DnsType.A, record.DnsType);
        Assert.Equal(ttl, record.Ttl);
        Assert.Equal(ip, record.Ip);
    }

    [Fact]
    public void RrAaaa_Creation_ShouldStoreIPv6Address()
    {
        // Arrange
        var name = "example.com.";
        var ip = IPAddress.Parse("2001:db8::1");
        uint ttl = 3600;

        // Act
        var record = new RrAaaa(name, ttl, ip);

        // Assert
        Assert.Equal(name, record.Name);
        Assert.Equal(DnsType.Aaaa, record.DnsType);
        Assert.Equal(ttl, record.Ttl);
        Assert.Equal(ip, record.Ip);
    }

    [Fact]
    public void RrNs_Creation_ShouldStoreNameServer()
    {
        // Arrange
        var name = "example.com.";
        var nsName = "ns1.example.com.";
        uint ttl = 3600;

        // Act
        var record = new RrNs(name, ttl, nsName);

        // Assert
        Assert.Equal(name, record.Name);
        Assert.Equal(DnsType.Ns, record.DnsType);
        Assert.Equal(ttl, record.Ttl);
        Assert.Equal(nsName, record.NsName);
    }

    [Fact]
    public void RrCname_Creation_ShouldStoreCanonicalName()
    {
        // Arrange
        var name = "www.example.com.";
        var cname = "example.com.";
        uint ttl = 3600;

        // Act
        var record = new RrCname(name, ttl, cname);

        // Assert
        Assert.Equal(name, record.Name);
        Assert.Equal(DnsType.Cname, record.DnsType);
        Assert.Equal(ttl, record.Ttl);
        Assert.Equal(cname, record.CName);
    }

    [Fact]
    public void RrMx_Creation_ShouldStoreMailExchangeData()
    {
        // Arrange
        var name = "example.com.";
        var mailHost = "mail.example.com.";
        ushort preference = 10;
        uint ttl = 3600;

        // Act
        var record = new RrMx(name, ttl, preference, mailHost);

        // Assert
        Assert.Equal(name, record.Name);
        Assert.Equal(DnsType.Mx, record.DnsType);
        Assert.Equal(ttl, record.Ttl);
        Assert.Equal(preference, record.Preference);
        Assert.Equal(mailHost, record.MailExchangeHost);
    }

    [Fact]
    public void RrPtr_Creation_ShouldStorePointer()
    {
        // Arrange
        var name = "1.2.0.192.in-addr.arpa.";
        var ptr = "example.com.";
        uint ttl = 3600;

        // Act
        var record = new RrPtr(name, ttl, ptr);

        // Assert
        Assert.Equal(name, record.Name);
        Assert.Equal(DnsType.Ptr, record.DnsType);
        Assert.Equal(ttl, record.Ttl);
        Assert.Equal(ptr, record.Ptr);
    }

    [Fact]
    public void RrSoa_Creation_ShouldStoreStartOfAuthority()
    {
        // Arrange
        var name = "example.com.";
        var nameServer = "ns1.example.com.";
        var postMaster = "postmaster.example.com.";
        uint serial = 2024011701;
        uint refresh = 3600;
        uint retry = 300;
        uint expire = 360000;
        uint minimum = 3600;
        uint ttl = 3600;

        // Act
        var record = new RrSoa(name, ttl, nameServer, postMaster, serial, refresh, retry, expire, minimum);

        // Assert
        Assert.Equal(name, record.Name);
        Assert.Equal(DnsType.Soa, record.DnsType);
        Assert.Equal(ttl, record.Ttl);
        Assert.Equal(nameServer, record.NameServer);
        Assert.Equal(postMaster, record.PostMaster);
        Assert.Equal(serial, record.Serial);
        Assert.Equal(refresh, record.Refresh);
        Assert.Equal(retry, record.Retry);
        Assert.Equal(expire, record.Expire);
        Assert.Equal(minimum, record.Minimum);
    }

    [Fact]
    public void OneRr_IsEffective_ShouldReturnTrueForValidTTL()
    {
        // Arrange
        var record = new RrA("example.com.", 3600, IPAddress.Parse("192.0.2.1"));
        var now = DateTime.Now.Ticks / 10000000;

        // Act
        var result = record.IsEffective(now);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OneRr_IsEffective_ShouldReturnTrueForPermanentRecord()
    {
        // Arrange
        var record = new RrA("example.com.", 0, IPAddress.Parse("192.0.2.1"));
        var now = DateTime.Now.Ticks / 10000000;

        // Act
        var result = record.IsEffective(now);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OneRr_Clone_ShouldCreateNewRecordWithDifferentTTL()
    {
        // Arrange
        var original = new RrA("example.com.", 0, IPAddress.Parse("192.0.2.1"));
        uint newTtl = 3600;

        // Act
        var cloned = original.Clone(newTtl);

        // Assert
        Assert.NotSame(original, cloned);
        Assert.Equal(original.Name, cloned.Name);
        Assert.Equal(original.DnsType, cloned.DnsType);
        Assert.Equal(newTtl, cloned.Ttl);
    }

    [Fact]
    public void OneRr_Equals_ShouldReturnTrueForIdenticalRecords()
    {
        // Arrange
        var record1 = new RrA("example.com.", 3600, IPAddress.Parse("192.0.2.1"));
        var record2 = new RrA("example.com.", 3600, IPAddress.Parse("192.0.2.1"));

        // Act
        var result = record1.Equals(record2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OneRr_Equals_ShouldReturnFalseForDifferentRecords()
    {
        // Arrange
        var record1 = new RrA("example.com.", 3600, IPAddress.Parse("192.0.2.1"));
        var record2 = new RrA("example.com.", 3600, IPAddress.Parse("192.0.2.2"));

        // Act
        var result = record1.Equals(record2);

        // Assert
        Assert.False(result);
    }
}
