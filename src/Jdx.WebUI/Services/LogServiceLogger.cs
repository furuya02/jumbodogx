using Microsoft.Extensions.Logging;

namespace Jdx.WebUI.Services;

/// <summary>
/// LogService にログを転送する ILogger 実装
/// </summary>
public class LogServiceLogger : ILogger
{
    private readonly string _categoryName;
    private readonly LogService _logService;

    public LogServiceLogger(string categoryName, LogService logService)
    {
        _categoryName = categoryName;
        _logService = logService;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);

        if (exception != null)
        {
            message += $"\n{exception}";
        }

        _logService.AddLog(logLevel, _categoryName, message);
    }
}
