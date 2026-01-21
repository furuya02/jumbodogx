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
public class ServerManager
{
    private readonly ILogger<ServerManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly LogService _logService;
    private readonly ISettingsService _settingsService;
    private readonly Dictionary<string, IServer> _servers;
    private readonly object _lock = new();

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

        // 初期サーバーを作成
        InitializeServers();
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
                    var httpServer = new HttpServer(httpLogger, vhost, settings.HttpServer, _settingsService);
                    var serverId = $"http:{vhost.Host}";
                    _servers[serverId] = httpServer;

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
