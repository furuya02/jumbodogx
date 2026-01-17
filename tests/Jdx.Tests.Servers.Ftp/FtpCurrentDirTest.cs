using System.IO;
using Jdx.Core.Settings;
using Jdx.Servers.Ftp;
using Xunit;

namespace Jdx.Tests.Servers.Ftp;

public class FtpCurrentDirTest : IDisposable
{
    private readonly string _testRoot;
    private readonly FtpMountManager _mountManager;

    public FtpCurrentDirTest()
    {
        // Create test directory structure
        _testRoot = Path.Combine(Path.GetTempPath(), "ftp_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testRoot);
        Directory.CreateDirectory(Path.Combine(_testRoot, "subdir1"));
        Directory.CreateDirectory(Path.Combine(_testRoot, "subdir2"));
        File.WriteAllText(Path.Combine(_testRoot, "file1.txt"), "test");

        _mountManager = new FtpMountManager(new List<FtpMountEntry>());
    }

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, true);
        }
    }

    [Fact]
    public void Constructor_InitializesWithHomeDirectory()
    {
        // Arrange & Act
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);

        // Assert
        Assert.Equal("/", currentDir.GetPwd());
    }

    [Fact]
    public void GetPhysicalPath_ReturnsCurrentDirectory()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);

        // Act
        var physicalPath = currentDir.GetPhysicalPath();

        // Assert
        Assert.Equal(_testRoot + Path.DirectorySeparatorChar, physicalPath);
    }

    [Fact]
    public void ChangeDirectory_Subdirectory_Success()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);

        // Act
        var result = currentDir.ChangeDirectory("subdir1");

        // Assert
        Assert.True(result);
        Assert.Equal("/subdir1", currentDir.GetPwd());
    }

    [Fact]
    public void ChangeDirectory_NonExistentDirectory_Fails()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);

        // Act
        var result = currentDir.ChangeDirectory("nonexistent");

        // Assert
        Assert.False(result);
        Assert.Equal("/", currentDir.GetPwd());
    }

    [Fact]
    public void ChangeDirectory_ParentDirectory_Success()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);
        var changeResult = currentDir.ChangeDirectory("subdir1");
        Assert.True(changeResult, "Should successfully change to subdir1");
        Assert.Equal("/subdir1", currentDir.GetPwd());

        // Act
        var result = currentDir.ChangeDirectory("..");

        // Assert
        Assert.True(result, "Should successfully change to parent directory");
        Assert.Equal("/", currentDir.GetPwd());
    }

    [Fact]
    public void ChangeDirectory_ParentFromRoot_Fails()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);

        // Act
        var result = currentDir.ChangeDirectory("..");

        // Assert
        Assert.False(result);
        Assert.Equal("/", currentDir.GetPwd());
    }

    [Fact]
    public void ChangeDirectory_CurrentDirectory_Success()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);

        // Act
        var result = currentDir.ChangeDirectory(".");

        // Assert
        Assert.True(result);
        Assert.Equal("/", currentDir.GetPwd());
    }

    [Fact]
    public void ChangeDirectory_AbsolutePath_Success()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);
        currentDir.ChangeDirectory("subdir1");

        // Act
        var result = currentDir.ChangeDirectory("/subdir2");

        // Assert
        Assert.True(result);
        Assert.Equal("/subdir2", currentDir.GetPwd());
    }

    [Fact]
    public void CreatePath_RelativePath_ReturnsFullPath()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);

        // Act
        var path = currentDir.CreatePath("file1.txt", false);

        // Assert
        Assert.Equal(Path.Combine(_testRoot, "file1.txt"), path);
    }

    [Fact]
    public void CreatePath_AbsolutePath_ReturnsFullPath()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);

        // Act
        var path = currentDir.CreatePath("/subdir1/file.txt", false);

        // Assert
        Assert.Equal(Path.Combine(_testRoot, "subdir1", "file.txt"), path);
    }

    [Fact]
    public void ListDirectory_ReturnsFilesAndDirectories()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);

        // Act
        var list = currentDir.ListDirectory();

        // Assert
        Assert.NotEmpty(list);
        Assert.Contains(list, item => item.Contains("subdir1"));
        Assert.Contains(list, item => item.Contains("subdir2"));
        Assert.Contains(list, item => item.Contains("file1.txt"));
    }

    [Fact]
    public void ListDirectory_NonExistentPath_ReturnsEmpty()
    {
        // Arrange
        var currentDir = new FtpCurrentDir(_testRoot, _mountManager);

        // Act
        var list = currentDir.ListDirectory("nonexistent");

        // Assert
        Assert.Empty(list);
    }
}
