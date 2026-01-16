using Jdx.Servers.Dns;
using Microsoft.Extensions.Logging;

Console.WriteLine("JumboDogX - Multi-Server Application (CLI)");
Console.WriteLine("=====================================");
Console.WriteLine();
Console.WriteLine("Note: For full server management with HTTP, use the WebUI:");
Console.WriteLine("  dotnet run --project src/Jdx.WebUI --urls \"http://localhost:5000\"");
Console.WriteLine();

// Create logger factory with console output
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddConsole();
});

var dnsLogger = loggerFactory.CreateLogger<DnsServer>();

// Create DNS server
var dnsServer = new DnsServer(dnsLogger, 5300);

// Add some DNS records for testing
dnsServer.AddRecord("example.com", "192.0.2.1");
dnsServer.AddRecord("jdx.local", "127.0.0.1");
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
    // Start DNS server
    await dnsServer.StartAsync(cts.Token);

    Console.WriteLine();
    Console.WriteLine("DNS Server started. Press Ctrl+C to stop.");
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
    // Stop server
    await dnsServer.StopAsync(CancellationToken.None);
    dnsServer.Dispose();
    Console.WriteLine("DNS Server stopped.");
}

return 0;
