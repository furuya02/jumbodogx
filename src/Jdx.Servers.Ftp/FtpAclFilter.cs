using System.Linq;
using System.Net;
using Jdx.Core.Network;
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
        var aclMode = _settings.EnableAcl == 0 ? "AllowList" : "DenyList";

        // ACL list is empty
        if (_settings.AclList.Count == 0)
        {
            // EnableAcl: 0=Allow list, 1=Deny list
            // Allow list + empty → deny all (fail-secure)
            // Deny list + empty → allow all (no one is denied)
            var result = _settings.EnableAcl != 0;

            if (!result)
            {
                _logger.LogWarning("ACL denied connection from {RemoteAddress} (Mode: {AclMode}, Matched: {MatchedRule})",
                    remoteAddress, aclMode, "EmptyList");
            }

            return result;
        }

        // Parse IP address from endpoint format
        if (!IpAddressMatcher.TryParseFromEndpoint(remoteAddress, out var ip) || ip == null)
        {
            _logger.LogWarning("ACL denied connection from {RemoteAddress} (Mode: {AclMode}, Matched: {MatchedRule})",
                remoteAddress, aclMode, "InvalidIP");
            return false;
        }

        // Check if IP matches any ACL entry
        bool matches = false;
        string? matchedRule = null;

        foreach (var aclEntry in _settings.AclList)
        {
            if (IpAddressMatcher.Matches(ip, aclEntry.Address))
            {
                matches = true;
                matchedRule = aclEntry.Address;
                break;
            }
        }

        // Allow mode (0): only listed IPs are allowed
        // Deny mode (1): listed IPs are denied
        var allowed = _settings.EnableAcl == 0 ? matches : !matches;

        if (allowed)
        {
            _logger.LogDebug("ACL allowed connection from {RemoteAddress} (Mode: {AclMode}, Matched: {MatchedRule})",
                remoteAddress, aclMode, matchedRule ?? "NoMatch");
        }
        else
        {
            _logger.LogWarning("ACL denied connection from {RemoteAddress} (Mode: {AclMode}, Matched: {MatchedRule})",
                remoteAddress, aclMode, matchedRule ?? "NoMatch");
        }

        return allowed;
    }
}
