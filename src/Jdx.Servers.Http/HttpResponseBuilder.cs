using System;
using System.IO;
using System.Reflection;
using Jdx.Core.Settings;

namespace Jdx.Servers.Http;

/// <summary>
/// HTTPレスポンスを構築するユーティリティクラス
/// </summary>
public static class HttpResponseBuilder
{
    /// <summary>
    /// ファイルレスポンスを構築する
    /// </summary>
    public static HttpResponse BuildFileResponse(
        string filePath,
        string contentType,
        HttpServerSettings settings)
    {
        var fileInfo = new FileInfo(filePath);

        // ファイルを読み込み
        var fileBytes = File.ReadAllBytes(filePath);

        var response = new HttpResponse
        {
            StatusCode = 200,
            StatusText = "OK",
            BodyBytes = fileBytes,
            ContentLength = fileBytes.Length,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = contentType,
                ["Content-Length"] = fileBytes.Length.ToString(),
                ["Server"] = ProcessServerHeader(settings.ServerHeader),
                ["Date"] = DateTime.UtcNow.ToString("R"),
                ["Last-Modified"] = fileInfo.LastWriteTimeUtc.ToString("R")
            }
        };

        // ETagサポート
        if (settings.UseEtag)
        {
            var etag = GenerateETag(fileInfo);
            response.Headers["ETag"] = etag;
        }

        return response;
    }

    /// <summary>
    /// ストリームレスポンスを構築する（大きなファイル用）
    /// </summary>
    public static HttpResponse BuildStreamResponse(
        string filePath,
        string contentType,
        HttpServerSettings settings)
    {
        var fileInfo = new FileInfo(filePath);
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var response = new HttpResponse
        {
            StatusCode = 200,
            StatusText = "OK",
            BodyStream = stream,
            ContentLength = fileInfo.Length,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = contentType,
                ["Content-Length"] = fileInfo.Length.ToString(),
                ["Server"] = ProcessServerHeader(settings.ServerHeader),
                ["Date"] = DateTime.UtcNow.ToString("R"),
                ["Last-Modified"] = fileInfo.LastWriteTimeUtc.ToString("R")
            }
        };

        // ETagサポート
        if (settings.UseEtag)
        {
            var etag = GenerateETag(fileInfo);
            response.Headers["ETag"] = etag;
        }

        return response;
    }

    /// <summary>
    /// エラーレスポンスを構築する
    /// </summary>
    public static HttpResponse BuildErrorResponse(
        int statusCode,
        string message,
        HttpServerSettings settings)
    {
        var html = settings.ErrorDocument;

        // ErrorDocumentテンプレートが設定されている場合
        if (!string.IsNullOrWhiteSpace(html))
        {
            html = html
                .Replace("$CODE", statusCode.ToString())
                .Replace("$MSG", message)
                .Replace("$URI", "")
                .Replace("$SERVER", ProcessServerHeader(settings.ServerHeader))
                .Replace("$VER", GetVersion());
        }
        else
        {
            // デフォルトエラーページ
            html = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>{statusCode} {message}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        h1 {{ color: #d32f2f; }}
        hr {{ border: 0; border-top: 1px solid #ccc; }}
        .footer {{ margin-top: 20px; color: #666; font-size: 0.9em; }}
    </style>
</head>
<body>
    <h1>{statusCode} {message}</h1>
    <p>The requested resource could not be found or accessed.</p>
    <hr>
    <div class=""footer"">
        {ProcessServerHeader(settings.ServerHeader)}
    </div>
</body>
</html>";
        }

        var response = new HttpResponse
        {
            StatusCode = statusCode,
            StatusText = message,
            Body = html,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "text/html; charset=utf-8",
                ["Server"] = ProcessServerHeader(settings.ServerHeader),
                ["Date"] = DateTime.UtcNow.ToString("R")
            }
        };

        return response;
    }

    /// <summary>
    /// ServerHeaderテンプレートを処理する（$v → バージョン番号）
    /// </summary>
    private static string ProcessServerHeader(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return $"BJD/{GetVersion()}";
        }

        return template.Replace("$v", GetVersion());
    }

    /// <summary>
    /// アプリケーションバージョンを取得
    /// </summary>
    private static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "9.0.0";
    }

    /// <summary>
    /// ETagを生成する（ファイルサイズと最終更新日時のハッシュ）
    /// </summary>
    private static string GenerateETag(FileInfo fileInfo)
    {
        var data = $"{fileInfo.Length}-{fileInfo.LastWriteTimeUtc.Ticks}";
        var hash = data.GetHashCode();
        return $"\"{hash:X}\"";
    }
}
