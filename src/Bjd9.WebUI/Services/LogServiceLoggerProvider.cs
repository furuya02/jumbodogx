using Microsoft.Extensions.Logging;

namespace Bjd9.WebUI.Services;

/// <summary>
/// LogService に接続された Logger を提供する Provider
/// </summary>
[ProviderAlias("LogService")]
public class LogServiceLoggerProvider : ILoggerProvider
{
    private readonly LogService _logService;

    public LogServiceLoggerProvider(LogService logService)
    {
        _logService = logService;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LogServiceLogger(categoryName, _logService);
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
