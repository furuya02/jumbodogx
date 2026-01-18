using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Dhcp;

/// <summary>
/// DHCP Lease Pool Manager
/// Manages IP address allocation and lease tracking
/// </summary>
public class LeasePool
{
    private readonly List<LeaseEntry> _leases = new();
    private readonly int _leaseTimeSeconds;
    private readonly string? _leaseDbPath;
    private readonly ILogger? _logger;
    private readonly object _lockObj = new();

    public LeasePool(IPAddress startIp, IPAddress endIp, int leaseTimeSeconds,
                    List<(PhysicalAddress mac, IPAddress ip)>? macReservations = null,
                    string? leaseDbPath = null,
                    ILogger? logger = null)
    {
        _leaseTimeSeconds = leaseTimeSeconds;
        _leaseDbPath = leaseDbPath;
        _logger = logger;

        // Build IP pool
        var start = BitConverter.ToUInt32(startIp.GetAddressBytes().Reverse().ToArray(), 0);
        var end = BitConverter.ToUInt32(endIp.GetAddressBytes().Reverse().ToArray(), 0);

        for (uint addr = start; addr <= end && _leases.Count < 2048; addr++)
        {
            var ipBytes = BitConverter.GetBytes(addr).Reverse().ToArray();
            var ip = new IPAddress(ipBytes);
            _leases.Add(new LeaseEntry(ip));
        }

        // Add MAC reservations
        if (macReservations != null)
        {
            foreach (var (mac, ip) in macReservations)
            {
                // Check if IP is in pool
                var existing = _leases.FirstOrDefault(l => l.IpAddress.Equals(ip));
                if (existing != null)
                {
                    // Replace with reserved entry
                    _leases.Remove(existing);
                }

                _leases.Add(new LeaseEntry(ip, mac));
            }
        }

        // Load existing leases from database
        if (!string.IsNullOrEmpty(_leaseDbPath) && File.Exists(_leaseDbPath))
        {
            LoadLeases();
        }

        _logger?.LogInformation("DHCP Lease Pool initialized: {Count} addresses", _leases.Count);
    }

    /// <summary>
    /// Check if MAC address is allowed (for MAC ACL)
    /// </summary>
    public bool IsMacAllowed(PhysicalAddress macAddress)
    {
        lock (_lockObj)
        {
            // Check if any reserved entry matches this MAC
            return _leases.Any(l => l.IsMacReserved && l.MatchesMac(macAddress));
        }
    }

    /// <summary>
    /// Handle DISCOVER message - find available IP
    /// </summary>
    public IPAddress? HandleDiscover(IPAddress? requestedIp, uint transactionId, PhysicalAddress macAddress)
    {
        lock (_lockObj)
        {
            RefreshAll();

            // 1. Check if already reserved with same transaction ID
            var existing = _leases.FirstOrDefault(l =>
                l.Status == LeaseStatus.Reserved && l.TransactionId == transactionId);
            if (existing != null)
            {
                return existing.IpAddress;
            }

            // 2. Check MAC reserved entry (not 255.255.255.255)
            var macReserved = _leases.FirstOrDefault(l =>
                l.IsMacReserved && l.MatchesMac(macAddress) && l.Status == LeaseStatus.Unused);
            if (macReserved != null)
            {
                macReserved.SetReserved(transactionId, macAddress);
                SaveLeases();
                return macReserved.IpAddress;
            }

            // 3. Release existing lease with same MAC
            foreach (var entry in _leases.Where(l => l.MatchesMac(macAddress)))
            {
                entry.SetUnused();
            }

            // 4. Try requested IP if available
            if (requestedIp != null)
            {
                var requestedEntry = _leases.FirstOrDefault(l =>
                    l.IpAddress.Equals(requestedIp) && l.Status == LeaseStatus.Unused);
                if (requestedEntry != null)
                {
                    requestedEntry.SetReserved(transactionId, macAddress);
                    SaveLeases();
                    return requestedEntry.IpAddress;
                }
            }

            // 5. Find any unused IP
            var available = _leases.FirstOrDefault(l => l.Status == LeaseStatus.Unused);
            if (available != null)
            {
                available.SetReserved(transactionId, macAddress);
                SaveLeases();
                return available.IpAddress;
            }

            return null; // No available IPs
        }
    }

    /// <summary>
    /// Handle REQUEST message - confirm IP allocation
    /// </summary>
    public IPAddress? HandleRequest(IPAddress requestedIp, uint transactionId, PhysicalAddress macAddress)
    {
        lock (_lockObj)
        {
            RefreshAll();

            // 1. Check if reserved with same transaction ID
            var reserved = _leases.FirstOrDefault(l =>
                l.Status == LeaseStatus.Reserved &&
                l.TransactionId == transactionId &&
                l.IpAddress.Equals(requestedIp));

            if (reserved != null)
            {
                reserved.SetUsed(transactionId, macAddress, _leaseTimeSeconds);
                SaveLeases();
                return reserved.IpAddress;
            }

            // 2. Check if already in use with same IP and MAC
            var inUse = _leases.FirstOrDefault(l =>
                l.Status == LeaseStatus.Used &&
                l.IpAddress.Equals(requestedIp) &&
                l.MatchesMac(macAddress));

            if (inUse != null)
            {
                // Refresh lease
                inUse.SetUsed(transactionId, macAddress, _leaseTimeSeconds);
                SaveLeases();
                return inUse.IpAddress;
            }

            return null; // Not found or conflict
        }
    }

    /// <summary>
    /// Handle RELEASE message - free IP address
    /// </summary>
    public void HandleRelease(PhysicalAddress macAddress)
    {
        lock (_lockObj)
        {
            foreach (var entry in _leases.Where(l => l.MatchesMac(macAddress)))
            {
                entry.SetUnused();
            }
            SaveLeases();
        }
    }

    /// <summary>
    /// Refresh all leases (check expiration)
    /// </summary>
    private void RefreshAll()
    {
        foreach (var entry in _leases)
        {
            entry.Refresh();
        }
    }

    /// <summary>
    /// Save leases to persistent storage
    /// </summary>
    private void SaveLeases()
    {
        if (string.IsNullOrEmpty(_leaseDbPath))
        {
            return;
        }

        try
        {
            var leaseData = _leases
                .Where(l => l.Status != LeaseStatus.Unused)
                .Select(l => new
                {
                    IpAddress = l.IpAddress.ToString(),
                    MacAddress = l.MacAddress?.ToString() ?? "",
                    TransactionId = l.TransactionId,
                    ExpiresAt = l.ExpiresAt.ToString("O"),
                    Status = l.Status.ToString()
                })
                .ToList();

            var json = JsonSerializer.Serialize(leaseData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_leaseDbPath, json);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save leases to {Path}", _leaseDbPath);
        }
    }

    /// <summary>
    /// Load leases from persistent storage
    /// </summary>
    private void LoadLeases()
    {
        if (string.IsNullOrEmpty(_leaseDbPath) || !File.Exists(_leaseDbPath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_leaseDbPath);
            var leaseData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json);

            if (leaseData == null)
            {
                return;
            }

            foreach (var data in leaseData)
            {
                var ip = IPAddress.Parse(data["IpAddress"]);
                var mac = PhysicalAddress.Parse(data["MacAddress"].Replace("-", "").Replace(":", ""));
                var transactionId = uint.Parse(data["TransactionId"]);
                var expiresAt = DateTime.Parse(data["ExpiresAt"]);
                var status = Enum.Parse<LeaseStatus>(data["Status"]);

                var entry = _leases.FirstOrDefault(l => l.IpAddress.Equals(ip));
                if (entry != null && expiresAt > DateTime.UtcNow)
                {
                    if (status == LeaseStatus.Used)
                    {
                        var remainingSeconds = (int)(expiresAt - DateTime.UtcNow).TotalSeconds;
                        entry.SetUsed(transactionId, mac, remainingSeconds);
                    }
                }
            }

            _logger?.LogInformation("Loaded {Count} leases from database", leaseData.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load leases from {Path}", _leaseDbPath);
        }
    }

    /// <summary>
    /// Get lease information for display
    /// </summary>
    public string GetInfo()
    {
        lock (_lockObj)
        {
            RefreshAll();

            var unused = _leases.Count(l => l.Status == LeaseStatus.Unused);
            var reserved = _leases.Count(l => l.Status == LeaseStatus.Reserved);
            var used = _leases.Count(l => l.Status == LeaseStatus.Used);

            return $"Total: {_leases.Count}, Unused: {unused}, Reserved: {reserved}, Used: {used}";
        }
    }
}
