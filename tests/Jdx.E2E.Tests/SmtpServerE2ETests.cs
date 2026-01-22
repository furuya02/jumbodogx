using System.Net.Sockets;
using System.Text;
using Jdx.Core.Settings;
using Jdx.E2E.Tests.Fixtures;
using Jdx.Servers.Smtp;

namespace Jdx.E2E.Tests;

/// <summary>
/// SMTPサーバーのE2Eテスト
/// </summary>
public class SmtpServerE2ETests : IAsyncLifetime
{
    private readonly SmtpServer _server;
    private readonly int _testPort = 12525;
    private readonly string _testDomain = "test.local";
    private readonly string _mailboxPath;

    public SmtpServerE2ETests()
    {
        _mailboxPath = Path.Combine(Path.GetTempPath(), $"jdx-test-mailbox-{Guid.NewGuid()}");
        Directory.CreateDirectory(_mailboxPath);

        var settings = new SmtpServerSettings
        {
            Enabled = true,
            Port = _testPort,
            BindAddress = "127.0.0.1",
            DomainName = _testDomain,
            MaxConnections = 10,
            TimeOut = 30,
            SizeLimit = 1024,  // 1KB for testing
            BannerMessage = "$d SMTP Server Ready"
        };

        var logger = TestLoggerFactory.CreateNullLogger<SmtpServer>();
        _server = new SmtpServer(logger, settings);
    }

    public async Task InitializeAsync()
    {
        await _server.StartAsync(CancellationToken.None);
        await Task.Delay(100);  // サーバーが起動するまで待機
    }

    public async Task DisposeAsync()
    {
        await _server.StopAsync(CancellationToken.None);

        // テスト用ディレクトリを削除
        if (Directory.Exists(_mailboxPath))
        {
            try
            {
                Directory.Delete(_mailboxPath, recursive: true);
            }
            catch
            {
                // クリーンアップ失敗は無視
            }
        }
    }

    [Fact]
    public async Task Connect_ReceivesBanner()
    {
        // Arrange & Act
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", _testPort);

        using var stream = client.GetStream();
        var response = await ReadResponseAsync(stream);

        // Assert
        Assert.StartsWith("220", response);  // SMTP Ready code
    }

    [Fact]
    public async Task Ehlo_ReturnsOk()
    {
        // Arrange
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", _testPort);
        using var stream = client.GetStream();

        // Read banner
        await ReadResponseAsync(stream);

        // Act
        await SendCommandAsync(stream, $"EHLO client.{_testDomain}");
        var response = await ReadMultilineResponseAsync(stream);

        // Assert
        Assert.Contains("250", response);
    }

    [Fact]
    public async Task Helo_ReturnsOk()
    {
        // Arrange
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", _testPort);
        using var stream = client.GetStream();

        // Read banner
        await ReadResponseAsync(stream);

        // Act
        await SendCommandAsync(stream, $"HELO client.{_testDomain}");
        var response = await ReadResponseAsync(stream);

        // Assert
        Assert.StartsWith("250", response);
    }

    [Fact]
    public async Task Quit_ClosesConnection()
    {
        // Arrange
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", _testPort);
        using var stream = client.GetStream();

        // Read banner
        await ReadResponseAsync(stream);

        // Act
        await SendCommandAsync(stream, "QUIT");
        var response = await ReadResponseAsync(stream);

        // Assert
        Assert.StartsWith("221", response);  // Service closing
    }

    [Fact]
    public async Task Noop_ReturnsOk()
    {
        // Arrange
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", _testPort);
        using var stream = client.GetStream();

        // Read banner and send HELO
        await ReadResponseAsync(stream);
        await SendCommandAsync(stream, $"HELO client.{_testDomain}");
        await ReadResponseAsync(stream);

        // Act
        await SendCommandAsync(stream, "NOOP");
        var response = await ReadResponseAsync(stream);

        // Assert
        Assert.StartsWith("250", response);
    }

    [Fact]
    public async Task Rset_ReturnsOk()
    {
        // Arrange
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", _testPort);
        using var stream = client.GetStream();

        // Read banner and send HELO
        await ReadResponseAsync(stream);
        await SendCommandAsync(stream, $"HELO client.{_testDomain}");
        await ReadResponseAsync(stream);

        // Act
        await SendCommandAsync(stream, "RSET");
        var response = await ReadResponseAsync(stream);

        // Assert
        Assert.StartsWith("250", response);
    }

    [Fact]
    public async Task UnknownCommand_ReturnsError()
    {
        // Arrange
        using var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", _testPort);
        using var stream = client.GetStream();

        // Read banner
        await ReadResponseAsync(stream);

        // Act
        await SendCommandAsync(stream, "INVALIDCMD");
        var response = await ReadResponseAsync(stream);

        // Assert
        // 500 or 502 for unknown command
        Assert.True(response.StartsWith("500") || response.StartsWith("502"),
            $"Expected 500 or 502 but got: {response}");
    }

    [Fact]
    public Task ServerStatus_IsRunning()
    {
        // Assert
        Assert.Equal(Jdx.Core.Abstractions.ServerStatus.Running, _server.Status);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task MultipleConnections_AllSucceed()
    {
        // Arrange
        var tasks = new List<Task<bool>>();

        // Act - 同時に3つの接続
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(ConnectAndReadBannerAsync());
        }
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result => Assert.True(result));
    }

    private async Task<bool> ConnectAndReadBannerAsync()
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", _testPort);
            using var stream = client.GetStream();
            var response = await ReadResponseAsync(stream);
            return response.StartsWith("220");
        }
        catch
        {
            return false;
        }
    }

    private static async Task SendCommandAsync(NetworkStream stream, string command)
    {
        var bytes = Encoding.ASCII.GetBytes(command + "\r\n");
        await stream.WriteAsync(bytes);
        await stream.FlushAsync();
    }

    private static async Task<string> ReadResponseAsync(NetworkStream stream)
    {
        var buffer = new byte[1024];
        var sb = new StringBuilder();

        stream.ReadTimeout = 5000;  // 5秒タイムアウト

        try
        {
            int bytesRead = await stream.ReadAsync(buffer);
            if (bytesRead > 0)
            {
                sb.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
            }
        }
        catch (IOException)
        {
            // タイムアウト
        }

        return sb.ToString().TrimEnd();
    }

    private static async Task<string> ReadMultilineResponseAsync(NetworkStream stream)
    {
        var sb = new StringBuilder();
        var buffer = new byte[1024];

        stream.ReadTimeout = 5000;

        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0)
                {
                    break;
                }

                var line = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                sb.Append(line);

                // Check if this is the last line (starts with "250 " not "250-")
                var lines = sb.ToString().Split('\n');
                var lastLine = lines.LastOrDefault(l => !string.IsNullOrWhiteSpace(l)) ?? "";
                if (lastLine.Length >= 4 && lastLine[3] == ' ')
                {
                    break;
                }
            }
        }
        catch (IOException)
        {
            // タイムアウト
        }

        return sb.ToString().TrimEnd();
    }
}
