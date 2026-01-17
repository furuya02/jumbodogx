namespace Jdx.Servers.Ftp;

/// <summary>
/// FTP transfer type (ASCII or Binary)
/// Based on bjd5-master/FtpServer/FtpType.cs
///
/// Note: On Windows, both ASCII and Binary modes operate the same way
/// due to \r\n line endings, so this is primarily for display purposes.
/// </summary>
public enum FtpTransferType
{
    ASCII,
    BINARY
}
