using Jdx.Core.Settings;

namespace Jdx.Servers.Dns;

/// <summary>
/// NS Record (Name Server)
/// Based on bjd5-master/DnsServer/RrNs.cs
/// </summary>
public class RrNs : OneRr
{
    public RrNs(string name, uint ttl, string nsName)
        : base(name, DnsType.Ns, ttl, DnsUtil.Str2DnsName(nsName))
    {
    }

    public RrNs(string name, uint ttl, byte[] data)
        : base(name, DnsType.Ns, ttl, data)
    {
    }

    public string NsName
    {
        get
        {
            return DnsUtil.DnsName2Str(Data);
        }
    }

    public override string ToString()
    {
        return $"{DnsType} {Name} TTL={Ttl} {NsName}";
    }
}
