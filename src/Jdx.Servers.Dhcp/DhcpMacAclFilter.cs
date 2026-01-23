using System.Net.NetworkInformation;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Dhcp;

/// <summary>
/// DHCP MAC ACL (Access Control List) filtering
/// Filters incoming DHCP requests based on MAC address allow list
/// </summary>
public class DhcpMacAclFilter
{
    private readonly DhcpServerSettings _settings;
    private readonly ILogger _logger;
    private readonly HashSet<string> _allowedMacs;

    public DhcpMacAclFilter(DhcpServerSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
        _allowedMacs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Build MAC allow list from MacAclList
        if (_settings.MacAclList != null)
        {
            foreach (var entry in _settings.MacAclList)
            {
                if (!string.IsNullOrWhiteSpace(entry.MacAddress))
                {
                    // Normalize MAC address format (remove separators)
                    var normalizedMac = entry.MacAddress.Replace("-", "").Replace(":", "").ToUpperInvariant();
                    _allowedMacs.Add(normalizedMac);
                }
            }
        }

        _logger.LogInformation("DHCP MAC ACL initialized with {Count} allowed MACs", _allowedMacs.Count);
    }

    /// <summary>
    /// Check if a DHCP request from the given MAC address is allowed
    /// </summary>
    /// <param name="macAddress">Physical MAC address</param>
    /// <returns>True if allowed, false if denied</returns>
    public bool IsAllowed(PhysicalAddress macAddress)
    {
        // MAC ACL無効の場合は全て許可
        if (!_settings.UseMacAcl)
        {
            return true;
        }

        // MAC ACL有効だがリストが空: fail-secure (deny all)
        if (_allowedMacs.Count == 0)
        {
            _logger.LogWarning("MAC ACL denied connection from {MacAddress} (Matched: {MatchedRule})",
                macAddress, "EmptyList");
            return false;
        }

        // Normalize MAC address for comparison
        var macString = macAddress.ToString().Replace("-", "").Replace(":", "").ToUpperInvariant();

        // Check if MAC is in allow list
        var allowed = _allowedMacs.Contains(macString);
        string? matchedRule = allowed ? macString : null;

        if (allowed)
        {
            _logger.LogDebug("MAC ACL allowed connection from {MacAddress} (Matched: {MatchedRule})",
                macAddress, matchedRule ?? "NoMatch");
        }
        else
        {
            _logger.LogWarning("MAC ACL denied connection from {MacAddress} (Matched: {MatchedRule})",
                macAddress, matchedRule ?? "NotInList");
        }

        return allowed;
    }
}
