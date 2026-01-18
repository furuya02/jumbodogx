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
        if (_settings.AclList.Count == 0)
        {
            // No ACL rules: fail-secure (deny all)
            _logger.LogDebug("ACL list is empty, denying all connections (fail-secure default)");
            return false;
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
