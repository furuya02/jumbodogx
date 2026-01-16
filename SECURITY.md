# Security Policy

## Supported Versions

JumboDogX is currently in active development. Security updates are provided for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| main    | :white_check_mark: |
| < 1.0   | :x:                |

**Note**: JumboDogX is designed for **local development and testing environments only**. It is NOT intended for production use or public-facing deployments.

## Security Considerations

### Development and Testing Only

JumboDogX is explicitly designed for:
- Local development environments
- Testing purposes
- Learning and experimentation

**DO NOT** use JumboDogX for:
- Production environments
- Public-facing servers
- Processing sensitive data
- Internet-exposed services

### Known Security Limitations

1. **No Security Audit**: This software has not undergone professional security auditing
2. **Development Focus**: Security features are minimal as this is a testing tool
3. **Sample Configurations**: Default settings prioritize ease of use over security
4. **No Warranty**: Provided "as-is" without security guarantees

## Reporting a Vulnerability

If you discover a security vulnerability in JumboDogX, please report it by:

1. **DO NOT** open a public issue
2. Send an email to: [hirauchi.shinichi@example.com]
3. Include:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if available)

### What to Expect

- **Acknowledgment**: Within 48 hours of report submission
- **Initial Assessment**: Within 7 days
- **Fix Timeline**: Depends on severity and complexity
- **Credit**: Security researchers will be credited (if desired)

### Disclosure Policy

- We follow responsible disclosure practices
- Vulnerabilities will be fixed before public disclosure
- Security advisories will be published via GitHub Security Advisories

## Security Best Practices for Users

When using JumboDogX, follow these guidelines:

### Required Actions

1. **Change Default Passwords**
   - Update the default password hash in `appsettings.json`
   - Use strong, unique passwords
   - Generate SHA-256 hashes for passwords:
     ```bash
     echo -n 'YourStrongPassword' | shasum -a 256
     ```

2. **Network Isolation**
   - Keep `BindAddress` as `127.0.0.1` (localhost)
   - Only change to `0.0.0.0` if absolutely necessary
   - Use firewall rules to restrict access

3. **Disable Unnecessary Features**
   - Keep `UseCgi` as `false` unless required
   - Keep `AllowWrite` as `false` for WebDAV unless needed
   - Disable unused server components

### Recommended Practices

- Run JumboDogX in isolated environments (VMs, containers)
- Never expose JumboDogX directly to the internet
- Use JumboDogX behind a reverse proxy if network access is required
- Regularly review server logs for suspicious activity
- Keep .NET SDK updated to the latest stable version
- Monitor for security updates via GitHub Releases

### What NOT to Do

- ❌ Use default/weak passwords
- ❌ Expose JumboDogX to the public internet
- ❌ Use for production workloads
- ❌ Process sensitive or personal data
- ❌ Run with elevated privileges unnecessarily
- ❌ Disable or bypass security features

## Security-Related Configuration

### Critical Settings in `appsettings.json`

```json
{
  "Jdx": {
    "HttpServer": {
      "BindAddress": "127.0.0.1",     // Localhost only (RECOMMENDED)
      "UseCgi": false,                 // CGI disabled (RECOMMENDED)
      "UseWebDav": true,
      "WebDavPaths": [
        {
          "AllowWrite": false          // Read-only (RECOMMENDED)
        }
      ],
      "UserList": [
        {
          "Password": "REPLACE_WITH_SECURE_SHA256_HASH"  // MUST CHANGE
        }
      ]
    }
  }
}
```

### Authentication and Authorization

- Default authentication uses SHA-256 password hashing
- No salt is applied (acceptable for testing environments only)
- For production-like testing, implement proper password hashing (bcrypt, Argon2)

### CGI and Script Execution

When enabling CGI (`"UseCgi": true`):
- ⚠️ **High Risk**: Potential for command injection
- Validate all user inputs
- Sanitize file paths and parameters
- Run CGI scripts with minimal privileges
- Consider using containerization for additional isolation

### WebDAV Write Access

When enabling WebDAV write (`"AllowWrite": true`):
- ⚠️ **Medium Risk**: Arbitrary file upload
- Require authentication for write operations
- Implement file type restrictions
- Set disk quota limits
- Monitor uploaded files

## Dependency Security

JumboDogX relies on the following frameworks:
- .NET 9 (Microsoft)
- ASP.NET Core 9 (Microsoft)

### Keeping Dependencies Secure

```bash
# Check for vulnerable packages
dotnet list package --vulnerable

# Update packages
dotnet restore
```

## Security Updates

Security updates will be announced via:
- GitHub Security Advisories
- GitHub Releases
- Repository README

Subscribe to repository notifications to stay informed.

## Contact

For security concerns, contact: [hirauchi.shinichi@example.com]

For general issues, use: https://github.com/furuya02/jumbodogx/issues

---

**Last Updated**: 2026-01-17
