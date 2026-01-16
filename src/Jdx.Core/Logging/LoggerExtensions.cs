using Microsoft.Extensions.Logging;

namespace Jdx.Core.Logging;

/// <summary>
/// ILogger拡張メソッド（Serilog非依存）
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// サーバーイベントのログ記録
    /// </summary>
    public static void LogServerEvent(
        this ILogger logger,
        string serverName,
        string eventType,
        string message,
        params object[] args)
    {
        logger.LogInformation(
            "[{ServerName}] {EventType}: " + message,
            serverName, eventType, args);
    }

    /// <summary>
    /// クライアント接続のログ記録
    /// </summary>
    public static void LogClientConnection(
        this ILogger logger,
        string serverName,
        string clientAddress,
        string action = "Connected")
    {
        logger.LogInformation(
            "[{ServerName}] Client {Action}: {ClientAddress}",
            serverName, action, clientAddress);
    }

    /// <summary>
    /// クライアント切断のログ記録
    /// </summary>
    public static void LogClientDisconnection(
        this ILogger logger,
        string serverName,
        string clientAddress,
        string? reason = null)
    {
        if (string.IsNullOrEmpty(reason))
        {
            logger.LogInformation(
                "[{ServerName}] Client disconnected: {ClientAddress}",
                serverName, clientAddress);
        }
        else
        {
            logger.LogInformation(
                "[{ServerName}] Client disconnected: {ClientAddress} (Reason: {Reason})",
                serverName, clientAddress, reason);
        }
    }

    /// <summary>
    /// サーバー統計のログ記録
    /// </summary>
    public static void LogServerStatistics(
        this ILogger logger,
        string serverName,
        long activeConnections,
        long totalRequests)
    {
        logger.LogInformation(
            "[{ServerName}] Stats - Active: {ActiveConnections}, Total: {TotalRequests}",
            serverName, activeConnections, totalRequests);
    }

    /// <summary>
    /// リクエスト処理のログ記録
    /// </summary>
    public static void LogRequestProcessed(
        this ILogger logger,
        string serverName,
        string method,
        string path,
        int statusCode,
        double elapsedMs)
    {
        logger.LogInformation(
            "[{ServerName}] {Method} {Path} -> {StatusCode} ({ElapsedMs}ms)",
            serverName, method, path, statusCode, elapsedMs);
    }
}
