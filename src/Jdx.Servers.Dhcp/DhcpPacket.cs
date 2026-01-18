using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace Jdx.Servers.Dhcp;

/// <summary>
/// DHCP Packet Handler (RFC 2131)
/// </summary>
public class DhcpPacket
{
    private const uint MagicCookie = 0x63538263;
    private const int MinPacketSize = 236;

    public byte OpCode { get; private set; }
    public byte HardwareType { get; private set; }
    public byte HardwareAddressLength { get; private set; }
    public uint TransactionId { get; private set; }
    public ushort Seconds { get; private set; }
    public ushort Flags { get; private set; }
    public IPAddress ClientIp { get; private set; }
    public IPAddress YourIp { get; private set; }
    public IPAddress ServerIp { get; private set; }
    public IPAddress GatewayIp { get; private set; }
    public PhysicalAddress ClientMac { get; private set; }
    public DhcpMessageType MessageType { get; private set; }
    public IPAddress? RequestedIp { get; private set; }
    public IPAddress? ServerIdentifier { get; private set; }

    private readonly Dictionary<byte, byte[]> _options = new();

    public DhcpPacket()
    {
        OpCode = 0;
        HardwareType = 1;
        HardwareAddressLength = 6;
        TransactionId = 0;
        ClientIp = IPAddress.Any;
        YourIp = IPAddress.Any;
        ServerIp = IPAddress.Any;
        GatewayIp = IPAddress.Any;
        ClientMac = PhysicalAddress.None;
        MessageType = DhcpMessageType.Unknown;
    }

    /// <summary>
    /// Parse DHCP packet from byte array
    /// </summary>
    public bool Parse(byte[] buffer)
    {
        if (buffer.Length < MinPacketSize)
        {
            return false;
        }

        try
        {
            using var ms = new MemoryStream(buffer);
            using var reader = new BinaryReader(ms);

            // Fixed header (236 bytes)
            OpCode = reader.ReadByte();
            HardwareType = reader.ReadByte();
            HardwareAddressLength = reader.ReadByte();
            reader.ReadByte(); // Hops

            TransactionId = ReadUInt32BigEndian(reader);
            Seconds = ReadUInt16BigEndian(reader);
            Flags = ReadUInt16BigEndian(reader);

            ClientIp = new IPAddress(reader.ReadBytes(4));
            YourIp = new IPAddress(reader.ReadBytes(4));
            ServerIp = new IPAddress(reader.ReadBytes(4));
            GatewayIp = new IPAddress(reader.ReadBytes(4));

            var macBytes = reader.ReadBytes(16);
            ClientMac = new PhysicalAddress(macBytes[..6]);

            reader.ReadBytes(64); // Server hostname
            reader.ReadBytes(128); // Boot filename

            // Magic cookie
            var cookie = ReadUInt32BigEndian(reader);
            if (cookie != MagicCookie)
            {
                return false;
            }

            // Parse options
            ParseOptions(reader);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Build DHCP response packet
    /// </summary>
    public byte[] Build(DhcpMessageType messageType, IPAddress assignedIp, IPAddress serverIp,
        int leaseTime, IPAddress? subnetMask = null, IPAddress? router = null,
        IPAddress? dnsServer1 = null, IPAddress? dnsServer2 = null, string? wpadUrl = null)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Fixed header
        writer.Write((byte)2); // Op: Boot Reply
        writer.Write(HardwareType);
        writer.Write(HardwareAddressLength);
        writer.Write((byte)0); // Hops

        WriteUInt32BigEndian(writer, TransactionId);
        WriteUInt16BigEndian(writer, Seconds);
        WriteUInt16BigEndian(writer, Flags);

        writer.Write(ClientIp.GetAddressBytes());
        writer.Write(assignedIp.GetAddressBytes());
        writer.Write(serverIp.GetAddressBytes());
        writer.Write(GatewayIp.GetAddressBytes());

        var macBytes = new byte[16];
        ClientMac.GetAddressBytes().CopyTo(macBytes, 0);
        writer.Write(macBytes);

        writer.Write(new byte[64]); // Server hostname
        writer.Write(new byte[128]); // Boot filename

        // Magic cookie
        WriteUInt32BigEndian(writer, MagicCookie);

        // Options
        WriteOption(writer, 53, new[] { (byte)messageType }); // Message Type

        if (subnetMask != null)
        {
            WriteOption(writer, 1, subnetMask.GetAddressBytes()); // Subnet Mask
        }

        if (router != null)
        {
            WriteOption(writer, 3, router.GetAddressBytes()); // Router
        }

        if (dnsServer1 != null || dnsServer2 != null)
        {
            var dnsBytes = new List<byte>();
            if (dnsServer1 != null)
            {
                dnsBytes.AddRange(dnsServer1.GetAddressBytes());
            }
            if (dnsServer2 != null)
            {
                dnsBytes.AddRange(dnsServer2.GetAddressBytes());
            }
            WriteOption(writer, 6, dnsBytes.ToArray()); // DNS
        }

        WriteOption(writer, 51, BitConverter.GetBytes(leaseTime).Reverse().ToArray()); // Lease Time
        WriteOption(writer, 54, serverIp.GetAddressBytes()); // Server Identifier

        var renewalTime = (int)(leaseTime * 0.5);
        WriteOption(writer, 58, BitConverter.GetBytes(renewalTime).Reverse().ToArray()); // Renewal Time (T1)

        var rebindingTime = (int)(leaseTime * 0.85);
        WriteOption(writer, 59, BitConverter.GetBytes(rebindingTime).Reverse().ToArray()); // Rebinding Time (T2)

        if (!string.IsNullOrEmpty(wpadUrl))
        {
            WriteOption(writer, 252, Encoding.ASCII.GetBytes(wpadUrl)); // WPAD
        }

        writer.Write((byte)255); // End option

        return ms.ToArray();
    }

    private void ParseOptions(BinaryReader reader)
    {
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var code = reader.ReadByte();

            if (code == 255) // End
            {
                break;
            }

            if (code == 0) // Pad
            {
                continue;
            }

            var length = reader.ReadByte();
            var data = reader.ReadBytes(length);

            _options[code] = data;

            // Parse important options
            switch (code)
            {
                case 53: // Message Type
                    if (data.Length > 0)
                    {
                        MessageType = (DhcpMessageType)data[0];
                    }
                    break;

                case 50: // Requested IP Address
                    if (data.Length == 4)
                    {
                        RequestedIp = new IPAddress(data);
                    }
                    break;

                case 54: // Server Identifier
                    if (data.Length == 4)
                    {
                        ServerIdentifier = new IPAddress(data);
                    }
                    break;
            }
        }
    }

    private void WriteOption(BinaryWriter writer, byte code, byte[] data)
    {
        writer.Write(code);
        writer.Write((byte)data.Length);
        writer.Write(data);
    }

    private static uint ReadUInt32BigEndian(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(4);
        Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    private static ushort ReadUInt16BigEndian(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(2);
        Array.Reverse(bytes);
        return BitConverter.ToUInt16(bytes, 0);
    }

    private static void WriteUInt32BigEndian(BinaryWriter writer, uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        writer.Write(bytes);
    }

    private static void WriteUInt16BigEndian(BinaryWriter writer, ushort value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        writer.Write(bytes);
    }
}
