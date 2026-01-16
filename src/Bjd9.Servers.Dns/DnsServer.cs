using System.Net;
using System.Net.Sockets;
using Bjd9.Core.Abstractions;
using Bjd9.Core.Network;
using Microsoft.Extensions.Logging;

namespace Bjd9.Servers.Dns;

/// <summary>
/// DNSサーバー（簡易実装：A レコードのみ）
/// </summary>
public class DnsServer : ServerBase
{
    private readonly int _port;
    private readonly Dictionary<string, string> _records;
    private ServerUdpListener? _listener;

    public DnsServer(ILogger<DnsServer> logger, int port = 5300) : base(logger)
    {
        _port = port;
        _records = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // テスト用の固定レコード
            ["example.com"] = "192.0.2.1",
            ["test.local"] = "192.168.1.100",
            ["bjd9.local"] = "127.0.0.1",
            ["localhost"] = "127.0.0.1"
        };
    }

    public override string Name => "DnsServer";
    public override ServerType Type => ServerType.Dns;
    public override int Port => _port;

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        // 既存のリスナーがあれば停止
        if (_listener != null)
        {
            try
            {
                await _listener.StopAsync(CancellationToken.None);
                _listener.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error stopping existing listener");
            }
        }

        _listener = new ServerUdpListener(_port, null, Logger);
        await _listener.StartAsync(cancellationToken);

        Logger.LogInformation("DNS Server listening on port {Port}", _port);

        // UDP リクエストを受信するループ
        _ = Task.Run(async () =>
        {
            while (!StopCts.Token.IsCancellationRequested)
            {
                try
                {
                    var (data, remoteEndPoint) = await _listener.ReceiveAsync(StopCts.Token);
                    Statistics.TotalConnections++;
                    Statistics.TotalBytesReceived += data.Length;

                    // リクエスト処理を非同期で実行
                    _ = Task.Run(async () =>
                    {
                        await HandleDnsQueryAsync(data, remoteEndPoint, StopCts.Token);
                    }, StopCts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error receiving DNS query");
                    Statistics.TotalErrors++;
                }
            }
        }, StopCts.Token);
    }

    protected override async Task StopListeningAsync(CancellationToken cancellationToken)
    {
        if (_listener != null)
        {
            await _listener.StopAsync(cancellationToken);
            _listener.Dispose();
            _listener = null;
        }
    }

    protected override Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        // UDP サーバーなので使用しない
        return Task.CompletedTask;
    }

    private async Task HandleDnsQueryAsync(byte[] data, EndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        var remoteAddress = remoteEndPoint.ToString() ?? "unknown";

        try
        {
            // DNS クエリをパース
            var query = DnsMessage.ParseQuery(data);
            Statistics.TotalRequests++;

            Logger.LogInformation("DNS query for {QueryName} (type={QueryType}) from {RemoteAddress}",
                query.QueryName, query.QueryType, remoteAddress);

            // A レコードクエリのみ処理（type=1）
            if (query.QueryType == 1)
            {
                string? ipAddress = null;

                // レコードを検索
                if (_records.TryGetValue(query.QueryName, out var recordIp))
                {
                    ipAddress = recordIp;
                }
                else
                {
                    // 見つからない場合はデフォルトIP
                    ipAddress = "0.0.0.0";
                    Logger.LogWarning("DNS record not found for {QueryName}, returning {DefaultIp}",
                        query.QueryName, ipAddress);
                }

                // DNS 応答を生成
                var response = query.CreateResponse(ipAddress);

                // 応答を送信
                if (_listener != null)
                {
                    await _listener.SendAsync(response, remoteEndPoint, cancellationToken);
                    Statistics.TotalBytesSent += response.Length;

                    Logger.LogInformation("DNS response sent: {QueryName} -> {IpAddress}",
                        query.QueryName, ipAddress);
                }
            }
            else
            {
                Logger.LogWarning("Unsupported DNS query type: {QueryType}", query.QueryType);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling DNS query from {RemoteAddress}", remoteAddress);
            Statistics.TotalErrors++;
        }
    }

    /// <summary>
    /// DNS レコードを追加
    /// </summary>
    public void AddRecord(string domainName, string ipAddress)
    {
        _records[domainName] = ipAddress;
        Logger.LogInformation("DNS record added: {DomainName} -> {IpAddress}", domainName, ipAddress);
    }

    /// <summary>
    /// DNS レコードを削除
    /// </summary>
    public void RemoveRecord(string domainName)
    {
        if (_records.Remove(domainName))
        {
            Logger.LogInformation("DNS record removed: {DomainName}", domainName);
        }
    }
}
