using System.Net;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Pop3;

/// <summary>
/// POP3 ACL (Access Control List) filtering
/// Filters incoming POP3 connections based on IP address allow/deny rules
/// </summary>
public class Pop3AclFilter
{
    private readonly Pop3ServerSettings _settings;
    private readonly ILogger _logger;

    public Pop3AclFilter(Pop3ServerSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Check if a POP3 connection from the given IP address is allowed
    /// </summary>
    /// <param name="remoteAddress">Remote IP address (can be in endpoint format)</param>
    /// <returns>True if allowed, false if denied</returns>
    public bool IsAllowed(string remoteAddress)
    {
        // ACL list is empty
        if (_settings.AclList == null || _settings.AclList.Count == 0)
        {
            // EnableAcl: 0=Allow list, 1=Deny list
            // Allow list + empty → deny all (fail-secure)
            // Deny list + empty → allow all (no one is denied)
            var result = _settings.EnableAcl != 0;
            _logger.LogDebug("ACL list is empty, {Action} all connections (EnableAcl={Mode})",
                result ? "allowing" : "denying", _settings.EnableAcl);
            return result;
        }

        // Parse IP address from endpoint format if necessary
        IPAddress? ipAddress;
        if (!IpAddressMatcher.TryParseFromEndpoint(remoteAddress, out ipAddress) || ipAddress == null)
        {
            // Try direct parsing
            if (!IPAddress.TryParse(remoteAddress, out ipAddress))
            {
                _logger.LogWarning("Invalid remote address: {RemoteAddress}", remoteAddress);
                return false;
            }
        }

        // Check if IP matches any ACL entry
        bool matches = false;
        foreach (var aclEntry in _settings.AclList)
        {
            if (IpAddressMatcher.Matches(ipAddress, aclEntry.Address))
            {
                matches = true;
                _logger.LogDebug("IP {IP} matched ACL entry: {Name} ({Address})",
                    remoteAddress, aclEntry.Name, aclEntry.Address);
                break;
            }
        }

        // Allow mode (0): only listed IPs are allowed
        // Deny mode (1): listed IPs are denied
        var allowed = _settings.EnableAcl == 0 ? matches : !matches;

        if (!allowed)
        {
            _logger.LogWarning("Connection denied by ACL: {RemoteAddress}", remoteAddress);
        }

        return allowed;
    }
}
