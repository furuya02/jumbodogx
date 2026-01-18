using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Ftp;

/// <summary>
/// FTP user authentication and management
/// Based on bjd5-master/FtpServer/ListUser.cs
/// </summary>
public class FtpUserManager
{
    private readonly List<FtpUserEntry> _users;
    private readonly ILogger _logger;

    public FtpUserManager(List<FtpUserEntry> users, ILogger logger)
    {
        _users = users;
        _logger = logger;
    }

    /// <summary>
    /// Get user by username
    /// Anonymous users are case-insensitive (bjd5-master compatible)
    /// </summary>
    public FtpUserEntry? GetUser(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            return null;
        }

        foreach (var user in _users)
        {
            // Anonymous users: case-insensitive matching
            if (userName.Equals("anonymous", StringComparison.OrdinalIgnoreCase))
            {
                if (user.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                {
                    return user;
                }
            }
            else
            {
                // Regular users: case-sensitive matching
                if (user.UserName == userName)
                {
                    return user;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Authenticate user with password
    /// </summary>
    public bool Authenticate(string userName, string password)
    {
        var user = GetUser(userName);
        if (user == null)
        {
            _logger.LogWarning("Authentication failed: user not found - {UserName}", userName);
            return false;
        }

        // Anonymous users: empty password allowed
        if (userName.Equals("anonymous", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(user.Password))
            {
                _logger.LogInformation("Anonymous user authenticated");
                return true;
            }
        }

        // Regular users: verify password
        // TODO: Implement password encryption/decryption compatible with bjd5-master
        // For now, use simple comparison (should be replaced with proper encryption)
        if (user.Password == password)
        {
            _logger.LogInformation("User authenticated: {UserName}", userName);
            return true;
        }

        _logger.LogWarning("Authentication failed: incorrect password - {UserName}", userName);
        return false;
    }

    /// <summary>
    /// Encrypt password (placeholder for bjd5-master compatible encryption)
    /// TODO: Implement Crypt.Encrypt compatible method
    /// </summary>
    public static string EncryptPassword(string plaintext)
    {
        // Temporary simple implementation
        // Should be replaced with bjd5-master compatible encryption
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Decrypt password (placeholder for bjd5-master compatible decryption)
    /// TODO: Implement Crypt.Decrypt compatible method
    /// </summary>
    public static string DecryptPassword(string encrypted)
    {
        // Temporary implementation - returns as-is
        // Should be replaced with bjd5-master compatible decryption
        return encrypted;
    }
}
