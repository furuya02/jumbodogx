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
        // ACL list is empty
        if (_settings.AclList.Count == 0)
        {
            // EnableAcl: 0=Allow list, 1=Deny list
            // Allow list + empty → deny all (fail-secure)
            // Deny list + empty → allow all (no one is denied)
            var result = _settings.EnableAcl != 0;
            _logger.LogDebug("ACL list is empty, {Action} all connections (EnableAcl={Mode})",
                result ? "allowing" : "denying", _settings.EnableAcl);
            return result;
        }

        // Parse IP address from endpoint format
        if (!IpAddressMatcher.TryParseFromEndpoint(remoteAddress, out var ip) || ip == null)
        {
            _logger.LogWarning("Failed to parse IP address: {RemoteAddress}", remoteAddress);
            return false;
        }

        // Check if IP matches any ACL entry
        var matches = _settings.AclList.Any(acl => IpAddressMatcher.Matches(ip, acl.Address));

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
