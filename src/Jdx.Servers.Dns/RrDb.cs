using System.Net;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Dns;

/// <summary>
/// DNS Resource Record Database
/// Based on bjd5-master/DnsServer/RrDb.cs (simplified version)
/// </summary>
public class RrDb
{
    private readonly List<OneRr> _ar = new();
    private string _domainName = "ERROR";
    private readonly uint _expire;
    private readonly ILogger? _logger;

    public bool Authority { get; private set; }

    public string GetDomainName()
    {
        return _domainName;
    }

    /// <summary>
    /// Default constructor for testing
    /// </summary>
    public RrDb()
    {
        SetDomainName("example.com.");
        _expire = 2400;
        Authority = true;
    }

    /// <summary>
    /// Constructor for resource-based initialization
    /// </summary>
    public RrDb(ILogger logger, List<DnsResourceEntry> resources, string domainName, bool authority,
                DnsServerSettings settings)
    {
        _logger = logger;
        SetDomainName(domainName);
        Authority = authority;
        _expire = (uint)settings.SoaExpire;

        // Load resources
        foreach (var res in resources)
        {
            try
            {
                AddResource(res);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to add resource for domain={Domain}", _domainName);
            }
        }

        // Add SOA record if NS records exist
        InitSoa(_domainName, settings.SoaMail, (uint)settings.SoaSerial,
                (uint)settings.SoaRefresh, (uint)settings.SoaRetry,
                (uint)settings.SoaExpire, (uint)settings.SoaMinimum);
    }

    /// <summary>
    /// Constructor for named.ca initialization (root cache)
    /// </summary>
    public RrDb(string namedCaPath, uint expire)
    {
        SetDomainName(".");
        _expire = expire;

        // Load named.ca if exists
        if (!string.IsNullOrEmpty(namedCaPath) && File.Exists(namedCaPath))
        {
            var lines = File.ReadAllLines(namedCaPath);
            var tmpName = "";
            foreach (var str in lines)
            {
                tmpName = AddNamedCaLine(tmpName, str);
            }
        }

        // Add localhost records
        InitLocalHost();
    }

    private void SetDomainName(string dname)
    {
        _domainName = dname.ToLower();
        if (!_domainName.EndsWith("."))
        {
            _domainName += ".";
        }
    }

    private void AddResource(DnsResourceEntry res)
    {
        OneRr? rr = null;
        uint ttl = _expire;

        switch (res.Type)
        {
            case DnsType.A:
                if (IPAddress.TryParse(res.Address, out var ipv4))
                {
                    rr = new RrA(res.Name, ttl, ipv4);
                }
                break;

            case DnsType.Aaaa:
                if (IPAddress.TryParse(res.Address, out var ipv6))
                {
                    rr = new RrAaaa(res.Name, ttl, ipv6);
                }
                break;

            case DnsType.Ns:
                rr = new RrNs(res.Name, ttl, res.Address);
                break;

            case DnsType.Mx:
                rr = new RrMx(res.Name, ttl, (ushort)res.Priority, res.Address);
                break;

            case DnsType.Cname:
                rr = new RrCname(res.Name, ttl, res.Alias);
                break;

            case DnsType.Ptr:
                rr = new RrPtr(res.Name, ttl, res.Address);
                break;
        }

        if (rr != null)
        {
            Add(rr);
        }
    }

    private string AddNamedCaLine(string tmpName, string str)
    {
        const int ttl = 0; // Root cache has no expiration

        // Remove comments
        var pos = str.IndexOf(';');
        if (pos >= 0)
        {
            str = str.Substring(0, pos);
        }

        str = str.Trim();
        if (string.IsNullOrEmpty(str))
        {
            return tmpName;
        }

        var tokens = str.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 4)
        {
            return tmpName;
        }

        var name = tokens[0];
        if (name == "")
        {
            name = tmpName;
        }
        else
        {
            tmpName = name;
        }

        // Parse type
        var typeStr = tokens[tokens.Length - 2];
        var dnsType = typeStr.ToUpper() switch
        {
            "A" => DnsType.A,
            "NS" => DnsType.Ns,
            "AAAA" => DnsType.Aaaa,
            _ => (DnsType)0
        };

        if (dnsType == 0)
        {
            return tmpName;
        }

        var data = tokens[tokens.Length - 1];

        try
        {
            OneRr? rr = dnsType switch
            {
                DnsType.A => IPAddress.TryParse(data, out var ip) ? new RrA(name, ttl, ip) : null,
                DnsType.Ns => new RrNs(name, ttl, data),
                DnsType.Aaaa => IPAddress.TryParse(data, out var ip6) ? new RrAaaa(name, ttl, ip6) : null,
                _ => null
            };

            if (rr != null)
            {
                Add(rr);
            }
        }
        catch
        {
            // Ignore parse errors
        }

        return tmpName;
    }

    private void InitLocalHost()
    {
        Add(new RrA("localhost.", 0, IPAddress.Loopback));
        Add(new RrAaaa("localhost.", 0, IPAddress.IPv6Loopback));
    }

    private bool InitSoa(string domainName, string mail, uint serial, uint refresh,
                         uint retry, uint expire, uint minimum)
    {
        // Find NS records for this domain
        var nsList = GetList(domainName, DnsType.Ns);
        if (nsList.Count == 0)
        {
            return false;
        }

        var ns = (RrNs)nsList[0];
        var soa = new RrSoa(domainName, 0, ns.NsName, mail, serial, refresh, retry, expire, minimum);
        Add(soa);
        return true;
    }

    /// <summary>
    /// Check if a record exists
    /// </summary>
    public bool Find(string name, DnsType dnsType)
    {
        name = name.ToLower();
        if (!name.EndsWith("."))
        {
            name += ".";
        }

        lock (_ar)
        {
            return _ar.Any(rr => rr.Name == name && rr.DnsType == dnsType);
        }
    }

    /// <summary>
    /// Get list of records matching name and type
    /// </summary>
    public List<OneRr> GetList(string name, DnsType dnsType)
    {
        name = name.ToLower();
        if (!name.EndsWith("."))
        {
            name += ".";
        }

        var now = DateTime.Now.Ticks / 10000000; // Seconds
        var result = new List<OneRr>();

        lock (_ar)
        {
            foreach (var rr in _ar)
            {
                if (rr.Name == name && rr.DnsType == dnsType && rr.IsEffective(now))
                {
                    // Clone with expire TTL if original TTL is 0
                    var clonedRr = rr.Ttl == 0 ? rr.Clone(_expire) : rr;
                    result.Add(clonedRr);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Add a resource record (prevents duplicates)
    /// </summary>
    public bool Add(OneRr oneRr)
    {
        lock (_ar)
        {
            // Check for duplicates
            foreach (var rr in _ar)
            {
                if (rr.Equals(oneRr))
                {
                    return false; // Already exists
                }
            }

            _ar.Add(oneRr);
            return true;
        }
    }

    public int Count
    {
        get
        {
            lock (_ar)
            {
                return _ar.Count;
            }
        }
    }
}
