using Jdx.Core.Metrics;
using Xunit;

namespace Jdx.Core.Tests.Metrics;

public class ServerMetricsTests
{
    [Fact]
    public void Constructor_SetsServerNameAndType()
    {
        // Arrange & Act
        var metrics = new ServerMetrics("TestServer", "Http");

        // Assert
        Assert.Equal("TestServer", metrics.ServerName);
        Assert.Equal("Http", metrics.ServerType);
    }

    [Fact]
    public void IncrementConnections_IncrementsCounters()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Act
        metrics.IncrementConnections();
        metrics.IncrementConnections();

        // Assert
        Assert.Equal(2, metrics.TotalConnections);
        Assert.Equal(2, metrics.ActiveConnections);
    }

    [Fact]
    public void DecrementActiveConnections_DecrementsCounter()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");
        metrics.IncrementConnections();
        metrics.IncrementConnections();

        // Act
        metrics.DecrementActiveConnections();

        // Assert
        Assert.Equal(2, metrics.TotalConnections);
        Assert.Equal(1, metrics.ActiveConnections);
    }

    [Fact]
    public void IncrementRequests_IncrementsCounter()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Act
        metrics.IncrementRequests();
        metrics.IncrementRequests();
        metrics.IncrementRequests();

        // Assert
        Assert.Equal(3, metrics.TotalRequests);
    }

    [Fact]
    public void IncrementErrors_IncrementsCounter()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Act
        metrics.IncrementErrors();

        // Assert
        Assert.Equal(1, metrics.TotalErrors);
    }

    [Fact]
    public void AddBytesReceived_AddsToCounter()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Act
        metrics.AddBytesReceived(100);
        metrics.AddBytesReceived(200);

        // Assert
        Assert.Equal(300, metrics.BytesReceived);
    }

    [Fact]
    public void AddBytesSent_AddsToCounter()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Act
        metrics.AddBytesSent(500);
        metrics.AddBytesSent(300);

        // Assert
        Assert.Equal(800, metrics.BytesSent);
    }

    [Fact]
    public void IncrementCustomCounter_CreatesAndIncrementsCounter()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Act
        metrics.IncrementCustomCounter("test_counter");
        metrics.IncrementCustomCounter("test_counter");
        metrics.IncrementCustomCounter("another_counter");

        // Assert
        Assert.Equal(2, metrics.GetCustomCounter("test_counter"));
        Assert.Equal(1, metrics.GetCustomCounter("another_counter"));
        Assert.Equal(0, metrics.GetCustomCounter("nonexistent"));
    }

    [Fact]
    public void GetAllCustomCounters_ReturnsSnapshot()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");
        metrics.IncrementCustomCounter("counter1");
        metrics.IncrementCustomCounter("counter2");

        // Act
        var counters = metrics.GetAllCustomCounters();

        // Assert
        Assert.Equal(2, counters.Count);
        Assert.True(counters.ContainsKey("counter1"));
        Assert.True(counters.ContainsKey("counter2"));
    }

    [Fact]
    public void Reset_ResetsAllCounters()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");
        metrics.IncrementConnections();
        metrics.IncrementRequests();
        metrics.IncrementErrors();
        metrics.AddBytesReceived(100);
        metrics.AddBytesSent(200);
        metrics.IncrementCustomCounter("test");

        // Act
        metrics.Reset();

        // Assert
        Assert.Equal(0, metrics.TotalConnections);
        Assert.Equal(0, metrics.ActiveConnections);
        Assert.Equal(0, metrics.TotalRequests);
        Assert.Equal(0, metrics.TotalErrors);
        Assert.Equal(0, metrics.BytesReceived);
        Assert.Equal(0, metrics.BytesSent);
        Assert.Equal(0, metrics.GetCustomCounter("test"));
    }

    [Fact]
    public void ToPrometheusFormat_GeneratesValidOutput()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");
        metrics.IncrementConnections();
        metrics.IncrementRequests();
        metrics.AddBytesReceived(100);

        // Act
        var output = metrics.ToPrometheusFormat();

        // Assert
        Assert.Contains("jumbodogx_http_total_connections", output);
        Assert.Contains("jumbodogx_http_active_connections", output);
        Assert.Contains("jumbodogx_http_total_requests", output);
        Assert.Contains("jumbodogx_http_bytes_received_total", output);
        Assert.Contains("server=\"TestServer\"", output);
    }

    [Fact]
    public void ToPrometheusFormat_SanitizesMetricNames()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");
        metrics.IncrementCustomCounter("test-counter.with/special@chars");

        // Act
        var output = metrics.ToPrometheusFormat();

        // Assert
        // All special characters should be replaced with underscores
        Assert.Contains("jumbodogx_http_test_counter_with_special_chars", output);
    }

    [Fact]
    public void StartTime_ReturnsCreationTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var metrics = new ServerMetrics("TestServer", "Http");
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(metrics.StartTime >= beforeCreation);
        Assert.True(metrics.StartTime <= afterCreation);
    }

    [Fact]
    public void Uptime_ReturnsPositiveTimeSpan()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Small delay to ensure uptime is measurable
        Thread.Sleep(10);

        // Act
        var uptime = metrics.Uptime;

        // Assert
        Assert.True(uptime.TotalMilliseconds >= 10);
    }

    [Fact]
    public void ToPrometheusFormat_IncludesUptimeMetric()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Act
        var output = metrics.ToPrometheusFormat();

        // Assert
        Assert.Contains("jumbodogx_http_uptime_seconds", output);
        Assert.Contains("TYPE jumbodogx_http_uptime_seconds gauge", output);
    }

    [Fact]
    public void ToPrometheusFormat_IncludesAllStandardMetrics()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Dns");

        // Act
        var output = metrics.ToPrometheusFormat();

        // Assert
        Assert.Contains("jumbodogx_dns_total_connections", output);
        Assert.Contains("jumbodogx_dns_active_connections", output);
        Assert.Contains("jumbodogx_dns_total_requests", output);
        Assert.Contains("jumbodogx_dns_total_errors", output);
        Assert.Contains("jumbodogx_dns_bytes_received_total", output);
        Assert.Contains("jumbodogx_dns_bytes_sent_total", output);
        Assert.Contains("jumbodogx_dns_uptime_seconds", output);
    }

    [Fact]
    public void ToPrometheusFormat_IncludesHelpAndTypeComments()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Act
        var output = metrics.ToPrometheusFormat();

        // Assert
        Assert.Contains("# HELP jumbodogx_http_total_connections Total number of connections", output);
        Assert.Contains("# TYPE jumbodogx_http_total_connections counter", output);
        Assert.Contains("# TYPE jumbodogx_http_active_connections gauge", output);
    }

    [Fact]
    public void IncrementCustomCounter_ThreadSafe()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");
        var iterations = 1000;
        var threadCount = 10;

        // Act
        var tasks = Enumerable.Range(0, threadCount)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    metrics.IncrementCustomCounter("concurrent_counter");
                }
            }))
            .ToArray();

        Task.WaitAll(tasks);

        // Assert
        Assert.Equal(iterations * threadCount, metrics.GetCustomCounter("concurrent_counter"));
    }

    [Fact]
    public void AllMetrics_ThreadSafe()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");
        var iterations = 100;
        var threadCount = 10;

        // Act
        var tasks = Enumerable.Range(0, threadCount)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    metrics.IncrementConnections();
                    metrics.IncrementRequests();
                    metrics.IncrementErrors();
                    metrics.AddBytesReceived(100);
                    metrics.AddBytesSent(200);
                    metrics.DecrementActiveConnections();
                }
            }))
            .ToArray();

        Task.WaitAll(tasks);

        // Assert - all operations should complete without exception
        var expectedTotal = iterations * threadCount;
        Assert.Equal(expectedTotal, metrics.TotalConnections);
        Assert.Equal(expectedTotal, metrics.TotalRequests);
        Assert.Equal(expectedTotal, metrics.TotalErrors);
        Assert.Equal(expectedTotal * 100, metrics.BytesReceived);
        Assert.Equal(expectedTotal * 200, metrics.BytesSent);
        Assert.Equal(0, metrics.ActiveConnections); // Incremented and decremented same number of times
    }

    [Fact]
    public void GetCustomCounter_NonExistentCounter_ReturnsZero()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Act
        var value = metrics.GetCustomCounter("nonexistent_counter");

        // Assert
        Assert.Equal(0, value);
    }

    [Fact]
    public void GetAllCustomCounters_EmptyInitially_ReturnsEmptyDictionary()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");

        // Act
        var counters = metrics.GetAllCustomCounters();

        // Assert
        Assert.Empty(counters);
    }

    [Fact]
    public void ToPrometheusFormat_WithCustomCounters_IncludesAllCustomCounters()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");
        metrics.IncrementCustomCounter("get_requests");
        metrics.IncrementCustomCounter("get_requests");
        metrics.IncrementCustomCounter("post_requests");

        // Act
        var output = metrics.ToPrometheusFormat();

        // Assert
        Assert.Contains("jumbodogx_http_get_requests", output);
        Assert.Contains("jumbodogx_http_post_requests", output);
    }

    [Fact]
    public void Reset_ClearsAllCustomCounters()
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", "Http");
        metrics.IncrementCustomCounter("counter1");
        metrics.IncrementCustomCounter("counter2");
        metrics.IncrementCustomCounter("counter3");

        // Act
        metrics.Reset();
        var counters = metrics.GetAllCustomCounters();

        // Assert
        Assert.Empty(counters);
    }

    [Theory]
    [InlineData("Http")]
    [InlineData("Dns")]
    [InlineData("Smtp")]
    [InlineData("Pop3")]
    [InlineData("Ftp")]
    public void ToPrometheusFormat_DifferentServerTypes_UsesCorrectPrefix(string serverType)
    {
        // Arrange
        var metrics = new ServerMetrics("TestServer", serverType);

        // Act
        var output = metrics.ToPrometheusFormat();

        // Assert
        Assert.Contains($"jumbodogx_{serverType.ToLowerInvariant()}_total_connections", output);
    }
}
