using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Proxy;

/// <summary>
/// コンテンツ制限機能
/// bjd5-master/ProxyHttpServer/LimitString.cs に対応
/// </summary>
public class ProxyLimitString
{
    private readonly List<string> _limitStrings;
    private readonly ILogger _logger;

    public int Length => _limitStrings.Count;

    public ProxyLimitString(List<ProxyLimitStringEntry> limitStringList, ILogger logger)
    {
        _limitStrings = limitStringList
            .Select(e => e.String)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        _logger = logger;

        if (_limitStrings.Count > 0)
        {
            _logger.LogInformation("Content filter enabled with {Count} patterns", _limitStrings.Count);
        }
    }

    /// <summary>
    /// 指定されたコンテンツに制限文字列が含まれているかチェック
    /// </summary>
    /// <param name="content">チェック対象のコンテンツ</param>
    /// <returns>true: 制限文字列が含まれている（ブロック対象）, false: 含まれていない（許可）</returns>
    public bool Contains(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;

        foreach (var limitString in _limitStrings)
        {
            if (content.Contains(limitString, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Content blocked: contains limited string '{LimitString}'", limitString);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 指定されたバイト配列に制限文字列が含まれているかチェック
    /// </summary>
    /// <param name="data">チェック対象のデータ</param>
    /// <returns>true: 制限文字列が含まれている（ブロック対象）, false: 含まれていない（許可）</returns>
    public bool Contains(byte[] data)
    {
        if (data == null || data.Length == 0)
            return false;

        // バイトデータを文字列に変換してチェック
        // 複数のエンコーディングを試す
        var encodings = new[] {
            System.Text.Encoding.UTF8,
            System.Text.Encoding.ASCII,
            System.Text.Encoding.GetEncoding("shift_jis")
        };

        foreach (var encoding in encodings)
        {
            try
            {
                var content = encoding.GetString(data);
                if (Contains(content))
                    return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error decoding content with {Encoding}", encoding.EncodingName);
            }
        }

        return false;
    }
}
