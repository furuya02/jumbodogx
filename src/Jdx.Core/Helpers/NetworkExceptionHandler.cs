using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Jdx.Core.Helpers;

/// <summary>
/// ネットワーク例外の処理を統一的に行うヘルパークラス
/// </summary>
public static class NetworkExceptionHandler
{
    /// <summary>
    /// ネットワーク例外をログに記録（再スローしない）
    /// </summary>
    public static void LogNetworkException(
        Exception ex,
        ILogger logger,
        string context,
        params object[] args)
    {
        switch (ex)
        {
            case OperationCanceledException:
                logger.LogDebug($"{context} cancelled", args);
                break;

            case IOException ioEx when ioEx.InnerException is SocketException:
                logger.LogDebug(ioEx, $"{context} connection closed (network error)", args);
                break;

            case SocketException sockEx:
                logger.LogDebug(sockEx, $"{context} socket error", args);
                break;

            default:
                logger.LogWarning(ex, $"{context} unexpected error", args);
                break;
        }
    }

    /// <summary>
    /// 終端的な例外（キャンセル等）かどうかを判定
    /// Accept/Receiveループを中断すべき例外の場合はtrue
    /// </summary>
    public static bool IsTerminalException(Exception ex)
    {
        return ex is OperationCanceledException;
    }

    /// <summary>
    /// ネットワーク例外を処理し、終端的な例外は再スロー
    /// Task.Run内のクライアント処理で使用することを想定
    /// </summary>
    public static void HandleOrRethrow(Exception ex, ILogger logger, string context)
    {
        // キャンセル例外は再スロー（上位でループ中断される）
        if (IsTerminalException(ex))
        {
            throw ex;
        }

        // その他の例外はログに記録のみ
        LogNetworkException(ex, logger, context);
    }
}
