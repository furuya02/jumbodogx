using System.Net;
using Jdx.Core.Settings;

namespace Jdx.Servers.Dns;

/// <summary>
/// AAAA Record (IPv6 address)
/// Based on bjd5-master/DnsServer/RrAaaa.cs
/// </summary>
public class RrAaaa : OneRr
{
    public RrAaaa(string name, uint ttl, IPAddress ip)
        : base(name, DnsType.Aaaa, ttl, ip.GetAddressBytes())
    {
    }

    public RrAaaa(string name, uint ttl, byte[] data)
        : base(name, DnsType.Aaaa, ttl, data)
    {
    }

    public IPAddress Ip
    {
        get
        {
            return new IPAddress(Data);
        }
    }

    public override string ToString()
    {
        return $"{DnsType} {Name} TTL={Ttl} {Ip}";
    }
}
