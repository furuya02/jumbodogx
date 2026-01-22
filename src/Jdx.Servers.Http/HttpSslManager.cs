using System;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Http;

/// <summary>
/// SSL/TLS証明書管理
/// </summary>
public class HttpSslManager
{
    private readonly ILogger _logger;
    private readonly X509Certificate2? _certificate;
    private readonly bool _isEnabled;

    public bool IsEnabled => _isEnabled;
    public X509Certificate2? Certificate => _certificate;

    public HttpSslManager(string? protocol, string? certificateFile, string? certificatePassword, ILogger logger)
    {
        _logger = logger;
        _isEnabled = false;

        // Protocolが"HTTPS"でない場合、SSL/TLSを無効化
        if (protocol != "HTTPS")
        {
            _logger.LogInformation("SSL/TLS disabled (Protocol is set to {Protocol})", protocol ?? "HTTP");
            return;
        }

        if (string.IsNullOrEmpty(certificateFile))
        {
            _logger.LogWarning("SSL/TLS cannot be enabled: Protocol is HTTPS but no certificate file specified");
            return;
        }

        if (!File.Exists(certificateFile))
        {
            _logger.LogError("SSL certificate file not found: {CertificateFile}", certificateFile);
            return;
        }

        try
        {
            _certificate = X509CertificateLoader.LoadPkcs12FromFile(certificateFile, certificatePassword);
            _isEnabled = true;
            _logger.LogInformation("SSL/TLS enabled with certificate: {Subject}", _certificate.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load SSL certificate from {CertificateFile}", certificateFile);
        }
    }

    /// <summary>
    /// SslStreamを作成（サーバー側）
    /// </summary>
    public SslStream CreateServerStream(Stream innerStream)
    {
        if (!_isEnabled || _certificate == null)
        {
            throw new InvalidOperationException("SSL is not enabled");
        }

        var sslStream = new SslStream(innerStream, false);
        return sslStream;
    }

    /// <summary>
    /// SSL/TLSハンドシェイクを実行（サーバー側）
    /// </summary>
    public async Task AuthenticateAsServerAsync(SslStream sslStream, CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || _certificate == null)
        {
            throw new InvalidOperationException("SSL is not enabled");
        }

        try
        {
            var sslOptions = new SslServerAuthenticationOptions
            {
                ServerCertificate = _certificate,
                ClientCertificateRequired = false,
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                CertificateRevocationCheckMode = X509RevocationMode.Online
            };

            await sslStream.AuthenticateAsServerAsync(sslOptions, cancellationToken);

            _logger.LogDebug("SSL/TLS handshake completed: {Protocol}, {CipherSuite}",
                sslStream.SslProtocol, sslStream.CipherAlgorithm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SSL/TLS handshake failed");
            throw;
        }
    }
}
