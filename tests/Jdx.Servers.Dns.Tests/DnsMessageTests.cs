using System.Text;
using Jdx.Servers.Dns;
using Xunit;

namespace Jdx.Servers.Dns.Tests;

public class DnsMessageTests
{
    [Fact]
    public void ParseQuery_WithValidQuery_ParsesCorrectly()
    {
        // Arrange
        var query = CreateDnsQuery("example.com", 1); // Type A

        // Act
        var message = DnsMessage.ParseQuery(query);

        // Assert
        Assert.Equal("example.com", message.QueryName);
        Assert.Equal(1, message.QuestionCount);
        Assert.Equal(1, (int)message.QueryType); // A record
        Assert.False(message.IsResponse);
    }

    [Fact]
    public void ParseQuery_WithTooShortData_ThrowsFormatException()
    {
        // Arrange
        var shortData = new byte[10]; // Less than 12 bytes minimum

        // Act & Assert
        Assert.Throws<FormatException>(() => DnsMessage.ParseQuery(shortData));
    }

    [Fact]
    public void CreateResponse_WithIpAddress_CreatesValidResponse()
    {
        // Arrange
        var query = CreateDnsQuery("test.com", 1);
        var message = DnsMessage.ParseQuery(query);

        // Act
        var response = message.CreateResponse("192.168.1.1");

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Length > 12); // Header + question + answer
        Assert.Equal(message.TransactionId, (ushort)((response[0] << 8) | response[1]));
        Assert.True((response[2] & 0x80) != 0); // QR bit set (response)
    }

    [Fact]
    public void CreateNXDomainResponse_CreatesValidNXDomain()
    {
        // Arrange
        var query = CreateDnsQuery("nonexistent.com", 1);
        var message = DnsMessage.ParseQuery(query);

        // Act
        var response = message.CreateNXDomainResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(message.TransactionId, (ushort)((response[0] << 8) | response[1]));
        Assert.True((response[2] & 0x80) != 0); // QR bit set (response)
        Assert.Equal(3, response[3] & 0x0F); // RCODE = 3 (NXDOMAIN)
        Assert.Equal(0, (response[6] << 8) | response[7]); // Answer count = 0
    }

    [Fact]
    public void ParseQuery_WithMultipleLabels_ParsesDomainCorrectly()
    {
        // Arrange
        var query = CreateDnsQuery("www.example.com", 1);

        // Act
        var message = DnsMessage.ParseQuery(query);

        // Assert
        Assert.Equal("www.example.com", message.QueryName);
    }

    [Fact]
    public void CreateResponse_WithInvalidIp_UsesDefaultIp()
    {
        // Arrange
        var query = CreateDnsQuery("test.com", 1);
        var message = DnsMessage.ParseQuery(query);

        // Act
        var response = message.CreateResponse("invalid-ip");

        // Assert - should create response with 127.0.0.1
        Assert.NotNull(response);
        var rdataStart = response.Length - 4;
        Assert.Equal(127, response[rdataStart]);
        Assert.Equal(0, response[rdataStart + 1]);
        Assert.Equal(0, response[rdataStart + 2]);
        Assert.Equal(1, response[rdataStart + 3]);
    }

    [Fact]
    public void ParseQuery_ExtractsTransactionId()
    {
        // Arrange
        var query = CreateDnsQuery("test.com", 1, transactionId: 0x1234);

        // Act
        var message = DnsMessage.ParseQuery(query);

        // Assert
        Assert.Equal(0x1234, message.TransactionId);
    }

    private byte[] CreateDnsQuery(string domainName, ushort queryType, ushort transactionId = 0x0001)
    {
        var query = new List<byte>();

        // Header (12 bytes)
        query.Add((byte)(transactionId >> 8));
        query.Add((byte)(transactionId & 0xFF));
        query.Add(0x01); // QR=0 (query), RD=1
        query.Add(0x00); // RA=0
        query.Add(0x00); // QDCOUNT high
        query.Add(0x01); // QDCOUNT low (1 question)
        query.Add(0x00); // ANCOUNT high
        query.Add(0x00); // ANCOUNT low
        query.Add(0x00); // NSCOUNT high
        query.Add(0x00); // NSCOUNT low
        query.Add(0x00); // ARCOUNT high
        query.Add(0x00); // ARCOUNT low

        // Question section
        foreach (var label in domainName.Split('.'))
        {
            query.Add((byte)label.Length);
            query.AddRange(Encoding.ASCII.GetBytes(label));
        }
        query.Add(0x00); // Null terminator

        query.Add((byte)(queryType >> 8));
        query.Add((byte)(queryType & 0xFF));
        query.Add(0x00); // QCLASS high (IN = 1)
        query.Add(0x01); // QCLASS low

        return query.ToArray();
    }
}
