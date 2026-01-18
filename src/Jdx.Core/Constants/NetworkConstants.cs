namespace Jdx.Core.Constants;

/// <summary>
/// Network protocol constants based on RFC specifications
/// </summary>
public static class NetworkConstants
{
    /// <summary>
    /// SMTP protocol constants (RFC 5321)
    /// </summary>
    public static class Smtp
    {
        /// <summary>
        /// Maximum line length: 998 characters + CRLF = 1000 (RFC 5321 Section 4.5.3.1.6)
        /// </summary>
        public const int MaxLineLength = 1000;

        /// <summary>
        /// Maximum recipients per message
        /// </summary>
        public const int MaxRecipients = 100;

        /// <summary>
        /// Maximum message size in bytes (default: 10MB)
        /// </summary>
        public const int DefaultMaxMessageSize = 10 * 1024 * 1024;

        /// <summary>
        /// Maximum lines in message body
        /// </summary>
        public const int MaxMessageLines = 100000;
    }

    /// <summary>
    /// POP3 protocol constants (RFC 1939)
    /// </summary>
    public static class Pop3
    {
        /// <summary>
        /// Maximum command line length: 512 octets (RFC 1939 Section 4)
        /// </summary>
        public const int MaxCommandLineLength = 512;

        /// <summary>
        /// Maximum message size (default: 10MB)
        /// </summary>
        public const int MaxMessageSize = 10 * 1024 * 1024;
    }

    /// <summary>
    /// FTP protocol constants (RFC 959)
    /// </summary>
    public static class Ftp
    {
        /// <summary>
        /// Maximum command line length
        /// </summary>
        public const int MaxCommandLineLength = 512;

        /// <summary>
        /// Minimum valid port number
        /// </summary>
        public const int MinPort = 1024;

        /// <summary>
        /// Maximum valid port number
        /// </summary>
        public const int MaxPort = 65535;
    }

    /// <summary>
    /// DHCP protocol constants (RFC 2131)
    /// </summary>
    public static class Dhcp
    {
        /// <summary>
        /// Minimum DHCP packet size: 300 octets (RFC 2131 Section 2)
        /// </summary>
        public const int MinPacketSize = 300;

        /// <summary>
        /// Maximum DHCP packet size: 576 octets (RFC 2131)
        /// </summary>
        public const int MaxPacketSize = 576;
    }

    /// <summary>
    /// DNS protocol constants (RFC 1035)
    /// </summary>
    public static class Dns
    {
        /// <summary>
        /// Maximum UDP packet size: 512 octets (RFC 1035 Section 4.2.1)
        /// </summary>
        public const int MaxUdpPacketSize = 512;

        /// <summary>
        /// Maximum TCP packet size
        /// </summary>
        public const int MaxTcpPacketSize = 65535;
    }

    /// <summary>
    /// TFTP protocol constants (RFC 1350)
    /// </summary>
    public static class Tftp
    {
        /// <summary>
        /// Block size: 512 octets (RFC 1350)
        /// </summary>
        public const int BlockSize = 512;

        /// <summary>
        /// Maximum file size (default: 100MB)
        /// </summary>
        public const int DefaultMaxFileSize = 100 * 1024 * 1024;

        /// <summary>
        /// Maximum filename length
        /// </summary>
        public const int MaxFilenameLength = 255;
    }

    /// <summary>
    /// HTTP protocol constants
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Maximum line length for headers: 8192 bytes
        /// </summary>
        public const int MaxLineLength = 8192;

        /// <summary>
        /// Maximum header size
        /// </summary>
        public const int MaxHeaderSize = 16384;

        /// <summary>
        /// Maximum request body size (default: 100MB)
        /// </summary>
        public const int MaxRequestBodySize = 100 * 1024 * 1024;
    }

    /// <summary>
    /// Common timeout values
    /// </summary>
    public static class Timeouts
    {
        /// <summary>
        /// Default network timeout in seconds
        /// </summary>
        public const int DefaultTimeoutSeconds = 30;

        /// <summary>
        /// Regex matching timeout in milliseconds (for ReDoS protection)
        /// </summary>
        public const int RegexTimeoutMilliseconds = 1000;
    }
}
