using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Http;

/// <summary>
/// CGI (Common Gateway Interface) ハンドラ
/// </summary>
public class HttpCgiHandler
{
    private readonly HttpServerSettings _settings;
    private readonly ILogger _logger;

    public HttpCgiHandler(HttpServerSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// リクエストがCGIスクリプトかどうか判定
    /// </summary>
    public bool IsCgiRequest(string requestPath, out string scriptPath, out string interpreter)
    {
        scriptPath = string.Empty;
        interpreter = string.Empty;

        if (!_settings.UseCgi)
        {
            return false;
        }

        // CgiPathsをチェック
        if (_settings.CgiPaths != null)
        {
            foreach (var cgiPath in _settings.CgiPaths)
            {
                if (requestPath.StartsWith(cgiPath.Path, StringComparison.OrdinalIgnoreCase))
                {
                    // CGIパス配下
                    var relativePath = requestPath.Substring(cgiPath.Path.Length).TrimStart('/');
                    scriptPath = Path.Combine(cgiPath.Directory, relativePath);

                    // 拡張子からインタプリタを判定
                    var extension = Path.GetExtension(scriptPath).TrimStart('.');
                    interpreter = GetInterpreterForExtension(extension);

                    if (!string.IsNullOrEmpty(interpreter) && File.Exists(scriptPath))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 拡張子に対応するインタプリタを取得
    /// </summary>
    private string GetInterpreterForExtension(string extension)
    {
        if (_settings.CgiCommands == null)
        {
            return string.Empty;
        }

        foreach (var cmd in _settings.CgiCommands)
        {
            if (cmd.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                return cmd.Program;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// CGIスクリプトを実行
    /// </summary>
    public async Task<HttpResponse> ExecuteCgiAsync(
        string scriptPath,
        string interpreter,
        HttpRequest request,
        string remoteAddress,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing CGI script: {ScriptPath} with {Interpreter}", scriptPath, interpreter);

        try
        {
            // プロセス起動設定
            var startInfo = new ProcessStartInfo
            {
                FileName = interpreter,
                Arguments = $"\"{scriptPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? Directory.GetCurrentDirectory()
            };

            // CGI環境変数を設定
            SetCgiEnvironmentVariables(startInfo.Environment, request, scriptPath, remoteAddress);

            using var process = new Process { StartInfo = startInfo };

            // タイムアウト設定
            var timeout = _settings.CgiTimeout > 0 ? _settings.CgiTimeout : 30;
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeout));

            process.Start();

            // POSTデータがある場合は標準入力に書き込み
            if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(request.Body))
            {
                await process.StandardInput.WriteAsync(request.Body);
                process.StandardInput.Close();
            }

            // 出力を読み取り
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            // プロセスの完了を待つ
            var processExited = await Task.Run(() => process.WaitForExit((int)timeout * 1000), cts.Token);

            if (!processExited)
            {
                _logger.LogWarning("CGI script timeout: {ScriptPath}", scriptPath);
                try
                {
                    process.Kill();
                }
                catch { }

                return HttpResponseBuilder.BuildErrorResponse(504, "Gateway Timeout", _settings);
            }

            var output = await outputTask;
            var error = await errorTask;

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning("CGI script stderr: {Error}", error);
            }

            // CGI出力をパース
            return ParseCgiOutput(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing CGI script: {ScriptPath}", scriptPath);
            return HttpResponseBuilder.BuildErrorResponse(500, "Internal Server Error", _settings);
        }
    }

    /// <summary>
    /// CGI環境変数を設定
    /// </summary>
    private void SetCgiEnvironmentVariables(
        IDictionary<string, string?> env,
        HttpRequest request,
        string scriptPath,
        string remoteAddress)
    {
        env["GATEWAY_INTERFACE"] = "CGI/1.1";
        env["SERVER_PROTOCOL"] = request.Version;
        env["SERVER_SOFTWARE"] = _settings.ServerHeader.Replace("$v", "9.0.0");
        env["REQUEST_METHOD"] = request.Method;
        env["SCRIPT_NAME"] = request.Path;
        env["SCRIPT_FILENAME"] = scriptPath;
        env["PATH_INFO"] = "";
        env["PATH_TRANSLATED"] = scriptPath;
        env["QUERY_STRING"] = request.QueryString ?? "";
        env["REMOTE_ADDR"] = remoteAddress;
        env["REMOTE_HOST"] = remoteAddress;
        env["REQUEST_URI"] = request.Path + (string.IsNullOrEmpty(request.QueryString) ? "" : "?" + request.QueryString);
        env["SERVER_NAME"] = request.Headers.ContainsKey("Host") ? request.Headers["Host"] : "localhost";
        env["SERVER_PORT"] = "8080"; // TODO: 実際のポート番号

        // HTTPヘッダーを環境変数に追加
        foreach (var header in request.Headers)
        {
            var envName = "HTTP_" + header.Key.ToUpperInvariant().Replace("-", "_");
            env[envName] = header.Value;
        }

        // Content-Type と Content-Length
        if (request.Headers.ContainsKey("Content-Type"))
        {
            env["CONTENT_TYPE"] = request.Headers["Content-Type"];
        }

        if (!string.IsNullOrEmpty(request.Body))
        {
            env["CONTENT_LENGTH"] = Encoding.UTF8.GetByteCount(request.Body).ToString();
        }
    }

    /// <summary>
    /// CGI出力をパース（ヘッダーとボディを分離）
    /// </summary>
    private HttpResponse ParseCgiOutput(string output)
    {
        var response = new HttpResponse
        {
            StatusCode = 200,
            StatusText = "OK",
            Headers = new Dictionary<string, string>
            {
                ["Server"] = _settings.ServerHeader.Replace("$v", "9.0.0"),
                ["Date"] = DateTime.UtcNow.ToString("R")
            }
        };

        // ヘッダーとボディを分離（空行で区切られる）
        var parts = output.Split(new[] { "\r\n\r\n", "\n\n" }, 2, StringSplitOptions.None);

        if (parts.Length == 2)
        {
            // ヘッダー部分をパース
            var headerLines = parts[0].Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in headerLines)
            {
                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    var key = line.Substring(0, colonIndex).Trim();
                    var value = line.Substring(colonIndex + 1).Trim();

                    // Statusヘッダーの特別処理
                    if (key.Equals("Status", StringComparison.OrdinalIgnoreCase))
                    {
                        var statusParts = value.Split(' ', 2);
                        if (statusParts.Length >= 1 && int.TryParse(statusParts[0], out var statusCode))
                        {
                            response.StatusCode = statusCode;
                            response.StatusText = statusParts.Length >= 2 ? statusParts[1] : "OK";
                        }
                    }
                    else
                    {
                        response.Headers[key] = value;
                    }
                }
            }

            // ボディ部分
            response.Body = parts[1];
        }
        else
        {
            // ヘッダーなし、全てボディとして扱う
            response.Body = output;
            response.Headers["Content-Type"] = "text/html; charset=utf-8";
        }

        return response;
    }
}
