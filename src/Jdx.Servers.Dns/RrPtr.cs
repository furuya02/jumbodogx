using Jdx.Core.Settings;

namespace Jdx.Servers.Dns;

/// <summary>
/// PTR Record (Pointer for reverse DNS lookup)
/// Based on bjd5-master/DnsServer/RrPtr.cs
/// </summary>
public class RrPtr : OneRr
{
    public RrPtr(string name, uint ttl, string ptr)
        : base(name, DnsType.Ptr, ttl, DnsUtil.Str2DnsName(ptr))
    {
    }

    public RrPtr(string name, uint ttl, byte[] data)
        : base(name, DnsType.Ptr, ttl, data)
    {
    }

    public string Ptr
    {
        get
        {
            return DnsUtil.DnsName2Str(Data);
        }
    }

    public override string ToString()
    {
        return $"{DnsType} {Name} TTL={Ttl} {Ptr}";
    }
}
