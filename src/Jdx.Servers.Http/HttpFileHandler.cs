using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Jdx.Core.Settings;

namespace Jdx.Servers.Http;

/// <summary>
/// ファイルハンドリングとレスポンス生成を担当するクラス
/// </summary>
public class HttpFileHandler
{
    private readonly ILogger _logger;
    private readonly HttpContentType _contentType;
    private HttpSsiProcessor? _ssiProcessor;

    // 大きなファイルの閾値（10MB）
    private const long LargeFileThreshold = 10 * 1024 * 1024;

    public HttpFileHandler(ILogger logger, HttpContentType contentType)
    {
        _logger = logger;
        _contentType = contentType;
    }

    public void SetSsiProcessor(HttpSsiProcessor ssiProcessor)
    {
        _ssiProcessor = ssiProcessor;
    }

    /// <summary>
    /// ファイルリクエストを処理する
    /// </summary>
    public async Task<HttpResponse> HandleFileAsync(
        TargetInfo target,
        HttpRequest request,
        HttpServerSettings settings,
        CancellationToken cancellationToken)
    {
        try
        {
            // ターゲットが無効な場合
            if (!target.IsValid)
            {
                return CreateErrorResponse(404, "Not Found", target.ErrorMessage, settings);
            }

            // ディレクトリの場合
            if (target.Type == TargetType.Directory)
            {
                return await HandleDirectoryAsync(target, settings, cancellationToken);
            }

            // 静的ファイルの場合
            if (target.Type == TargetType.StaticFile && target.FileInfo != null)
            {
                return await HandleStaticFileAsync(target, request, settings, cancellationToken);
            }

            // その他（本来ここには来ないはず）
            return CreateErrorResponse(500, "Internal Server Error", "Unexpected target type", settings);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access denied to file: {Path}", target.PhysicalPath);
            return CreateErrorResponse(403, "Forbidden", "Access denied", settings);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found: {Path}", target.PhysicalPath);
            return CreateErrorResponse(404, "Not Found", "File not found", settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling file request: {Path}", target.PhysicalPath);
            return CreateErrorResponse(500, "Internal Server Error", "An error occurred while processing the request", settings);
        }
    }

    /// <summary>
    /// 静的ファイルを処理する
    /// </summary>
    private async Task<HttpResponse> HandleStaticFileAsync(
        TargetInfo target,
        HttpRequest request,
        HttpServerSettings settings,
        CancellationToken cancellationToken)
    {
        var filePath = target.PhysicalPath;
        var fileInfo = target.FileInfo!;

        _logger.LogInformation("Serving static file: {Path} ({Size} bytes)", filePath, fileInfo.Length);

        // MIMEタイプを取得
        var mimeType = _contentType.GetMimeType(filePath);

        // Conditional GET対応（If-Modified-Since）
        if (request.Headers.TryGetValue("If-Modified-Since", out var ifModifiedSince))
        {
            if (DateTime.TryParse(ifModifiedSince, out var ifModifiedSinceDate))
            {
                // 秒単位で比較（HTTPヘッダーは秒精度）
                var lastModified = new DateTime(fileInfo.LastWriteTimeUtc.Year, fileInfo.LastWriteTimeUtc.Month,
                    fileInfo.LastWriteTimeUtc.Day, fileInfo.LastWriteTimeUtc.Hour, fileInfo.LastWriteTimeUtc.Minute,
                    fileInfo.LastWriteTimeUtc.Second, DateTimeKind.Utc);

                if (lastModified <= ifModifiedSinceDate)
                {
                    return new HttpResponse
                    {
                        StatusCode = 304,
                        StatusText = "Not Modified",
                        Headers = new Dictionary<string, string>
                        {
                            ["Server"] = settings.ServerHeader,
                            ["Date"] = DateTime.UtcNow.ToString("R")
                        }
                    };
                }
            }
        }

        // SSI処理チェック
        if (_ssiProcessor != null && _ssiProcessor.IsSsiFile(filePath))
        {
            _logger.LogDebug("Processing SSI file: {Path}", filePath);
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            var processedContent = await _ssiProcessor.ProcessSsiAsync(content, filePath);

            return new HttpResponse
            {
                StatusCode = 200,
                StatusText = "OK",
                Body = processedContent,
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = mimeType,
                    ["Server"] = settings.ServerHeader,
                    ["Date"] = DateTime.UtcNow.ToString("R"),
                    ["Last-Modified"] = fileInfo.LastWriteTimeUtc.ToString("R")
                }
            };
        }

        // ファイルサイズに応じて処理を分岐
        if (fileInfo.Length > LargeFileThreshold)
        {
            // 大きなファイルはストリーミング
            _logger.LogDebug("Using streaming for large file: {Path}", filePath);
            return HttpResponseBuilder.BuildStreamResponse(filePath, mimeType, settings);
        }
        else
        {
            // 小さなファイルは一括読み込み
            return HttpResponseBuilder.BuildFileResponse(filePath, mimeType, settings);
        }
    }

    /// <summary>
    /// ディレクトリリスティングを処理する
    /// </summary>
    private async Task<HttpResponse> HandleDirectoryAsync(
        TargetInfo target,
        HttpServerSettings settings,
        CancellationToken cancellationToken)
    {
        var directoryPath = target.PhysicalPath;

        _logger.LogInformation("Generating directory listing: {Path}", directoryPath);

        // IndexDocumentテンプレートが設定されている場合
        if (!string.IsNullOrWhiteSpace(settings.IndexDocument))
        {
            var html = await GenerateDirectoryListingFromTemplateAsync(directoryPath, settings, cancellationToken);
            return new HttpResponse
            {
                StatusCode = 200,
                StatusText = "OK",
                Body = html,
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "text/html; charset=utf-8",
                    ["Server"] = settings.ServerHeader,
                    ["Date"] = DateTime.UtcNow.ToString("R")
                }
            };
        }
        else
        {
            // デフォルトディレクトリリスティング
            var html = GenerateDefaultDirectoryListing(directoryPath, settings.DocumentRoot);
            return new HttpResponse
            {
                StatusCode = 200,
                StatusText = "OK",
                Body = html,
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "text/html; charset=utf-8",
                    ["Server"] = settings.ServerHeader,
                    ["Date"] = DateTime.UtcNow.ToString("R")
                }
            };
        }
    }

    /// <summary>
    /// テンプレートからディレクトリリスティングを生成
    /// </summary>
    private Task<string> GenerateDirectoryListingFromTemplateAsync(
        string directoryPath,
        HttpServerSettings settings,
        CancellationToken cancellationToken)
    {
        var directoryInfo = new DirectoryInfo(directoryPath);
        var relativePath = directoryPath.Substring(settings.DocumentRoot.Length).Replace('\\', '/');
        if (string.IsNullOrEmpty(relativePath))
        {
            relativePath = "/";
        }

        // ディレクトリリストHTML生成
        var listHtml = GenerateListHtml(directoryInfo, relativePath);

        // テンプレート変数を置換
        var html = settings.IndexDocument
            .Replace("$URI", relativePath)
            .Replace("$LIST", listHtml)
            .Replace("$SERVER", settings.ServerHeader.Replace("$v", GetVersion()))
            .Replace("$VER", GetVersion());

        return Task.FromResult(html);
    }

    /// <summary>
    /// ディレクトリ一覧のHTMLリストを生成
    /// </summary>
    private string GenerateListHtml(DirectoryInfo directoryInfo, string relativePath)
    {
        var listHtml = "";

        // 親ディレクトリへのリンク
        if (relativePath != "/")
        {
            listHtml += "<li><a href=\"../\">📁 Parent Directory</a></li>\n";
        }

        // ディレクトリ一覧
        foreach (var dir in directoryInfo.GetDirectories().OrderBy(d => d.Name))
        {
            var name = dir.Name;
            var lastModified = dir.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
            listHtml += $"<li><a href=\"{name}/\">📁 {name}</a> - {lastModified}</li>\n";
        }

        // ファイル一覧
        foreach (var file in directoryInfo.GetFiles().OrderBy(f => f.Name))
        {
            var name = file.Name;
            var size = FormatFileSize(file.Length);
            var lastModified = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
            listHtml += $"<li><a href=\"{name}\">📄 {name}</a> - {size} - {lastModified}</li>\n";
        }

        return listHtml;
    }

    /// <summary>
    /// アプリケーションバージョンを取得
    /// </summary>
    private string GetVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "9.0.0";
    }

    /// <summary>
    /// デフォルトディレクトリリスティングを生成
    /// </summary>
    private string GenerateDefaultDirectoryListing(string directoryPath, string documentRoot)
    {
        var directoryInfo = new DirectoryInfo(directoryPath);
        var relativePath = directoryPath.Substring(documentRoot.Length).Replace('\\', '/');
        if (string.IsNullOrEmpty(relativePath))
        {
            relativePath = "/";
        }

        var html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Index of {relativePath}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ border-bottom: 2px solid #333; padding-bottom: 10px; }}
        table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
        th {{ background-color: #f0f0f0; padding: 10px; text-align: left; border-bottom: 2px solid #333; }}
        td {{ padding: 8px; border-bottom: 1px solid #ddd; }}
        tr:hover {{ background-color: #f5f5f5; }}
        a {{ text-decoration: none; color: #0066cc; }}
        a:hover {{ text-decoration: underline; }}
        .size {{ text-align: right; }}
        .date {{ white-space: nowrap; }}
    </style>
</head>
<body>
    <h1>Index of {relativePath}</h1>
    <table>
        <thead>
            <tr>
                <th>Name</th>
                <th class=""size"">Size</th>
                <th class=""date"">Last Modified</th>
            </tr>
        </thead>
        <tbody>";

        // 親ディレクトリへのリンク
        if (relativePath != "/")
        {
            html += @"
            <tr>
                <td><a href=""../"">📁 Parent Directory</a></td>
                <td class=""size"">-</td>
                <td class=""date"">-</td>
            </tr>";
        }

        // ディレクトリ一覧
        foreach (var dir in directoryInfo.GetDirectories().OrderBy(d => d.Name))
        {
            var name = dir.Name;
            var lastModified = dir.LastWriteTime.ToString("yyyy-MM-dd HH:mm");

            html += $@"
            <tr>
                <td><a href=""{name}/"">📁 {name}</a></td>
                <td class=""size"">-</td>
                <td class=""date"">{lastModified}</td>
            </tr>";
        }

        // ファイル一覧
        foreach (var file in directoryInfo.GetFiles().OrderBy(f => f.Name))
        {
            var name = file.Name;
            var size = FormatFileSize(file.Length);
            var lastModified = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm");

            html += $@"
            <tr>
                <td><a href=""{name}"">📄 {name}</a></td>
                <td class=""size"">{size}</td>
                <td class=""date"">{lastModified}</td>
            </tr>";
        }

        html += @"
        </tbody>
    </table>
</body>
</html>";

        return html;
    }

    /// <summary>
    /// ファイルサイズをフォーマット
    /// </summary>
    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// エラーレスポンスを作成
    /// </summary>
    private HttpResponse CreateErrorResponse(int code, string statusText, string message, HttpServerSettings settings)
    {
        return HttpResponseBuilder.BuildErrorResponse(code, statusText, settings);
    }
}
