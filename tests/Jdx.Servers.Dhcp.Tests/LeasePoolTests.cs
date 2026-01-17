using System.Net;
using System.Net.NetworkInformation;
using Jdx.Servers.Dhcp;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Servers.Dhcp.Tests;

public class LeasePoolTests
{
    [Fact]
    public void Constructor_ShouldInitializePool()
    {
        // Arrange
        var startIp = IPAddress.Parse("192.168.1.100");
        var endIp = IPAddress.Parse("192.168.1.110");
        var leaseTime = 3600;
        var mockLogger = new Mock<ILogger>();

        // Act
        var pool = new LeasePool(startIp, endIp, leaseTime, null, null, mockLogger.Object);

        // Assert
        Assert.NotNull(pool);
    }

    [Fact]
    public void HandleDiscover_ShouldReturnAvailableIp()
    {
        // Arrange
        var startIp = IPAddress.Parse("10.0.0.10");
        var endIp = IPAddress.Parse("10.0.0.12");
        var leaseTime = 3600;
        var mockLogger = new Mock<ILogger>();
        var pool = new LeasePool(startIp, endIp, leaseTime, null, null, mockLogger.Object);
        var macAddress = PhysicalAddress.Parse("00-11-22-33-44-55");
        uint transactionId = 12345;

        // Act
        var assignedIp = pool.HandleDiscover(null, transactionId, macAddress);

        // Assert
        Assert.NotNull(assignedIp);
    }

    [Fact]
    public void HandleRequest_WithReservedIp_ShouldReturnSameIp()
    {
        // Arrange
        var startIp = IPAddress.Parse("10.0.0.10");
        var endIp = IPAddress.Parse("10.0.0.12");
        var leaseTime = 3600;
        var mockLogger = new Mock<ILogger>();
        var pool = new LeasePool(startIp, endIp, leaseTime, null, null, mockLogger.Object);
        var macAddress = PhysicalAddress.Parse("AA-BB-CC-DD-EE-FF");
        uint transactionId = 67890;

        // First discover to reserve an IP
        var discoveredIp = pool.HandleDiscover(null, transactionId, macAddress);

        // Act
        var requestedIp = pool.HandleRequest(discoveredIp!, transactionId, macAddress);

        // Assert
        Assert.NotNull(requestedIp);
        Assert.Equal(discoveredIp, requestedIp);
    }

    [Fact]
    public void HandleRelease_ShouldFreeIp()
    {
        // Arrange
        var startIp = IPAddress.Parse("10.0.0.10");
        var endIp = IPAddress.Parse("10.0.0.10"); // Single IP
        var leaseTime = 3600;
        var mockLogger = new Mock<ILogger>();
        var pool = new LeasePool(startIp, endIp, leaseTime, null, null, mockLogger.Object);
        var macAddress1 = PhysicalAddress.Parse("11-22-33-44-55-66");
        var macAddress2 = PhysicalAddress.Parse("77-88-99-AA-BB-CC");
        uint transactionId1 = 111;
        uint transactionId2 = 222;

        // Get the only available IP
        var ip1 = pool.HandleDiscover(null, transactionId1, macAddress1);
        var confirmedIp1 = pool.HandleRequest(ip1!, transactionId1, macAddress1);
        Assert.NotNull(confirmedIp1);

        // Second request should fail (pool exhausted)
        var ip2 = pool.HandleDiscover(null, transactionId2, macAddress2);
        Assert.Null(ip2);

        // Act - Release first IP
        pool.HandleRelease(macAddress1);

        // Third request should succeed now
        var ip3 = pool.HandleDiscover(null, transactionId2, macAddress2);

        // Assert
        Assert.NotNull(ip3);
    }
}
