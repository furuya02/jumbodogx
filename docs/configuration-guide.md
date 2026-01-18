# JumboDogX Configuration Guide

This guide provides basic configuration examples for each server type in JumboDogX.

## Configuration File Location

The main configuration file is located at:
- **Web UI**: `src/Jdx.WebUI/appsettings.json`
- **CLI**: `src/Jdx.Host/appsettings.json`

## HTTP Server Configuration

### Basic HTTP Server

```json
{
  "HttpServer": {
    "Enabled": true,
    "Port": 8080,
    "BindAddress": "127.0.0.1",
    "DocumentRoot": "./wwwroot",
    "DefaultDocument": "index.html",
    "Timeout": 30,
    "MaxConnections": 100
  }
}
```

### HTTP Server with Virtual Hosts

```json
{
  "HttpServer": {
    "Enabled": true,
    "Port": 8080,
    "VirtualHosts": [
      {
        "HostName": "site1.local",
        "DocumentRoot": "./sites/site1",
        "DefaultDocument": "index.html"
      },
      {
        "HostName": "site2.local",
        "DocumentRoot": "./sites/site2",
        "DefaultDocument": "index.html"
      }
    ]
  }
}
```

### HTTP Server with ACL (Access Control)

```json
{
  "HttpServer": {
    "Enabled": true,
    "Port": 8080,
    "EnableAcl": 1,
    "AclList": [
      {
        "Name": "Local Network",
        "Address": "192.168.1.0/24"
      },
      {
        "Name": "Localhost",
        "Address": "127.0.0.1"
      }
    ]
  }
}
```

**ACL Modes**:
- `0`: Disabled (allow all)
- `1`: Allow list (only listed IPs allowed)
- `2`: Deny list (listed IPs denied)

## DNS Server Configuration

### Basic DNS Server

```json
{
  "DnsServer": {
    "Enabled": true,
    "Port": 53,
    "UseRecursion": false,
    "SoaExpire": 86400,
    "DomainList": [
      {
        "Name": "example.local.",
        "IsAuthority": true
      }
    ],
    "ResourceList": [
      {
        "Name": "www.example.local.",
        "Type": "A",
        "Data": "192.168.1.100"
      },
      {
        "Name": "mail.example.local.",
        "Type": "A",
        "Data": "192.168.1.101"
      }
    ]
  }
}
```

### DNS Server with Multiple Record Types

```json
{
  "DnsServer": {
    "Enabled": true,
    "Port": 53,
    "ResourceList": [
      {
        "Name": "www.example.local.",
        "Type": "A",
        "Data": "192.168.1.100"
      },
      {
        "Name": "example.local.",
        "Type": "MX",
        "Data": "10 mail.example.local."
      },
      {
        "Name": "www.example.local.",
        "Type": "AAAA",
        "Data": "2001:db8::1"
      },
      {
        "Name": "alias.example.local.",
        "Type": "CNAME",
        "Data": "www.example.local."
      }
    ]
  }
}
```

## SMTP Server Configuration

### Basic SMTP Server

```json
{
  "SmtpServer": {
    "Enabled": true,
    "Port": 25,
    "BindAddress": "127.0.0.1",
    "MaxConnections": 50,
    "Timeout": 60,
    "EnableAuth": true,
    "Users": [
      {
        "Name": "user1",
        "Password": "hashed_password_here"
      }
    ],
    "MailDir": "./mail"
  }
}
```

### SMTP Server with Authentication

```json
{
  "SmtpServer": {
    "Enabled": true,
    "Port": 587,
    "EnableAuth": true,
    "RequireAuth": true,
    "Users": [
      {
        "Name": "sender@example.local",
        "Password": "password_hash"
      }
    ]
  }
}
```

## POP3 Server Configuration

### Basic POP3 Server

```json
{
  "Pop3Server": {
    "Enabled": true,
    "Port": 110,
    "BindAddress": "127.0.0.1",
    "MaxConnections": 50,
    "Timeout": 60,
    "Users": [
      {
        "Name": "user1",
        "Password": "hashed_password_here",
        "MailDir": "./mail/user1"
      }
    ]
  }
}
```

## FTP Server Configuration

### Basic FTP Server

```json
{
  "FtpServer": {
    "Enabled": true,
    "Port": 21,
    "BindAddress": "127.0.0.1",
    "MaxConnections": 50,
    "Timeout": 300,
    "RootDir": "./ftp",
    "Users": [
      {
        "Name": "ftpuser",
        "Password": "hashed_password_here",
        "HomeDir": "/ftpuser"
      }
    ]
  }
}
```

### FTP Server with Anonymous Access

```json
{
  "FtpServer": {
    "Enabled": true,
    "Port": 21,
    "AllowAnonymous": true,
    "AnonymousDir": "./ftp/public",
    "AllowUpload": false
  }
}
```

## DHCP Server Configuration

### Basic DHCP Server

```json
{
  "DhcpServer": {
    "Enabled": true,
    "Port": 67,
    "BindAddress": "192.168.1.1",
    "StartIp": "192.168.1.100",
    "EndIp": "192.168.1.200",
    "MaskIp": "255.255.255.0",
    "GwIp": "192.168.1.1",
    "DnsIp": "192.168.1.1",
    "LeaseTime": 86400
  }
}
```

### DHCP Server with Reserved Addresses

```json
{
  "DhcpServer": {
    "Enabled": true,
    "Port": 67,
    "StartIp": "192.168.1.100",
    "EndIp": "192.168.1.200",
    "Reservations": [
      {
        "MacAddress": "00:11:22:33:44:55",
        "IpAddress": "192.168.1.50"
      }
    ]
  }
}
```

## TFTP Server Configuration

### Basic TFTP Server

```json
{
  "TftpServer": {
    "Enabled": true,
    "Port": 69,
    "BindAddress": "127.0.0.1",
    "RootDir": "./tftp",
    "AllowWrite": false,
    "MaxFileSize": 10485760
  }
}
```

## Proxy Server Configuration

### Basic HTTP Proxy

```json
{
  "ProxyServer": {
    "Enabled": true,
    "Port": 8888,
    "BindAddress": "127.0.0.1",
    "MaxConnections": 100,
    "EnableCache": true,
    "CacheDir": "./cache",
    "CacheSize": 104857600
  }
}
```

### Proxy Server with URL Filtering

```json
{
  "ProxyServer": {
    "Enabled": true,
    "Port": 8888,
    "EnableFilter": true,
    "BlockedDomains": [
      "*.ads.example.com",
      "tracking.example.com"
    ],
    "AllowedDomains": [
      "*.safe.example.com"
    ]
  }
}
```

## Common Configuration Patterns

### Multiple Servers

You can enable multiple servers simultaneously:

```json
{
  "HttpServer": {
    "Enabled": true,
    "Port": 8080
  },
  "DnsServer": {
    "Enabled": true,
    "Port": 53
  },
  "DhcpServer": {
    "Enabled": true,
    "Port": 67
  }
}
```

### Security Best Practices

1. **Bind to localhost for local testing**:
   ```json
   {
     "BindAddress": "127.0.0.1"
   }
   ```

2. **Use strong passwords** (SHA-256 hash):
   ```bash
   echo -n 'YourPassword' | shasum -a 256
   ```

3. **Limit connections**:
   ```json
   {
     "MaxConnections": 100
   }
   ```

4. **Enable ACL for HTTP**:
   ```json
   {
     "EnableAcl": 1,
     "AclList": [
       {"Name": "Local", "Address": "127.0.0.1"}
     ]
   }
   ```

5. **Disable dangerous features** in production:
   ```json
   {
     "UseCgi": false,
     "AllowWrite": false
   }
   ```

## Environment-Specific Configuration

### Development

```json
{
  "HttpServer": {
    "Port": 8080,
    "BindAddress": "127.0.0.1",
    "UseCgi": true,
    "AllowWrite": true
  }
}
```

### Testing

```json
{
  "HttpServer": {
    "Port": 8080,
    "BindAddress": "0.0.0.0",
    "EnableAcl": 1,
    "AclList": [
      {"Name": "Test Network", "Address": "192.168.1.0/24"}
    ]
  }
}
```

### Production (Local Network Only)

```json
{
  "HttpServer": {
    "Port": 80,
    "BindAddress": "192.168.1.100",
    "UseCgi": false,
    "AllowWrite": false,
    "EnableAcl": 1,
    "AclList": [
      {"Name": "Internal", "Address": "192.168.1.0/24"}
    ]
  }
}
```

## Troubleshooting

### Port Already in Use

If you see "port already in use" errors:
- Change the port number
- Stop other services using the same port
- Use `netstat` or `lsof` to find the conflicting process

### Permission Denied

Ports below 1024 require elevated privileges:
- Use ports above 1024 (e.g., 8080 instead of 80)
- Run with sudo (not recommended for development)

### Cannot Access from Network

If the server is not accessible:
- Check `BindAddress` (use `0.0.0.0` for all interfaces)
- Verify firewall settings
- Check ACL configuration

## Additional Resources

- [README.md](../README.md) - Project overview
- [startup.md](../startup.md) - Detailed startup instructions
- [SECURITY.md](../SECURITY.md) - Security considerations
