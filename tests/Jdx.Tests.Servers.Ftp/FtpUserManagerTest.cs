using Jdx.Core.Settings;
using Jdx.Servers.Ftp;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Tests.Servers.Ftp;

public class FtpUserManagerTest
{
    private readonly Mock<ILogger> _mockLogger;

    public FtpUserManagerTest()
    {
        _mockLogger = new Mock<ILogger>();
    }

    [Fact]
    public void GetUser_FindsExistingUser()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "testuser", Password = "password", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var user = manager.GetUser("testuser");

        // Assert
        Assert.NotNull(user);
        Assert.Equal("testuser", user.UserName);
    }

    [Fact]
    public void GetUser_ReturnsNullForNonExistentUser()
    {
        // Arrange
        var users = new List<FtpUserEntry>();
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var user = manager.GetUser("nonexistent");

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public void GetUser_Anonymous_IsCaseInsensitive()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "anonymous", Password = "", HomeDirectory = "/tmp/ftp" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act & Assert
        Assert.NotNull(manager.GetUser("anonymous"));
        Assert.NotNull(manager.GetUser("ANONYMOUS"));
        Assert.NotNull(manager.GetUser("Anonymous"));
        Assert.NotNull(manager.GetUser("aNonymOus"));
    }

    [Fact]
    public void GetUser_RegularUser_IsCaseSensitive()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "TestUser", Password = "password", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act & Assert
        Assert.NotNull(manager.GetUser("TestUser"));
        Assert.Null(manager.GetUser("testuser"));
        Assert.Null(manager.GetUser("TESTUSER"));
    }

    [Fact]
    public void Authenticate_AnonymousUser_EmptyPassword()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "anonymous", Password = "", HomeDirectory = "/tmp/ftp" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.Authenticate("anonymous", "anypassword");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Authenticate_ValidCredentials_ReturnsTrue()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "testuser", Password = "password123", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.Authenticate("testuser", "password123");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Authenticate_InvalidPassword_ReturnsFalse()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "testuser", Password = "password123", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.Authenticate("testuser", "wrongpassword");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Authenticate_NonExistentUser_ReturnsFalse()
    {
        // Arrange
        var users = new List<FtpUserEntry>();
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.Authenticate("nonexistent", "password");

        // Assert
        Assert.False(result);
    }
}
