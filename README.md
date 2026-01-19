# JumboDogX

English | [日本語](README.ja.md)

**JumboDogX** is the next-generation multi-platform integrated server software, a complete rewrite of BlackJumboDog (BJD) using .NET 9. JumboDogX represents the evolution of BJD, bringing modern cross-platform capabilities to the beloved Japanese server software.

**⚠️ Important**: This software is designed for **local testing purposes only**. It is NOT intended for production use or public-facing deployments. Use it exclusively in development and testing environments.

## Overview

- **Original Project**: BlackJumboDog Ver6.1.9
- **License**: MIT License
- **Purpose**: Local testing environment only (NOT recommended for production or public servers)
- **Platforms**: Windows, macOS, Linux
- **Framework**: .NET 9
- **Language**: C# 13

## Key Features (Planned)

- HTTP/HTTPS Server
- DNS Server
- SMTP/POP3 Mail Server
- FTP Server
- DHCP Server
- Various Proxy Servers
- Web UI Management Console (Blazor Server)

## Project Structure

```
jdx/
├── src/
│   ├── Jdx.Core/              # Core library
│   ├── Jdx.Servers.Http/      # HTTP server implementation
│   ├── Jdx.Servers.Dns/       # DNS server implementation
│   ├── Jdx.Host/              # Host application
│   └── Jdx.WebUI/             # Web UI (Blazor Server)
├── tests/
│   ├── Jdx.Tests.Core/        # Core unit tests
│   └── Jdx.Tests.Servers.Http/ # HTTP server tests
└── docs/                        # Design documents
```

## Development Status

### Core Infrastructure
- [x] .NET 9 SDK installation
- [x] Project structure creation
- [x] Solution configuration
- [x] Core implementation (ServerBase, IServer abstraction)
- [x] Common infrastructure (NetworkHelper, ConnectionLimiter, NetworkExceptionHandler)
- [x] Server refactoring Phase 1-3 completed (PR #13, #14)

### Server Implementations
- [x] HTTP/HTTPS server (Range Requests, Keep-Alive, Virtual Host, SSL/TLS structure)
- [x] DNS server (A, AAAA, CNAME, MX, NS, PTR, SOA records)
- [x] SMTP server (mail sending with authentication)
- [x] POP3 server (mail retrieval with authentication)
- [x] FTP server (file transfer, user management, ACL)
- [x] DHCP server (IP address assignment, lease management)
- [x] TFTP server (simple file transfer)
- [x] Proxy server (HTTP proxy with cache, URL filtering)

### Web UI (Blazor Server)
- [x] Dashboard (server status monitoring)
- [x] Logs viewer (real-time log display)
- [x] Settings pages for all servers (100% coverage)
  - HTTP/HTTPS: General, Document, CGI, SSI, WebDAV, Alias & MIME, Authentication, Template, ACL, Virtual Hosts, SSL/TLS, Advanced
  - DNS: General, Records management
  - SMTP, POP3, FTP, DHCP, TFTP, Proxy: Configuration pages

### Advanced Features
- [x] Apache Killer protection (DoS attack defense)
- [x] AttackDb (time-window based attack detection)
- [x] Range Requests (partial content delivery - RFC 7233)
- [x] Keep-Alive (HTTP persistent connections)
- [x] Virtual Host (host header-based routing)
- [x] SSL/TLS basic structure (certificate management)
- [x] SSL/TLS full integration (actual SSL communication - PR #20)
- [x] AttackDb ACL auto-addition (PR #18)
- [x] Metrics collection (Prometheus format export)
- [x] Structured logging with Serilog (JSON format, log rotation)

## Requirements

- .NET 9 SDK or later
- Visual Studio 2022 / JetBrains Rider / VS Code

## Build

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Run benchmarks
dotnet run --project benchmarks/Jdx.Benchmarks -c Release
```

## Running

### CLI Version (Host Application)

```bash
cd src/Jdx.Host
dotnet run
```

### Web UI Version

```bash
cd src/Jdx.WebUI
dotnet run
```

Then, open http://localhost:5001 in your browser.

For detailed instructions, see [startup.md](startup.md).

## Metrics

JumboDogX provides Prometheus-compatible metrics for monitoring server performance.

### Accessing Metrics

Metrics are available via the HTTP server's `/metrics` endpoint:

```bash
curl http://localhost:2001/metrics
```

### Metrics Available

- **Total Connections**: Number of connections received
- **Active Connections**: Currently active connections
- **Total Requests**: Number of requests processed
- **Total Errors**: Number of errors encountered
- **Bytes Received/Sent**: Network traffic statistics
- **Server Uptime**: How long each server has been running

### Integration with Prometheus

Add the following to your Prometheus configuration (`prometheus.yml`):

```yaml
scrape_configs:
  - job_name: 'jumbodogx'
    static_configs:
      - targets: ['localhost:2001']
```

### Grafana Dashboard

Metrics can be visualized using Grafana by:
1. Adding Prometheus as a data source
2. Creating a dashboard with queries like:
   - `jumbodogx_http_total_connections`
   - `jumbodogx_http_active_connections`
   - `rate(jumbodogx_http_total_requests[5m])`

## Logging

JumboDogX uses structured logging with Serilog to provide machine-readable, searchable logs.

### Log Format

All logs are output in **Compact JSON format**, making them easy to parse and analyze with log aggregation tools.

### Log Outputs

Logs are written to two destinations:

1. **Console**: Real-time log output in JSON format
2. **File**: Persistent log files with automatic rotation

### Log Files

- **Host Application**: `logs/jumbodogx-host-YYYYMMDD.log`
- **WebUI Application**: `logs/jumbodogx-webui-YYYYMMDD.log`

### Log Rotation

Log files are automatically rotated based on:

- **Daily rotation**: New log file created each day
- **Size-based rotation**: When a file exceeds 10MB
- **Retention**: Last 30 days of logs are kept

### Log Structure

Each log entry contains:

```json
{
  "@t": "2026-01-19T10:30:45.123Z",
  "@mt": "HTTP request received",
  "@l": "Information",
  "Application": "JumboDogX.Host",
  "SourceContext": "Jdx.Servers.Http.HttpServer",
  "Method": "GET",
  "Path": "/index.html"
}
```

### Analyzing Logs

Use tools like `jq` to analyze JSON logs:

```bash
# View all error logs
cat logs/jumbodogx-host-*.log | jq 'select(.["@l"] == "Error")'

# Count requests by method
cat logs/jumbodogx-host-*.log | jq -r '.Method' | sort | uniq -c

# Extract timestamps and messages
cat logs/jumbodogx-host-*.log | jq -r '"\(.["@t"]) - \(.["@mt"])"'
```

## Testing

### Running Unit Tests

Run all tests:
```bash
dotnet test
```

Run tests for a specific project:
```bash
dotnet test tests/Jdx.Core.Tests
dotnet test tests/Jdx.Servers.Http.Tests
dotnet test tests/Jdx.Servers.Dns.Tests
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Running Benchmarks

Run all benchmarks:
```bash
dotnet run --project benchmarks/Jdx.Benchmarks -c Release
```

Run specific benchmark:
```bash
dotnet run --project benchmarks/Jdx.Benchmarks -c Release -- --filter *HttpRequestBenchmark*
```

For more details, see [benchmarks/Jdx.Benchmarks/README.md](benchmarks/Jdx.Benchmarks/README.md).

## Configuration

For detailed configuration examples, see [docs/configuration-guide.md](docs/configuration-guide.md).

## Design Documents

Detailed design documents are stored in the `docs/` directory:

- Migration Plan: `docs/00_migration/`
- Technical Design: `docs/01_technical/`

## Security Notice

This software is designed for **local development and testing environments only**. Please be aware of the following:

- Do NOT expose directly to the internet
- NOT recommended for production use
- No security audit has been performed
- Intended for development, testing, and learning purposes only

## Security Configuration

Before using JumboDogX, review and update the following security settings in `src/Jdx.WebUI/appsettings.json`:

### Critical Security Settings

1. **User Passwords** (High Priority)
   - Default password hash: `REPLACE_WITH_SECURE_SHA256_HASH`
   - ⚠️ **MUST** be changed before use
   - Generate a strong password and create its SHA-256 hash:
     ```bash
     echo -n 'YourStrongPassword' | shasum -a 256
     ```
   - Replace the placeholder with the generated hash

2. **Network Binding**
   - Default: `"BindAddress": "127.0.0.1"` (localhost only)
   - For network access, change to `"0.0.0.0"` (all interfaces)
   - ⚠️ Only change if necessary and behind a firewall

3. **CGI Execution** (Medium Priority)
   - Default: `"UseCgi": false` (disabled)
   - Enable only if CGI scripts are required
   - ⚠️ Potential command injection risk when enabled
   - Ensure proper input validation when using CGI

4. **WebDAV Write Access** (Medium Priority)
   - Default: `"AllowWrite": false` (read-only)
   - Enable write access only when necessary
   - ⚠️ Use with authentication to prevent unauthorized file uploads

### Recommended Security Practices

- Use strong, unique passwords for all user accounts
- Keep BindAddress as `127.0.0.1` unless network access is required
- Enable CGI and WebDAV write access only when absolutely necessary
- Regularly review and update security settings
- Monitor server logs for suspicious activity

For security vulnerability reports, see [SECURITY.md](SECURITY.md).

## License

MIT License

Copyright (c) 2026 Hirauchi Shinichi (SIN)

See the [LICENSE](LICENSE) file for details.

## Related Links

- Original Project: BlackJumboDog Ver6.1.9
- .NET 9: https://dotnet.microsoft.com/
