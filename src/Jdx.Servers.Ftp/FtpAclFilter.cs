using System;
using System.Linq;
using System.Net;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Ftp;

/// <summary>
/// ACL (Access Control List) filter for FTP
/// Filters incoming connections based on IP address allow/deny rules
/// </summary>
public class FtpAclFilter
{
    private readonly FtpServerSettings _settings;
    private readonly ILogger _logger;

    public FtpAclFilter(FtpServerSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Check if connection is allowed from the given IP address
    /// </summary>
    public bool IsAllowed(string remoteAddress)
    {
        if (_settings.AclList.Count == 0)
        {
            // No ACL rules: allow all in allow mode, deny all in deny mode
            return _settings.EnableAcl == 0;
        }

        // Parse IP address
        if (!TryParseIpAddress(remoteAddress, out var ip))
        {
            _logger.LogWarning("Failed to parse IP address: {RemoteAddress}", remoteAddress);
            return false;
        }

        // Check if IP matches any ACL entry
        var matches = _settings.AclList.Any(acl => MatchesAcl(ip, acl.Address));

        // Allow mode (0): only listed IPs are allowed
        // Deny mode (1): listed IPs are denied
        var allowed = _settings.EnableAcl == 0 ? matches : !matches;

        if (!allowed)
        {
            _logger.LogWarning("Connection denied by ACL: {RemoteAddress}", remoteAddress);
        }

        return allowed;
    }

    private bool TryParseIpAddress(string address, out IPAddress? ip)
    {
        ip = null;

        // Extract IP from endpoint format (IP:port)
        var parts = address.Split(':');
        var ipStr = parts[0];

        // Remove IPv6 brackets
        ipStr = ipStr.Trim('[', ']');

        return IPAddress.TryParse(ipStr, out ip);
    }

    private bool MatchesAcl(IPAddress ip, string aclPattern)
    {
        try
        {
            // Check for CIDR notation (e.g., 192.168.1.0/24)
            if (aclPattern.Contains('/'))
            {
                var parts = aclPattern.Split('/');
                if (parts.Length != 2)
                    return false;

                if (!IPAddress.TryParse(parts[0], out var networkIp))
                    return false;

                if (!int.TryParse(parts[1], out var prefixLength))
                    return false;

                return IsInSubnet(ip, networkIp, prefixLength);
            }

            // Single IP address
            if (IPAddress.TryParse(aclPattern, out var aclIp))
            {
                return ip.Equals(aclIp);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error matching ACL pattern: {Pattern}", aclPattern);
            return false;
        }
    }

    private bool IsInSubnet(IPAddress address, IPAddress network, int prefixLength)
    {
        if (address.AddressFamily != network.AddressFamily)
            return false;

        var addressBytes = address.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();

        if (addressBytes.Length != networkBytes.Length)
            return false;

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        // Check full bytes
        for (int i = 0; i < fullBytes; i++)
        {
            if (addressBytes[i] != networkBytes[i])
                return false;
        }

        // Check remaining bits
        if (remainingBits > 0 && fullBytes < addressBytes.Length)
        {
            var mask = (byte)(0xFF << (8 - remainingBits));
            if ((addressBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask))
                return false;
        }

        return true;
    }
}
