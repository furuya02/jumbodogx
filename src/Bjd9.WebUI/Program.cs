using Bjd9.WebUI.Components;
using Bjd9.WebUI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add BJD9 services
builder.Services.AddSingleton<LogService>();
builder.Services.AddSingleton<ServerManager>();

var app = builder.Build();

// Add LogServiceLoggerProvider to capture all logs
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logService = app.Services.GetRequiredService<LogService>();
loggerFactory.AddProvider(new LogServiceLoggerProvider(logService));

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

app.Run();
