using System;
using System.IO;
using System.Linq;
using Jdx.Core.Settings;

namespace Jdx.Servers.Http;

/// <summary>
/// ファイル拡張子からMIMEタイプを解決するクラス
/// </summary>
public class HttpContentType
{
    private readonly HttpServerSettings _settings;
    private readonly Dictionary<string, string> _mimeMap;

    public HttpContentType(HttpServerSettings settings)
    {
        _settings = settings;
        _mimeMap = BuildMimeMap();
    }

    /// <summary>
    /// ファイルパスからMIMEタイプを取得する
    /// </summary>
    public string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath);

        // 拡張子がない場合はデフォルト
        if (string.IsNullOrEmpty(extension))
        {
            return "application/octet-stream";
        }

        // 先頭のドットを除去（例: ".txt" → "txt"）
        extension = extension.TrimStart('.').ToLowerInvariant();

        // MIMEマップから検索
        if (_mimeMap.TryGetValue(extension, out var mimeType))
        {
            return mimeType;
        }

        // デフォルトMIMEタイプ
        return "application/octet-stream";
    }

    /// <summary>
    /// 拡張子 → MIMEタイプのマップを構築
    /// </summary>
    private Dictionary<string, string> BuildMimeMap()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 設定からMIMEタイプをロード
        foreach (var entry in _settings.MimeTypes)
        {
            if (!string.IsNullOrWhiteSpace(entry.Extension) && !string.IsNullOrWhiteSpace(entry.MimeType))
            {
                var ext = entry.Extension.TrimStart('.').ToLowerInvariant();
                map[ext] = entry.MimeType;
            }
        }

        // デフォルトのMIMEタイプ（設定に含まれていない場合のフォールバック）
        var defaults = new Dictionary<string, string>
        {
            // Text
            ["txt"] = "text/plain",
            ["html"] = "text/html",
            ["htm"] = "text/html",
            ["css"] = "text/css",
            ["js"] = "text/javascript",
            ["json"] = "application/json",
            ["xml"] = "text/xml",
            ["csv"] = "text/csv",

            // Images
            ["gif"] = "image/gif",
            ["jpg"] = "image/jpeg",
            ["jpeg"] = "image/jpeg",
            ["png"] = "image/png",
            ["svg"] = "image/svg+xml",
            ["ico"] = "image/x-icon",
            ["webp"] = "image/webp",
            ["bmp"] = "image/bmp",

            // Documents
            ["pdf"] = "application/pdf",
            ["doc"] = "application/msword",
            ["docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ["xls"] = "application/vnd.ms-excel",
            ["xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ["ppt"] = "application/vnd.ms-powerpoint",
            ["pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",

            // Archives
            ["zip"] = "application/zip",
            ["tar"] = "application/x-tar",
            ["gz"] = "application/gzip",
            ["7z"] = "application/x-7z-compressed",
            ["rar"] = "application/vnd.rar",

            // Audio
            ["mp3"] = "audio/mpeg",
            ["wav"] = "audio/wav",
            ["ogg"] = "audio/ogg",
            ["m4a"] = "audio/mp4",

            // Video
            ["mp4"] = "video/mp4",
            ["avi"] = "video/x-msvideo",
            ["mov"] = "video/quicktime",
            ["wmv"] = "video/x-ms-wmv",
            ["webm"] = "video/webm",

            // Fonts
            ["woff"] = "font/woff",
            ["woff2"] = "font/woff2",
            ["ttf"] = "font/ttf",
            ["otf"] = "font/otf",

            // Others
            ["bin"] = "application/octet-stream",
            ["exe"] = "application/octet-stream"
        };

        // デフォルトをマップに追加（設定値が優先）
        foreach (var kvp in defaults)
        {
            if (!map.ContainsKey(kvp.Key))
            {
                map[kvp.Key] = kvp.Value;
            }
        }

        return map;
    }
}
