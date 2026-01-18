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
        // EnableAcl: 0=無効, 1=許可リスト, 2=拒否リスト
        // ACL無効の場合は全て許可
        if (_settings.EnableAcl == 0)
        {
            return true;
        }

        // ACL有効だがリストが空/null: fail-secure (deny all)
        if (_settings.AclList == null || _settings.AclList.Count == 0)
        {
            _logger.LogDebug("ACL enabled but list is empty, denying all connections (fail-secure default)");
            return false;
        }

        // IPアドレスをパース
        if (!IPAddress.TryParse(remoteAddress, out var ipAddress))
        {
            _logger.LogWarning("Invalid remote address: {RemoteAddress}", remoteAddress);
            return false;
        }

        bool isInList = false;
        foreach (var aclEntry in _settings.AclList)
        {
            if (IpAddressMatcher.Matches(ipAddress, aclEntry.Address))
            {
                isInList = true;
                _logger.LogDebug("IP {IP} matched ACL entry: {Name} ({Address})",
                    remoteAddress, aclEntry.Name, aclEntry.Address);
                break;
            }
        }

        // EnableAcl == 1: 許可リスト（リストにあるIPのみ許可）
        if (_settings.EnableAcl == 1)
        {
            if (!isInList)
            {
                _logger.LogWarning("IP {IP} not in allow list, rejecting", remoteAddress);
            }
            return isInList;
        }

        // EnableAcl == 2: 拒否リスト（リストにあるIPを拒否）
        if (_settings.EnableAcl == 2)
        {
            if (isInList)
            {
                _logger.LogWarning("IP {IP} in deny list, rejecting", remoteAddress);
            }
            return !isInList;
        }

        return true;
    }
}
