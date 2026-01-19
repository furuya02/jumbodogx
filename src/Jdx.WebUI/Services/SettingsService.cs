using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Jdx.Core.Settings;

namespace Jdx.WebUI.Services;

/// <summary>
/// アプリケーション設定を管理するサービス
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly IConfiguration _configuration;
    private readonly string _settingsFilePath;
    private ApplicationSettings _currentSettings;

    public event EventHandler<ApplicationSettings>? SettingsChanged;

    public SettingsService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _settingsFilePath = Path.Combine(environment.ContentRootPath, "appsettings.json");
        _currentSettings = LoadSettings();
    }

    public ApplicationSettings GetSettings()
    {
        return _currentSettings;
    }

    public async Task SaveSettingsAsync(ApplicationSettings settings)
    {
        _currentSettings = settings;

        // appsettings.json ファイルを読み込み
        string json = await File.ReadAllTextAsync(_settingsFilePath);
        var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;

        // 既存の設定を保持しつつ、Jdxセクションのみ更新
        var updatedJson = new Dictionary<string, object>();

        // 既存のセクションをコピー
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name != "Jdx")
            {
                updatedJson[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText())!;
            }
        }

        // Jdxセクションを更新（全サーバー設定を含む）
        updatedJson["Jdx"] = new
        {
            HttpServer = settings.HttpServer,
            DnsServer = settings.DnsServer,
            FtpServer = settings.FtpServer,
            TftpServer = settings.TftpServer,
            DhcpServer = settings.DhcpServer,
            Pop3Server = settings.Pop3Server,
            SmtpServer = settings.SmtpServer,
            ProxyServer = settings.ProxyServer,
            Logging = settings.Logging
        };

        // JSONファイルに書き込み
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        string updatedJsonString = JsonSerializer.Serialize(updatedJson, options);
        await File.WriteAllTextAsync(_settingsFilePath, updatedJsonString);

        OnSettingsChanged(settings);
    }

    public ApplicationSettings GetDefaultSettings()
    {
        var httpSettings = new HttpServerSettings
        {
            Enabled = true,
            Protocol = "HTTP",
            Port = 8080,
            BindAddress = "0.0.0.0",
            UseResolve = false,
            TimeOut = 3,
            MaxConnections = 100,
            DocumentRoot = "",
            WelcomeFileName = "index.html",
            UseHidden = false,
            UseDot = false,
            UseDirectoryEnum = false,
            ServerHeader = "BlackJumboDog Version $v",
            UseEtag = false,
            ServerAdmin = "",
            UseCgi = false,
            CgiTimeout = 10,
            UseSsi = false,
            SsiExt = "html,htm",
            UseExec = false,
            UseWebDav = false,
            Encode = "UTF-8",
            UseAutoAcl = false,
            AutoAclApacheKiller = false,
            EnableAcl = 0
        };

        // デフォルトMIMEタイプ
        httpSettings.MimeTypes = new List<MimeEntry>
        {
            new() { Extension = "txt", MimeType = "text/plain" },
            new() { Extension = "html", MimeType = "text/html" },
            new() { Extension = "htm", MimeType = "text/html" },
            new() { Extension = "css", MimeType = "text/css" },
            new() { Extension = "js", MimeType = "text/javascript" },
            new() { Extension = "json", MimeType = "application/json" },
            new() { Extension = "xml", MimeType = "text/xml" },
            new() { Extension = "gif", MimeType = "image/gif" },
            new() { Extension = "jpg", MimeType = "image/jpeg" },
            new() { Extension = "jpeg", MimeType = "image/jpeg" },
            new() { Extension = "png", MimeType = "image/png" },
            new() { Extension = "svg", MimeType = "image/svg+xml" },
            new() { Extension = "pdf", MimeType = "application/pdf" },
            new() { Extension = "zip", MimeType = "application/zip" }
        };

        return new ApplicationSettings
        {
            HttpServer = httpSettings,
            DnsServer = new DnsServerSettings
            {
                Enabled = true,
                Port = 5300
            },
            Logging = new LoggingSettings
            {
                LogLevel = "Information",
                MaxEntries = 1000
            }
        };
    }

    private ApplicationSettings LoadSettings()
    {
        var settings = new ApplicationSettings();

        // Jdxセクション全体をバインド
        var jdxSection = _configuration.GetSection("Jdx");
        if (jdxSection.Exists())
        {
            // HttpServer
            var httpSection = jdxSection.GetSection("HttpServer");
            if (httpSection.Exists())
            {
                httpSection.Bind(settings.HttpServer);
            }

            // DnsServer
            var dnsSection = jdxSection.GetSection("DnsServer");
            if (dnsSection.Exists())
            {
                dnsSection.Bind(settings.DnsServer);
            }

            // Logging
            var loggingSection = jdxSection.GetSection("Logging");
            if (loggingSection.Exists())
            {
                loggingSection.Bind(settings.Logging);
            }
        }

        // デフォルト値の設定（空の場合）
        if (settings.HttpServer.MimeTypes.Count == 0)
        {
            settings.HttpServer.MimeTypes = GetDefaultSettings().HttpServer.MimeTypes;
        }

        return settings;
    }

    public async Task<string> ExportSettingsAsync()
    {
        var settings = GetSettings();
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null
        };
        return await Task.FromResult(JsonSerializer.Serialize(settings, options));
    }

    public async Task ImportSettingsAsync(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON文字列が空です", nameof(json));
        }

        ApplicationSettings? settings;
        try
        {
            settings = JsonSerializer.Deserialize<ApplicationSettings>(json);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("無効なJSON形式です", nameof(json), ex);
        }

        if (settings == null)
        {
            throw new ArgumentException("設定のデシリアライズに失敗しました", nameof(json));
        }

        // インポートした設定を保存
        await SaveSettingsAsync(settings);
    }

    protected virtual void OnSettingsChanged(ApplicationSettings settings)
    {
        SettingsChanged?.Invoke(this, settings);
    }
}
