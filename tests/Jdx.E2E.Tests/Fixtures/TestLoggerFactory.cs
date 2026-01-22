using Microsoft.Extensions.Logging;

namespace Jdx.E2E.Tests.Fixtures;

/// <summary>
/// E2Eテスト用のロガーファクトリ
/// </summary>
public static class TestLoggerFactory
{
    /// <summary>
    /// テスト用のロガーを作成
    /// </summary>
    public static ILogger<T> CreateLogger<T>()
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddConsole();
        });
        return factory.CreateLogger<T>();
    }

    /// <summary>
    /// Nullロガーを作成（ログ出力なし）
    /// </summary>
    public static ILogger<T> CreateNullLogger<T>()
    {
        return new NullLogger<T>();
    }
}

/// <summary>
/// 何も出力しないロガー
/// </summary>
public class NullLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
