namespace Jdx.Servers.Ftp;

/// <summary>
/// FTP response codes (RFC 959)
/// </summary>
/// <remarks>
/// Response codes are organized into categories:
/// 1xx - Positive Preliminary reply
/// 2xx - Positive Completion reply
/// 3xx - Positive Intermediate reply
/// 4xx - Transient Negative Completion reply
/// 5xx - Permanent Negative Completion reply
/// </remarks>
public static class FtpResponseCodes
{
    // 1xx - Positive Preliminary
    /// <summary>150 File status okay; about to open data connection</summary>
    public const string FileStatusOk = "150 File status okay; about to open data connection.";

    /// <summary>125 Data connection already open; transfer starting</summary>
    public const string DataConnectionOpen = "125 Data connection already open; transfer starting.";

    // 2xx - Positive Completion
    /// <summary>200 Command okay</summary>
    public const string CommandOk = "200 Command okay.";

    /// <summary>202 Command not implemented, superfluous at this site</summary>
    public const string CommandNotImplemented = "202 Command not implemented, superfluous at this site.";

    /// <summary>211 System status, or system help reply</summary>
    public const string SystemStatus = "211 System status, or system help reply.";

    /// <summary>214 Help message</summary>
    public const string HelpMessage = "214 Help message.";

    /// <summary>215 NAME system type (UNIX Type: L8)</summary>
    public const string SystemType = "215 UNIX Type: L8";

    /// <summary>220 Service ready for new user</summary>
    public const string ServiceReady = "220 Service ready for new user.";

    /// <summary>221 Service closing control connection</summary>
    public const string ServiceClosing = "221 Service closing control connection.";

    /// <summary>225 Data connection open; no transfer in progress</summary>
    public const string DataConnectionNoTransfer = "225 Data connection open; no transfer in progress.";

    /// <summary>226 Closing data connection; transfer complete</summary>
    public const string TransferComplete = "226 Closing data connection. Requested file action successful.";

    /// <summary>227 Entering Passive Mode (h1,h2,h3,h4,p1,p2)</summary>
    public static string EnteringPassiveMode(string host, int port)
    {
        var parts = host.Split('.');
        int p1 = port / 256;
        int p2 = port % 256;
        return $"227 Entering Passive Mode ({parts[0]},{parts[1]},{parts[2]},{parts[3]},{p1},{p2}).";
    }

    /// <summary>230 User logged in, proceed</summary>
    public const string UserLoggedIn = "230 User logged in, proceed.";

    /// <summary>250 Requested file action okay, completed</summary>
    public const string ActionCompleted = "250 Requested file action okay, completed.";

    /// <summary>257 "PATHNAME" created</summary>
    public static string PathCreated(string path) => $"257 \"{path}\" created.";

    /// <summary>257 "PATHNAME" is current directory</summary>
    public static string CurrentDirectory(string path) => $"257 \"{path}\" is current directory.";

    // 3xx - Positive Intermediate
    /// <summary>331 User name okay, need password</summary>
    public const string PasswordRequired = "331 User name okay, need password.";

    /// <summary>332 Need account for login</summary>
    public const string AccountRequired = "332 Need account for login.";

    /// <summary>350 Requested file action pending further information</summary>
    public const string ActionPending = "350 Requested file action pending further information.";

    // 4xx - Transient Negative Completion
    /// <summary>421 Service not available, closing control connection</summary>
    public const string ServiceNotAvailable = "421 Service not available, closing control connection.";

    /// <summary>425 Can't open data connection</summary>
    public const string DataConnectionFailed = "425 Can't open data connection.";

    /// <summary>426 Connection closed; transfer aborted</summary>
    public const string TransferAborted = "426 Connection closed; transfer aborted.";

    /// <summary>450 Requested file action not taken (file unavailable)</summary>
    public const string FileNotAvailable = "450 Requested file action not taken.";

    /// <summary>451 Requested action aborted: local error in processing</summary>
    public const string ActionAbortedLocalError = "451 Requested action aborted: local error in processing.";

    /// <summary>452 Requested action not taken (insufficient storage)</summary>
    public const string InsufficientStorage = "452 Requested action not taken.";

    // 5xx - Permanent Negative Completion
    /// <summary>500 Syntax error, command unrecognized</summary>
    public const string SyntaxError = "500 Syntax error, command unrecognized.";

    /// <summary>501 Syntax error in parameters or arguments</summary>
    public const string ParameterSyntaxError = "501 Syntax error in parameters or arguments.";

    /// <summary>502 Command not implemented</summary>
    public const string NotImplemented = "502 Command not implemented.";

    /// <summary>503 Bad sequence of commands</summary>
    public const string BadSequence = "503 Bad sequence of commands.";

    /// <summary>504 Command not implemented for that parameter</summary>
    public const string ParameterNotImplemented = "504 Command not implemented for that parameter.";

    /// <summary>530 Not logged in</summary>
    public const string NotLoggedIn = "530 Not logged in.";

    /// <summary>532 Need account for storing files</summary>
    public const string AccountNeededForStorage = "532 Need account for storing files.";

    /// <summary>550 Requested action not taken (file unavailable, access denied)</summary>
    public const string FileActionFailed = "550 Requested action not taken.";

    /// <summary>550 Requested action not taken (access denied)</summary>
    public const string AccessDenied = "550 Access denied.";

    /// <summary>550 Requested action not taken (path not found)</summary>
    public static string PathNotFound(string path) => $"550 {path}: No such file or directory.";

    /// <summary>550 Requested action not taken (file already exists)</summary>
    public static string FileExists(string path) => $"550 {path}: File exists.";

    /// <summary>551 Requested action aborted: page type unknown</summary>
    public const string UnknownPageType = "551 Requested action aborted: page type unknown.";

    /// <summary>552 Requested file action aborted (exceeded storage allocation)</summary>
    public const string StorageExceeded = "552 Requested file action aborted.";

    /// <summary>553 Requested action not taken (file name not allowed)</summary>
    public const string FileNameNotAllowed = "553 Requested action not taken.";

    /// <summary>500 Command requires a parameter</summary>
    public static string ParameterRequired(FtpCommand command) => $"500 {command}: command requires a parameter.";
}
