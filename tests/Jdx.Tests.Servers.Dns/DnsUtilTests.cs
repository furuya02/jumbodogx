using Jdx.Core.Settings;
using Jdx.Servers.Dns;

namespace Jdx.Tests.Servers.Dns;

/// <summary>
/// DNS Utility Tests
/// Tests for DNS name format conversion and utility methods
/// </summary>
public class DnsUtilTests
{
    [Fact]
    public void Str2DnsName_SimpleName_ShouldConvertCorrectly()
    {
        // Arrange
        var name = "example.com";

        // Act
        var result = DnsUtil.Str2DnsName(name);

        // Assert
        // [7]example[3]com[0]
        Assert.Equal(7, result[0]); // Length of "example"
        Assert.Equal((byte)'e', result[1]);
        Assert.Equal((byte)'x', result[2]);
        Assert.Equal((byte)'a', result[3]);
        Assert.Equal((byte)'m', result[4]);
        Assert.Equal((byte)'p', result[5]);
        Assert.Equal((byte)'l', result[6]);
        Assert.Equal((byte)'e', result[7]);
        Assert.Equal(3, result[8]); // Length of "com"
        Assert.Equal((byte)'c', result[9]);
        Assert.Equal((byte)'o', result[10]);
        Assert.Equal((byte)'m', result[11]);
        Assert.Equal(0, result[12]); // Null terminator
    }

    [Fact]
    public void Str2DnsName_WithTrailingDot_ShouldRemoveDotAndConvert()
    {
        // Arrange
        var name = "example.com.";

        // Act
        var result = DnsUtil.Str2DnsName(name);

        // Assert
        // Should be same as without trailing dot
        Assert.Equal(13, result.Length); // 7+1+3+1+1 = 13
        Assert.Equal(0, result[12]); // Null terminator
    }

    [Fact]
    public void Str2DnsName_EmptyString_ShouldReturnNullTerminator()
    {
        // Arrange
        var name = "";

        // Act
        var result = DnsUtil.Str2DnsName(name);

        // Assert
        Assert.Single(result);
        Assert.Equal(0, result[0]);
    }

    [Fact]
    public void Str2DnsName_Null_ShouldReturnNullTerminator()
    {
        // Arrange
        string? name = null;

        // Act
        var result = DnsUtil.Str2DnsName(name!);

        // Assert
        Assert.Single(result);
        Assert.Equal(0, result[0]);
    }

    [Fact]
    public void DnsName2Str_SimpleName_ShouldConvertBack()
    {
        // Arrange
        var original = "example.com";
        var dnsName = DnsUtil.Str2DnsName(original);

        // Act
        var result = DnsUtil.DnsName2Str(dnsName);

        // Assert
        Assert.Equal(original + ".", result);
    }

    [Fact]
    public void DnsName2Str_RoundTrip_ShouldPreserveName()
    {
        // Arrange
        var names = new[] { "example.com", "www.example.com", "test.local", "a.b.c.d.e" };

        foreach (var name in names)
        {
            // Act
            var dnsName = DnsUtil.Str2DnsName(name);
            var result = DnsUtil.DnsName2Str(dnsName);

            // Assert
            Assert.Equal(name + ".", result);
        }
    }

    [Fact]
    public void Short2DnsType_ValidTypes_ShouldConvertCorrectly()
    {
        // Act & Assert
        Assert.Equal(DnsType.A, DnsUtil.Short2DnsType(1));
        Assert.Equal(DnsType.Ns, DnsUtil.Short2DnsType(2));
        Assert.Equal(DnsType.Cname, DnsUtil.Short2DnsType(5));
        Assert.Equal(DnsType.Soa, DnsUtil.Short2DnsType(6));
        Assert.Equal(DnsType.Ptr, DnsUtil.Short2DnsType(12));
        Assert.Equal(DnsType.Mx, DnsUtil.Short2DnsType(15));
        Assert.Equal(DnsType.Aaaa, DnsUtil.Short2DnsType(28));
    }

    [Fact]
    public void Short2DnsType_InvalidType_ShouldReturnZero()
    {
        // Act
        var result = DnsUtil.Short2DnsType(999);

        // Assert
        Assert.Equal((DnsType)0, result); // Default to 0 (unknown)
    }

    [Fact]
    public void Str2DnsName_Subdomain_ShouldHandleMultipleLevels()
    {
        // Arrange
        var name = "www.mail.example.com";

        // Act
        var result = DnsUtil.Str2DnsName(name);

        // Assert
        // [3]www[4]mail[7]example[3]com[0]
        Assert.Equal(3, result[0]); // Length of "www"
        Assert.Equal(4, result[4]); // Length of "mail"
        Assert.Equal(7, result[9]); // Length of "example"
        Assert.Equal(3, result[17]); // Length of "com"
        Assert.Equal(0, result[21]); // Null terminator
    }

    [Fact]
    public void Str2DnsName_SingleLabel_ShouldConvertCorrectly()
    {
        // Arrange
        var name = "localhost";

        // Act
        var result = DnsUtil.Str2DnsName(name);

        // Assert
        // [9]localhost[0]
        Assert.Equal(9, result[0]); // Length of "localhost"
        Assert.Equal((byte)'l', result[1]);
        Assert.Equal((byte)'o', result[2]);
        Assert.Equal((byte)'c', result[3]);
        Assert.Equal((byte)'a', result[4]);
        Assert.Equal((byte)'l', result[5]);
        Assert.Equal((byte)'h', result[6]);
        Assert.Equal((byte)'o', result[7]);
        Assert.Equal((byte)'s', result[8]);
        Assert.Equal((byte)'t', result[9]);
        Assert.Equal(0, result[10]); // Null terminator
    }

    [Fact]
    public void DnsName2Str_RootDomain_ShouldReturnEmptyString()
    {
        // Arrange
        var rootDomain = new byte[] { 0 };

        // Act
        var result = DnsUtil.DnsName2Str(rootDomain);

        // Assert
        Assert.Equal("", result);
    }
}
