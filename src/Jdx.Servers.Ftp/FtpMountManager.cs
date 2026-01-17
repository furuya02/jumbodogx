using System.Collections.Generic;
using System.Linq;
using Jdx.Core.Settings;

namespace Jdx.Servers.Ftp;

/// <summary>
/// Virtual folder mount management
/// Based on bjd5-master/FtpServer/ListMount.cs and MountList.cs
/// </summary>
public class FtpMountManager
{
    private readonly List<FtpMountEntry> _mounts;

    public FtpMountManager(List<FtpMountEntry> mounts)
    {
        _mounts = mounts ?? new List<FtpMountEntry>();
    }

    /// <summary>
    /// Get all mount entries
    /// </summary>
    public IReadOnlyList<FtpMountEntry> GetMounts() => _mounts.AsReadOnly();

    /// <summary>
    /// Find mount entry by virtual path (FromFolder)
    /// </summary>
    public FtpMountEntry? FindByVirtualPath(string virtualPath)
    {
        return _mounts.FirstOrDefault(m =>
            m.FromFolder.TrimEnd('/', '\\').Equals(virtualPath.TrimEnd('/', '\\'),
            System.StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Find mount entry by physical path (ToFolder)
    /// </summary>
    public FtpMountEntry? FindByPhysicalPath(string physicalPath)
    {
        return _mounts.FirstOrDefault(m =>
            physicalPath.StartsWith(m.ToFolder, System.StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if path is within a mount point
    /// </summary>
    public bool IsInMountPoint(string path)
    {
        return _mounts.Any(m =>
            path.StartsWith(m.ToFolder, System.StringComparison.OrdinalIgnoreCase));
    }
}
