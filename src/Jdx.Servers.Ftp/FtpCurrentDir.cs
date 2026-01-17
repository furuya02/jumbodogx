using System;
using System.IO;
using System.Linq;
using Jdx.Core.Settings;

namespace Jdx.Servers.Ftp;

/// <summary>
/// Current directory management for FTP session
/// Based on bjd5-master/FtpServer/CurrentDir.cs
/// Handles directory navigation and virtual folder mapping
/// </summary>
public class FtpCurrentDir
{
    private string _current = "";
    private readonly string _homeDir;
    private readonly FtpMountManager _mountManager;
    private FtpMountEntry? _currentMount; // null when outside virtual folders

    /// <summary>
    /// Initialize with home directory
    /// </summary>
    public FtpCurrentDir(string homeDir, FtpMountManager mountManager)
    {
        _mountManager = mountManager;

        // Ensure homeDir ends with directory separator
        _homeDir = homeDir.TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
        _current = _homeDir;
    }

    /// <summary>
    /// Get current working directory (PWD)
    /// Returns Unix-style path for FTP clients
    /// </summary>
    public string GetPwd()
    {
        if (_currentMount != null)
        {
            // Inside virtual folder
            var relative = _current.Substring(_currentMount.FromFolder.TrimEnd('\\', '/').Length);
            var virtualPath = _currentMount.ToFolder.TrimEnd('\\', '/') + relative;
            return virtualPath.Replace(Path.DirectorySeparatorChar, '/');
        }
        else
        {
            // Regular directory
            var homeWithoutTrailing = _homeDir.TrimEnd(Path.DirectorySeparatorChar);
            var currentWithoutTrailing = _current.TrimEnd(Path.DirectorySeparatorChar);

            if (currentWithoutTrailing == homeWithoutTrailing)
            {
                // At home directory root
                return "/";
            }

            var relative = currentWithoutTrailing.Substring(homeWithoutTrailing.Length);
            return relative.Replace(Path.DirectorySeparatorChar, '/');
        }
    }

    /// <summary>
    /// Get physical path for file operations
    /// </summary>
    public string GetPhysicalPath()
    {
        return _current;
    }

    /// <summary>
    /// Change directory
    /// </summary>
    public bool ChangeDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        // Handle absolute vs relative paths
        if (path.StartsWith("/"))
        {
            // Absolute path from home directory
            return ChangeToAbsolutePath(path);
        }
        else
        {
            // Relative path
            return ChangeToRelativePath(path);
        }
    }

    /// <summary>
    /// Change to absolute path (from home directory)
    /// </summary>
    private bool ChangeToAbsolutePath(string path)
    {
        // Convert Unix path to system path
        var systemPath = path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_homeDir, systemPath);

        // Normalize path
        try
        {
            fullPath = Path.GetFullPath(fullPath);
        }
        catch
        {
            return false;
        }

        // Check if within home directory
        if (!fullPath.StartsWith(_homeDir, StringComparison.OrdinalIgnoreCase))
            return false;

        // Check directory exists
        if (Directory.Exists(fullPath))
        {
            _current = fullPath.TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
            _currentMount = null;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Change to relative path (from current directory)
    /// </summary>
    private bool ChangeToRelativePath(string name)
    {
        // Handle parent directory
        if (name == "..")
        {
            // Check if already at home directory root
            var currentWithoutTrailing = _current.TrimEnd(Path.DirectorySeparatorChar);
            var homeWithoutTrailing = _homeDir.TrimEnd(Path.DirectorySeparatorChar);

            if (currentWithoutTrailing == homeWithoutTrailing)
            {
                // Already at root, cannot go up
                return false;
            }

            var parent = Directory.GetParent(_current.TrimEnd(Path.DirectorySeparatorChar));
            if (parent == null)
            {
                return false;
            }

            var parentPath = parent.FullName.TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
            var parentWithoutTrailing = parentPath.TrimEnd(Path.DirectorySeparatorChar);

            // Check if parent is at or above home directory
            if (parentWithoutTrailing.Length < homeWithoutTrailing.Length)
            {
                // Parent is above home directory - not allowed
                return false;
            }

            // Check if parent is within home directory tree
            if (parentPath.StartsWith(_homeDir, StringComparison.OrdinalIgnoreCase) ||
                parentWithoutTrailing == homeWithoutTrailing)
            {
                _current = parentPath;

                // Check if leaving virtual folder
                if (_currentMount != null)
                {
                    if (!_current.StartsWith(_currentMount.FromFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        _currentMount = null;
                    }
                }

                return true;
            }

            return false;
        }

        // Handle current directory
        if (name == ".")
        {
            return true;
        }

        // Regular directory navigation
        var newPath = Path.Combine(_current, name);

        // Normalize path
        try
        {
            newPath = Path.GetFullPath(newPath);
        }
        catch
        {
            return false;
        }

        // Check if within allowed boundaries
        if (_currentMount != null)
        {
            // Inside virtual folder - check mount boundaries
            if (newPath.StartsWith(_currentMount.FromFolder, StringComparison.OrdinalIgnoreCase))
            {
                if (Directory.Exists(newPath))
                {
                    _current = newPath.TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
                    return true;
                }
            }
        }
        else
        {
            // Outside virtual folder - check home directory boundaries
            if (newPath.StartsWith(_homeDir, StringComparison.OrdinalIgnoreCase))
            {
                if (Directory.Exists(newPath))
                {
                    _current = newPath.TrimEnd('\\', '/') + Path.DirectorySeparatorChar;

                    // Check if entering virtual folder
                    var mount = _mountManager.FindByPhysicalPath(_current);
                    if (mount != null)
                    {
                        _currentMount = mount;
                    }

                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Create full physical path for file operations
    /// </summary>
    public string CreatePath(string name, bool isDirectory)
    {
        if (string.IsNullOrEmpty(name))
            return _current;

        // Handle absolute paths
        if (name.StartsWith("/"))
        {
            var systemPath = name.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_homeDir, systemPath);
        }

        // Relative path
        var result = Path.Combine(_current, name.Replace('/', Path.DirectorySeparatorChar));

        try
        {
            result = Path.GetFullPath(result);
        }
        catch
        {
            return "";
        }

        // Security check: ensure within home directory
        if (!result.StartsWith(_homeDir, StringComparison.OrdinalIgnoreCase))
            return "";

        return result;
    }

    /// <summary>
    /// List directory contents
    /// </summary>
    public string[] ListDirectory(string path = "")
    {
        var targetPath = string.IsNullOrEmpty(path) ? _current : CreatePath(path, true);

        if (string.IsNullOrEmpty(targetPath) || !Directory.Exists(targetPath))
            return Array.Empty<string>();

        try
        {
            var files = Directory.GetFiles(targetPath);
            var dirs = Directory.GetDirectories(targetPath);
            return dirs.Concat(files).ToArray();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
