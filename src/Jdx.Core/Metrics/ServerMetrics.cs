using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Jdx.Core.Metrics;

/// <summary>
/// Server metrics collection
/// Tracks connections, requests, errors, and other statistics
/// </summary>
public class ServerMetrics
{
    private long _totalConnections;
    private long _activeConnections;
    private long _totalRequests;
    private long _totalErrors;
    private long _bytesReceived;
    private long _bytesSent;
    private readonly DateTime _startTime;
    private readonly ConcurrentDictionary<string, long> _customCounters;

    public string ServerName { get; }
    public string ServerType { get; }

    public long TotalConnections => Interlocked.Read(ref _totalConnections);
    public long ActiveConnections => Interlocked.Read(ref _activeConnections);
    public long TotalRequests => Interlocked.Read(ref _totalRequests);
    public long TotalErrors => Interlocked.Read(ref _totalErrors);
    public long BytesReceived => Interlocked.Read(ref _bytesReceived);
    public long BytesSent => Interlocked.Read(ref _bytesSent);
    public DateTime StartTime => _startTime;
    public TimeSpan Uptime => DateTime.UtcNow - _startTime;

    public ServerMetrics(string serverName, string serverType)
    {
        ServerName = serverName;
        ServerType = serverType;
        _startTime = DateTime.UtcNow;
        _customCounters = new ConcurrentDictionary<string, long>();
    }

    /// <summary>
    /// Increment total connections counter
    /// </summary>
    public void IncrementConnections()
    {
        Interlocked.Increment(ref _totalConnections);
        Interlocked.Increment(ref _activeConnections);
    }

    /// <summary>
    /// Decrement active connections counter
    /// </summary>
    public void DecrementActiveConnections()
    {
        Interlocked.Decrement(ref _activeConnections);
    }

    /// <summary>
    /// Increment total requests counter
    /// </summary>
    public void IncrementRequests()
    {
        Interlocked.Increment(ref _totalRequests);
    }

    /// <summary>
    /// Increment total errors counter
    /// </summary>
    public void IncrementErrors()
    {
        Interlocked.Increment(ref _totalErrors);
    }

    /// <summary>
    /// Add bytes received
    /// </summary>
    public void AddBytesReceived(long bytes)
    {
        Interlocked.Add(ref _bytesReceived, bytes);
    }

    /// <summary>
    /// Add bytes sent
    /// </summary>
    public void AddBytesSent(long bytes)
    {
        Interlocked.Add(ref _bytesSent, bytes);
    }

    /// <summary>
    /// Increment a custom counter
    /// </summary>
    public void IncrementCustomCounter(string name)
    {
        _customCounters.AddOrUpdate(name, 1, (_, count) => count + 1);
    }

    /// <summary>
    /// Get custom counter value
    /// </summary>
    public long GetCustomCounter(string name)
    {
        return _customCounters.TryGetValue(name, out var value) ? value : 0;
    }

    /// <summary>
    /// Get all custom counters
    /// </summary>
    public ConcurrentDictionary<string, long> GetAllCustomCounters()
    {
        return _customCounters;
    }

    /// <summary>
    /// Reset all metrics
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _totalConnections, 0);
        Interlocked.Exchange(ref _activeConnections, 0);
        Interlocked.Exchange(ref _totalRequests, 0);
        Interlocked.Exchange(ref _totalErrors, 0);
        Interlocked.Exchange(ref _bytesReceived, 0);
        Interlocked.Exchange(ref _bytesSent, 0);
        _customCounters.Clear();
    }

    /// <summary>
    /// Generate Prometheus-format metrics
    /// </summary>
    public string ToPrometheusFormat()
    {
        var lines = new System.Text.StringBuilder();
        var prefix = $"jumbodogx_{ServerType.ToLowerInvariant()}";

        lines.AppendLine($"# HELP {prefix}_total_connections Total number of connections");
        lines.AppendLine($"# TYPE {prefix}_total_connections counter");
        lines.AppendLine($"{prefix}_total_connections{{server=\"{ServerName}\"}} {TotalConnections}");

        lines.AppendLine($"# HELP {prefix}_active_connections Current number of active connections");
        lines.AppendLine($"# TYPE {prefix}_active_connections gauge");
        lines.AppendLine($"{prefix}_active_connections{{server=\"{ServerName}\"}} {ActiveConnections}");

        lines.AppendLine($"# HELP {prefix}_total_requests Total number of requests processed");
        lines.AppendLine($"# TYPE {prefix}_total_requests counter");
        lines.AppendLine($"{prefix}_total_requests{{server=\"{ServerName}\"}} {TotalRequests}");

        lines.AppendLine($"# HELP {prefix}_total_errors Total number of errors");
        lines.AppendLine($"# TYPE {prefix}_total_errors counter");
        lines.AppendLine($"{prefix}_total_errors{{server=\"{ServerName}\"}} {TotalErrors}");

        lines.AppendLine($"# HELP {prefix}_bytes_received_total Total bytes received");
        lines.AppendLine($"# TYPE {prefix}_bytes_received_total counter");
        lines.AppendLine($"{prefix}_bytes_received_total{{server=\"{ServerName}\"}} {BytesReceived}");

        lines.AppendLine($"# HELP {prefix}_bytes_sent_total Total bytes sent");
        lines.AppendLine($"# TYPE {prefix}_bytes_sent_total counter");
        lines.AppendLine($"{prefix}_bytes_sent_total{{server=\"{ServerName}\"}} {BytesSent}");

        lines.AppendLine($"# HELP {prefix}_uptime_seconds Server uptime in seconds");
        lines.AppendLine($"# TYPE {prefix}_uptime_seconds gauge");
        lines.AppendLine($"{prefix}_uptime_seconds{{server=\"{ServerName}\"}} {(long)Uptime.TotalSeconds}");

        // Custom counters
        foreach (var counter in _customCounters)
        {
            var counterName = $"{prefix}_{counter.Key.ToLowerInvariant().Replace(' ', '_')}";
            lines.AppendLine($"# HELP {counterName} Custom counter: {counter.Key}");
            lines.AppendLine($"# TYPE {counterName} counter");
            lines.AppendLine($"{counterName}{{server=\"{ServerName}\"}} {counter.Value}");
        }

        return lines.ToString();
    }
}
