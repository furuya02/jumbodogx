using Jdx.Core.Settings;

namespace Jdx.Servers.Dns;

/// <summary>
/// SOA Record (Start of Authority)
/// Based on bjd5-master/DnsServer/RrSoa.cs
/// </summary>
public class RrSoa : OneRr
{
    public RrSoa(string name, uint ttl, string nameServer, string postMaster,
                 uint serial, uint refresh, uint retry, uint expire, uint minimum)
        : base(name, DnsType.Soa, ttl, CreateData(nameServer, postMaster, serial, refresh, retry, expire, minimum))
    {
    }

    public RrSoa(string name, uint ttl, byte[] data)
        : base(name, DnsType.Soa, ttl, data)
    {
    }

    private static byte[] CreateData(string nameServer, string postMaster,
                                     uint serial, uint refresh, uint retry, uint expire, uint minimum)
    {
        var ns = DnsUtil.Str2DnsName(nameServer);
        var pm = DnsUtil.Str2DnsName(postMaster);
        var serialBytes = GetUIntBytes(serial);
        var refreshBytes = GetUIntBytes(refresh);
        var retryBytes = GetUIntBytes(retry);
        var expireBytes = GetUIntBytes(expire);
        var minimumBytes = GetUIntBytes(minimum);

        var result = new byte[ns.Length + pm.Length + 20]; // 5 * 4 bytes for uint fields
        var offset = 0;

        Buffer.BlockCopy(ns, 0, result, offset, ns.Length);
        offset += ns.Length;

        Buffer.BlockCopy(pm, 0, result, offset, pm.Length);
        offset += pm.Length;

        Buffer.BlockCopy(serialBytes, 0, result, offset, 4);
        offset += 4;

        Buffer.BlockCopy(refreshBytes, 0, result, offset, 4);
        offset += 4;

        Buffer.BlockCopy(retryBytes, 0, result, offset, 4);
        offset += 4;

        Buffer.BlockCopy(expireBytes, 0, result, offset, 4);
        offset += 4;

        Buffer.BlockCopy(minimumBytes, 0, result, offset, 4);

        return result;
    }

    private static byte[] GetUIntBytes(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes); // Convert to network byte order
        }
        return bytes;
    }

    private uint GetUInt(int offset)
    {
        int p = NameServer.Length + PostMaster.Length + 2;
        var bytes = new byte[4];
        Buffer.BlockCopy(Data, p + offset, bytes, 0, 4);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToUInt32(bytes, 0);
    }

    public string NameServer
    {
        get
        {
            return DnsUtil.DnsName2Str(Data);
        }
    }

    public string PostMaster
    {
        get
        {
            return DnsUtil.DnsName2Str(GetData(NameServer.Length + 1));
        }
    }

    public uint Serial
    {
        get
        {
            return GetUInt(0);
        }
    }

    public uint Refresh
    {
        get
        {
            return GetUInt(4);
        }
    }

    public uint Retry
    {
        get
        {
            return GetUInt(8);
        }
    }

    public uint Expire
    {
        get
        {
            return GetUInt(12);
        }
    }

    public uint Minimum
    {
        get
        {
            return GetUInt(16);
        }
    }

    public override string ToString()
    {
        return $"{DnsType} {Name} TTL={Ttl} {NameServer} {PostMaster} {Serial:X8} {Refresh:X8} {Retry:X8} {Expire:X8} {Minimum:X8}";
    }
}
