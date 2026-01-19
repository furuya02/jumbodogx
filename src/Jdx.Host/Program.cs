using System.Net;
using Jdx.Core.Settings;
using Jdx.Servers.Dns;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

Console.WriteLine("JumboDogX - Multi-Server Application (CLI)");
Console.WriteLine("=====================================");
Console.WriteLine();
Console.WriteLine("Note: For full server management with HTTP, use the WebUI:");
Console.WriteLine("  dotnet run --project src/Jdx.WebUI --urls \"http://localhost:5000\"");
Console.WriteLine();

// Configure Serilog with structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "JumboDogX.Host")
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(
        new CompactJsonFormatter(),
        "logs/jumbodogx-host-.log",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 10_485_760, // 10MB
        retainedFileCountLimit: 30)
    .CreateLogger();

// Create logger factory with Serilog
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddSerilog(dispose: true);
});

var dnsLogger = loggerFactory.CreateLogger<DnsServer>();

// Create DNS server settings
var dnsSettings = new DnsServerSettings
{
    Enabled = true,
    Port = 5300,
    MaxConnections = 10,
    TimeOut = 30,
    RootCache = "named.ca",
    UseRecursion = false,
    SoaMail = "postmaster",
    SoaSerial = 1,
    SoaRefresh = 3600,
    SoaRetry = 300,
    SoaExpire = 360000,
    SoaMinimum = 3600,
    DomainList = new List<DnsDomainEntry>
    {
        new() { Name = "localhost", IsAuthority = true },
        new() { Name = "test.local", IsAuthority = true }
    },
    ResourceList = new List<DnsResourceEntry>
    {
        new() { Type = DnsType.A, Name = "localhost", Address = "127.0.0.1" },
        new() { Type = DnsType.Aaaa, Name = "localhost", Address = "::1" },
        new() { Type = DnsType.A, Name = "example.com", Address = "192.0.2.1" },
        new() { Type = DnsType.A, Name = "jdx.local", Address = "127.0.0.1" },
        new() { Type = DnsType.A, Name = "test.local", Address = "192.168.1.100" }
    }
};

// Create DNS server
var dnsServer = new DnsServer(dnsLogger, dnsSettings);

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

    // Flush and close Serilog
    await Log.CloseAndFlushAsync();
}

return 0;
