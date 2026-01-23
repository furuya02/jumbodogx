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

        // Sanitize message to prevent displaying binary/invalid UTF-8 data
        message = SanitizeMessage(message);

        _logService.AddLog(logLevel, _categoryName, message);
    }

    /// <summary>
    /// メッセージをサニタイズして、不正な文字やバイナリデータを除去
    /// </summary>
    private static string SanitizeMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return message;
        }

        // 制御文字（表示可能な文字以外）を置換
        // ただし、改行(\n, \r)とタブ(\t)は保持
        var chars = message.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            // 制御文字（0x00-0x1F, 0x7F-0x9F）を除外、ただし\t, \n, \rは許可
            if (char.IsControl(c) && c != '\t' && c != '\n' && c != '\r')
            {
                chars[i] = '�'; // 置換文字(U+FFFD)
            }
            // サロゲートペアの不完全な文字を検出
            else if (char.IsSurrogate(c))
            {
                // サロゲートペアの前半だが次の文字がない、または不正
                if (char.IsHighSurrogate(c))
                {
                    if (i + 1 >= chars.Length || !char.IsLowSurrogate(chars[i + 1]))
                    {
                        chars[i] = '�';
                    }
                }
                // サロゲートペアの後半だが前の文字がない、または不正
                else if (char.IsLowSurrogate(c))
                {
                    if (i == 0 || !char.IsHighSurrogate(chars[i - 1]))
                    {
                        chars[i] = '�';
                    }
                }
            }
        }

        return new string(chars);
    }
}
