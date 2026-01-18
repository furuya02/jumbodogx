using System;
using System.Net;
using System.Net.Sockets;

namespace Jdx.Core.Network;

/// <summary>
/// IP address matching utility for ACL filtering
/// Supports both single IP addresses and CIDR notation ranges
/// </summary>
public static class IpAddressMatcher
{
    /// <summary>
    /// Check if an IP address matches a pattern (single IP or CIDR range)
    /// </summary>
    /// <param name="ipAddress">The IP address to check</param>
    /// <param name="pattern">The pattern to match against (e.g., "192.168.1.1" or "192.168.1.0/24")</param>
    /// <returns>True if the IP matches the pattern, false otherwise</returns>
    public static bool Matches(IPAddress ipAddress, string pattern)
    {
        if (ipAddress == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        // Check for CIDR notation (e.g., 192.168.1.0/24)
        if (pattern.Contains('/'))
        {
            return MatchesCidr(ipAddress, pattern);
        }

        // Single IP address
        if (IPAddress.TryParse(pattern, out var patternIp))
        {
            return ipAddress.Equals(patternIp);
        }

        return false;
    }

    /// <summary>
    /// Check if an IP address is within a CIDR range
    /// </summary>
    /// <param name="ipAddress">The IP address to check</param>
    /// <param name="cidr">CIDR notation (e.g., "192.168.1.0/24")</param>
    /// <returns>True if the IP is within the CIDR range, false otherwise</returns>
    public static bool MatchesCidr(IPAddress ipAddress, string cidr)
    {
        if (ipAddress == null || string.IsNullOrWhiteSpace(cidr))
        {
            return false;
        }

        var parts = cidr.Split('/');
        if (parts.Length != 2)
        {
            return false;
        }

        if (!IPAddress.TryParse(parts[0], out var networkAddress))
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        // Check address family compatibility
        if (ipAddress.AddressFamily != networkAddress.AddressFamily)
        {
            return false;
        }

        // Validate prefix length
        var maxPrefixLength = ipAddress.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;
        if (prefixLength < 0 || prefixLength > maxPrefixLength)
        {
            return false;
        }

        var ipBytes = ipAddress.GetAddressBytes();
        var networkBytes = networkAddress.GetAddressBytes();

        if (ipBytes.Length != networkBytes.Length)
        {
            return false;
        }

        // Calculate full bytes and remaining bits
        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        // Check full bytes
        for (int i = 0; i < fullBytes; i++)
        {
            if (ipBytes[i] != networkBytes[i])
            {
                return false;
            }
        }

        // Check remaining bits
        if (remainingBits > 0 && fullBytes < ipBytes.Length)
        {
            var mask = (byte)(0xFF << (8 - remainingBits));
            if ((ipBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Parse IP address from endpoint format (e.g., "192.168.1.1:8080" or "[::1]:8080")
    /// </summary>
    /// <param name="endpoint">The endpoint string</param>
    /// <param name="ipAddress">The parsed IP address</param>
    /// <returns>True if parsing succeeded, false otherwise</returns>
    public static bool TryParseFromEndpoint(string endpoint, out IPAddress? ipAddress)
    {
        ipAddress = null;

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return false;
        }

        string ipStr;

        // Handle IPv6 endpoint format: [::1]:8080
        if (endpoint.StartsWith('['))
        {
            var endBracket = endpoint.IndexOf(']');
            if (endBracket > 0)
            {
                ipStr = endpoint.Substring(1, endBracket - 1);
            }
            else
            {
                return false;
            }
        }
        else
        {
            // Handle IPv4 endpoint format: 192.168.1.1:8080
            var parts = endpoint.Split(':');
            ipStr = parts[0];
        }

        return IPAddress.TryParse(ipStr, out ipAddress);
    }
}
