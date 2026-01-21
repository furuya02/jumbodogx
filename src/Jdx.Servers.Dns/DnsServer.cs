using System.Net;
using System.Net.Sockets;
using Jdx.Core.Abstractions;
using Jdx.Core.Constants;
using Jdx.Core.Helpers;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Dns;

/// <summary>
/// DNS Server with full resource record support
/// Based on bjd5-master/DnsServer/Server.cs (simplified)
/// </summary>
public class DnsServer : ServerBase
{
    private readonly int _port;
    private readonly DnsServerSettings _settings;
    private readonly Dictionary<string, RrDb> _cacheList; // Domain caches
    private readonly List<string> _sortedDomains; // Pre-sorted domain list by length (descending)
    private readonly RrDb? _rootCache; // Root nameserver cache
    private ServerUdpListener? _listener;

    public DnsServer(ILogger<DnsServer> logger, DnsServerSettings settings) : base(logger)
    {
        _settings = settings;
        _port = settings.Port;
        _cacheList = new Dictionary<string, RrDb>(StringComparer.OrdinalIgnoreCase);

        // Initialize domain caches
        foreach (var domain in settings.DomainList)
        {
            var resources = settings.ResourceList
                .Where(r => r.Name.EndsWith(domain.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var cache = new RrDb(logger, resources, domain.Name, domain.IsAuthority, settings);
            _cacheList[domain.Name] = cache;
        }

        // Pre-sort domains by length (descending) for efficient lookup
        _sortedDomains = _cacheList.Keys.OrderByDescending(k => k.Length).ToList();

        // Initialize root cache if named.ca exists
        if (File.Exists(settings.RootCache))
        {
            try
            {
                _rootCache = new RrDb(settings.RootCache, (uint)settings.SoaExpire);
                Logger.LogInformation("Loaded root cache from {RootCache}", settings.RootCache);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to load root cache from {RootCache}", settings.RootCache);
            }
        }

        Logger.LogInformation("DNS Server initialized with {DomainCount} domains, {ResourceCount} resources",
            settings.DomainList.Count, settings.ResourceList.Count);
    }

    public override string Name => "DnsServer";
    public override ServerType Type => ServerType.Dns;
    public override int Port => _port;

    public void AddRecord(string domainName, string ipAddress)
    {
        // Add record to appropriate cache
        var domain = _cacheList.Keys.FirstOrDefault(d => domainName.EndsWith(d, StringComparison.OrdinalIgnoreCase));
        if (domain != null)
        {
            // IPAddress.Parse()をTryParse()に置き換え（DoS対策）
            if (!IPAddress.TryParse(ipAddress, out var ip))
            {
                Logger.LogWarning("Invalid IP address in AddRecord: {IPAddress}", ipAddress);
                return;
            }
            var rr = new RrA(domainName, (uint)_settings.SoaExpire, ip);
            _cacheList[domain].Add(rr);
            Logger.LogInformation("Added A record: {Name} -> {IP}", domainName, ipAddress);
        }
    }

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        _listener = await CreateUdpListenerAsync(_port, null, cancellationToken);

        Logger.LogInformation("DNS Server listening on port {Port}", _port);

        _ = Task.Run(() => RunUdpReceiveLoopAsync(
            _listener,
            async (data, remoteEndPoint, ct) =>
            {
                Statistics.TotalConnections++;
                Statistics.TotalBytesReceived += data.Length;

                // DNSメッセージサイズ検証（DoS対策）
                // RFC 1035: UDPの場合は512バイトまで（NetworkConstants.Dns.MaxUdpPacketSize）
                // RFC 6891: EDNS0拡張で最大4096バイト
                const int DnsHeaderSize = 12;
                const int EdnsMaxSize = 4096; // EDNS0 extension (RFC 6891)

                if (data.Length < DnsHeaderSize || data.Length > EdnsMaxSize)
                {
                    Logger.LogWarning("Invalid DNS message size from {RemoteEndPoint}: {Size} bytes (min={Min}, max={Max})",
                        remoteEndPoint, data.Length, DnsHeaderSize, EdnsMaxSize);
                    Statistics.TotalErrors++;
                    return;
                }

                await HandleDnsQueryAsync(data, remoteEndPoint, ct);
            },
            null,
            StopCts.Token), StopCts.Token);
    }

    protected override async Task StopListeningAsync(CancellationToken cancellationToken)
    {
        await StopUdpListenerAsync();
        Logger.LogInformation("DNS Server stopped");
    }

    protected override Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        return Task.CompletedTask; // UDP server doesn't use this
    }

    private async Task HandleDnsQueryAsync(byte[] data, EndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        var remoteAddress = remoteEndPoint.ToString() ?? "unknown";

        try
        {
            var query = DnsMessage.ParseQuery(data);
            Statistics.TotalRequests++;

            var queryName = query.QueryName.ToLower();
            if (!queryName.EndsWith("."))
            {
                queryName += ".";
            }

            var queryType = DnsUtil.Short2DnsType((short)query.QueryType);

            Logger.LogDebug("DNS query for {QueryName} (type={QueryType}/{TypeName}) from {RemoteAddress}",
                query.QueryName, query.QueryType, queryType, remoteAddress);

            // Find matching cache (using pre-sorted domain list)
            RrDb? cache = null;
            foreach (var domain in _sortedDomains)
            {
                if (queryName.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
                {
                    cache = _cacheList[domain];
                    break;
                }
            }

            // Try to find the record
            List<OneRr>? records = null;
            if (cache != null)
            {
                records = cache.GetList(queryName, queryType);
            }

            // If not found and recursion enabled, try root cache
            if ((records == null || records.Count == 0) && _settings.UseRecursion && _rootCache != null)
            {
                records = _rootCache.GetList(queryName, queryType);
            }

            // Generate response
            if (records != null && records.Count > 0)
            {
                // Found - return the first matching record
                var firstRecord = records[0];

                // Create DNS response with the record
                var response = query.CreateResponse(firstRecord);
                if (_listener != null)
                {
                    await _listener.SendAsync(response, remoteEndPoint, cancellationToken);
                    Statistics.TotalBytesSent += response.Length;

                    var responseData = GetRecordDataString(firstRecord);
                    Logger.LogDebug("DNS response sent: {QueryName} (type={QueryType}) -> {Data}",
                        query.QueryName, queryType, responseData);
                }
            }
            else
            {
                // Not found - return NXDOMAIN
                Logger.LogDebug("DNS record not found for {QueryName} (type={QueryType}), returning NXDOMAIN",
                    query.QueryName, queryType);

                var response = query.CreateNXDomainResponse();
                if (_listener != null)
                {
                    await _listener.SendAsync(response, remoteEndPoint, cancellationToken);
                    Statistics.TotalBytesSent += response.Length;
                }
            }
        }
        catch (Exception ex)
        {
            NetworkExceptionHandler.LogNetworkException(ex, Logger, "DNS query handling");
            Statistics.TotalErrors++;
        }
    }

    private string GetRecordDataString(OneRr record)
    {
        return record switch
        {
            RrA rrA => rrA.Ip.ToString(),
            RrAaaa rrAaaa => rrAaaa.Ip.ToString(),
            RrNs rrNs => rrNs.NsName,
            RrCname rrCname => rrCname.CName,
            RrMx rrMx => $"{rrMx.Preference} {rrMx.MailExchangeHost}",
            RrPtr rrPtr => rrPtr.Ptr,
            RrSoa rrSoa => $"{rrSoa.NameServer} {rrSoa.PostMaster}",
            _ => record.ToString() ?? ""
        };
    }
}
