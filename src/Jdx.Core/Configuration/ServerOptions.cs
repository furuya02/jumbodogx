namespace Jdx.Core.Configuration;

/// <summary>
/// サーバー共通設定オプション
/// </summary>
public class ServerOptions
{
    /// <summary>設定セクション名</summary>
    public const string SectionName = "Servers";

    /// <summary>サーバー有効化</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>リスニングポート</summary>
    public int Port { get; set; }

    /// <summary>バインドアドレス（nullの場合は0.0.0.0）</summary>
    public string? BindAddress { get; set; }

    /// <summary>最大並行接続数</summary>
    public int MaxConnections { get; set; } = 100;

    /// <summary>接続タイムアウト（秒）</summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>読み取りタイムアウト（秒）</summary>
    public int ReadTimeoutSeconds { get; set; } = 30;

    /// <summary>書き込みタイムアウト（秒）</summary>
    public int WriteTimeoutSeconds { get; set; } = 30;

    /// <summary>バッファサイズ（バイト）</summary>
    public int BufferSize { get; set; } = 8192;
}
