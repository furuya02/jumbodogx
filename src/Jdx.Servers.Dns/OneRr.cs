using Jdx.Core.Settings;

namespace Jdx.Servers.Dns;

/// <summary>
/// Base class for DNS resource records
/// Based on bjd5-master/DnsServer/OneRr.cs
/// Internal data is stored in network byte order
/// Supports: A, AAAA, NS, CNAME, MX, PTR, SOA
/// </summary>
public abstract class OneRr
{
    public DnsType DnsType { get; private set; }
    private readonly long _createTime; // Creation time in seconds
    public uint Ttl { get; private set; } // Time to live
    public string Name { get; private set; }
    public byte[] Data { get; private set; }

    protected OneRr(string name, DnsType dnsType, uint ttl, byte[] d)
    {
        _createTime = DateTime.Now.Ticks / 10000000; // Convert to seconds
        Name = name.ToLower(); // Store in lowercase for case-insensitive comparison
        DnsType = dnsType;
        Ttl = ttl;
        Data = new byte[d.Length];
        Buffer.BlockCopy(d, 0, Data, 0, d.Length);
    }

    /// <summary>
    /// Create a clone with modified TTL value
    /// </summary>
    public OneRr Clone(uint t)
    {
        return DnsType switch
        {
            DnsType.A => new RrA(Name, t, Data),
            DnsType.Aaaa => new RrAaaa(Name, t, Data),
            DnsType.Ns => new RrNs(Name, t, Data),
            DnsType.Mx => new RrMx(Name, t, Data),
            DnsType.Cname => new RrCname(Name, t, Data),
            DnsType.Ptr => new RrPtr(Name, t, Data),
            DnsType.Soa => new RrSoa(Name, t, Data),
            _ => throw new InvalidOperationException($"OneRr.Clone() not implemented for DnsType={DnsType}")
        };
    }

    /// <summary>
    /// Get data slice from offset with specified length
    /// </summary>
    public byte[] GetData(int offset, int len)
    {
        var dst = new byte[len];
        Buffer.BlockCopy(Data, offset, dst, 0, len);
        return dst;
    }

    /// <summary>
    /// Get data slice from offset to end
    /// </summary>
    public byte[] GetData(int offset)
    {
        var len = Data.Length - offset;
        return GetData(offset, len);
    }

    /// <summary>
    /// Equals implementation for testing
    /// Compares all internal values including TTL
    /// </summary>
    public override bool Equals(object? o)
    {
        if (o == null)
        {
            return false;
        }

        if (o is not OneRr r)
        {
            return false;
        }

        if (Name != r.Name)
        {
            return false;
        }

        if (DnsType != r.DnsType)
        {
            return false;
        }

        if (Ttl != r.Ttl)
        {
            return false;
        }

        var tmp = r.Data;
        if (Data.Length != tmp.Length)
        {
            return false;
        }

        for (int i = 0; i < Data.Length; i++)
        {
            if (Data[i] != tmp[i])
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, DnsType, Ttl);
    }

    /// <summary>
    /// Check if data is still effective based on TTL
    /// </summary>
    /// <param name="now">Current time in seconds since epoch</param>
    public bool IsEffective(long now)
    {
        if (Ttl == 0)
        {
            return true; // Permanent record
        }

        if (_createTime + Ttl >= now)
        {
            return true;
        }

        return false;
    }
}
