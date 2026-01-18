using BenchmarkDotNet.Attributes;
using Jdx.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jdx.Benchmarks;

[MemoryDiagnoser]
public class NetworkHelperBenchmark
{
    private ILogger _logger = null!;

    [GlobalSetup]
    public void Setup()
    {
        _logger = NullLogger.Instance;
    }

    [Benchmark]
    public void ParseBindAddress_Null()
    {
        NetworkHelper.ParseBindAddress(null, _logger);
    }

    [Benchmark]
    public void ParseBindAddress_ZeroAddress()
    {
        NetworkHelper.ParseBindAddress("0.0.0.0", _logger);
    }

    [Benchmark]
    public void ParseBindAddress_Localhost()
    {
        NetworkHelper.ParseBindAddress("127.0.0.1", _logger);
    }

    [Benchmark]
    public void ParseBindAddress_PrivateAddress()
    {
        NetworkHelper.ParseBindAddress("192.168.1.100", _logger);
    }

    [Benchmark]
    public void ParseBindAddress_Invalid()
    {
        NetworkHelper.ParseBindAddress("invalid-address", _logger);
    }
}
