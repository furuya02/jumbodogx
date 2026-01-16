# BJD9 - BlackJumboDog 9

English | [日本語](README.ja.md)

BJD9 is a cross-platform integrated server software, porting BlackJumboDog (BJD5) to .NET 9.

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
bjd9/
├── src/
│   ├── Bjd9.Core/              # Core library
│   ├── Bjd9.Servers.Http/      # HTTP server implementation
│   ├── Bjd9.Servers.Dns/       # DNS server implementation
│   ├── Bjd9.Host/              # Host application
│   └── Bjd9.WebUI/             # Web UI (Blazor Server)
├── tests/
│   ├── Bjd9.Tests.Core/        # Core unit tests
│   └── Bjd9.Tests.Servers.Http/ # HTTP server tests
└── docs/                        # Design documents
```

## Development Status

- [x] .NET 9 SDK installation
- [x] Project structure creation
- [x] Solution configuration
- [x] Core implementation (ServerBase, IServer abstraction)
- [x] HTTP server implementation (basic features)
- [x] DNS server implementation (basic features)
- [x] Web UI implementation (Dashboard, Logs, DNS Records management)
- [ ] SMTP/POP3 server implementation
- [ ] FTP server implementation
- [ ] DHCP server implementation
- [ ] Proxy server implementation

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
```

## Running

### CLI Version (Host Application)

```bash
cd src/Bjd9.Host
dotnet run
```

### Web UI Version

```bash
cd src/Bjd9.WebUI
dotnet run
```

Then, open http://localhost:5000 in your browser.

For detailed instructions, see [startup.md](startup.md).

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

## License

MIT License

Copyright (c) 2026 Hirauchi Shinichi (SIN)

See the [LICENSE](LICENSE) file for details.

## Related Links

- Original Project: BlackJumboDog Ver6.1.9
- .NET 9: https://dotnet.microsoft.com/
