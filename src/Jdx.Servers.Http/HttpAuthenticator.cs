using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Http;

/// <summary>
/// HTTP Basic認証処理を担当するクラス
/// </summary>
public class HttpAuthenticator
{
    private readonly HttpServerSettings _settings;
    private readonly ILogger _logger;

    public HttpAuthenticator(HttpServerSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// リクエストパスに対して認証が必要かチェックし、必要な場合は認証を検証する
    /// </summary>
    public AuthResult Authenticate(string requestPath, HttpRequest request)
    {
        // AuthListからマッチする認証設定を探す
        var authConfig = FindAuthEntry(requestPath);
        if (authConfig == null)
        {
            // 認証不要
            return AuthResult.Success();
        }

        // Authorizationヘッダーを取得
        if (!request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            _logger.LogDebug("No Authorization header found for path: {Path}", requestPath);
            return AuthResult.Unauthorized(authConfig.AuthName);
        }

        // Basic認証のパース
        if (!TryParseBasicAuth(authHeader, out var username, out var password))
        {
            _logger.LogWarning("Invalid Authorization header format");
            return AuthResult.Unauthorized(authConfig.AuthName);
        }

        // ユーザー認証
        if (!ValidateUser(username, password))
        {
            _logger.LogWarning("Authentication failed for user: {User}", username);
            return AuthResult.Unauthorized(authConfig.AuthName);
        }

        // Require条件チェック
        if (!CheckRequire(authConfig.Require, username))
        {
            _logger.LogWarning("User {User} does not meet require condition: {Require}", username, authConfig.Require);
            return AuthResult.Forbidden();
        }

        _logger.LogInformation("User {User} authenticated successfully for path: {Path}", username, requestPath);
        return AuthResult.Success();
    }

    /// <summary>
    /// リクエストパスに対する認証設定を探す
    /// </summary>
    private AuthEntry? FindAuthEntry(string requestPath)
    {
        if (_settings.AuthList == null || _settings.AuthList.Count == 0)
        {
            return null;
        }

        // 最も長いマッチを優先
        foreach (var config in _settings.AuthList.OrderByDescending(a => a.Directory.Length))
        {
            if (requestPath.StartsWith(config.Directory, StringComparison.OrdinalIgnoreCase))
            {
                return config;
            }
        }

        return null;
    }

    /// <summary>
    /// Basic認証ヘッダーをパースする
    /// </summary>
    private bool TryParseBasicAuth(string authHeader, out string username, out string password)
    {
        username = string.Empty;
        password = string.Empty;

        // "Basic " プレフィックスを確認
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            // Base64デコード
            var base64Credentials = authHeader.Substring(6);
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(base64Credentials));

            // ユーザー名とパスワードを分割
            var separatorIndex = credentials.IndexOf(':');
            if (separatorIndex < 0)
            {
                return false;
            }

            username = credentials.Substring(0, separatorIndex);
            password = credentials.Substring(separatorIndex + 1);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// ユーザー名とパスワードを検証する
    /// </summary>
    private bool ValidateUser(string username, string password)
    {
        if (_settings.UserList == null || _settings.UserList.Count == 0)
        {
            return false;
        }

        var user = _settings.UserList.FirstOrDefault(u =>
            u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return false;
        }

        // パスワードのSHA-256ハッシュを計算
        var passwordHashBytes = ComputeSha256HashBytes(password);

        // 定数時間比較（タイミング攻撃対策）
        var storedHashBytes = ConvertHexToBytes(user.Password);
        if (storedHashBytes == null || storedHashBytes.Length != passwordHashBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(passwordHashBytes, storedHashBytes);
    }

    /// <summary>
    /// SHA-256ハッシュを計算する（バイト配列）
    /// </summary>
    private byte[] ComputeSha256HashBytes(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        return sha256.ComputeHash(bytes);
    }

    /// <summary>
    /// 16進文字列をバイト配列に変換
    /// </summary>
    private byte[]? ConvertHexToBytes(string hex)
    {
        try
        {
            hex = hex.Replace("-", "");
            if (hex.Length % 2 != 0)
            {
                return null;
            }

            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// SHA-256ハッシュを計算する（16進文字列）
    /// </summary>
    private string ComputeSha256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Require条件をチェックする
    /// </summary>
    private bool CheckRequire(string require, string username)
    {
        if (string.IsNullOrWhiteSpace(require))
        {
            // Require指定なしは全ユーザー許可
            return true;
        }

        // "user username" 形式
        if (require.StartsWith("user ", StringComparison.OrdinalIgnoreCase))
        {
            var requiredUsers = require.Substring(5).Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return requiredUsers.Any(u => u.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        // "group groupname" 形式
        if (require.StartsWith("group ", StringComparison.OrdinalIgnoreCase))
        {
            var requiredGroups = require.Substring(6).Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return requiredGroups.Any(g => IsUserInGroup(username, g));
        }

        // "valid-user" - 認証されたすべてのユーザーを許可
        if (require.Equals("valid-user", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// ユーザーが指定されたグループに所属しているかチェック
    /// </summary>
    private bool IsUserInGroup(string username, string groupName)
    {
        if (_settings.GroupList == null || _settings.GroupList.Count == 0)
        {
            return false;
        }

        return _settings.GroupList.Any(g =>
            g.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase) &&
            g.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// 認証結果
/// </summary>
public class AuthResult
{
    public bool IsSuccess { get; set; }
    public bool IsUnauthorized { get; set; }
    public bool IsForbidden { get; set; }
    public string? Realm { get; set; }

    public static AuthResult Success() => new AuthResult { IsSuccess = true };

    public static AuthResult Unauthorized(string realm) => new AuthResult
    {
        IsUnauthorized = true,
        Realm = realm
    };

    public static AuthResult Forbidden() => new AuthResult { IsForbidden = true };
}
