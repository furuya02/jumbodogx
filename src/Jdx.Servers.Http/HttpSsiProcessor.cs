using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Http;

/// <summary>
/// SSI (Server Side Includes) プロセッサ
/// </summary>
public class HttpSsiProcessor
{
    private readonly HttpServerSettings _settings;
    private readonly ILogger _logger;

    // SSIディレクティブのパターン
    private static readonly Regex SsiPattern = new Regex(
        @"<!--#(\w+)\s+([^-]+)-->",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public HttpSsiProcessor(HttpServerSettings _settings, ILogger logger)
    {
        this._settings = _settings;
        _logger = logger;
    }

    /// <summary>
    /// ファイルがSSI処理対象かどうか判定
    /// </summary>
    public bool IsSsiFile(string filePath)
    {
        if (!_settings.UseSsi || string.IsNullOrWhiteSpace(_settings.SsiExt))
        {
            return false;
        }

        var extension = Path.GetExtension(filePath).TrimStart('.');
        var ssiExtensions = _settings.SsiExt.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var ext in ssiExtensions)
        {
            if (ext.Trim().Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// HTMLコンテンツのSSI処理
    /// </summary>
    public async Task<string> ProcessSsiAsync(string content, string baseFilePath)
    {
        _logger.LogDebug("Processing SSI for file: {FilePath}", baseFilePath);

        var result = SsiPattern.Replace(content, match =>
        {
            var directive = match.Groups[1].Value.ToLowerInvariant();
            var parameters = match.Groups[2].Value.Trim();

            try
            {
                return directive switch
                {
                    "include" => ProcessInclude(parameters, baseFilePath),
                    "echo" => ProcessEcho(parameters),
                    "exec" => ProcessExec(parameters),
                    "config" => ProcessConfig(parameters),
                    "fsize" => ProcessFsize(parameters, baseFilePath),
                    "flastmod" => ProcessFlastmod(parameters, baseFilePath),
                    _ => $"[Unknown SSI directive: {directive}]"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SSI directive: {Directive}", directive);
                return $"[Error in SSI: {directive}]";
            }
        });

        return await Task.FromResult(result);
    }

    /// <summary>
    /// #include ディレクティブ処理
    /// </summary>
    private string ProcessInclude(string parameters, string baseFilePath)
    {
        var parts = parameters.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return "[Invalid include syntax]";
        }

        var type = parts[0].ToLowerInvariant();
        var path = parts[1].Trim('"', '\'');

        string includePath;

        if (type == "file")
        {
            // file: 同じディレクトリからの相対パス
            var baseDir = Path.GetDirectoryName(baseFilePath) ?? _settings.DocumentRoot;
            includePath = Path.Combine(baseDir, path);
        }
        else if (type == "virtual")
        {
            // virtual: DocumentRootからの絶対パス
            includePath = Path.Combine(_settings.DocumentRoot, path.TrimStart('/'));
        }
        else
        {
            return $"[Unknown include type: {type}]";
        }

        // セキュリティチェック: パストラバーサル防止
        var fullPath = Path.GetFullPath(includePath);
        var allowedRoot = Path.GetFullPath(_settings.DocumentRoot);

        if (!fullPath.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("SSI include path traversal attempt blocked: {Path}", includePath);
            return "[Access denied]";
        }

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("SSI include file not found: {Path}", fullPath);
            return $"[File not found: {path}]";
        }

        try
        {
            var content = File.ReadAllText(fullPath, Encoding.UTF8);
            _logger.LogDebug("SSI included file: {Path}", fullPath);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading SSI include file: {Path}", fullPath);
            return $"[Error reading file: {path}]";
        }
    }

    /// <summary>
    /// #echo ディレクティブ処理
    /// </summary>
    private string ProcessEcho(string parameters)
    {
        var parts = parameters.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !parts[0].Equals("var", StringComparison.OrdinalIgnoreCase))
        {
            return "[Invalid echo syntax]";
        }

        var varName = parts[1].Trim('"', '\'');

        // サポートする変数
        return varName.ToUpperInvariant() switch
        {
            "DATE_LOCAL" => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            "DATE_GMT" => DateTime.UtcNow.ToString("R"),
            "LAST_MODIFIED" => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            "DOCUMENT_NAME" => Path.GetFileName(_settings.DocumentRoot),
            "DOCUMENT_URI" => "/",
            "SERVER_SOFTWARE" => _settings.ServerHeader.Replace("$v", "9.0.0"),
            _ => $"[Unknown variable: {varName}]"
        };
    }

    /// <summary>
    /// #exec ディレクティブ処理
    /// </summary>
    private string ProcessExec(string parameters)
    {
        if (!_settings.UseExec)
        {
            _logger.LogWarning("SSI exec directive disabled");
            return "[exec disabled]";
        }

        var parts = parameters.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return "[Invalid exec syntax]";
        }

        var type = parts[0].ToLowerInvariant();
        var command = parts[1].Trim('"', '\'');

        if (type == "cmd")
        {
            // コマンド実行（セキュリティリスクが高い）
            _logger.LogWarning("SSI exec cmd: {Command}", command);

            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/bin/sh",
                        Arguments = $"-c \"{command}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000); // 5秒タイムアウト

                if (!process.HasExited)
                {
                    process.Kill();
                    return "[Command timeout]";
                }

                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SSI command: {Command}", command);
                return $"[Error executing command]";
            }
        }

        return $"[Unknown exec type: {type}]";
    }

    /// <summary>
    /// #config ディレクティブ処理
    /// </summary>
    private string ProcessConfig(string parameters)
    {
        // config は設定変更なので、実際には何も出力しない
        // TODO: timefmt, sizefmt等の設定を保持する実装
        return string.Empty;
    }

    /// <summary>
    /// #fsize ディレクティブ処理（ファイルサイズ表示）
    /// </summary>
    private string ProcessFsize(string parameters, string baseFilePath)
    {
        var parts = parameters.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return "[Invalid fsize syntax]";
        }

        var type = parts[0].ToLowerInvariant();
        var path = parts[1].Trim('"', '\'');

        string targetPath = GetTargetPath(type, path, baseFilePath);
        if (targetPath == null)
        {
            return "[File not found]";
        }

        try
        {
            var fileInfo = new FileInfo(targetPath);
            return FormatFileSize(fileInfo.Length);
        }
        catch
        {
            return "[Error getting file size]";
        }
    }

    /// <summary>
    /// #flastmod ディレクティブ処理（最終更新日時表示）
    /// </summary>
    private string ProcessFlastmod(string parameters, string baseFilePath)
    {
        var parts = parameters.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return "[Invalid flastmod syntax]";
        }

        var type = parts[0].ToLowerInvariant();
        var path = parts[1].Trim('"', '\'');

        string? targetPath = GetTargetPath(type, path, baseFilePath);
        if (targetPath == null)
        {
            return "[File not found]";
        }

        try
        {
            var fileInfo = new FileInfo(targetPath);
            return fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {
            return "[Error getting last modified]";
        }
    }

    /// <summary>
    /// ターゲットパスを解決
    /// </summary>
    private string? GetTargetPath(string type, string path, string baseFilePath)
    {
        string targetPath;

        if (type == "file")
        {
            var baseDir = Path.GetDirectoryName(baseFilePath) ?? _settings.DocumentRoot;
            targetPath = Path.Combine(baseDir, path);
        }
        else if (type == "virtual")
        {
            targetPath = Path.Combine(_settings.DocumentRoot, path.TrimStart('/'));
        }
        else
        {
            return null;
        }

        var fullPath = Path.GetFullPath(targetPath);
        var allowedRoot = Path.GetFullPath(_settings.DocumentRoot);

        if (!fullPath.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return File.Exists(fullPath) ? fullPath : null;
    }

    /// <summary>
    /// ファイルサイズをフォーマット
    /// </summary>
    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
