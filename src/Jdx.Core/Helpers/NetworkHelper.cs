using System.Net;
using Microsoft.Extensions.Logging;

namespace Jdx.Core.Helpers;

/// <summary>
/// ネットワーク関連のヘルパーメソッドを提供
/// </summary>
public static class NetworkHelper
{
    /// <summary>
    /// BindAddressを解析し、IPAddressオブジェクトを返す
    /// 不正なアドレスの場合は警告ログを出力し、IPAddress.Anyを返す
    /// </summary>
    /// <param name="bindAddress">バインドアドレス文字列</param>
    /// <param name="logger">ログ出力用（必須）</param>
    /// <returns>解析されたIPAddressまたはIPAddress.Any</returns>
    public static IPAddress ParseBindAddress(
        string? bindAddress,
        ILogger logger)
    {
        // null、空文字、または "0.0.0.0" の場合は IPAddress.Any
        if (string.IsNullOrWhiteSpace(bindAddress) || bindAddress == "0.0.0.0")
        {
            return IPAddress.Any;
        }

        // IPアドレスとしてパース可能な場合はそのまま返す
        if (IPAddress.TryParse(bindAddress, out var result))
        {
            return result;
        }

        // パース失敗時は警告を出力（セキュリティ/運用上重要な情報）
        logger.LogWarning("Invalid bind address '{Address}', using Any", bindAddress);
        return IPAddress.Any;
    }
}
