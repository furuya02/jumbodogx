using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Jdx.Core.Settings;

namespace Jdx.Servers.Http;

/// <summary>
/// HTTPリクエストのターゲット（URI → 物理パス）解決とセキュリティ検証を担当
/// </summary>
public class HttpTarget
{
    private readonly HttpServerSettings _settings;
    private readonly ILogger _logger;

    public HttpTarget(HttpServerSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// リクエストパスからターゲット情報を解決する
    /// </summary>
    public TargetInfo ResolveTarget(string requestPath)
    {
        try
        {
            // クエリ文字列を除去
            var uriParts = requestPath.Split('?', 2);
            var path = uriParts[0];

            // DocumentRootが設定されていない場合
            if (string.IsNullOrWhiteSpace(_settings.DocumentRoot))
            {
                _logger.LogWarning("DocumentRoot is not configured");
                return TargetInfo.Invalid("DocumentRoot is not configured");
            }

            // パスを正規化（%エンコード対応）
            path = Uri.UnescapeDataString(path);

            // Alias解決（Aliasが優先）
            var aliasResult = ResolveAlias(path);
            string physicalPath;
            string rootPath;

            if (aliasResult.matched)
            {
                physicalPath = aliasResult.physicalPath;
                rootPath = aliasResult.rootDirectory;
                _logger.LogDebug("Alias matched: {RequestPath} -> {PhysicalPath}", path, physicalPath);
            }
            else
            {
                // パス結合（通常のDocumentRoot）
                physicalPath = Path.Combine(_settings.DocumentRoot, path.TrimStart('/'));
                rootPath = _settings.DocumentRoot;
            }

            // セキュリティ検証（適切なrootPathを指定）
            if (!ValidatePath(physicalPath, rootPath))
            {
                return TargetInfo.Invalid("Access denied");
            }

            // ファイルが存在する場合
            if (File.Exists(physicalPath))
            {
                var fileInfo = new FileInfo(physicalPath);
                return new TargetInfo
                {
                    IsValid = true,
                    Type = TargetType.StaticFile,
                    PhysicalPath = physicalPath,
                    FileInfo = fileInfo
                };
            }

            // ディレクトリが存在する場合
            if (Directory.Exists(physicalPath))
            {
                // Welcome fileを探す
                var welcomeFile = FindWelcomeFile(physicalPath);
                if (welcomeFile != null)
                {
                    var fileInfo = new FileInfo(welcomeFile);
                    return new TargetInfo
                    {
                        IsValid = true,
                        Type = TargetType.StaticFile,
                        PhysicalPath = welcomeFile,
                        FileInfo = fileInfo
                    };
                }

                // ディレクトリリスティングが有効な場合
                if (_settings.UseDirectoryEnum)
                {
                    return new TargetInfo
                    {
                        IsValid = true,
                        Type = TargetType.Directory,
                        PhysicalPath = physicalPath
                    };
                }

                _logger.LogWarning("Directory listing disabled for: {Path}", physicalPath);
                return TargetInfo.Invalid("Forbidden");
            }

            // ファイルもディレクトリも存在しない
            _logger.LogInformation("Not found: {Path}", physicalPath);
            return TargetInfo.Invalid("Not Found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving target for path: {Path}", requestPath);
            return TargetInfo.Invalid("Internal Server Error");
        }
    }

    /// <summary>
    /// パスのセキュリティ検証
    /// </summary>
    private bool ValidatePath(string physicalPath, string? rootPath = null)
    {
        try
        {
            // フルパスに正規化
            var fullPath = Path.GetFullPath(physicalPath);
            var allowedRoot = Path.GetFullPath(rootPath ?? _settings.DocumentRoot);

            // rootPath外へのアクセス防止（パストラバーサル対策）
            if (!fullPath.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal attempt blocked: {Path}", physicalPath);
                return false;
            }

            // Hidden filesチェック
            if (!_settings.UseHidden && File.Exists(fullPath))
            {
                var fileInfo = new FileInfo(fullPath);
                if ((fileInfo.Attributes & FileAttributes.Hidden) != 0)
                {
                    _logger.LogWarning("Hidden file access denied: {Path}", fullPath);
                    return false;
                }
            }

            // Dot filesチェック（ファイル名が.で始まる）
            if (!_settings.UseDot)
            {
                var fileName = Path.GetFileName(fullPath);
                if (!string.IsNullOrEmpty(fileName) && fileName.StartsWith("."))
                {
                    _logger.LogWarning("Dot file access denied: {Path}", fullPath);
                    return false;
                }

                // パス内の各ディレクトリもチェック
                var pathParts = fullPath.Substring(allowedRoot.Length).Split(Path.DirectorySeparatorChar);
                foreach (var part in pathParts)
                {
                    if (!string.IsNullOrEmpty(part) && part.StartsWith("."))
                    {
                        _logger.LogWarning("Dot directory access denied: {Path}", fullPath);
                        return false;
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating path: {Path}", physicalPath);
            return false;
        }
    }

    /// <summary>
    /// ディレクトリ内でWelcome fileを探す
    /// </summary>
    private string? FindWelcomeFile(string directory)
    {
        if (string.IsNullOrWhiteSpace(_settings.WelcomeFileName))
        {
            return null;
        }

        var welcomeFiles = _settings.WelcomeFileName.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var welcomeFile in welcomeFiles)
        {
            var filePath = Path.Combine(directory, welcomeFile.Trim());
            if (File.Exists(filePath))
            {
                // セキュリティ検証
                if (ValidatePath(filePath))
                {
                    return filePath;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Alias解決を行う
    /// </summary>
    private (bool matched, string physicalPath, string rootDirectory) ResolveAlias(string requestPath)
    {
        if (_settings.Aliases == null || _settings.Aliases.Count == 0)
        {
            return (false, string.Empty, string.Empty);
        }

        // 最も長いマッチを優先（/docs/test より /docs を優先的に扱わない）
        foreach (var alias in _settings.Aliases.OrderByDescending(a => a.Name.Length))
        {
            if (requestPath.StartsWith(alias.Name, StringComparison.OrdinalIgnoreCase))
            {
                // Alias部分を置換
                var relativePath = requestPath.Substring(alias.Name.Length).TrimStart('/');
                var physicalPath = Path.Combine(alias.Directory, relativePath);
                return (true, physicalPath, alias.Directory);
            }
        }

        return (false, string.Empty, string.Empty);
    }
}

/// <summary>
/// ターゲット解決結果
/// </summary>
public class TargetInfo
{
    public bool IsValid { get; set; }
    public TargetType Type { get; set; }
    public string PhysicalPath { get; set; } = "";
    public FileInfo? FileInfo { get; set; }
    public string ErrorMessage { get; set; } = "";

    public static TargetInfo Invalid(string errorMessage)
    {
        return new TargetInfo
        {
            IsValid = false,
            Type = TargetType.NotFound,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// ターゲットタイプ
/// </summary>
public enum TargetType
{
    /// <summary>ファイルやディレクトリが見つからない</summary>
    NotFound,

    /// <summary>静的ファイル</summary>
    StaticFile,

    /// <summary>ディレクトリ</summary>
    Directory
}
