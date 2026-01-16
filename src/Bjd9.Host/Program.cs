using Bjd9.Servers.Http;
using Bjd9.Servers.Dns;
using Microsoft.Extensions.Logging;

Console.WriteLine("BJD9 - Multi-Server Application");
Console.WriteLine("================================");
Console.WriteLine();

// Create logger factory with console output
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddConsole();
});

var httpLogger = loggerFactory.CreateLogger<HttpServer>();
var dnsLogger = loggerFactory.CreateLogger<DnsServer>();

// Create servers
var httpServer = new HttpServer(httpLogger, port: 8080);
var dnsServer = new DnsServer(dnsLogger, port: 5300);

// Add some DNS records for testing
dnsServer.AddRecord("example.com", "192.0.2.1");
dnsServer.AddRecord("bjd9.local", "127.0.0.1");
dnsServer.AddRecord("test.local", "192.168.1.100");

// Create cancellation token source for graceful shutdown
using var cts = new CancellationTokenSource();

// Handle Ctrl+C for graceful shutdown
Console.CancelKeyPress += (sender, eventArgs) =>
{
    Console.WriteLine();
    Console.WriteLine("Shutdown signal received. Stopping servers...");
    eventArgs.Cancel = true;
    cts.Cancel();
};

try
{
    // Start all servers
    await httpServer.StartAsync(cts.Token);
    await dnsServer.StartAsync(cts.Token);

    Console.WriteLine();
    Console.WriteLine("All servers started. Press Ctrl+C to stop.");
    Console.WriteLine("  - HTTP Server: http://localhost:8080");
    Console.WriteLine("  - DNS Server: port 5300 (use: dig @localhost -p 5300 example.com)");
    Console.WriteLine();

    // Wait until cancellation is requested
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{
    // Expected when Ctrl+C is pressed
    Console.WriteLine("Shutting down...");
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    return 1;
}
finally
{
    // Stop all servers
    await httpServer.StopAsync(CancellationToken.None);
    await dnsServer.StopAsync(CancellationToken.None);
    httpServer.Dispose();
    dnsServer.Dispose();
    Console.WriteLine("All servers stopped.");
}

return 0;
