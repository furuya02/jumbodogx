# JumboDogX

English | [日本語](README.ja.md)

**JumboDogX** is the next-generation multi-platform integrated server software, a complete rewrite of BlackJumboDog (BJD) using .NET 9. JumboDogX represents the evolution of BJD, bringing modern cross-platform capabilities to the beloved Japanese server software that has been widely used.
(Under development as of January 2026)

**⚠️ Important**: This software is designed for **local testing purposes only**. It is NOT intended for production use or public-facing deployments. Use it exclusively in development and testing environments.

## Overview

- **Original Project**: BlackJumboDog Ver6.1.9
- **License**: MIT License
- **Purpose**: Local testing environment only (NOT recommended for production or public servers)
- **Platforms**: Windows, macOS, Linux
- **Framework**: .NET 9
- **Language**: C# 13

## Key Features

- HTTP/HTTPS Server
- DNS Server
- SMTP/POP3 Mail Server
- FTP Server
- DHCP Server
- TFTP Server
- Proxy Server
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
- .NET 9 SDK installed
- Project structure created
- Solution configuration completed
- Core functionality implemented (ServerBase, IServer abstraction)
- Common infrastructure implemented (NetworkHelper, ConnectionLimiter, NetworkExceptionHandler)
- Server refactoring Phase 1-3 completed (PR #13, #14)

### Server Implementations
- HTTP/HTTPS server implemented (Range Requests, Keep-Alive, Virtual Host, SSL/TLS structure)
- DNS server implemented (A, AAAA, CNAME, MX, NS, PTR, SOA records)
- SMTP server implemented (mail sending with authentication)
- POP3 server implemented (mail retrieval with authentication)
- FTP server implemented (file transfer, user management, ACL)
- DHCP server implemented (IP address assignment, lease management)
- TFTP server implemented (simple file transfer)
- Proxy server implemented (HTTP proxy with cache, URL filtering)

### Web UI (Blazor Server)
- Dashboard implemented (server status monitoring)
- Logs viewer implemented (real-time log display)
- Settings pages for all servers implemented (100% coverage)
  - HTTP/HTTPS: General, Document, CGI, SSI, WebDAV, Alias & MIME, Authentication, Template, ACL, Virtual Hosts, SSL/TLS, Advanced
  - DNS: General, Records management
  - SMTP, POP3, FTP, DHCP, TFTP, Proxy: Configuration pages

### Advanced Features
- Apache Killer protection implemented (DoS attack defense)
- AttackDb implemented (time-window based attack detection)
- Range Requests implemented (partial content delivery - RFC 7233)
- Keep-Alive implemented (HTTP persistent connections)
- Virtual Host implemented (host header-based routing)
- SSL/TLS basic structure implemented (certificate management)
- SSL/TLS full integration completed (actual SSL communication - PR #20)
- AttackDb ACL auto-addition completed (PR #18)

### Phase 2: Next-Generation Features and Enterprise Support

**Phase 1 Completion**: All 27 TODO items in Phase 1 were completed on January 19, 2026!
Phase 2 focuses on improving operability, further enhancing quality, and adding new features.

**Progress**: 4/18 items completed (22.2%)

#### Completed (Critical Priority)
- Metrics collection feature (PR #26)
- Log rotation feature (PR #27)
- Structured logging with Serilog (PR #27)
- Settings import/export feature (PR #28)

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

- **Host Application**: `logs/jumbodogx-host20260119.log`
- **WebUI Application**: `logs/jumbodogx-webui20260119.log`

Note: Serilog automatically appends the date (YYYYMMDD) between the filename and extension when daily rotation is enabled.

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
cat logs/jumbodogx-host*.log | jq 'select(.["@l"] == "Error")'

# Count requests by method
cat logs/jumbodogx-host*.log | jq -r '.Method' | sort | uniq -c

# Extract timestamps and messages
cat logs/jumbodogx-host*.log | jq -r '"\(.["@t"]) - \(.["@mt"])"'
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

### Settings Backup & Restore

JumboDogX provides settings import/export functionality for easy backup and migration.

#### Exporting Settings

1. Access the Web UI at `http://localhost:5001`
2. Navigate to **Settings** → **Backup & Restore**
3. Click **Export Settings** button
4. Save the downloaded JSON file (e.g., `jumbodogx-settings-20260119-120000.json`)

The exported file contains all server configurations including HTTP, DNS, FTP, SMTP, POP3, DHCP, TFTP, and Proxy settings.

#### Importing Settings

1. Access the Web UI at `http://localhost:5001`
2. Navigate to **Settings** → **Backup & Restore**
3. Select a previously exported JSON file
4. Click **Import Settings** button
5. Restart the application to apply the imported settings

**Note**: After importing settings, you must restart the application for changes to take effect.

#### Use Cases

- **Backup**: Save current settings before making changes
- **Migration**: Move settings between different instances
- **Template**: Share configuration across development team
- **Recovery**: Restore previous working configuration

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
