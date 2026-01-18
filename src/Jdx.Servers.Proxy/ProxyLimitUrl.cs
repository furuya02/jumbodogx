using System.Text.RegularExpressions;
using Jdx.Core.Constants;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Proxy;

/// <summary>
/// URL制限機能
/// bjd5-master/ProxyHttpServer/LimitUrl.cs に対応
/// </summary>
public class ProxyLimitUrl
{
    private readonly List<ProxyLimitUrlEntry> _allowList;
    private readonly List<ProxyLimitUrlEntry> _denyList;
    private readonly ILogger _logger;

    public ProxyLimitUrl(
        List<ProxyLimitUrlEntry> allowList,
        List<ProxyLimitUrlEntry> denyList,
        ILogger logger)
    {
        _allowList = allowList ?? new List<ProxyLimitUrlEntry>();
        _denyList = denyList ?? new List<ProxyLimitUrlEntry>();
        _logger = logger;

        // 正規表現の妥当性チェック
        ValidateRegexPatterns(_allowList);
        ValidateRegexPatterns(_denyList);
    }

    private void ValidateRegexPatterns(List<ProxyLimitUrlEntry> list)
    {
        foreach (var entry in list)
        {
            if (entry.Matching == 3) // 正規表現
            {
                try
                {
                    var regex = new Regex(entry.Url);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Invalid regex pattern in URL limit: {Pattern}", entry.Url);
                }
            }
        }
    }

    /// <summary>
    /// 指定されたURLがアクセス可能かどうかをチェック
    /// </summary>
    /// <param name="url">チェック対象のURL</param>
    /// <returns>true: アクセス可能, false: アクセス不可</returns>
    public bool IsAllowed(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // 拒否リストにマッチする場合は拒否
        if (MatchesList(url, _denyList))
        {
            _logger.LogWarning("URL denied by deny list: {Url}", url);
            return false;
        }

        // 許可リストが空の場合は全て許可
        if (_allowList.Count == 0)
            return true;

        // 許可リストにマッチする場合のみ許可
        var allowed = MatchesList(url, _allowList);
        if (!allowed)
        {
            _logger.LogWarning("URL not in allow list: {Url}", url);
        }

        return allowed;
    }

    private bool MatchesList(string url, List<ProxyLimitUrlEntry> list)
    {
        foreach (var entry in list)
        {
            if (string.IsNullOrWhiteSpace(entry.Url))
                continue;

            try
            {
                bool matches = entry.Matching switch
                {
                    0 => url.StartsWith(entry.Url, StringComparison.OrdinalIgnoreCase), // 前方一致
                    1 => url.EndsWith(entry.Url, StringComparison.OrdinalIgnoreCase),   // 後方一致
                    2 => url.Contains(entry.Url, StringComparison.OrdinalIgnoreCase),   // 部分一致
                    3 => Regex.IsMatch(url, entry.Url, RegexOptions.None, TimeSpan.FromMilliseconds(NetworkConstants.Timeouts.RegexTimeoutMilliseconds)),  // 正規表現（ReDoS対策）
                    _ => false
                };

                if (matches)
                    return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching URL pattern: {Pattern}", entry.Url);
            }
        }

        return false;
    }
}
