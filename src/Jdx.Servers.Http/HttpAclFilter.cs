using System.Linq;
using System.Net;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Http;

/// <summary>
/// HTTP ACL (Access Control List) フィルタリング
/// </summary>
public class HttpAclFilter
{
    private readonly HttpServerSettings _settings;
    private readonly ILogger _logger;

    public HttpAclFilter(HttpServerSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// 接続元IPアドレスをチェック
    /// </summary>
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

        // IPアドレスをパース
        if (!IPAddress.TryParse(remoteAddress, out var ipAddress))
        {
            _logger.LogWarning("Invalid remote address: {RemoteAddress}", remoteAddress);
            return false;
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
