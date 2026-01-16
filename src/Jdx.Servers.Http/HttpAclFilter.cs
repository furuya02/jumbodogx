using System;
using System.Linq;
using System.Net;
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
        // ACLが無効の場合は全て許可
        if (_settings.AclList == null || _settings.AclList.Count == 0)
        {
            return true;
        }

        // IPアドレスをパース
        if (!IPAddress.TryParse(remoteAddress, out var ipAddress))
        {
            _logger.LogWarning("Invalid remote address: {RemoteAddress}", remoteAddress);
            return false;
        }

        // EnableAcl: 0=無効, 1=許可リスト, 2=拒否リスト
        if (_settings.EnableAcl == 0)
        {
            return true;
        }

        bool isInList = false;
        foreach (var aclEntry in _settings.AclList)
        {
            if (IsIpInRange(ipAddress, aclEntry.Address))
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

    /// <summary>
    /// IPアドレスが指定範囲に含まれるかチェック
    /// </summary>
    private bool IsIpInRange(IPAddress ipAddress, string range)
    {
        // CIDR notation (例: 192.168.1.0/24)
        if (range.Contains('/'))
        {
            return IsIpInCidrRange(ipAddress, range);
        }

        // 単一IPアドレス
        if (IPAddress.TryParse(range, out var rangeIp))
        {
            return ipAddress.Equals(rangeIp);
        }

        _logger.LogWarning("Invalid ACL address format: {Range}", range);
        return false;
    }

    /// <summary>
    /// CIDR範囲チェック
    /// </summary>
    private bool IsIpInCidrRange(IPAddress ipAddress, string cidr)
    {
        try
        {
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

            // IPv4のみサポート
            if (ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork ||
                networkAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return false;
            }

            var ipBytes = ipAddress.GetAddressBytes();
            var networkBytes = networkAddress.GetAddressBytes();

            // プレフィックス長からサブネットマスクを計算
            uint mask = 0xFFFFFFFF << (32 - prefixLength);
            var maskBytes = BitConverter.GetBytes(mask);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(maskBytes);
            }

            // ネットワークアドレスを計算して比較
            for (int i = 0; i < 4; i++)
            {
                if ((ipBytes[i] & maskBytes[i]) != (networkBytes[i] & maskBytes[i]))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking CIDR range: {CIDR}", cidr);
            return false;
        }
    }
}
