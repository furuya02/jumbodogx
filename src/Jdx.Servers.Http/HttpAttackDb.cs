using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Http;

/// <summary>
/// 攻撃検出データベース
/// 短期間に複数回の不正アクセスを検出する
///
/// TODO: ACL自動追加機能の実装
/// 現在は攻撃検出のみ実装されており、ACL自動追加機能は未実装です。
/// ACL自動追加を実装するには、ISettingsServiceを使用してACL設定を更新し、
/// 攻撃元IPアドレスをAclListに追加する必要があります。
/// </summary>
public class HttpAttackDb
{
    private readonly ILogger _logger;
    private readonly int _timeWindowSeconds;  // 対象期間（秒）
    private readonly int _maxAttempts;        // 発生回数閾値
    private readonly List<AttackRecord> _records = new();
    private readonly object _lock = new();

    public HttpAttackDb(ILogger logger, int timeWindowSeconds = 120, int maxAttempts = 1)
    {
        _logger = logger;
        _timeWindowSeconds = timeWindowSeconds;
        _maxAttempts = maxAttempts;
    }

    /// <summary>
    /// アクセス記録を追加し、不正なアクセスかどうかを判定
    /// </summary>
    /// <param name="success">アクセスが正常だった場合true、不正だった場合false</param>
    /// <param name="remoteIp">リモートIPアドレス</param>
    /// <returns>不正なアクセスと判定された場合true</returns>
    public bool IsInjustice(bool success, string remoteIp)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;

            // 古いレコードを削除（時間窓の外）
            _records.RemoveAll(r => (now - r.Timestamp).TotalSeconds > _timeWindowSeconds);

            // 成功した場合は、該当IPのレコードをクリア
            if (success)
            {
                _records.RemoveAll(r => r.RemoteIp == remoteIp);
                return false;
            }

            // 失敗した場合は、レコードを追加
            _records.Add(new AttackRecord
            {
                RemoteIp = remoteIp,
                Timestamp = now
            });

            // 時間窓内での失敗回数をカウント
            var failureCount = _records.Count(r => r.RemoteIp == remoteIp);

            if (failureCount > _maxAttempts)
            {
                _logger.LogWarning("Attack detected from {RemoteIp}: {Count} failures in {Window} seconds",
                    remoteIp, failureCount, _timeWindowSeconds);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// 攻撃記録
    /// </summary>
    private class AttackRecord
    {
        public string RemoteIp { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
