using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Http;

/// <summary>
/// WebDAV (Web Distributed Authoring and Versioning) ハンドラ
/// </summary>
public class HttpWebDavHandler
{
    private readonly HttpServerSettings _settings;
    private readonly ILogger _logger;

    public HttpWebDavHandler(HttpServerSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// リクエストがWebDAVパスかどうか判定
    /// </summary>
    public bool IsWebDavRequest(string requestPath, string method, out string physicalPath, out bool allowWrite)
    {
        physicalPath = string.Empty;
        allowWrite = false;

        if (!_settings.UseWebDav || _settings.WebDavPaths == null)
        {
            return false;
        }

        foreach (var webDavPath in _settings.WebDavPaths)
        {
            if (requestPath.StartsWith(webDavPath.Path, StringComparison.OrdinalIgnoreCase))
            {
                var relativePath = requestPath.Substring(webDavPath.Path.Length).TrimStart('/');
                physicalPath = Path.Combine(webDavPath.Directory, relativePath);
                allowWrite = webDavPath.AllowWrite;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// WebDAVリクエストを処理
    /// </summary>
    public async Task<HttpResponse> HandleWebDavAsync(
        string method,
        string requestPath,
        string physicalPath,
        bool allowWrite,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("WebDAV {Method} request: {Path}", method, requestPath);

        // 書き込みメソッドのチェック
        if (!allowWrite && IsWriteMethod(method))
        {
            _logger.LogWarning("WebDAV write method denied (AllowWrite=false): {Method}", method);
            return HttpResponseBuilder.BuildErrorResponse(403, "Forbidden", _settings);
        }

        return method.ToUpperInvariant() switch
        {
            "PROPFIND" => await HandlePropfindAsync(physicalPath, request, cancellationToken),
            "PROPPATCH" => HandleProppatch(physicalPath),
            "MKCOL" => HandleMkcol(physicalPath),
            "PUT" => await HandlePutAsync(physicalPath, request, cancellationToken),
            "DELETE" => HandleDelete(physicalPath),
            "COPY" => HandleCopy(physicalPath, request),
            "MOVE" => HandleMove(physicalPath, request),
            "LOCK" => HandleLock(physicalPath, request),
            "UNLOCK" => HandleUnlock(physicalPath, request),
            _ => HttpResponseBuilder.BuildErrorResponse(405, "Method Not Allowed", _settings)
        };
    }

    /// <summary>
    /// 書き込みメソッドかどうか判定
    /// </summary>
    private bool IsWriteMethod(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "PUT" or "DELETE" or "MKCOL" or "PROPPATCH" or "COPY" or "MOVE" => true,
            _ => false
        };
    }

    /// <summary>
    /// PROPFIND: プロパティ取得（ディレクトリ一覧）
    /// </summary>
    private Task<HttpResponse> HandlePropfindAsync(string physicalPath, HttpRequest request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("PROPFIND: {Path}", physicalPath);

        // Depthヘッダー: 0=自身のみ, 1=直下, infinity=全て
        var depth = request.Headers.ContainsKey("Depth") ? request.Headers["Depth"] : "infinity";

        if (!Directory.Exists(physicalPath) && !File.Exists(physicalPath))
        {
            return Task.FromResult(HttpResponseBuilder.BuildErrorResponse(404, "Not Found", _settings));
        }

        // XML応答を生成
        var xml = GeneratePropertiesXml(physicalPath, depth);

        return Task.FromResult(new HttpResponse
        {
            StatusCode = 207, // Multi-Status
            StatusText = "Multi-Status",
            Body = xml,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/xml; charset=utf-8",
                ["DAV"] = "1, 2",
                ["Server"] = _settings.ServerHeader,
                ["Date"] = DateTime.UtcNow.ToString("R")
            }
        });
    }

    /// <summary>
    /// プロパティXMLを生成
    /// </summary>
    private string GeneratePropertiesXml(string physicalPath, string depth)
    {
        var ns = XNamespace.Get("DAV:");
        var root = new XElement(ns + "multistatus");

        if (Directory.Exists(physicalPath))
        {
            // ディレクトリの場合
            AddResourceResponse(root, ns, physicalPath, true);

            if (depth != "0")
            {
                var dirInfo = new DirectoryInfo(physicalPath);

                // サブディレクトリ
                foreach (var dir in dirInfo.GetDirectories())
                {
                    AddResourceResponse(root, ns, dir.FullName, true);
                }

                // ファイル
                foreach (var file in dirInfo.GetFiles())
                {
                    AddResourceResponse(root, ns, file.FullName, false);
                }
            }
        }
        else if (File.Exists(physicalPath))
        {
            // ファイルの場合
            AddResourceResponse(root, ns, physicalPath, false);
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        return doc.ToString();
    }

    /// <summary>
    /// リソースのプロパティ応答を追加
    /// </summary>
    private void AddResourceResponse(XElement parent, XNamespace ns, string path, bool isDirectory)
    {
        var href = path.Replace(_settings.DocumentRoot, "").Replace('\\', '/');
        if (string.IsNullOrEmpty(href))
        {
            href = "/";
        }

        var response = new XElement(ns + "response",
            new XElement(ns + "href", href)
        );

        var propstat = new XElement(ns + "propstat");
        var prop = new XElement(ns + "prop");

        if (isDirectory)
        {
            var dirInfo = new DirectoryInfo(path);
            prop.Add(
                new XElement(ns + "displayname", dirInfo.Name),
                new XElement(ns + "resourcetype", new XElement(ns + "collection")),
                new XElement(ns + "getlastmodified", dirInfo.LastWriteTimeUtc.ToString("R")),
                new XElement(ns + "creationdate", dirInfo.CreationTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            );
        }
        else
        {
            var fileInfo = new FileInfo(path);
            prop.Add(
                new XElement(ns + "displayname", fileInfo.Name),
                new XElement(ns + "resourcetype"),
                new XElement(ns + "getcontentlength", fileInfo.Length),
                new XElement(ns + "getlastmodified", fileInfo.LastWriteTimeUtc.ToString("R")),
                new XElement(ns + "creationdate", fileInfo.CreationTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                new XElement(ns + "getcontenttype", GetContentType(fileInfo.Extension))
            );
        }

        propstat.Add(prop);
        propstat.Add(new XElement(ns + "status", "HTTP/1.1 200 OK"));
        response.Add(propstat);
        parent.Add(response);
    }

    /// <summary>
    /// PROPPATCH: プロパティ更新
    /// </summary>
    private HttpResponse HandleProppatch(string physicalPath)
    {
        _logger.LogDebug("PROPPATCH: {Path}", physicalPath);
        // 簡易実装: プロパティ更新は未サポート
        return new HttpResponse
        {
            StatusCode = 403,
            StatusText = "Forbidden",
            Body = "Property modification not supported",
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "text/plain",
                ["Server"] = _settings.ServerHeader,
                ["Date"] = DateTime.UtcNow.ToString("R")
            }
        };
    }

    /// <summary>
    /// MKCOL: コレクション（ディレクトリ）作成
    /// </summary>
    private HttpResponse HandleMkcol(string physicalPath)
    {
        _logger.LogInformation("MKCOL: {Path}", physicalPath);

        try
        {
            if (Directory.Exists(physicalPath))
            {
                return HttpResponseBuilder.BuildErrorResponse(405, "Method Not Allowed", _settings);
            }

            if (File.Exists(physicalPath))
            {
                return HttpResponseBuilder.BuildErrorResponse(405, "Method Not Allowed", _settings);
            }

            Directory.CreateDirectory(physicalPath);

            return new HttpResponse
            {
                StatusCode = 201,
                StatusText = "Created",
                Headers = new Dictionary<string, string>
                {
                    ["Server"] = _settings.ServerHeader,
                    ["Date"] = DateTime.UtcNow.ToString("R")
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory: {Path}", physicalPath);
            return HttpResponseBuilder.BuildErrorResponse(500, "Internal Server Error", _settings);
        }
    }

    /// <summary>
    /// PUT: ファイルアップロード
    /// </summary>
    private async Task<HttpResponse> HandlePutAsync(string physicalPath, HttpRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("PUT: {Path}", physicalPath);

        try
        {
            // Check if file exists BEFORE writing
            var fileExisted = File.Exists(physicalPath);

            // ディレクトリが存在しない場合は作成
            var directory = Path.GetDirectoryName(physicalPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // ファイルに書き込み
            await File.WriteAllTextAsync(physicalPath, request.Body ?? string.Empty, cancellationToken);

            // 201 if newly created, 204 if updated existing file
            var statusCode = fileExisted ? 204 : 201;
            var statusText = statusCode == 201 ? "Created" : "No Content";

            return new HttpResponse
            {
                StatusCode = statusCode,
                StatusText = statusText,
                Headers = new Dictionary<string, string>
                {
                    ["Server"] = _settings.ServerHeader,
                    ["Date"] = DateTime.UtcNow.ToString("R")
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing file: {Path}", physicalPath);
            return HttpResponseBuilder.BuildErrorResponse(500, "Internal Server Error", _settings);
        }
    }

    /// <summary>
    /// DELETE: ファイル/ディレクトリ削除
    /// </summary>
    private HttpResponse HandleDelete(string physicalPath)
    {
        _logger.LogInformation("DELETE: {Path}", physicalPath);

        try
        {
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
            else if (Directory.Exists(physicalPath))
            {
                Directory.Delete(physicalPath, true);
            }
            else
            {
                return HttpResponseBuilder.BuildErrorResponse(404, "Not Found", _settings);
            }

            return new HttpResponse
            {
                StatusCode = 204,
                StatusText = "No Content",
                Headers = new Dictionary<string, string>
                {
                    ["Server"] = _settings.ServerHeader,
                    ["Date"] = DateTime.UtcNow.ToString("R")
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting: {Path}", physicalPath);
            return HttpResponseBuilder.BuildErrorResponse(500, "Internal Server Error", _settings);
        }
    }

    /// <summary>
    /// COPY: ファイル/ディレクトリコピー
    /// </summary>
    private HttpResponse HandleCopy(string physicalPath, HttpRequest request)
    {
        _logger.LogInformation("COPY: {Path}", physicalPath);

        if (!request.Headers.ContainsKey("Destination"))
        {
            return HttpResponseBuilder.BuildErrorResponse(400, "Bad Request", _settings);
        }

        var destination = request.Headers["Destination"];
        // TODO: Destinationパスの解析と実装

        return HttpResponseBuilder.BuildErrorResponse(501, "Not Implemented", _settings);
    }

    /// <summary>
    /// MOVE: ファイル/ディレクトリ移動
    /// </summary>
    private HttpResponse HandleMove(string physicalPath, HttpRequest request)
    {
        _logger.LogInformation("MOVE: {Path}", physicalPath);

        if (!request.Headers.ContainsKey("Destination"))
        {
            return HttpResponseBuilder.BuildErrorResponse(400, "Bad Request", _settings);
        }

        var destination = request.Headers["Destination"];
        // TODO: Destinationパスの解析と実装

        return HttpResponseBuilder.BuildErrorResponse(501, "Not Implemented", _settings);
    }

    /// <summary>
    /// LOCK: リソースロック
    /// </summary>
    private HttpResponse HandleLock(string physicalPath, HttpRequest request)
    {
        _logger.LogInformation("LOCK: {Path}", physicalPath);
        // 簡易実装: ロック機能は未サポート
        return HttpResponseBuilder.BuildErrorResponse(501, "Not Implemented", _settings);
    }

    /// <summary>
    /// UNLOCK: リソースアンロック
    /// </summary>
    private HttpResponse HandleUnlock(string physicalPath, HttpRequest request)
    {
        _logger.LogInformation("UNLOCK: {Path}", physicalPath);
        // 簡易実装: ロック機能は未サポート
        return HttpResponseBuilder.BuildErrorResponse(501, "Not Implemented", _settings);
    }

    /// <summary>
    /// Content-Typeを取得
    /// </summary>
    private string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".xml" => "application/xml",
            ".json" => "application/json",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
