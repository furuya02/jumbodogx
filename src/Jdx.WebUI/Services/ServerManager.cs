using Jdx.Core.Abstractions;
using Jdx.Core.Settings;
using Jdx.Servers.Http;
using Jdx.Servers.Dns;
using Jdx.Servers.Ftp;
using Microsoft.Extensions.Logging;

namespace Jdx.WebUI.Services;

/// <summary>
/// サーバーインスタンスのライフサイクル管理サービス
/// </summary>
public class ServerManager : IDisposable
{
    private readonly ILogger<ServerManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly LogService _logService;
    private readonly ISettingsService _settingsService;
    private readonly Dictionary<string, IServer> _servers;
    private readonly Dictionary<string, VirtualHostInfo> _virtualHostInfos; // VirtualHostの情報を保持
    private readonly object _lock = new();
    private bool _disposed;

    public event EventHandler<ServerEventArgs>? ServerStateChanged;

    public ServerManager(
        ILogger<ServerManager> logger,
        ILoggerFactory loggerFactory,
        LogService logService,
        ISettingsService settingsService)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _logService = logService;
        _settingsService = settingsService;
        _servers = new Dictionary<string, IServer>();
        _virtualHostInfos = new Dictionary<string, VirtualHostInfo>();

        // 初期サーバーを作成
        InitializeServers();

        // 設定変更イベントを購読
        _settingsService.SettingsChanged += OnSettingsChanged;
    }

    private void InitializeServers()
    {
        lock (_lock)
        {
            var settings = _settingsService.GetSettings();

            // HTTP VirtualHosts サーバー（各VirtualHostを個別のHttpServerインスタンスとして作成）
            if (settings.HttpServer.VirtualHosts != null && settings.HttpServer.VirtualHosts.Count > 0)
            {
                foreach (var vhost in settings.HttpServer.VirtualHosts)
                {
                    var port = vhost.GetPort();
                    if (port == 0)
                    {
                        _logger.LogWarning("VirtualHost {Host} has invalid port, skipping", vhost.Host);
                        continue;
                    }

                    var httpLogger = _loggerFactory.CreateLogger<HttpServer>();
                    var httpServer = new HttpServer(httpLogger, vhost, settings.HttpServer, _settingsService, _logService.AddLog);
                    var serverId = $"http:{vhost.Host}";
                    _servers[serverId] = httpServer;

                    // VirtualHost情報を保存（変更検出用）
                    _virtualHostInfos[serverId] = new VirtualHostInfo(vhost.Host, vhost.BindAddress ?? "0.0.0.0");

                    _logger.LogInformation("HTTP VirtualHost server registered: {ServerId}", serverId);
                }
            }

            // FTP サーバー（SettingsService注入）
            var ftpLogger = _loggerFactory.CreateLogger<FtpServer>();
            var ftpServer = new FtpServer(ftpLogger, _settingsService);
            _servers["ftp"] = ftpServer;

            // DNS サーバー（DnsServerSettings注入）
            var dnsLogger = _loggerFactory.CreateLogger<DnsServer>();
            var dnsServer = new DnsServer(dnsLogger, settings.DnsServer);
            _servers["dns"] = dnsServer;

            _logger.LogInformation("ServerManager initialized with {Count} servers", _servers.Count);
            _logService.AddLog(LogLevel.Information, "ServerManager", $"Initialized with {_servers.Count} servers");

            // 設定で有効になっているサーバーを自動起動
            Task.Run(async () =>
            {
                try
                {
                    // VirtualHostsの自動起動
                    if (settings.HttpServer.VirtualHosts != null)
                    {
                        foreach (var vhost in settings.HttpServer.VirtualHosts)
                        {
                            if (vhost.Enabled && vhost.GetPort() > 0)
                            {
                                var serverId = $"http:{vhost.Host}";
                                await StartServerAsync(serverId);
                            }
                        }
                    }

                    if (settings.FtpServer.Enabled)
                    {
                        await StartServerAsync("ftp");
                    }
                    if (settings.DnsServer.Enabled)
                    {
                        await StartServerAsync("dns");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error auto-starting servers");
                }
            });
        }
    }

    public IEnumerable<IServer> GetAllServers()
    {
        lock (_lock)
        {
            return _servers.Values.ToList();
        }
    }

    public IServer? GetServer(string serverId)
    {
        lock (_lock)
        {
            return _servers.TryGetValue(serverId, out var server) ? server : null;
        }
    }

    public string? GetServerId(IServer server)
    {
        lock (_lock)
        {
            foreach (var kvp in _servers)
            {
                if (kvp.Value == server)
                {
                    return kvp.Key;
                }
            }
            return null;
        }
    }

    public async Task StartServerAsync(string serverId, CancellationToken cancellationToken = default)
    {
        IServer? server;
        lock (_lock)
        {
            if (!_servers.TryGetValue(serverId, out server))
            {
                throw new InvalidOperationException($"Server {serverId} not found");
            }
        }

        if (server.Status == ServerStatus.Running)
        {
            _logger.LogWarning("Server {ServerId} is already running", serverId);
            _logService.AddLog(LogLevel.Warning, "ServerManager", $"Server {serverId} is already running");
            return;
        }

        _logService.AddLog(LogLevel.Information, "ServerManager", $"Starting server {serverId} ({server.Name})...");
        await server.StartAsync(cancellationToken);
        _logger.LogInformation("Server {ServerId} started", serverId);
        _logService.AddLog(LogLevel.Information, "ServerManager", $"Server {serverId} ({server.Name}) started successfully on port {server.Port}");

        OnServerStateChanged(new ServerEventArgs(serverId, server.Status));
    }

    public async Task StopServerAsync(string serverId, CancellationToken cancellationToken = default)
    {
        IServer? server;
        lock (_lock)
        {
            if (!_servers.TryGetValue(serverId, out server))
            {
                throw new InvalidOperationException($"Server {serverId} not found");
            }
        }

        if (server.Status == ServerStatus.Stopped)
        {
            _logger.LogWarning("Server {ServerId} is already stopped", serverId);
            _logService.AddLog(LogLevel.Warning, "ServerManager", $"Server {serverId} is already stopped");
            return;
        }

        _logService.AddLog(LogLevel.Information, "ServerManager", $"Stopping server {serverId} ({server.Name})...");
        await server.StopAsync(cancellationToken);
        _logger.LogInformation("Server {ServerId} stopped", serverId);
        _logService.AddLog(LogLevel.Information, "ServerManager", $"Server {serverId} ({server.Name}) stopped successfully");

        OnServerStateChanged(new ServerEventArgs(serverId, server.Status));
    }

    public async Task StartAllServersAsync(CancellationToken cancellationToken = default)
    {
        var servers = GetAllServers().ToList();
        foreach (var server in servers)
        {
            try
            {
                if (server.Status != ServerStatus.Running)
                {
                    await server.StartAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start server {ServerName}", server.Name);
                _logService.AddLog(LogLevel.Error, "ServerManager", $"Failed to start server {server.Name}: {ex.Message}");
            }
        }

        OnServerStateChanged(new ServerEventArgs("all", ServerStatus.Running));
    }

    public async Task StopAllServersAsync(CancellationToken cancellationToken = default)
    {
        var servers = GetAllServers().ToList();
        foreach (var server in servers)
        {
            try
            {
                if (server.Status != ServerStatus.Stopped)
                {
                    await server.StopAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop server {ServerName}", server.Name);
                _logService.AddLog(LogLevel.Error, "ServerManager", $"Failed to stop server {server.Name}: {ex.Message}");
            }
        }

        OnServerStateChanged(new ServerEventArgs("all", ServerStatus.Stopped));
    }

    public DnsServer? GetDnsServer()
    {
        return GetServer("dns") as DnsServer;
    }

    protected virtual void OnServerStateChanged(ServerEventArgs e)
    {
        ServerStateChanged?.Invoke(this, e);
    }

    private void OnSettingsChanged(object? sender, ApplicationSettings settings)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await SyncVirtualHostsAsync(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing VirtualHosts after settings change");
            }
        });
    }

    private async Task SyncVirtualHostsAsync(ApplicationSettings settings)
    {
        var newVirtualHosts = settings.HttpServer.VirtualHosts ?? new List<VirtualHostEntry>();

        // 現在のHTTPサーバーIDを取得
        List<string> currentHttpServerIds;
        lock (_lock)
        {
            currentHttpServerIds = _servers.Keys.Where(k => k.StartsWith("http:")).ToList();
        }

        // 新しいVirtualHostsのサーバーIDを生成
        var newServerIds = newVirtualHosts
            .Where(v => v.GetPort() > 0)
            .Select(v => $"http:{v.Host}")
            .ToHashSet();

        // 削除されたVirtualHostsを検出して停止・削除
        foreach (var serverId in currentHttpServerIds)
        {
            if (!newServerIds.Contains(serverId))
            {
                await RemoveVirtualHostServerAsync(serverId);
            }
        }

        // 新規または変更されたVirtualHostsを処理
        foreach (var vhost in newVirtualHosts)
        {
            var port = vhost.GetPort();
            if (port == 0) continue;

            var serverId = $"http:{vhost.Host}";
            var bindAddress = vhost.BindAddress ?? "0.0.0.0";

            bool serverExists;
            bool needsRecreate = false;
            bool wasRunning = false;

            lock (_lock)
            {
                serverExists = _servers.ContainsKey(serverId);

                if (serverExists && _virtualHostInfos.TryGetValue(serverId, out var info))
                {
                    // HostまたはBindAddressが変更された場合は再作成が必要
                    if (info.Host != vhost.Host || info.BindAddress != bindAddress)
                    {
                        needsRecreate = true;
                        wasRunning = _servers[serverId].Status == ServerStatus.Running;
                    }
                }
            }

            if (!serverExists)
            {
                // 新規追加
                await AddVirtualHostServerAsync(vhost, settings.HttpServer);
            }
            else if (needsRecreate)
            {
                // ポートまたはBindAddressが変更された場合は再作成
                await RecreateVirtualHostServerAsync(serverId, vhost, settings.HttpServer, wasRunning);
            }
        }

        // 状態変更を通知
        OnServerStateChanged(new ServerEventArgs("virtualhost-sync", ServerStatus.Running));
    }

    private async Task AddVirtualHostServerAsync(VirtualHostEntry vhost, HttpServerSettings parentSettings)
    {
        var serverId = $"http:{vhost.Host}";
        var bindAddress = vhost.BindAddress ?? "0.0.0.0";

        lock (_lock)
        {
            if (_servers.ContainsKey(serverId))
            {
                _logger.LogWarning("Server {ServerId} already exists, skipping add", serverId);
                return;
            }

            var httpLogger = _loggerFactory.CreateLogger<HttpServer>();
            var httpServer = new HttpServer(httpLogger, vhost, parentSettings, _settingsService, _logService.AddLog);
            _servers[serverId] = httpServer;
            _virtualHostInfos[serverId] = new VirtualHostInfo(vhost.Host, bindAddress);

            _logger.LogInformation("HTTP VirtualHost server added: {ServerId}", serverId);
            _logService.AddLog(LogLevel.Information, "ServerManager", $"VirtualHost {serverId} added");
        }

        // 有効な場合は自動起動
        if (vhost.Enabled)
        {
            try
            {
                await StartServerAsync(serverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start new VirtualHost {ServerId}", serverId);
            }
        }
    }

    private async Task RemoveVirtualHostServerAsync(string serverId)
    {
        IServer? server;
        lock (_lock)
        {
            if (!_servers.TryGetValue(serverId, out server))
            {
                return;
            }
        }

        // サーバーを停止
        if (server.Status == ServerStatus.Running)
        {
            try
            {
                await StopServerAsync(serverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop VirtualHost {ServerId} before removal", serverId);
            }
        }

        // サーバーを削除
        lock (_lock)
        {
            _servers.Remove(serverId);
            _virtualHostInfos.Remove(serverId);
            server.Dispose();

            _logger.LogInformation("HTTP VirtualHost server removed: {ServerId}", serverId);
            _logService.AddLog(LogLevel.Information, "ServerManager", $"VirtualHost {serverId} removed");
        }
    }

    private async Task RecreateVirtualHostServerAsync(string serverId, VirtualHostEntry vhost, HttpServerSettings parentSettings, bool wasRunning)
    {
        _logger.LogInformation("Recreating VirtualHost server {ServerId} due to configuration change", serverId);
        _logService.AddLog(LogLevel.Information, "ServerManager", $"Recreating VirtualHost {serverId} due to configuration change");

        // 古いサーバーを削除
        await RemoveVirtualHostServerAsync(serverId);

        // 新しいサーバーを作成
        var newServerId = $"http:{vhost.Host}";
        var bindAddress = vhost.BindAddress ?? "0.0.0.0";

        lock (_lock)
        {
            var httpLogger = _loggerFactory.CreateLogger<HttpServer>();
            var httpServer = new HttpServer(httpLogger, vhost, parentSettings, _settingsService, _logService.AddLog);
            _servers[newServerId] = httpServer;
            _virtualHostInfos[newServerId] = new VirtualHostInfo(vhost.Host, bindAddress);

            _logger.LogInformation("HTTP VirtualHost server recreated: {ServerId}", newServerId);
        }

        // 元々起動中だった場合は再起動
        if (wasRunning || vhost.Enabled)
        {
            try
            {
                await StartServerAsync(newServerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart recreated VirtualHost {ServerId}", newServerId);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _settingsService.SettingsChanged -= OnSettingsChanged;

        lock (_lock)
        {
            foreach (var server in _servers.Values)
            {
                try
                {
                    server.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing server {ServerName}", server.Name);
                }
            }
            _servers.Clear();
            _virtualHostInfos.Clear();
        }

        _disposed = true;
    }
}

/// <summary>
/// VirtualHostの情報を保持するクラス（変更検出用）
/// </summary>
public class VirtualHostInfo
{
    public string Host { get; }
    public string BindAddress { get; }

    public VirtualHostInfo(string host, string bindAddress)
    {
        Host = host;
        BindAddress = bindAddress;
    }
}

public class ServerEventArgs : EventArgs
{
    public string ServerId { get; }
    public ServerStatus Status { get; }

    public ServerEventArgs(string serverId, ServerStatus status)
    {
        ServerId = serverId;
        Status = status;
    }
}
