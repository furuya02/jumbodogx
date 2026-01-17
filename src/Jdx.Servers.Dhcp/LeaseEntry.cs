using System.Net;
using System.Net.NetworkInformation;

namespace Jdx.Servers.Dhcp;

/// <summary>
/// DHCP Lease Entry
/// Represents a single IP address lease
/// </summary>
public class LeaseEntry
{
    public IPAddress IpAddress { get; }
    public PhysicalAddress? MacAddress { get; private set; }
    public uint TransactionId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public LeaseStatus Status { get; private set; }
    public bool IsMacReserved { get; }

    /// <summary>
    /// Create a new lease entry
    /// </summary>
    public LeaseEntry(IPAddress ipAddress, PhysicalAddress? reservedMac = null)
    {
        IpAddress = ipAddress;
        MacAddress = null;
        TransactionId = 0;
        ExpiresAt = DateTime.MinValue;
        Status = LeaseStatus.Unused;
        IsMacReserved = reservedMac != null;

        if (reservedMac != null)
        {
            MacAddress = reservedMac;
        }
    }

    /// <summary>
    /// Set lease as unused (available)
    /// </summary>
    public void SetUnused()
    {
        if (!IsMacReserved)
        {
            MacAddress = null;
        }

        TransactionId = 0;
        ExpiresAt = DateTime.MinValue;
        Status = LeaseStatus.Unused;
    }

    /// <summary>
    /// Reserve lease for DISCOVER (5 seconds)
    /// </summary>
    public void SetReserved(uint transactionId, PhysicalAddress macAddress)
    {
        TransactionId = transactionId;
        MacAddress = macAddress;
        ExpiresAt = DateTime.UtcNow.AddSeconds(5);
        Status = LeaseStatus.Reserved;
    }

    /// <summary>
    /// Set lease as used (assigned)
    /// </summary>
    public void SetUsed(uint transactionId, PhysicalAddress macAddress, int leaseTimeSeconds)
    {
        TransactionId = transactionId;
        MacAddress = macAddress;
        ExpiresAt = DateTime.UtcNow.AddSeconds(leaseTimeSeconds);
        Status = LeaseStatus.Used;
    }

    /// <summary>
    /// Check if lease is expired and refresh status
    /// </summary>
    public void Refresh()
    {
        if (Status != LeaseStatus.Unused && DateTime.UtcNow >= ExpiresAt)
        {
            SetUnused();
        }
    }

    /// <summary>
    /// Check if this entry matches the given MAC address
    /// </summary>
    public bool MatchesMac(PhysicalAddress macAddress)
    {
        return MacAddress != null && MacAddress.Equals(macAddress);
    }
}
