using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;
using Moq;

namespace Jdx.Servers.Ftp.Tests;

/// <summary>
/// FtpUserManagerのユニットテスト
/// </summary>
public class FtpUserManagerTests
{
    private readonly Mock<ILogger> _mockLogger;

    public FtpUserManagerTests()
    {
        _mockLogger = new Mock<ILogger>();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "user1", Password = "pass1", HomeDirectory = "/home/user1" }
        };

        // Act
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Assert
        Assert.NotNull(manager);
    }

    [Fact]
    public void Constructor_WithEmptyUserList_InitializesCorrectly()
    {
        // Arrange
        var users = new List<FtpUserEntry>();

        // Act
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Assert
        Assert.NotNull(manager);
    }

    #endregion

    #region GetUser Tests

    [Fact]
    public void GetUser_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "testuser", Password = "testpass", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.GetUser("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.UserName);
    }

    [Fact]
    public void GetUser_WithNonExistingUser_ReturnsNull()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "testuser", Password = "testpass", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.GetUser("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUser_WithNullUserName_ReturnsNull()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "testuser", Password = "testpass", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.GetUser(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUser_WithEmptyUserName_ReturnsNull()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "testuser", Password = "testpass", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.GetUser("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUser_AnonymousLowerCase_ReturnsCaseInsensitiveMatch()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "anonymous", Password = "", HomeDirectory = "/home/anonymous" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.GetUser("ANONYMOUS");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("anonymous", result.UserName);
    }

    [Fact]
    public void GetUser_AnonymousUpperCase_ReturnsCaseInsensitiveMatch()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "ANONYMOUS", Password = "", HomeDirectory = "/home/anonymous" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.GetUser("anonymous");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ANONYMOUS", result.UserName);
    }

    [Fact]
    public void GetUser_RegularUserCaseSensitive_ReturnsExactMatch()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "TestUser", Password = "pass", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.GetUser("testuser");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Authenticate Tests

    [Fact]
    public void Authenticate_WithValidCredentials_ReturnsTrue()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "testuser", Password = "testpass", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.Authenticate("testuser", "testpass");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Authenticate_WithInvalidPassword_ReturnsFalse()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "testuser", Password = "testpass", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.Authenticate("testuser", "wrongpass");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Authenticate_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "testuser", Password = "testpass", HomeDirectory = "/home/testuser" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.Authenticate("nonexistent", "testpass");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Authenticate_AnonymousWithEmptyPassword_ReturnsTrue()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "anonymous", Password = "", HomeDirectory = "/home/anonymous" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.Authenticate("anonymous", "");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Authenticate_AnonymousWithEmail_ReturnsTrue()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "anonymous", Password = "", HomeDirectory = "/home/anonymous" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.Authenticate("anonymous", "user@example.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Authenticate_AnonymousCaseInsensitive_ReturnsTrue()
    {
        // Arrange
        var users = new List<FtpUserEntry>
        {
            new() { UserName = "anonymous", Password = "", HomeDirectory = "/home/anonymous" }
        };
        var manager = new FtpUserManager(users, _mockLogger.Object);

        // Act
        var result = manager.Authenticate("ANONYMOUS", "");

        // Assert
        Assert.True(result);
    }

    #endregion

    #region EncryptPassword Tests

    [Fact]
    public void EncryptPassword_WithValidInput_ReturnsNonEmptyString()
    {
        // Arrange
        var password = "testpassword";

        // Act
        var result = FtpUserManager.EncryptPassword(password);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void EncryptPassword_SameInput_ReturnsSameOutput()
    {
        // Arrange
        var password = "testpassword";

        // Act
        var result1 = FtpUserManager.EncryptPassword(password);
        var result2 = FtpUserManager.EncryptPassword(password);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void EncryptPassword_DifferentInputs_ReturnsDifferentOutputs()
    {
        // Arrange
        var password1 = "password1";
        var password2 = "password2";

        // Act
        var result1 = FtpUserManager.EncryptPassword(password1);
        var result2 = FtpUserManager.EncryptPassword(password2);

        // Assert
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public void EncryptPassword_EmptyInput_ReturnsNonEmptyString()
    {
        // Arrange
        var password = "";

        // Act
        var result = FtpUserManager.EncryptPassword(password);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region DecryptPassword Tests

    [Fact]
    public void DecryptPassword_WithValidInput_ReturnsInput()
    {
        // Arrange
        var encrypted = "encryptedvalue";

        // Act
        var result = FtpUserManager.DecryptPassword(encrypted);

        // Assert
        Assert.Equal(encrypted, result);
    }

    #endregion
}
