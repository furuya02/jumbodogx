using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Jdx.Core.Metrics;

/// <summary>
/// Global metrics collector for all servers
/// Aggregates metrics from multiple server instances
/// </summary>
public class MetricsCollector
{
    private static readonly Lazy<MetricsCollector> _instance = new(() => new MetricsCollector());
    private readonly ConcurrentDictionary<string, ServerMetrics> _serverMetrics;

    public static MetricsCollector Instance => _instance.Value;

    private MetricsCollector()
    {
        _serverMetrics = new ConcurrentDictionary<string, ServerMetrics>();
    }

    /// <summary>
    /// Register a server's metrics
    /// </summary>
    public void RegisterServer(ServerMetrics metrics)
    {
        _serverMetrics.TryAdd(metrics.ServerName, metrics);
    }

    /// <summary>
    /// Unregister a server's metrics
    /// </summary>
    public void UnregisterServer(string serverName)
    {
        _serverMetrics.TryRemove(serverName, out _);
    }

    /// <summary>
    /// Get metrics for a specific server
    /// </summary>
    public ServerMetrics? GetServerMetrics(string serverName)
    {
        return _serverMetrics.TryGetValue(serverName, out var metrics) ? metrics : null;
    }

    /// <summary>
    /// Get all server metrics
    /// </summary>
    public ServerMetrics[] GetAllMetrics()
    {
        return _serverMetrics.Values.ToArray();
    }

    /// <summary>
    /// Get aggregated metrics across all servers
    /// </summary>
    public AggregatedMetrics GetAggregatedMetrics()
    {
        var all = _serverMetrics.Values.ToArray();
        return new AggregatedMetrics
        {
            TotalServers = all.Length,
            TotalConnections = all.Sum(m => m.TotalConnections),
            ActiveConnections = all.Sum(m => m.ActiveConnections),
            TotalRequests = all.Sum(m => m.TotalRequests),
            TotalErrors = all.Sum(m => m.TotalErrors),
            BytesReceived = all.Sum(m => m.BytesReceived),
            BytesSent = all.Sum(m => m.BytesSent)
        };
    }

    /// <summary>
    /// Generate Prometheus-format metrics for all servers
    /// </summary>
    public string ToPrometheusFormat()
    {
        var sb = new StringBuilder();

        // Add aggregated metrics
        var agg = GetAggregatedMetrics();
        sb.AppendLine("# HELP jumbodogx_total_servers Total number of active servers");
        sb.AppendLine("# TYPE jumbodogx_total_servers gauge");
        sb.AppendLine($"jumbodogx_total_servers {agg.TotalServers}");

        sb.AppendLine("# HELP jumbodogx_total_connections_all Total connections across all servers");
        sb.AppendLine("# TYPE jumbodogx_total_connections_all counter");
        sb.AppendLine($"jumbodogx_total_connections_all {agg.TotalConnections}");

        sb.AppendLine("# HELP jumbodogx_active_connections_all Active connections across all servers");
        sb.AppendLine("# TYPE jumbodogx_active_connections_all gauge");
        sb.AppendLine($"jumbodogx_active_connections_all {agg.ActiveConnections}");

        sb.AppendLine("# HELP jumbodogx_total_requests_all Total requests across all servers");
        sb.AppendLine("# TYPE jumbodogx_total_requests_all counter");
        sb.AppendLine($"jumbodogx_total_requests_all {agg.TotalRequests}");

        sb.AppendLine("# HELP jumbodogx_total_errors_all Total errors across all servers");
        sb.AppendLine("# TYPE jumbodogx_total_errors_all counter");
        sb.AppendLine($"jumbodogx_total_errors_all {agg.TotalErrors}");

        sb.AppendLine("# HELP jumbodogx_bytes_received_all_total Total bytes received across all servers");
        sb.AppendLine("# TYPE jumbodogx_bytes_received_all_total counter");
        sb.AppendLine($"jumbodogx_bytes_received_all_total {agg.BytesReceived}");

        sb.AppendLine("# HELP jumbodogx_bytes_sent_all_total Total bytes sent across all servers");
        sb.AppendLine("# TYPE jumbodogx_bytes_sent_all_total counter");
        sb.AppendLine($"jumbodogx_bytes_sent_all_total {agg.BytesSent}");

        sb.AppendLine();

        // Add individual server metrics
        foreach (var metrics in _serverMetrics.Values.OrderBy(m => m.ServerName))
        {
            sb.Append(metrics.ToPrometheusFormat());
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Reset all metrics
    /// </summary>
    public void ResetAll()
    {
        foreach (var metrics in _serverMetrics.Values)
        {
            metrics.Reset();
        }
    }
}

/// <summary>
/// Aggregated metrics across all servers
/// </summary>
public class AggregatedMetrics
{
    public int TotalServers { get; set; }
    public long TotalConnections { get; set; }
    public long ActiveConnections { get; set; }
    public long TotalRequests { get; set; }
    public long TotalErrors { get; set; }
    public long BytesReceived { get; set; }
    public long BytesSent { get; set; }
}
