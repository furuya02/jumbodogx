using System.Text;
using Jdx.Core.Settings;

namespace Jdx.Servers.Dns;

/// <summary>
/// DNS utility class for converting between strings and DNS name format
/// Based on bjd5-master/DnsServer/DnsUtil.cs
/// </summary>
public static class DnsUtil
{
    /// <summary>
    /// Convert DNS name format (with length bytes) to string
    /// Example: [3]www[5]nifty[3]com[0] -> "www.nifty.com"
    /// </summary>
    public static string DnsName2Str(byte[] data)
    {
        var tmp = new byte[data.Length - 1];
        for (int src = 0, dst = 0; src < data.Length - 1;)
        {
            var c = data[src++];
            if (c == 0)
            {
                var buf = new byte[dst];
                Buffer.BlockCopy(tmp, 0, buf, 0, dst);
                tmp = buf;
                break;
            }
            for (var i = 0; i < c; i++)
            {
                tmp[dst++] = data[src++];
            }
            tmp[dst++] = (byte)'.';
        }
        return Encoding.ASCII.GetString(tmp);
    }

    /// <summary>
    /// Convert string to DNS name format
    /// Example: "www.nifty.com" -> [3]www[5]nifty[3]com[0]
    /// </summary>
    public static byte[] Str2DnsName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return new byte[] { 0 };
        }

        // Remove trailing dot if present
        if (name[name.Length - 1] == '.')
        {
            name = name.Substring(0, name.Length - 1);
        }

        var tmp = name.Split('.');
        // Add space for length bytes and null terminator
        var data = new byte[name.Length + 2];
        var d = 0;

        foreach (var t in tmp)
        {
            data[d++] = (byte)t.Length;
            var dd = Encoding.ASCII.GetBytes(t);
            for (var e = 0; e < t.Length; e++)
            {
                data[d++] = dd[e];
            }
        }
        data[d] = 0; // Null terminator

        return data;
    }

    /// <summary>
    /// Convert short value to DnsType enum
    /// </summary>
    public static DnsType Short2DnsType(short d)
    {
        return d switch
        {
            0x0001 => DnsType.A,
            0x0002 => DnsType.Ns,
            0x0005 => DnsType.Cname,
            0x0006 => DnsType.Soa,
            0x000c => DnsType.Ptr,
            0x000f => DnsType.Mx,
            0x001c => DnsType.Aaaa,
            _ => (DnsType)0 // Unknown
        };
    }

    /// <summary>
    /// Convert DnsType enum to short value
    /// </summary>
    public static short DnsType2Short(DnsType dnsType)
    {
        return dnsType switch
        {
            DnsType.A => 0x0001,
            DnsType.Ns => 0x0002,
            DnsType.Cname => 0x0005,
            DnsType.Soa => 0x0006,
            DnsType.Ptr => 0x000c,
            DnsType.Mx => 0x000f,
            DnsType.Aaaa => 0x001c,
            _ => 0x0000
        };
    }

    /// <summary>
    /// Create OneRr instance based on DnsType
    /// </summary>
    public static OneRr? CreateRr(string name, DnsType dnsType, uint ttl, byte[] data)
    {
        return dnsType switch
        {
            DnsType.A => new RrA(name, ttl, data),
            DnsType.Aaaa => new RrAaaa(name, ttl, data),
            DnsType.Ns => new RrNs(name, ttl, data),
            DnsType.Mx => new RrMx(name, ttl, data),
            DnsType.Soa => new RrSoa(name, ttl, data),
            DnsType.Ptr => new RrPtr(name, ttl, data),
            DnsType.Cname => new RrCname(name, ttl, data),
            _ => null
        };
    }
}
