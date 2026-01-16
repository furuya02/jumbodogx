using Bjd9.Core.Abstractions;
using Bjd9.Servers.Http;
using Bjd9.Servers.Dns;
using Microsoft.Extensions.Logging;

namespace Bjd9.WebUI.Services;

/// <summary>
/// サーバーインスタンスのライフサイクル管理サービス
/// </summary>
public class ServerManager
{
    private readonly ILogger<ServerManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly LogService _logService;
    private readonly Dictionary<string, IServer> _servers;
    private readonly object _lock = new();

    public event EventHandler<ServerEventArgs>? ServerStateChanged;

    public ServerManager(ILogger<ServerManager> logger, ILoggerFactory loggerFactory, LogService logService)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _logService = logService;
        _servers = new Dictionary<string, IServer>();

        // 初期サーバーを作成
        InitializeServers();
    }

    private void InitializeServers()
    {
        lock (_lock)
        {
            // HTTP サーバー
            var httpLogger = _loggerFactory.CreateLogger<HttpServer>();
            var httpServer = new HttpServer(httpLogger, port: 8080);
            _servers["http"] = httpServer;

            // DNS サーバー
            var dnsLogger = _loggerFactory.CreateLogger<DnsServer>();
            var dnsServer = new DnsServer(dnsLogger, port: 5300);

            // テスト用DNSレコード追加
            dnsServer.AddRecord("example.com", "192.0.2.1");
            dnsServer.AddRecord("bjd9.local", "127.0.0.1");
            dnsServer.AddRecord("test.local", "192.168.1.100");

            _servers["dns"] = dnsServer;

            _logger.LogInformation("ServerManager initialized with {Count} servers", _servers.Count);
            _logService.AddLog(LogLevel.Information, "ServerManager", $"Initialized with {_servers.Count} servers");
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
