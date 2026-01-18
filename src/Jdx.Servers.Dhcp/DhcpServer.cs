using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Jdx.Core.Abstractions;
using Jdx.Core.Helpers;
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
    private readonly ConnectionLimiter _connectionLimiter;

    public DhcpServer(ILogger<DhcpServer> logger, DhcpServerSettings settings)
        : base(logger)
    {
        _settings = settings;
        _connectionLimiter = new ConnectionLimiter(settings.MaxConnections);
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

        // IPAddress.Parse()をTryParse()に置き換え（DoS対策）
        if (!IPAddress.TryParse(_settings.StartIp, out var startIp))
        {
            throw new InvalidOperationException($"Invalid StartIp: {_settings.StartIp}");
        }
        if (!IPAddress.TryParse(_settings.EndIp, out var endIp))
        {
            throw new InvalidOperationException($"Invalid EndIp: {_settings.EndIp}");
        }

        // Build MAC reservations
        List<(PhysicalAddress mac, IPAddress ip)>? macReservations = null;
        if (_settings.UseMacAcl && _settings.MacAclList.Any())
        {
            // MAC/IPアドレス解析のエラーハンドリング（DoS対策）
            macReservations = new List<(PhysicalAddress mac, IPAddress ip)>();
            foreach (var m in _settings.MacAclList)
            {
                if (string.IsNullOrWhiteSpace(m.MacAddress) || string.IsNullOrWhiteSpace(m.V4Address))
                    continue;

                try
                {
                    var mac = PhysicalAddress.Parse(m.MacAddress.Replace("-", "").Replace(":", ""));
                    if (!IPAddress.TryParse(m.V4Address, out var ip))
                    {
                        Logger.LogWarning("Invalid IP address in MAC ACL: {IPAddress}", m.V4Address);
                        continue;
                    }
                    macReservations.Add((mac, ip));
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to parse MAC/IP reservation: {Mac}/{IP}", m.MacAddress, m.V4Address);
                }
            }
        }

        // Initialize lease pool
        var leaseDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dhcp_leases.json");
        _leasePool = new LeasePool(startIp, endIp, _settings.LeaseTime, macReservations, leaseDbPath, Logger);

        // Create UDP listener
        _udpListener = await CreateUdpListenerAsync(
            _settings.Port,
            _settings.BindAddress,
            cancellationToken);

        Logger.LogInformation(
            "DHCP Server started on {Address}:{Port} (Pool: {StartIp}-{EndIp}, Lease: {LeaseTime}s)",
            _settings.BindAddress, _settings.Port, _settings.StartIp, _settings.EndIp, _settings.LeaseTime);

        // Start listening loop
        _ = Task.Run(() => RunUdpReceiveLoopAsync(
            _udpListener,
            HandleRequestAsync,
            _connectionLimiter,
            StopCts.Token), StopCts.Token);
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
        // Client handling is done in RunUdpReceiveLoopAsync -> HandleRequestAsync
        return Task.CompletedTask;
    }

    private async Task HandleRequestAsync(byte[] data, EndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        if (_leasePool == null)
            return;

        // Cast to IPEndPoint for UDP operations
        var ipEndPoint = (IPEndPoint)remoteEndPoint;

        try
        {
            // DHCPパケットサイズ検証（DoS対策）
            // RFC 2131: 最小300バイト、標準最大576バイト
            const int MinSize = 300;
            const int MaxSize = 576;

            if (data.Length < MinSize || data.Length > MaxSize)
            {
                Logger.LogWarning("Invalid DHCP packet size from {RemoteEndPoint}: {Size} bytes (min={Min}, max={Max})",
                    ipEndPoint, data.Length, MinSize, MaxSize);
                return;
            }

            var packet = new DhcpPacket();
            if (!packet.Parse(data))
            {
                Logger.LogWarning("Failed to parse DHCP packet from {RemoteEndPoint}", ipEndPoint);
                return;
            }

            Logger.LogDebug("DHCP {MessageType} from {Mac} ({RemoteEndPoint})",
                packet.MessageType, packet.ClientMac, ipEndPoint);

            // Check MAC ACL if enabled
            if (_settings.UseMacAcl && !_leasePool.IsMacAllowed(packet.ClientMac))
            {
                Logger.LogWarning("DHCP request denied by MAC ACL: {Mac}", packet.ClientMac);
                return;
            }

            switch (packet.MessageType)
            {
                case DhcpMessageType.Discover:
                    await HandleDiscoverAsync(packet, ipEndPoint, cancellationToken);
                    break;

                case DhcpMessageType.Request:
                    await HandleRequestAsync(packet, ipEndPoint, cancellationToken);
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
            NetworkExceptionHandler.LogNetworkException(ex, Logger, "DHCP request handling");
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
            var serverIpStr = _settings.BindAddress == "0.0.0.0" ? "127.0.0.1" : _settings.BindAddress;
            if (IPAddress.TryParse(serverIpStr, out var serverIp) && !request.ServerIdentifier.Equals(serverIp))
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
            // IPAddress.Parse()をTryParse()に置き換え（DoS対策）
            IPAddress? subnetMask = null;
            if (!string.IsNullOrWhiteSpace(_settings.MaskIp) && !IPAddress.TryParse(_settings.MaskIp, out subnetMask))
            {
                Logger.LogWarning("Invalid subnet mask: {MaskIp}", _settings.MaskIp);
            }

            IPAddress? router = null;
            if (!string.IsNullOrWhiteSpace(_settings.GwIp) && !IPAddress.TryParse(_settings.GwIp, out router))
            {
                Logger.LogWarning("Invalid gateway IP: {GwIp}", _settings.GwIp);
            }

            IPAddress? dns1 = null;
            if (!string.IsNullOrWhiteSpace(_settings.DnsIp0) && !IPAddress.TryParse(_settings.DnsIp0, out dns1))
            {
                Logger.LogWarning("Invalid DNS IP 0: {DnsIp0}", _settings.DnsIp0);
            }

            IPAddress? dns2 = null;
            if (!string.IsNullOrWhiteSpace(_settings.DnsIp1) && !IPAddress.TryParse(_settings.DnsIp1, out dns2))
            {
                Logger.LogWarning("Invalid DNS IP 1: {DnsIp1}", _settings.DnsIp1);
            }

            var wpadUrl = _settings.UseWpad ? _settings.WpadUrl : null;

            var serverIpStr = _settings.BindAddress == "0.0.0.0" ? "127.0.0.1" : _settings.BindAddress;
            if (!IPAddress.TryParse(serverIpStr, out var serverIp))
            {
                Logger.LogError("Invalid server IP address: {ServerIp}", serverIpStr);
                return;
            }

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
            var serverIpStr = _settings.BindAddress == "0.0.0.0" ? "127.0.0.1" : _settings.BindAddress;
            if (!IPAddress.TryParse(serverIpStr, out var serverIp))
            {
                Logger.LogError("Invalid server IP address: {ServerIp}", serverIpStr);
                return;
            }
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
            _connectionLimiter?.Dispose();
        }
        base.Dispose(disposing);
    }
}
