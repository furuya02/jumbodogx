using Jdx.Core.Settings;
using Jdx.WebUI.Components;
using Jdx.WebUI.Services;
using Serilog;
using Serilog.Formatting.Compact;

// Ensure logs directory exists
Directory.CreateDirectory("logs");

// Configure Serilog with structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "JumboDogX.WebUI")
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(
        new CompactJsonFormatter(),
        "logs/jumbodogx-webui.log",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 10_485_760, // 10MB
        retainedFileCountLimit: 30)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog to the logging pipeline
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add JumboDogX services
builder.Services.AddSingleton<LogService>();
builder.Services.AddSingleton<ISettingsService, SettingsService>();
builder.Services.AddSingleton<ServerManager>();

var app = builder.Build();

// Add LogServiceLoggerProvider to capture all logs
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logService = app.Services.GetRequiredService<LogService>();
loggerFactory.AddProvider(new LogServiceLoggerProvider(logService));

// Initialize ServerManager to start servers
var serverManager = app.Services.GetRequiredService<ServerManager>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

try
{
    app.Run();
}
finally
{
    // Flush and close Serilog
    await Log.CloseAndFlushAsync();
}
