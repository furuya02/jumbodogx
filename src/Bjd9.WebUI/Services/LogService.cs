using System.Collections.Concurrent;

namespace Bjd9.WebUI.Services;

/// <summary>
/// ログメッセージを収集・管理するサービス
/// </summary>
public class LogService
{
    private readonly ConcurrentQueue<LogEntry> _logs = new();
    private const int MaxLogEntries = 1000;

    public event EventHandler<LogEntry>? LogAdded;

    public void AddLog(LogLevel level, string category, string message)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Category = category,
            Message = message
        };

        _logs.Enqueue(entry);

        // 最大数を超えたら古いログを削除
        while (_logs.Count > MaxLogEntries)
        {
            _logs.TryDequeue(out _);
        }

        OnLogAdded(entry);
    }

    public IEnumerable<LogEntry> GetRecentLogs(int count = 100)
    {
        return _logs.TakeLast(count).ToList();
    }

    public IEnumerable<LogEntry> GetLogsByLevel(LogLevel level)
    {
        return _logs.Where(l => l.Level == level).ToList();
    }

    public void ClearLogs()
    {
        _logs.Clear();
    }

    protected virtual void OnLogAdded(LogEntry entry)
    {
        LogAdded?.Invoke(this, entry);
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Category { get; set; } = "";
    public string Message { get; set; } = "";

    public string LevelClass => Level switch
    {
        LogLevel.Error or LogLevel.Critical => "text-danger",
        LogLevel.Warning => "text-warning",
        LogLevel.Information => "text-info",
        LogLevel.Debug or LogLevel.Trace => "text-secondary",
        _ => ""
    };

    public string LevelIcon => Level switch
    {
        LogLevel.Error or LogLevel.Critical => "❌",
        LogLevel.Warning => "⚠️",
        LogLevel.Information => "ℹ️",
        LogLevel.Debug or LogLevel.Trace => "🔍",
        _ => "📝"
    };
}
