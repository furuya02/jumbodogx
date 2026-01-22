using System.Net;
using Jdx.Core.Network;

namespace Jdx.Core.Tests.Network;

/// <summary>
/// IpAddressMatcherのユニットテスト
/// </summary>
public class IpAddressMatcherTests
{
    #region Matches メソッド

    [Fact]
    public void Matches_NullIpAddress_ReturnsFalse()
    {
        // Arrange & Act
        var result = IpAddressMatcher.Matches(null!, "192.168.1.1");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Matches_NullOrEmptyPattern_ReturnsFalse(string? pattern)
    {
        // Arrange
        var ip = IPAddress.Parse("192.168.1.1");

        // Act
        var result = IpAddressMatcher.Matches(ip, pattern!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("192.168.1.1", "192.168.1.1", true)]
    [InlineData("192.168.1.1", "192.168.1.2", false)]
    [InlineData("10.0.0.1", "10.0.0.1", true)]
    [InlineData("255.255.255.255", "255.255.255.255", true)]
    [InlineData("0.0.0.0", "0.0.0.0", true)]
    public void Matches_SingleIPv4Address_ReturnsExpected(string ipStr, string pattern, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.Matches(ip, pattern);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("::1", "::1", true)]
    [InlineData("::1", "::2", false)]
    [InlineData("2001:db8::1", "2001:db8::1", true)]
    [InlineData("fe80::1", "fe80::1", true)]
    public void Matches_SingleIPv6Address_ReturnsExpected(string ipStr, string pattern, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.Matches(ip, pattern);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Matches_InvalidPattern_ReturnsFalse()
    {
        // Arrange
        var ip = IPAddress.Parse("192.168.1.1");

        // Act
        var result = IpAddressMatcher.Matches(ip, "invalid-ip-pattern");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("192.168.1.100", "192.168.1.0/24", true)]
    [InlineData("192.168.2.1", "192.168.1.0/24", false)]
    [InlineData("10.0.0.50", "10.0.0.0/8", true)]
    [InlineData("172.16.0.1", "172.16.0.0/16", true)]
    public void Matches_CidrNotation_ReturnsExpected(string ipStr, string pattern, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.Matches(ip, pattern);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region MatchesCidr メソッド

    [Fact]
    public void MatchesCidr_NullIpAddress_ReturnsFalse()
    {
        // Arrange & Act
        var result = IpAddressMatcher.MatchesCidr(null!, "192.168.1.0/24");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MatchesCidr_NullOrEmptyCidr_ReturnsFalse(string? cidr)
    {
        // Arrange
        var ip = IPAddress.Parse("192.168.1.1");

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesCidr_InvalidCidrFormat_ReturnsFalse()
    {
        // Arrange
        var ip = IPAddress.Parse("192.168.1.1");

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, "192.168.1.0");  // No prefix

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesCidr_InvalidNetworkAddress_ReturnsFalse()
    {
        // Arrange
        var ip = IPAddress.Parse("192.168.1.1");

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, "invalid/24");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesCidr_InvalidPrefixLength_ReturnsFalse()
    {
        // Arrange
        var ip = IPAddress.Parse("192.168.1.1");

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, "192.168.1.0/abc");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(33)]
    public void MatchesCidr_PrefixLengthOutOfRange_IPv4_ReturnsFalse(int prefixLength)
    {
        // Arrange
        var ip = IPAddress.Parse("192.168.1.1");

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, $"192.168.1.0/{prefixLength}");

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(129)]
    public void MatchesCidr_PrefixLengthOutOfRange_IPv6_ReturnsFalse(int prefixLength)
    {
        // Arrange
        var ip = IPAddress.Parse("2001:db8::1");

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, $"2001:db8::/{ prefixLength}");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesCidr_AddressFamilyMismatch_ReturnsFalse()
    {
        // Arrange - IPv4 address with IPv6 CIDR
        var ipv4 = IPAddress.Parse("192.168.1.1");

        // Act
        var result = IpAddressMatcher.MatchesCidr(ipv4, "2001:db8::/32");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesCidr_AddressFamilyMismatch_IPv6ToIPv4_ReturnsFalse()
    {
        // Arrange - IPv6 address with IPv4 CIDR
        var ipv6 = IPAddress.Parse("2001:db8::1");

        // Act
        var result = IpAddressMatcher.MatchesCidr(ipv6, "192.168.1.0/24");

        // Assert
        Assert.False(result);
    }

    // IPv4 CIDR tests
    [Theory]
    [InlineData("192.168.1.0", "192.168.1.0/24", true)]
    [InlineData("192.168.1.1", "192.168.1.0/24", true)]
    [InlineData("192.168.1.128", "192.168.1.0/24", true)]
    [InlineData("192.168.1.255", "192.168.1.0/24", true)]
    [InlineData("192.168.2.0", "192.168.1.0/24", false)]
    [InlineData("192.168.0.255", "192.168.1.0/24", false)]
    public void MatchesCidr_IPv4_Prefix24_ReturnsExpected(string ipStr, string cidr, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("10.0.0.0", "10.0.0.0/8", true)]
    [InlineData("10.255.255.255", "10.0.0.0/8", true)]
    [InlineData("10.128.64.32", "10.0.0.0/8", true)]
    [InlineData("11.0.0.0", "10.0.0.0/8", false)]
    [InlineData("9.255.255.255", "10.0.0.0/8", false)]
    public void MatchesCidr_IPv4_Prefix8_ReturnsExpected(string ipStr, string cidr, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("172.16.0.0", "172.16.0.0/16", true)]
    [InlineData("172.16.255.255", "172.16.0.0/16", true)]
    [InlineData("172.17.0.0", "172.16.0.0/16", false)]
    public void MatchesCidr_IPv4_Prefix16_ReturnsExpected(string ipStr, string cidr, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("192.168.1.1", "192.168.1.1/32", true)]
    [InlineData("192.168.1.2", "192.168.1.1/32", false)]
    public void MatchesCidr_IPv4_Prefix32_ReturnsExpected(string ipStr, string cidr, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("0.0.0.0", "0.0.0.0/0", true)]
    [InlineData("255.255.255.255", "0.0.0.0/0", true)]
    [InlineData("192.168.1.1", "0.0.0.0/0", true)]
    public void MatchesCidr_IPv4_Prefix0_MatchesAll(string ipStr, string cidr, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr);

        // Assert
        Assert.Equal(expected, result);
    }

    // Non-octet boundary prefix tests (e.g., /25, /27)
    [Theory]
    [InlineData("192.168.1.0", "192.168.1.0/25", true)]
    [InlineData("192.168.1.127", "192.168.1.0/25", true)]
    [InlineData("192.168.1.128", "192.168.1.0/25", false)]
    [InlineData("192.168.1.255", "192.168.1.0/25", false)]
    public void MatchesCidr_IPv4_Prefix25_ReturnsExpected(string ipStr, string cidr, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("192.168.1.0", "192.168.1.0/27", true)]
    [InlineData("192.168.1.31", "192.168.1.0/27", true)]
    [InlineData("192.168.1.32", "192.168.1.0/27", false)]
    public void MatchesCidr_IPv4_Prefix27_ReturnsExpected(string ipStr, string cidr, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr);

        // Assert
        Assert.Equal(expected, result);
    }

    // IPv6 CIDR tests
    [Theory]
    [InlineData("2001:db8::1", "2001:db8::/32", true)]
    [InlineData("2001:db8:ffff:ffff:ffff:ffff:ffff:ffff", "2001:db8::/32", true)]
    [InlineData("2001:db9::1", "2001:db8::/32", false)]
    public void MatchesCidr_IPv6_Prefix32_ReturnsExpected(string ipStr, string cidr, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("fe80::1", "fe80::/10", true)]
    [InlineData("fe80:ffff:ffff:ffff:ffff:ffff:ffff:ffff", "fe80::/10", true)]
    [InlineData("fec0::1", "fe80::/10", false)]
    public void MatchesCidr_IPv6_Prefix10_ReturnsExpected(string ipStr, string cidr, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("::1", "::1/128", true)]
    [InlineData("::2", "::1/128", false)]
    public void MatchesCidr_IPv6_Prefix128_ReturnsExpected(string ipStr, string cidr, bool expected)
    {
        // Arrange
        var ip = IPAddress.Parse(ipStr);

        // Act
        var result = IpAddressMatcher.MatchesCidr(ip, cidr);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region TryParseFromEndpoint メソッド

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryParseFromEndpoint_NullOrEmpty_ReturnsFalse(string? endpoint)
    {
        // Act
        var result = IpAddressMatcher.TryParseFromEndpoint(endpoint!, out var ipAddress);

        // Assert
        Assert.False(result);
        Assert.Null(ipAddress);
    }

    [Theory]
    [InlineData("192.168.1.1:8080", "192.168.1.1")]
    [InlineData("192.168.1.1:80", "192.168.1.1")]
    [InlineData("10.0.0.1:443", "10.0.0.1")]
    [InlineData("127.0.0.1:5000", "127.0.0.1")]
    public void TryParseFromEndpoint_IPv4WithPort_ParsesCorrectly(string endpoint, string expectedIp)
    {
        // Act
        var result = IpAddressMatcher.TryParseFromEndpoint(endpoint, out var ipAddress);

        // Assert
        Assert.True(result);
        Assert.NotNull(ipAddress);
        Assert.Equal(IPAddress.Parse(expectedIp), ipAddress);
    }

    [Theory]
    [InlineData("192.168.1.1", "192.168.1.1")]
    [InlineData("10.0.0.1", "10.0.0.1")]
    public void TryParseFromEndpoint_IPv4WithoutPort_ParsesCorrectly(string endpoint, string expectedIp)
    {
        // Act
        var result = IpAddressMatcher.TryParseFromEndpoint(endpoint, out var ipAddress);

        // Assert
        Assert.True(result);
        Assert.NotNull(ipAddress);
        Assert.Equal(IPAddress.Parse(expectedIp), ipAddress);
    }

    [Theory]
    [InlineData("[::1]:8080", "::1")]
    [InlineData("[2001:db8::1]:443", "2001:db8::1")]
    [InlineData("[fe80::1]:80", "fe80::1")]
    public void TryParseFromEndpoint_IPv6WithPort_ParsesCorrectly(string endpoint, string expectedIp)
    {
        // Act
        var result = IpAddressMatcher.TryParseFromEndpoint(endpoint, out var ipAddress);

        // Assert
        Assert.True(result);
        Assert.NotNull(ipAddress);
        Assert.Equal(IPAddress.Parse(expectedIp), ipAddress);
    }

    [Fact]
    public void TryParseFromEndpoint_IPv6MissingCloseBracket_ReturnsFalse()
    {
        // Arrange
        var endpoint = "[::1:8080";  // Missing closing bracket

        // Act
        var result = IpAddressMatcher.TryParseFromEndpoint(endpoint, out var ipAddress);

        // Assert
        Assert.False(result);
        Assert.Null(ipAddress);
    }

    [Fact]
    public void TryParseFromEndpoint_InvalidIpAddress_ReturnsFalse()
    {
        // Arrange
        var endpoint = "invalid:8080";

        // Act
        var result = IpAddressMatcher.TryParseFromEndpoint(endpoint, out var ipAddress);

        // Assert
        Assert.False(result);
        Assert.Null(ipAddress);
    }

    #endregion
}
