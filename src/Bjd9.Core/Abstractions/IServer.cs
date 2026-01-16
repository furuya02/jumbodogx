namespace Bjd9.Core.Abstractions;

/// <summary>
/// すべてのサーバー実装が実装する統一インターフェース
/// </summary>
public interface IServer : IDisposable
{
    /// <summary>サーバー名</summary>
    string Name { get; }

    /// <summary>サーバーの種類</summary>
    ServerType Type { get; }

    /// <summary>現在の状態</summary>
    ServerStatus Status { get; }

    /// <summary>リスニングポート</summary>
    int Port { get; }

    /// <summary>サーバーを起動します</summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>サーバーを停止します</summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    Task StopAsync(CancellationToken cancellationToken);

    /// <summary>統計情報を取得します</summary>
    ServerStatistics GetStatistics();

    /// <summary>ヘルスチェックを実行します</summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    Task<bool> CheckHealthAsync(CancellationToken cancellationToken);
}
