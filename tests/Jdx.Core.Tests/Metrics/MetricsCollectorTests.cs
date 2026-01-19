using Jdx.Core.Metrics;
using Xunit;

namespace Jdx.Core.Tests.Metrics;

public class MetricsCollectorTests
{
    [Fact]
    public void Instance_ReturnsSingleton()
    {
        // Act
        var instance1 = MetricsCollector.Instance;
        var instance2 = MetricsCollector.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void RegisterServer_AddsMetrics()
    {
        // Arrange
        var collector = MetricsCollector.Instance;
        var metrics = new ServerMetrics("Server1", "Http");

        // Act
        collector.RegisterServer(metrics);
        var retrieved = collector.GetServerMetrics("Server1");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Server1", retrieved.ServerName);
    }

    [Fact]
    public void UnregisterServer_RemovesMetrics()
    {
        // Arrange
        var collector = MetricsCollector.Instance;
        var metrics = new ServerMetrics("Server2", "Ftp");
        collector.RegisterServer(metrics);

        // Act
        collector.UnregisterServer("Server2");
        var retrieved = collector.GetServerMetrics("Server2");

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void GetAllMetrics_ReturnsAllRegisteredServers()
    {
        // Arrange
        var collector = MetricsCollector.Instance;
        // Clear any existing metrics first
        foreach (var m in collector.GetAllMetrics())
        {
            collector.UnregisterServer(m.ServerName);
        }

        var metrics1 = new ServerMetrics("Server3", "Http");
        var metrics2 = new ServerMetrics("Server4", "Ftp");
        collector.RegisterServer(metrics1);
        collector.RegisterServer(metrics2);

        // Act
        var all = collector.GetAllMetrics();

        // Assert
        Assert.Equal(2, all.Length);

        // Cleanup
        collector.UnregisterServer("Server3");
        collector.UnregisterServer("Server4");
    }

    [Fact]
    public void GetAggregatedMetrics_AggregatesAllServers()
    {
        // Arrange
        var collector = MetricsCollector.Instance;
        // Clear any existing metrics first
        foreach (var m in collector.GetAllMetrics())
        {
            collector.UnregisterServer(m.ServerName);
        }

        var metrics1 = new ServerMetrics("Server5", "Http");
        var metrics2 = new ServerMetrics("Server6", "Ftp");

        metrics1.IncrementConnections();
        metrics1.IncrementRequests();
        metrics1.AddBytesReceived(100);

        metrics2.IncrementConnections();
        metrics2.IncrementConnections();
        metrics2.IncrementRequests();
        metrics2.AddBytesReceived(200);

        collector.RegisterServer(metrics1);
        collector.RegisterServer(metrics2);

        // Act
        var aggregated = collector.GetAggregatedMetrics();

        // Assert
        Assert.Equal(2, aggregated.TotalServers);
        Assert.Equal(3, aggregated.TotalConnections);
        Assert.Equal(3, aggregated.ActiveConnections);
        Assert.Equal(2, aggregated.TotalRequests);
        Assert.Equal(300, aggregated.BytesReceived);

        // Cleanup
        collector.UnregisterServer("Server5");
        collector.UnregisterServer("Server6");
    }

    [Fact]
    public void ToPrometheusFormat_GeneratesValidOutput()
    {
        // Arrange
        var collector = MetricsCollector.Instance;
        // Clear any existing metrics first
        foreach (var m in collector.GetAllMetrics())
        {
            collector.UnregisterServer(m.ServerName);
        }

        var metrics = new ServerMetrics("Server7", "Http");
        metrics.IncrementConnections();
        collector.RegisterServer(metrics);

        // Act
        var output = collector.ToPrometheusFormat();

        // Assert
        Assert.Contains("jumbodogx_total_servers", output);
        Assert.Contains("jumbodogx_total_connections_all", output);
        Assert.Contains("jumbodogx_http_total_connections", output);
        Assert.Contains("server=\"Server7\"", output);

        // Cleanup
        collector.UnregisterServer("Server7");
    }

    [Fact]
    public void ResetAll_ResetsAllServerMetrics()
    {
        // Arrange
        var collector = MetricsCollector.Instance;
        // Clear any existing metrics first
        foreach (var m in collector.GetAllMetrics())
        {
            collector.UnregisterServer(m.ServerName);
        }

        var metrics1 = new ServerMetrics("Server8", "Http");
        var metrics2 = new ServerMetrics("Server9", "Ftp");

        metrics1.IncrementConnections();
        metrics2.IncrementConnections();

        collector.RegisterServer(metrics1);
        collector.RegisterServer(metrics2);

        // Act
        collector.ResetAll();

        // Assert
        Assert.Equal(0, metrics1.TotalConnections);
        Assert.Equal(0, metrics2.TotalConnections);

        // Cleanup
        collector.UnregisterServer("Server8");
        collector.UnregisterServer("Server9");
    }
}
