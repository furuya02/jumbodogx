using System.Net;
using Jdx.Core.Settings;

namespace Jdx.Servers.Dns;

/// <summary>
/// A Record (IPv4 address)
/// Based on bjd5-master/DnsServer/RrA.cs
/// </summary>
public class RrA : OneRr
{
    public RrA(string name, uint ttl, IPAddress ip)
        : base(name, DnsType.A, ttl, ip.GetAddressBytes())
    {
    }

    public RrA(string name, uint ttl, byte[] data)
        : base(name, DnsType.A, ttl, data)
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
