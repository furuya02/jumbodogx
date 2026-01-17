using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Jdx.Core.Abstractions;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Dhcp;

/// <summary>
/// DHCP Server Implementation (RFC 2131)
/// Dynamic Host Configuration Protocol
/// </summary>
public class DhcpServer : ServerBase
{
    private readonly DhcpServerSettings _settings;
    private ServerUdpListener? _udpListener;
    private LeasePool? _leasePool;
    private readonly SemaphoreSlim _connectionSemaphore;

    public DhcpServer(ILogger<DhcpServer> logger, DhcpServerSettings settings)
        : base(logger)
    {
        _settings = settings;
        _connectionSemaphore = new SemaphoreSlim(settings.MaxConnections, settings.MaxConnections);
    }

    public override string Name => "DhcpServer";
    public override ServerType Type => ServerType.Dhcp;
    public override int Port => _settings.Port;

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        // Validate settings
        if (string.IsNullOrWhiteSpace(_settings.StartIp) || string.IsNullOrWhiteSpace(_settings.EndIp))
        {
            throw new InvalidOperationException("StartIp and EndIp must be configured");
        }

        var startIp = IPAddress.Parse(_settings.StartIp);
        var endIp = IPAddress.Parse(_settings.EndIp);

        // Build MAC reservations
        List<(PhysicalAddress mac, IPAddress ip)>? macReservations = null;
        if (_settings.UseMacAcl && _settings.MacAclList.Any())
        {
            macReservations = _settings.MacAclList
                .Where(m => !string.IsNullOrWhiteSpace(m.MacAddress) && !string.IsNullOrWhiteSpace(m.V4Address))
                .Select(m =>
                {
                    var mac = PhysicalAddress.Parse(m.MacAddress.Replace("-", "").Replace(":", ""));
                    var ip = IPAddress.Parse(m.V4Address);
                    return (mac, ip);
                })
                .ToList();
        }

        // Initialize lease pool
        var leaseDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dhcp_leases.json");
        _leasePool = new LeasePool(startIp, endIp, _settings.LeaseTime, macReservations, leaseDbPath, Logger);

        // Create UDP listener
        var bindAddress = string.IsNullOrWhiteSpace(_settings.BindAddress) || _settings.BindAddress == "0.0.0.0"
            ? IPAddress.Any
            : IPAddress.Parse(_settings.BindAddress);

        _udpListener = new ServerUdpListener(_settings.Port, bindAddress, Logger);
        await _udpListener.StartAsync(cancellationToken);

        Logger.LogInformation(
            "DHCP Server started on {Address}:{Port} (Pool: {StartIp}-{EndIp}, Lease: {LeaseTime}s)",
            _settings.BindAddress, _settings.Port, _settings.StartIp, _settings.EndIp, _settings.LeaseTime);

        // Start listening loop
        _ = Task.Run(() => ListenLoopAsync(StopCts.Token), StopCts.Token);
    }

    protected override async Task StopListeningAsync(CancellationToken cancellationToken)
    {
        if (_udpListener != null)
        {
            await _udpListener.StopAsync(cancellationToken);
            _udpListener = null;
        }

        Logger.LogInformation("DHCP Server stopped");
    }

    protected override Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        // DHCP uses UDP, not TCP, so this method is not used
        // Client handling is done in ListenLoopAsync -> HandleRequestAsync
        return Task.CompletedTask;
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        if (_udpListener == null || _leasePool == null)
            return;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var (data, remoteEndPoint) = await _udpListener.ReceiveAsync(cancellationToken);

                if (data.Length == 0)
                    continue;

                // Handle request in background
                _ = Task.Run(async () =>
                {
                    await _connectionSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        await HandleRequestAsync(data, (IPEndPoint)remoteEndPoint, cancellationToken);
                    }
                    finally
                    {
                        _connectionSemaphore.Release();
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in DHCP listen loop");
            }
        }
    }

    private async Task HandleRequestAsync(byte[] data, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        if (_leasePool == null)
            return;

        try
        {
            var packet = new DhcpPacket();
            if (!packet.Parse(data))
            {
                Logger.LogWarning("Failed to parse DHCP packet from {RemoteEndPoint}", remoteEndPoint);
                return;
            }

            Logger.LogDebug("DHCP {MessageType} from {Mac} ({RemoteEndPoint})",
                packet.MessageType, packet.ClientMac, remoteEndPoint);

            // Check MAC ACL if enabled
            if (_settings.UseMacAcl && !_leasePool.IsMacAllowed(packet.ClientMac))
            {
                Logger.LogWarning("DHCP request denied by MAC ACL: {Mac}", packet.ClientMac);
                return;
            }

            switch (packet.MessageType)
            {
                case DhcpMessageType.Discover:
                    await HandleDiscoverAsync(packet, remoteEndPoint, cancellationToken);
                    break;

                case DhcpMessageType.Request:
                    await HandleRequestAsync(packet, remoteEndPoint, cancellationToken);
                    break;

                case DhcpMessageType.Release:
                    HandleRelease(packet);
                    break;

                case DhcpMessageType.Inform:
                    // DHCPINFORM - client already has IP, just wants configuration
                    Logger.LogInformation("DHCP INFORM from {Mac} - not implemented", packet.ClientMac);
                    break;

                default:
                    Logger.LogWarning("Unsupported DHCP message type: {MessageType}", packet.MessageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling DHCP request from {RemoteEndPoint}", remoteEndPoint);
        }
    }

    private async Task HandleDiscoverAsync(DhcpPacket request, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        if (_leasePool == null)
            return;

        var assignedIp = _leasePool.HandleDiscover(request.RequestedIp, request.TransactionId, request.ClientMac);

        if (assignedIp == null)
        {
            Logger.LogWarning("No available IP for DISCOVER from {Mac}", request.ClientMac);
            return;
        }

        Logger.LogInformation("DHCP OFFER: {Ip} to {Mac}", assignedIp, request.ClientMac);

        await SendResponseAsync(request, DhcpMessageType.Offer, assignedIp, remoteEndPoint, cancellationToken);
    }

    private async Task HandleRequestAsync(DhcpPacket request, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        if (_leasePool == null || request.RequestedIp == null)
            return;

        // Check if request is for this server
        if (request.ServerIdentifier != null)
        {
            var serverIp = IPAddress.Parse(_settings.BindAddress == "0.0.0.0" ? "127.0.0.1" : _settings.BindAddress);
            if (!request.ServerIdentifier.Equals(serverIp))
            {
                // Request is for another server, release our reservation
                _leasePool.HandleRelease(request.ClientMac);
                Logger.LogInformation("DHCP REQUEST for another server from {Mac}, releasing reservation", request.ClientMac);
                return;
            }
        }

        var assignedIp = _leasePool.HandleRequest(request.RequestedIp, request.TransactionId, request.ClientMac);

        if (assignedIp == null)
        {
            Logger.LogWarning("DHCP NAK: Request denied for {Ip} from {Mac}", request.RequestedIp, request.ClientMac);
            await SendNakAsync(request, remoteEndPoint, cancellationToken);
            return;
        }

        Logger.LogInformation("DHCP ACK: {Ip} to {Mac}", assignedIp, request.ClientMac);

        await SendResponseAsync(request, DhcpMessageType.Ack, assignedIp, remoteEndPoint, cancellationToken);
    }

    private void HandleRelease(DhcpPacket request)
    {
        if (_leasePool == null)
            return;

        _leasePool.HandleRelease(request.ClientMac);
        Logger.LogInformation("DHCP RELEASE from {Mac}", request.ClientMac);
    }

    private async Task SendResponseAsync(DhcpPacket request, DhcpMessageType messageType, IPAddress assignedIp,
        IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        try
        {
            IPAddress? subnetMask = string.IsNullOrWhiteSpace(_settings.MaskIp) ? null : IPAddress.Parse(_settings.MaskIp);
            IPAddress? router = string.IsNullOrWhiteSpace(_settings.GwIp) ? null : IPAddress.Parse(_settings.GwIp);
            IPAddress? dns1 = string.IsNullOrWhiteSpace(_settings.DnsIp0) ? null : IPAddress.Parse(_settings.DnsIp0);
            IPAddress? dns2 = string.IsNullOrWhiteSpace(_settings.DnsIp1) ? null : IPAddress.Parse(_settings.DnsIp1);
            var wpadUrl = _settings.UseWpad ? _settings.WpadUrl : null;

            var serverIp = IPAddress.Parse(_settings.BindAddress == "0.0.0.0" ? "127.0.0.1" : _settings.BindAddress);

            var responseData = request.Build(messageType, assignedIp, serverIp, _settings.LeaseTime,
                subnetMask, router, dns1, dns2, wpadUrl);

            using var udpClient = new UdpClient();

            // DHCP responses are sent to broadcast or client IP
            var targetEndPoint = new IPEndPoint(IPAddress.Broadcast, 68);
            await udpClient.SendAsync(responseData, targetEndPoint, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending DHCP response");
        }
    }

    private async Task SendNakAsync(DhcpPacket request, IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        try
        {
            var serverIp = IPAddress.Parse(_settings.BindAddress == "0.0.0.0" ? "127.0.0.1" : _settings.BindAddress);
            var responseData = request.Build(DhcpMessageType.Nak, IPAddress.Any, serverIp, 0);

            using var udpClient = new UdpClient();
            var targetEndPoint = new IPEndPoint(IPAddress.Broadcast, 68);
            await udpClient.SendAsync(responseData, targetEndPoint, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending DHCP NAK");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _udpListener?.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
            _connectionSemaphore?.Dispose();
        }
        base.Dispose(disposing);
    }
}
