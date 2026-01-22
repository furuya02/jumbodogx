using Jdx.Core.Settings;

namespace Jdx.Servers.Ftp.Tests;

/// <summary>
/// FtpMountManagerのユニットテスト
/// </summary>
public class FtpMountManagerTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidMounts_InitializesCorrectly()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual", ToFolder = "/real/path" }
        };

        // Act
        var manager = new FtpMountManager(mounts);

        // Assert
        Assert.NotNull(manager);
        Assert.Single(manager.GetMounts());
    }

    [Fact]
    public void Constructor_WithEmptyMounts_InitializesCorrectly()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>();

        // Act
        var manager = new FtpMountManager(mounts);

        // Assert
        Assert.NotNull(manager);
        Assert.Empty(manager.GetMounts());
    }

    [Fact]
    public void Constructor_WithNullMounts_InitializesWithEmptyList()
    {
        // Act
        var manager = new FtpMountManager(null!);

        // Assert
        Assert.NotNull(manager);
        Assert.Empty(manager.GetMounts());
    }

    #endregion

    #region GetMounts Tests

    [Fact]
    public void GetMounts_ReturnsAllMounts()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual1", ToFolder = "/real/path1" },
            new() { FromFolder = "/virtual2", ToFolder = "/real/path2" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.GetMounts();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetMounts_ReturnsReadOnlyList()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual", ToFolder = "/real/path" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.GetMounts();

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<FtpMountEntry>>(result);
    }

    #endregion

    #region FindByVirtualPath Tests

    [Fact]
    public void FindByVirtualPath_WithExistingPath_ReturnsMount()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual/folder", ToFolder = "/real/path" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.FindByVirtualPath("/virtual/folder");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/virtual/folder", result.FromFolder);
    }

    [Fact]
    public void FindByVirtualPath_WithTrailingSlash_ReturnsMount()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual/folder/", ToFolder = "/real/path" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.FindByVirtualPath("/virtual/folder");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FindByVirtualPath_CaseInsensitive_ReturnsMount()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/Virtual/Folder", ToFolder = "/real/path" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.FindByVirtualPath("/virtual/folder");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FindByVirtualPath_WithNonExistingPath_ReturnsNull()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual/folder", ToFolder = "/real/path" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.FindByVirtualPath("/nonexistent");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region FindByPhysicalPath Tests

    [Fact]
    public void FindByPhysicalPath_WithExistingPath_ReturnsMount()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual", ToFolder = "/real/path" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.FindByPhysicalPath("/real/path/subdir");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/real/path", result.ToFolder);
    }

    [Fact]
    public void FindByPhysicalPath_ExactMatch_ReturnsMount()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual", ToFolder = "/real/path" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.FindByPhysicalPath("/real/path");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FindByPhysicalPath_WithNonExistingPath_ReturnsNull()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual", ToFolder = "/real/path" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.FindByPhysicalPath("/other/path");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region IsInMountPoint Tests

    [Fact]
    public void IsInMountPoint_WithPathInMount_ReturnsTrue()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual", ToFolder = "/real/path" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.IsInMountPoint("/real/path/subdir/file.txt");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInMountPoint_WithPathNotInMount_ReturnsFalse()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual", ToFolder = "/real/path" }
        };
        var manager = new FtpMountManager(mounts);

        // Act
        var result = manager.IsInMountPoint("/other/path");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInMountPoint_WithEmptyMounts_ReturnsFalse()
    {
        // Arrange
        var manager = new FtpMountManager(new List<FtpMountEntry>());

        // Act
        var result = manager.IsInMountPoint("/any/path");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInMountPoint_MultipleMounts_ChecksAll()
    {
        // Arrange
        var mounts = new List<FtpMountEntry>
        {
            new() { FromFolder = "/virtual1", ToFolder = "/real/path1" },
            new() { FromFolder = "/virtual2", ToFolder = "/real/path2" }
        };
        var manager = new FtpMountManager(mounts);

        // Act & Assert
        Assert.True(manager.IsInMountPoint("/real/path1/file"));
        Assert.True(manager.IsInMountPoint("/real/path2/file"));
        Assert.False(manager.IsInMountPoint("/real/path3/file"));
    }

    #endregion
}
