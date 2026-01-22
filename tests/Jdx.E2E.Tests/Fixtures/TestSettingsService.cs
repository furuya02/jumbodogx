using Jdx.Core.Settings;

namespace Jdx.E2E.Tests.Fixtures;

/// <summary>
/// E2Eテスト用のSettingsService実装
/// </summary>
public class TestSettingsService : ISettingsService
{
    private ApplicationSettings _settings;

    public TestSettingsService()
    {
        _settings = GetDefaultSettings();
    }

    public TestSettingsService(ApplicationSettings settings)
    {
        _settings = settings;
    }

    public ApplicationSettings GetSettings() => _settings;

    public Task SaveSettingsAsync(ApplicationSettings settings)
    {
        _settings = settings;
        SettingsChanged?.Invoke(this, _settings);
        return Task.CompletedTask;
    }

    public ApplicationSettings GetDefaultSettings()
    {
        return new ApplicationSettings
        {
            HttpServer = new HttpServerSettings
            {
                Enabled = true,
                Port = 18080,  // テスト用ポート
                BindAddress = "127.0.0.1",
                DocumentRoot = Path.Combine(Path.GetTempPath(), "jdx-test-www"),
                WelcomeFileName = "index.html",
                MaxConnections = 10,
                TimeOut = 5
            },
            DnsServer = new DnsServerSettings
            {
                Enabled = true,
                Port = 15353,  // テスト用ポート
                BindAddress = "127.0.0.1"
            },
            SmtpServer = new SmtpServerSettings
            {
                Enabled = true,
                Port = 12525,  // テスト用ポート
                BindAddress = "127.0.0.1",
                DomainName = "test.local",
                SizeLimit = 1024 * 1024  // 1MB
            }
        };
    }

    public VirtualHostSettings GetDefaultVirtualHostSettings()
    {
        return new VirtualHostSettings
        {
            DocumentRoot = Path.Combine(Path.GetTempPath(), "jdx-test-www"),
            WelcomeFileName = "index.html"
        };
    }

    public Task<string> ExportSettingsAsync()
    {
        return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(_settings));
    }

    public Task ImportSettingsAsync(string json)
    {
        var settings = System.Text.Json.JsonSerializer.Deserialize<ApplicationSettings>(json);
        if (settings != null)
        {
            _settings = settings;
            SettingsChanged?.Invoke(this, _settings);
        }
        return Task.CompletedTask;
    }

    public event EventHandler<ApplicationSettings>? SettingsChanged;

    /// <summary>
    /// HTTPサーバーの設定を更新
    /// </summary>
    public void ConfigureHttpServer(Action<HttpServerSettings> configure)
    {
        configure(_settings.HttpServer);
    }

    /// <summary>
    /// DNSサーバーの設定を更新
    /// </summary>
    public void ConfigureDnsServer(Action<DnsServerSettings> configure)
    {
        configure(_settings.DnsServer);
    }

    /// <summary>
    /// SMTPサーバーの設定を更新
    /// </summary>
    public void ConfigureSmtpServer(Action<SmtpServerSettings> configure)
    {
        configure(_settings.SmtpServer);
    }
}
