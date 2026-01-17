using Jdx.Core.Settings;

namespace Jdx.Servers.Dns;

/// <summary>
/// MX Record (Mail Exchange)
/// Based on bjd5-master/DnsServer/RrMx.cs
/// </summary>
public class RrMx : OneRr
{
    public RrMx(string name, uint ttl, ushort preference, string mailExchangerHost)
        : base(name, DnsType.Mx, ttl, CreateData(preference, mailExchangerHost))
    {
    }

    public RrMx(string name, uint ttl, byte[] data)
        : base(name, DnsType.Mx, ttl, data)
    {
    }

    private static byte[] CreateData(ushort preference, string mailExchangerHost)
    {
        var prefBytes = BitConverter.GetBytes(preference);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(prefBytes); // Convert to network byte order
        }

        var hostBytes = DnsUtil.Str2DnsName(mailExchangerHost);
        var result = new byte[prefBytes.Length + hostBytes.Length];
        Buffer.BlockCopy(prefBytes, 0, result, 0, prefBytes.Length);
        Buffer.BlockCopy(hostBytes, 0, result, prefBytes.Length, hostBytes.Length);
        return result;
    }

    public ushort Preference
    {
        get
        {
            var bytes = GetData(0, 2);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt16(bytes, 0);
        }
    }

    public string MailExchangeHost
    {
        get
        {
            return DnsUtil.DnsName2Str(GetData(2));
        }
    }

    public override string ToString()
    {
        return $"{DnsType} {Name} TTL={Ttl} {Preference} {MailExchangeHost}";
    }
}
