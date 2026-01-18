using BenchmarkDotNet.Attributes;
using Jdx.Servers.Dns;
using System.Text;

namespace Jdx.Benchmarks;

[MemoryDiagnoser]
public class DnsQueryBenchmark
{
    private byte[] _shortQuery = null!;
    private byte[] _longQuery = null!;
    private DnsMessage _simpleMessage = null!;

    [GlobalSetup]
    public void Setup()
    {
        _shortQuery = CreateDnsQuery("test.com");
        _longQuery = CreateDnsQuery("www.subdomain.example.com");
        _simpleMessage = DnsMessage.ParseQuery(_shortQuery);
    }

    [Benchmark]
    public DnsMessage ParseShortQuery()
    {
        return DnsMessage.ParseQuery(_shortQuery);
    }

    [Benchmark]
    public DnsMessage ParseLongQuery()
    {
        return DnsMessage.ParseQuery(_longQuery);
    }

    [Benchmark]
    public byte[] CreateResponse()
    {
        return _simpleMessage.CreateResponse("192.168.1.1");
    }

    [Benchmark]
    public byte[] CreateNXDomainResponse()
    {
        return _simpleMessage.CreateNXDomainResponse();
    }

    private byte[] CreateDnsQuery(string domainName)
    {
        var query = new List<byte>();

        // Header (12 bytes)
        query.Add(0x00);
        query.Add(0x01); // Transaction ID
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

        query.Add(0x00); // QTYPE high (A = 1)
        query.Add(0x01); // QTYPE low
        query.Add(0x00); // QCLASS high (IN = 1)
        query.Add(0x01); // QCLASS low

        return query.ToArray();
    }
}
