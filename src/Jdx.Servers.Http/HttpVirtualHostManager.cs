using System;
using System.Collections.Generic;
using System.Linq;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Http;

/// <summary>
/// Virtual Host管理
/// Hostヘッダーに基づいて適切なDocumentRootを選択する
/// </summary>
public class HttpVirtualHostManager
{
    private readonly ILogger _logger;
    private readonly List<VirtualHostEntry> _virtualHosts;
    private readonly string _defaultDocumentRoot;

    public HttpVirtualHostManager(HttpServerSettings settings, ILogger logger)
    {
        _logger = logger;
        _virtualHosts = settings.VirtualHosts ?? new List<VirtualHostEntry>();
        _defaultDocumentRoot = settings.DocumentRoot;
    }

    /// <summary>
    /// Hostヘッダーに基づいてDocumentRootを解決
    /// </summary>
    /// <param name="hostHeader">Hostヘッダーの値（例: example.com:8080）</param>
    /// <param name="localAddress">ローカルIPアドレス</param>
    /// <param name="localPort">ローカルポート</param>
    /// <returns>解決されたDocumentRoot</returns>
    public string ResolveDocumentRoot(string? hostHeader, string localAddress, int localPort)
    {
        if (_virtualHosts.Count == 0)
        {
            // Virtual Host設定が無い場合はデフォルトを返す
            return _defaultDocumentRoot;
        }

        if (string.IsNullOrEmpty(hostHeader))
        {
            _logger.LogDebug("No Host header, using default DocumentRoot");
            return _defaultDocumentRoot;
        }

        // Hostヘッダーを正規化（ポート番号を含める）
        var normalizedHost = NormalizeHost(hostHeader, localPort);

        // 1回目: ホスト名で検索
        var match = FindVirtualHost(normalizedHost);
        if (match != null)
        {
            _logger.LogDebug("Virtual host matched by hostname: {Host} -> {DocumentRoot}",
                normalizedHost, match.DocumentRoot);
            return match.DocumentRoot;
        }

        // 2回目: IPアドレス:ポートで検索
        var ipHost = $"{localAddress}:{localPort}";
        match = FindVirtualHost(ipHost);
        if (match != null)
        {
            _logger.LogDebug("Virtual host matched by IP: {IpHost} -> {DocumentRoot}",
                ipHost, match.DocumentRoot);
            return match.DocumentRoot;
        }

        // マッチしない場合はデフォルト
        _logger.LogDebug("No virtual host matched, using default DocumentRoot");
        return _defaultDocumentRoot;
    }

    /// <summary>
    /// Virtual Host設定を検索
    /// </summary>
    private VirtualHostEntry? FindVirtualHost(string host)
    {
        return _virtualHosts.FirstOrDefault(vh =>
            vh.Host.Equals(host, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Hostヘッダーを正規化（ポート番号を含める）
    /// </summary>
    private string NormalizeHost(string host, int defaultPort)
    {
        // ポート番号が含まれていない場合は追加
        if (!host.Contains(':'))
        {
            return $"{host}:{defaultPort}";
        }

        return host;
    }

    /// <summary>
    /// 指定されたホストに対応するSSL証明書情報を取得
    /// </summary>
    public (string? certificateFile, string? certificatePassword) GetCertificate(string? hostHeader, string localAddress, int localPort)
    {
        if (_virtualHosts.Count == 0 || string.IsNullOrEmpty(hostHeader))
        {
            return (null, null);
        }

        var normalizedHost = NormalizeHost(hostHeader, localPort);
        var match = FindVirtualHost(normalizedHost);

        if (match != null && !string.IsNullOrEmpty(match.CertificateFile))
        {
            return (match.CertificateFile, match.CertificatePassword);
        }

        // IPアドレスでも試す
        var ipHost = $"{localAddress}:{localPort}";
        match = FindVirtualHost(ipHost);

        if (match != null && !string.IsNullOrEmpty(match.CertificateFile))
        {
            return (match.CertificateFile, match.CertificatePassword);
        }

        return (null, null);
    }
}
