namespace Jdx.Core.Abstractions;

/// <summary>
/// サーバーの統計情報
/// </summary>
public class ServerStatistics
{
    /// <summary>現在のアクティブ接続数</summary>
    public long ActiveConnections { get; set; }

    /// <summary>累計接続数</summary>
    public long TotalConnections { get; set; }

    /// <summary>累計リクエスト数</summary>
    public long TotalRequests { get; set; }

    /// <summary>累計送信バイト数</summary>
    public long TotalBytesSent { get; set; }

    /// <summary>累計受信バイト数</summary>
    public long TotalBytesReceived { get; set; }

    /// <summary>累計エラー数</summary>
    public long TotalErrors { get; set; }

    /// <summary>サーバー起動時刻</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>稼働時間</summary>
    public TimeSpan Uptime => StartTime.HasValue
        ? DateTime.UtcNow - StartTime.Value
        : TimeSpan.Zero;
}
