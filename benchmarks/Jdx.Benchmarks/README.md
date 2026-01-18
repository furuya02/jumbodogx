# JumboDogX Benchmarks

This project contains performance benchmarks for JumboDogX using BenchmarkDotNet.

## Running Benchmarks

Run all benchmarks:
```bash
dotnet run --project benchmarks/Jdx.Benchmarks -c Release
```

Run specific benchmark:
```bash
dotnet run --project benchmarks/Jdx.Benchmarks -c Release -- --filter *HttpRequestBenchmark*
```

Run specific method:
```bash
dotnet run --project benchmarks/Jdx.Benchmarks -c Release -- --filter *ParseFullComplexRequest*
```

## Available Benchmarks

### HttpRequestBenchmark
- HTTP request line parsing
- Full HTTP request parsing with headers and query strings

### DnsQueryBenchmark
- DNS query parsing
- DNS response generation
- NXDOMAIN response generation

### NetworkHelperBenchmark
- IP address parsing and validation
- Bind address resolution

## Interpreting Results

BenchmarkDotNet will display:
- **Mean**: Average execution time
- **Error**: Half of 99.9% confidence interval
- **StdDev**: Standard deviation
- **Gen0/Gen1/Gen2**: Garbage collection statistics
- **Allocated**: Memory allocated per operation

## Tips

- Always run in Release mode (`-c Release`)
- Close other applications to reduce noise
- Run benchmarks multiple times for consistency
- Use `--filter` to run specific benchmarks
