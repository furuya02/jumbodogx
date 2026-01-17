using Jdx.Core.Settings;

namespace Jdx.Servers.Dns;

/// <summary>
/// CNAME Record (Canonical Name / Alias)
/// Based on bjd5-master/DnsServer/RrCname.cs
/// </summary>
public class RrCname : OneRr
{
    public RrCname(string name, uint ttl, string cname)
        : base(name, DnsType.Cname, ttl, DnsUtil.Str2DnsName(cname))
    {
    }

    public RrCname(string name, uint ttl, byte[] data)
        : base(name, DnsType.Cname, ttl, data)
    {
    }

    public string CName
    {
        get
        {
            return DnsUtil.DnsName2Str(Data);
        }
    }

    public override string ToString()
    {
        return $"{DnsType} {Name} TTL={Ttl} {CName}";
    }
}
