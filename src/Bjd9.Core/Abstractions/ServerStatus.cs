namespace Bjd9.Core.Abstractions;

/// <summary>
/// サーバーの状態を表す列挙型
/// </summary>
public enum ServerStatus
{
    /// <summary>停止中</summary>
    Stopped,

    /// <summary>起動中</summary>
    Starting,

    /// <summary>実行中</summary>
    Running,

    /// <summary>停止処理中</summary>
    Stopping,

    /// <summary>エラー</summary>
    Error
}
