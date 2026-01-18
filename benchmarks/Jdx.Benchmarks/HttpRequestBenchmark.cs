using BenchmarkDotNet.Attributes;
using Jdx.Servers.Http;

namespace Jdx.Benchmarks;

[MemoryDiagnoser]
public class HttpRequestBenchmark
{
    private string _simpleRequest = null!;
    private string _complexRequest = null!;
    private string[] _simpleRequestLines = null!;
    private string[] _complexRequestLines = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simpleRequest = "GET /index.html HTTP/1.1";
        _complexRequest = "POST /api/submit?q=test&lang=en HTTP/1.1";

        _simpleRequestLines = new[]
        {
            "GET /index.html HTTP/1.1",
            "Host: localhost",
            "User-Agent: TestClient/1.0",
            ""
        };

        _complexRequestLines = new[]
        {
            "POST /api/submit?q=hello%20world&name=John%20Doe&action=save HTTP/1.1",
            "Host: api.example.com",
            "User-Agent: Mozilla/5.0",
            "Accept: application/json",
            "Content-Type: application/x-www-form-urlencoded",
            "Content-Length: 50",
            "Authorization: Bearer token123",
            "X-Custom-Header: CustomValue",
            ""
        };
    }

    [Benchmark]
    public HttpRequest ParseSimpleRequestLine()
    {
        return HttpRequest.Parse(_simpleRequest);
    }

    [Benchmark]
    public HttpRequest ParseComplexRequestLine()
    {
        return HttpRequest.Parse(_complexRequest);
    }

    [Benchmark]
    public HttpRequest ParseFullSimpleRequest()
    {
        return HttpRequest.ParseFull(_simpleRequestLines);
    }

    [Benchmark]
    public HttpRequest ParseFullComplexRequest()
    {
        return HttpRequest.ParseFull(_complexRequestLines);
    }
}
