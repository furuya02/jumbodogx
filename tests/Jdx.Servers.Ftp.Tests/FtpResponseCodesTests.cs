namespace Jdx.Servers.Ftp.Tests;

/// <summary>
/// FtpResponseCodesのユニットテスト
/// </summary>
public class FtpResponseCodesTests
{
    #region Constant Response Codes Tests

    [Fact]
    public void FileStatusOk_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("150", FtpResponseCodes.FileStatusOk);
    }

    [Fact]
    public void DataConnectionOpen_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("125", FtpResponseCodes.DataConnectionOpen);
    }

    [Fact]
    public void CommandOk_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("200", FtpResponseCodes.CommandOk);
    }

    [Fact]
    public void SystemType_ReturnsUnixType()
    {
        // Assert
        Assert.StartsWith("215", FtpResponseCodes.SystemType);
        Assert.Contains("UNIX", FtpResponseCodes.SystemType);
    }

    [Fact]
    public void ServiceReady_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("220", FtpResponseCodes.ServiceReady);
    }

    [Fact]
    public void ServiceClosing_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("221", FtpResponseCodes.ServiceClosing);
    }

    [Fact]
    public void TransferComplete_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("226", FtpResponseCodes.TransferComplete);
    }

    [Fact]
    public void UserLoggedIn_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("230", FtpResponseCodes.UserLoggedIn);
    }

    [Fact]
    public void ActionCompleted_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("250", FtpResponseCodes.ActionCompleted);
    }

    [Fact]
    public void PasswordRequired_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("331", FtpResponseCodes.PasswordRequired);
    }

    [Fact]
    public void ServiceNotAvailable_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("421", FtpResponseCodes.ServiceNotAvailable);
    }

    [Fact]
    public void DataConnectionFailed_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("425", FtpResponseCodes.DataConnectionFailed);
    }

    [Fact]
    public void SyntaxError_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("500", FtpResponseCodes.SyntaxError);
    }

    [Fact]
    public void ParameterSyntaxError_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("501", FtpResponseCodes.ParameterSyntaxError);
    }

    [Fact]
    public void NotImplemented_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("502", FtpResponseCodes.NotImplemented);
    }

    [Fact]
    public void BadSequence_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("503", FtpResponseCodes.BadSequence);
    }

    [Fact]
    public void NotLoggedIn_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("530", FtpResponseCodes.NotLoggedIn);
    }

    [Fact]
    public void FileActionFailed_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("550", FtpResponseCodes.FileActionFailed);
    }

    [Fact]
    public void AccessDenied_ReturnsCorrectCode()
    {
        // Assert
        Assert.StartsWith("550", FtpResponseCodes.AccessDenied);
    }

    #endregion

    #region Dynamic Response Code Tests

    [Fact]
    public void EnteringPassiveMode_WithValidHostAndPort_ReturnsCorrectFormat()
    {
        // Arrange
        var host = "192.168.1.100";
        var port = 50000;

        // Act
        var result = FtpResponseCodes.EnteringPassiveMode(host, port);

        // Assert
        Assert.StartsWith("227", result);
        Assert.Contains("Entering Passive Mode", result);
        Assert.Contains("192,168,1,100", result);
        // port 50000 = 195 * 256 + 80
        Assert.Contains("195,80", result);
    }

    [Fact]
    public void EnteringPassiveMode_WithPort256_ReturnsCorrectFormat()
    {
        // Arrange
        var host = "127.0.0.1";
        var port = 256;

        // Act
        var result = FtpResponseCodes.EnteringPassiveMode(host, port);

        // Assert
        // port 256 = 1 * 256 + 0
        Assert.Contains("1,0", result);
    }

    [Fact]
    public void EnteringPassiveMode_WithLowPort_ReturnsCorrectFormat()
    {
        // Arrange
        var host = "10.0.0.1";
        var port = 21;

        // Act
        var result = FtpResponseCodes.EnteringPassiveMode(host, port);

        // Assert
        // port 21 = 0 * 256 + 21
        Assert.Contains("0,21", result);
    }

    [Fact]
    public void PathCreated_WithPath_ReturnsCorrectFormat()
    {
        // Arrange
        var path = "/home/user/newdir";

        // Act
        var result = FtpResponseCodes.PathCreated(path);

        // Assert
        Assert.StartsWith("257", result);
        Assert.Contains(path, result);
        Assert.Contains("created", result);
    }

    [Fact]
    public void CurrentDirectory_WithPath_ReturnsCorrectFormat()
    {
        // Arrange
        var path = "/home/user";

        // Act
        var result = FtpResponseCodes.CurrentDirectory(path);

        // Assert
        Assert.StartsWith("257", result);
        Assert.Contains(path, result);
        Assert.Contains("current directory", result);
    }

    [Fact]
    public void PathNotFound_WithPath_ReturnsCorrectFormat()
    {
        // Arrange
        var path = "/nonexistent/path";

        // Act
        var result = FtpResponseCodes.PathNotFound(path);

        // Assert
        Assert.StartsWith("550", result);
        Assert.Contains(path, result);
        Assert.Contains("No such file or directory", result);
    }

    [Fact]
    public void FileExists_WithPath_ReturnsCorrectFormat()
    {
        // Arrange
        var path = "/existing/file.txt";

        // Act
        var result = FtpResponseCodes.FileExists(path);

        // Assert
        Assert.StartsWith("550", result);
        Assert.Contains(path, result);
        Assert.Contains("File exists", result);
    }

    [Fact]
    public void ParameterRequired_WithCommand_ReturnsCorrectFormat()
    {
        // Arrange
        var command = FtpCommand.USER;

        // Act
        var result = FtpResponseCodes.ParameterRequired(command);

        // Assert
        Assert.StartsWith("500", result);
        Assert.Contains("USER", result);
        Assert.Contains("requires a parameter", result);
    }

    #endregion

    #region RFC 959 Compliance Tests

    [Theory]
    [InlineData("FileStatusOk", "150")]
    [InlineData("DataConnectionOpen", "125")]
    [InlineData("CommandOk", "200")]
    [InlineData("ServiceReady", "220")]
    [InlineData("ServiceClosing", "221")]
    [InlineData("TransferComplete", "226")]
    [InlineData("UserLoggedIn", "230")]
    [InlineData("ActionCompleted", "250")]
    [InlineData("PasswordRequired", "331")]
    [InlineData("ServiceNotAvailable", "421")]
    [InlineData("SyntaxError", "500")]
    [InlineData("NotLoggedIn", "530")]
    public void ResponseCodes_FollowRfc959Format(string codeName, string expectedPrefix)
    {
        // Arrange
        var property = typeof(FtpResponseCodes).GetField(codeName);

        // Act
        var value = property?.GetValue(null) as string;

        // Assert
        Assert.NotNull(value);
        Assert.StartsWith(expectedPrefix, value);
        // RFC 959 format: 3-digit code followed by space
        Assert.Matches(@"^\d{3}\s", value);
    }

    #endregion
}
