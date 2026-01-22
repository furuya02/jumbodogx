using Jdx.Core.Settings;
using Jdx.E2E.Tests.Fixtures;
using Jdx.Servers.Http;

namespace Jdx.E2E.Tests;

/// <summary>
/// HTTPサーバーのE2Eテスト
/// </summary>
public class HttpServerE2ETests : IAsyncLifetime
{
    private readonly TestSettingsService _settingsService;
    private readonly HttpServer _server;
    private readonly HttpClient _client;
    private readonly string _testDocRoot;
    private readonly int _testPort = 18080;

    public HttpServerE2ETests()
    {
        _testDocRoot = Path.Combine(Path.GetTempPath(), $"jdx-test-www-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDocRoot);

        _settingsService = new TestSettingsService();
        _settingsService.ConfigureHttpServer(settings =>
        {
            settings.Port = _testPort;
            settings.BindAddress = "127.0.0.1";
            settings.DocumentRoot = _testDocRoot;
            settings.WelcomeFileName = "index.html";
            settings.MaxConnections = 10;
        });

        var logger = TestLoggerFactory.CreateNullLogger<HttpServer>();
        _server = new HttpServer(logger, _settingsService);

        _client = new HttpClient
        {
            BaseAddress = new Uri($"http://127.0.0.1:{_testPort}"),
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public async Task InitializeAsync()
    {
        // テスト用のindex.htmlを作成
        var indexPath = Path.Combine(_testDocRoot, "index.html");
        await File.WriteAllTextAsync(indexPath, "<html><body>Hello, JumboDogX!</body></html>");

        // サーバーを起動
        await _server.StartAsync(CancellationToken.None);

        // サーバーが起動するまで少し待機
        await Task.Delay(100);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _server.StopAsync(CancellationToken.None);

        // テスト用ディレクトリを削除
        if (Directory.Exists(_testDocRoot))
        {
            try
            {
                Directory.Delete(_testDocRoot, recursive: true);
            }
            catch
            {
                // クリーンアップ失敗は無視
            }
        }
    }

    [Fact]
    public async Task GetIndexHtml_ReturnsSuccessAndContent()
    {
        // Arrange - InitializeAsyncで設定済み

        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success status code but got {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hello, JumboDogX!", content);
    }

    [Fact]
    public async Task GetIndexHtml_ReturnsCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task GetNonExistentFile_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/nonexistent.html");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStaticTextFile_ReturnsContent()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDocRoot, "test.txt");
        await File.WriteAllTextAsync(testFilePath, "This is a test file.");

        // Act
        var response = await _client.GetAsync("/test.txt");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("This is a test file.", content);
    }

    [Fact]
    public async Task HeadRequest_ReturnsHeadersWithoutBody()
    {
        // Act
        var request = new HttpRequestMessage(HttpMethod.Head, "/");
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Empty(content);  // HEADリクエストはボディを返さない
        Assert.True(response.Content.Headers.ContentLength > 0);  // Content-Lengthは設定されている
    }

    [Fact]
    public async Task MultipleRequests_AllSucceed()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - 同時に5つのリクエストを送信
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_client.GetAsync("/"));
        }
        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            Assert.True(response.IsSuccessStatusCode);
        }
    }

    [Fact]
    public async Task GetSubdirectoryIndex_ReturnsContent()
    {
        // Arrange
        var subDir = Path.Combine(_testDocRoot, "subdir");
        Directory.CreateDirectory(subDir);
        await File.WriteAllTextAsync(Path.Combine(subDir, "index.html"), "<html><body>Subdir content</body></html>");

        // Act
        var response = await _client.GetAsync("/subdir/");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Subdir content", content);
    }

    [Fact]
    public async Task ServerHeader_IsPresent()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.Headers.Contains("Server"));
    }
}
