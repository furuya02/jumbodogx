namespace Jdx.Servers.Dhcp;

/// <summary>
/// DHCP Message Types (RFC 2131)
/// </summary>
public enum DhcpMessageType : byte
{
    Unknown = 0,
    Discover = 1,    // Client → Server: IP address request
    Offer = 2,       // Server → Client: IP address offer
    Request = 3,     // Client → Server: Request offered/existing IP
    Decline = 4,     // Client → Server: Decline offered IP (conflict)
    Ack = 5,         // Server → Client: Acknowledge request
    Nak = 6,         // Server → Client: Refuse request
    Release = 7,     // Client → Server: Release IP address
    Inform = 8       // Client → Server: Request configuration only
}

/// <summary>
/// DHCP Lease Status
/// </summary>
public enum LeaseStatus
{
    Unused = 0,      // Available for allocation
    Reserved = 1,    // Reserved by DISCOVER (5 seconds)
    Used = 2         // Assigned by REQUEST ACK
}
