namespace Jdx.Servers.Ftp;

/// <summary>
/// FTP command definitions
/// Based on bjd5-master/FtpServer/FtpCmd.cs
/// </summary>
public enum FtpCommand
{
    // Authentication
    USER,
    PASS,

    // Directory navigation
    CWD,
    CDUP,
    PWD,
    XPWD,  // Alternative PWD command

    // Data connection
    PORT,
    EPRT,  // Extended PORT
    PASV,
    EPSV,  // Extended PASV

    // File transfer
    RETR,  // Retrieve (download)
    STOR,  // Store (upload)

    // File/Directory operations
    DELE,  // Delete file
    MKD,   // Make directory
    RMD,   // Remove directory
    RNFR,  // Rename from
    RNTO,  // Rename to

    // Listing
    LIST,
    NLST,  // Name list

    // Control
    TYPE,  // Transfer type (ASCII/Binary)
    SYST,  // System information
    NOOP,  // No operation
    QUIT,
    ABOR,  // Abort transfer

    // Unknown command
    UNKNOWN
}
