using System.Text;

namespace Jdx.Servers.Tftp;

/// <summary>
/// TFTP Opcode definitions (RFC 1350)
/// </summary>
public enum TftpOpcode : ushort
{
    Unknown = 0,
    RRQ = 1,      // Read Request
    WRQ = 2,      // Write Request
    DATA = 3,     // Data
    ACK = 4,      // Acknowledgment
    ERROR = 5,    // Error
    OACK = 6      // Option Acknowledgment (RFC 2347)
}

/// <summary>
/// TFTP Transfer Mode
/// </summary>
public enum TftpMode
{
    Netascii = 0,
    Octet = 1
}

/// <summary>
/// TFTP Error Codes (RFC 1350)
/// </summary>
public enum TftpErrorCode : ushort
{
    NotDefined = 0,
    FileNotFound = 1,
    AccessViolation = 2,
    DiskFull = 3,
    IllegalOperation = 4,
    UnknownTransferId = 5,
    FileAlreadyExists = 6,
    NoSuchUser = 7
}

/// <summary>
/// TFTP Protocol Packet Builder
/// </summary>
public static class TftpPacket
{
    /// <summary>
    /// Build RRQ/WRQ packet
    /// Format: [Opcode:2][Filename:string][0][Mode:string][0]
    /// </summary>
    public static byte[] BuildRequest(TftpOpcode opcode, string filename, TftpMode mode)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Opcode (network byte order - big endian)
        writer.Write(HostToNetworkOrder((ushort)opcode));

        // Filename (null-terminated ASCII string)
        writer.Write(Encoding.ASCII.GetBytes(filename));
        writer.Write((byte)0);

        // Mode (null-terminated ASCII string)
        var modeStr = mode == TftpMode.Octet ? "octet" : "netascii";
        writer.Write(Encoding.ASCII.GetBytes(modeStr));
        writer.Write((byte)0);

        return ms.ToArray();
    }

    /// <summary>
    /// Build DATA packet
    /// Format: [Opcode:2][BlockNumber:2][Data:0-512]
    /// </summary>
    public static byte[] BuildData(ushort blockNumber, byte[] data)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Opcode
        writer.Write(HostToNetworkOrder((ushort)TftpOpcode.DATA));

        // Block number
        writer.Write(HostToNetworkOrder(blockNumber));

        // Data
        if (data.Length > 0)
        {
            writer.Write(data);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Build ACK packet
    /// Format: [Opcode:2][BlockNumber:2]
    /// </summary>
    public static byte[] BuildAck(ushort blockNumber)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Opcode
        writer.Write(HostToNetworkOrder((ushort)TftpOpcode.ACK));

        // Block number
        writer.Write(HostToNetworkOrder(blockNumber));

        return ms.ToArray();
    }

    /// <summary>
    /// Build ERROR packet
    /// Format: [Opcode:2][ErrorCode:2][ErrMsg:string][0]
    /// </summary>
    public static byte[] BuildError(TftpErrorCode errorCode, string errorMessage)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Opcode
        writer.Write(HostToNetworkOrder((ushort)TftpOpcode.ERROR));

        // Error code
        writer.Write(HostToNetworkOrder((ushort)errorCode));

        // Error message (null-terminated ASCII string)
        writer.Write(Encoding.ASCII.GetBytes(errorMessage));
        writer.Write((byte)0);

        return ms.ToArray();
    }

    /// <summary>
    /// Parse opcode from packet
    /// </summary>
    public static TftpOpcode GetOpcode(byte[] packet)
    {
        if (packet.Length < 2)
            return TftpOpcode.Unknown;

        var opcode = NetworkToHostOrder(BitConverter.ToUInt16(packet, 0));
        return Enum.IsDefined(typeof(TftpOpcode), (int)opcode)
            ? (TftpOpcode)opcode
            : TftpOpcode.Unknown;
    }

    /// <summary>
    /// Parse block number from DATA or ACK packet
    /// </summary>
    public static ushort GetBlockNumber(byte[] packet)
    {
        if (packet.Length < 4)
            return 0;

        return NetworkToHostOrder(BitConverter.ToUInt16(packet, 2));
    }

    /// <summary>
    /// Parse error code from ERROR packet
    /// </summary>
    public static TftpErrorCode GetErrorCode(byte[] packet)
    {
        if (packet.Length < 4)
            return TftpErrorCode.NotDefined;

        var code = NetworkToHostOrder(BitConverter.ToUInt16(packet, 2));
        return Enum.IsDefined(typeof(TftpErrorCode), (int)code)
            ? (TftpErrorCode)code
            : TftpErrorCode.NotDefined;
    }

    /// <summary>
    /// Extract null-terminated string from packet
    /// </summary>
    public static string ExtractString(byte[] packet, int offset)
    {
        var endIndex = Array.IndexOf(packet, (byte)0, offset);
        if (endIndex == -1)
            endIndex = packet.Length;

        var length = endIndex - offset;
        if (length <= 0)
            return string.Empty;

        return Encoding.ASCII.GetString(packet, offset, length);
    }

    /// <summary>
    /// Parse RRQ/WRQ request packet
    /// </summary>
    public static (string filename, TftpMode mode) ParseRequest(byte[] packet)
    {
        // Extract filename
        var filename = ExtractString(packet, 2);
        var modeOffset = 2 + filename.Length + 1;

        // Extract mode
        var modeStr = ExtractString(packet, modeOffset);
        var mode = modeStr.Equals("octet", StringComparison.OrdinalIgnoreCase)
            ? TftpMode.Octet
            : TftpMode.Netascii;

        return (filename, mode);
    }

    /// <summary>
    /// Extract data from DATA packet
    /// </summary>
    public static byte[] ExtractData(byte[] packet)
    {
        if (packet.Length <= 4)
            return Array.Empty<byte>();

        var dataLength = packet.Length - 4;
        var data = new byte[dataLength];
        Array.Copy(packet, 4, data, 0, dataLength);
        return data;
    }

    /// <summary>
    /// Convert host byte order to network byte order (big endian)
    /// </summary>
    private static ushort HostToNetworkOrder(ushort value)
    {
        if (BitConverter.IsLittleEndian)
        {
            return (ushort)((value >> 8) | (value << 8));
        }
        return value;
    }

    /// <summary>
    /// Convert network byte order to host byte order
    /// </summary>
    private static ushort NetworkToHostOrder(ushort value)
    {
        return HostToNetworkOrder(value); // Same operation
    }
}
