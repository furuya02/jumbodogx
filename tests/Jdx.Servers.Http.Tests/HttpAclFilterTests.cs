using Jdx.Core.Settings;
using Jdx.Servers.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jdx.Servers.Http.Tests;

public class HttpAclFilterTests
{
    [Fact]
    public void IsAllowed_WithEmptyAcl_ReturnsFalse_FailSecure()
    {
        // Arrange
        var settings = new HttpServerSettings
        {
            EnableAcl = 0,
            AclList = new List<AclEntry>()
        };
        var mockLogger = new Mock<ILogger>();
        var filter = new HttpAclFilter(settings, mockLogger.Object);

        // Act
        var result = filter.IsAllowed("192.168.1.1");

        // Assert - Fail-secure: deny all when ACL list is empty
        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_WithNullAclList_ReturnsFalse_FailSecure()
    {
        // Arrange
        var settings = new HttpServerSettings
        {
            EnableAcl = 1,
            AclList = null
        };
        var mockLogger = new Mock<ILogger>();
        var filter = new HttpAclFilter(settings, mockLogger.Object);

        // Act
        var result = filter.IsAllowed("192.168.1.1");

        // Assert - Fail-secure: deny all when ACL list is null
        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_WithAllowList_AllowsListedIp()
    {
        // Arrange
        var settings = new HttpServerSettings
        {
            EnableAcl = 1,
            AclList = new List<AclEntry>
            {
                new AclEntry { Name = "Allowed", Address = "192.168.1.1" }
            }
        };
        var mockLogger = new Mock<ILogger>();
        var filter = new HttpAclFilter(settings, mockLogger.Object);

        // Act
        var result = filter.IsAllowed("192.168.1.1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_WithAllowList_RejectsUnlistedIp()
    {
        // Arrange
        var settings = new HttpServerSettings
        {
            EnableAcl = 1,
            AclList = new List<AclEntry>
            {
                new AclEntry { Name = "Allowed", Address = "192.168.1.1" }
            }
        };
        var mockLogger = new Mock<ILogger>();
        var filter = new HttpAclFilter(settings, mockLogger.Object);

        // Act
        var result = filter.IsAllowed("192.168.1.2");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_WithDenyList_AllowsUnlistedIp()
    {
        // Arrange
        var settings = new HttpServerSettings
        {
            EnableAcl = 2,
            AclList = new List<AclEntry>
            {
                new AclEntry { Name = "Denied", Address = "10.0.0.1" }
            }
        };
        var mockLogger = new Mock<ILogger>();
        var filter = new HttpAclFilter(settings, mockLogger.Object);

        // Act
        var result = filter.IsAllowed("192.168.1.1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_WithDenyList_RejectsListedIp()
    {
        // Arrange
        var settings = new HttpServerSettings
        {
            EnableAcl = 2,
            AclList = new List<AclEntry>
            {
                new AclEntry { Name = "Denied", Address = "192.168.1.1" }
            }
        };
        var mockLogger = new Mock<ILogger>();
        var filter = new HttpAclFilter(settings, mockLogger.Object);

        // Act
        var result = filter.IsAllowed("192.168.1.1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_WithCidrRange_AllowsIpInRange()
    {
        // Arrange
        var settings = new HttpServerSettings
        {
            EnableAcl = 1,
            AclList = new List<AclEntry>
            {
                new AclEntry { Name = "Local Network", Address = "192.168.1.0/24" }
            }
        };
        var mockLogger = new Mock<ILogger>();
        var filter = new HttpAclFilter(settings, mockLogger.Object);

        // Act
        var result = filter.IsAllowed("192.168.1.50");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAllowed_WithCidrRange_RejectsIpOutsideRange()
    {
        // Arrange
        var settings = new HttpServerSettings
        {
            EnableAcl = 1,
            AclList = new List<AclEntry>
            {
                new AclEntry { Name = "Local Network", Address = "192.168.1.0/24" }
            }
        };
        var mockLogger = new Mock<ILogger>();
        var filter = new HttpAclFilter(settings, mockLogger.Object);

        // Act
        var result = filter.IsAllowed("192.168.2.1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_WithInvalidIpAddress_ReturnsFalse()
    {
        // Arrange
        var settings = new HttpServerSettings
        {
            EnableAcl = 1,
            AclList = new List<AclEntry>
            {
                new AclEntry { Name = "Test", Address = "192.168.1.1" }
            }
        };
        var mockLogger = new Mock<ILogger>();
        var filter = new HttpAclFilter(settings, mockLogger.Object);

        // Act
        var result = filter.IsAllowed("invalid-ip");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAllowed_WithMultipleAclEntries_MatchesFirst()
    {
        // Arrange
        var settings = new HttpServerSettings
        {
            EnableAcl = 1,
            AclList = new List<AclEntry>
            {
                new AclEntry { Name = "Range1", Address = "192.168.1.0/24" },
                new AclEntry { Name = "Range2", Address = "10.0.0.0/8" }
            }
        };
        var mockLogger = new Mock<ILogger>();
        var filter = new HttpAclFilter(settings, mockLogger.Object);

        // Act
        var result1 = filter.IsAllowed("192.168.1.100");
        var result2 = filter.IsAllowed("10.0.0.1");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
    }
}
