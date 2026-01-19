using Jdx.Core.Settings;
using Jdx.WebUI.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Xunit;
using System.Text.Json;

namespace Jdx.Core.Tests.Settings;

public class SettingsServiceTests
{
    [Fact]
    public async Task ExportSettingsAsync_ReturnsValidJson()
    {
        // Arrange
        var service = CreateSettingsService();

        // Act
        var json = await service.ExportSettingsAsync();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);

        // JSONとしてパース可能であることを確認
        var settings = JsonSerializer.Deserialize<ApplicationSettings>(json);
        Assert.NotNull(settings);
    }

    [Fact]
    public async Task ExportSettingsAsync_ContainsAllSections()
    {
        // Arrange
        var service = CreateSettingsService();

        // Act
        var json = await service.ExportSettingsAsync();
        var settings = JsonSerializer.Deserialize<ApplicationSettings>(json);

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.HttpServer);
        Assert.NotNull(settings.DnsServer);
        Assert.NotNull(settings.Logging);
    }

    [Fact]
    public async Task ImportSettingsAsync_WithValidJson_Succeeds()
    {
        // Arrange
        var service = CreateSettingsService(createAppSettingsFile: true);
        var originalJson = await service.ExportSettingsAsync();

        // Act & Assert
        try
        {
            await service.ImportSettingsAsync(originalJson);
        }
        finally
        {
            CleanupAppSettingsFile();
        }
    }

    [Fact]
    public async Task ImportSettingsAsync_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateSettingsService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ImportSettingsAsync(string.Empty));
    }

    [Fact]
    public async Task ImportSettingsAsync_WithNull_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateSettingsService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ImportSettingsAsync(null!));
    }

    [Fact]
    public async Task ImportSettingsAsync_WithInvalidJson_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateSettingsService();
        var invalidJson = "{ invalid json }";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ImportSettingsAsync(invalidJson));
    }

    [Fact]
    public async Task ImportSettingsAsync_WithNonObjectJson_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateSettingsService();
        var invalidJson = "\"just a string\"";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ImportSettingsAsync(invalidJson));
    }

    [Fact]
    public async Task ExportImportRoundTrip_PreservesAllSettings()
    {
        // Arrange
        var service = CreateSettingsService(createAppSettingsFile: true);
        var original = service.GetSettings();

        try
        {
            // Act
            var exported = await service.ExportSettingsAsync();
            await service.ImportSettingsAsync(exported);
            var imported = service.GetSettings();

            // Assert - 全サーバーセクションが保存されることを確認
            Assert.Equal(original.HttpServer.Port, imported.HttpServer.Port);
            Assert.Equal(original.HttpServer.Enabled, imported.HttpServer.Enabled);
            Assert.Equal(original.DnsServer.Port, imported.DnsServer.Port);
            Assert.Equal(original.DnsServer.Enabled, imported.DnsServer.Enabled);
            Assert.Equal(original.FtpServer.Port, imported.FtpServer.Port);
            Assert.Equal(original.FtpServer.Enabled, imported.FtpServer.Enabled);
            Assert.Equal(original.TftpServer.Port, imported.TftpServer.Port);
            Assert.Equal(original.TftpServer.Enabled, imported.TftpServer.Enabled);
            Assert.Equal(original.DhcpServer.Port, imported.DhcpServer.Port);
            Assert.Equal(original.DhcpServer.Enabled, imported.DhcpServer.Enabled);
            Assert.Equal(original.Pop3Server.Port, imported.Pop3Server.Port);
            Assert.Equal(original.Pop3Server.Enabled, imported.Pop3Server.Enabled);
            Assert.Equal(original.SmtpServer.Port, imported.SmtpServer.Port);
            Assert.Equal(original.SmtpServer.Enabled, imported.SmtpServer.Enabled);
            Assert.Equal(original.ProxyServer.Port, imported.ProxyServer.Port);
            Assert.Equal(original.ProxyServer.Enabled, imported.ProxyServer.Enabled);
            Assert.Equal(original.Logging.LogLevel, imported.Logging.LogLevel);
        }
        finally
        {
            CleanupAppSettingsFile();
        }
    }

    private const string TestAppSettingsPath = "/tmp/appsettings.json";

    private ISettingsService CreateSettingsService(bool createAppSettingsFile = false)
    {
        if (createAppSettingsFile)
        {
            var initialSettings = new
            {
                Jdx = new
                {
                    HttpServer = new { Enabled = true, Port = 8080 },
                    DnsServer = new { Enabled = true, Port = 5300 },
                    FtpServer = new { Enabled = true, Port = 2100 },
                    TftpServer = new { Enabled = true, Port = 6900 },
                    DhcpServer = new { Enabled = true, Port = 6700 },
                    Pop3Server = new { Enabled = true, Port = 11000 },
                    SmtpServer = new { Enabled = true, Port = 2500 },
                    ProxyServer = new { Enabled = true, Port = 8081 },
                    Logging = new { LogLevel = "Information" }
                }
            };
            File.WriteAllText(TestAppSettingsPath, JsonSerializer.Serialize(initialSettings, new JsonSerializerOptions { WriteIndented = true }));
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jdx:HttpServer:Enabled"] = "true",
                ["Jdx:HttpServer:Port"] = "8080",
                ["Jdx:DnsServer:Enabled"] = "true",
                ["Jdx:DnsServer:Port"] = "5300",
                ["Jdx:FtpServer:Enabled"] = "true",
                ["Jdx:FtpServer:Port"] = "2100",
                ["Jdx:TftpServer:Enabled"] = "true",
                ["Jdx:TftpServer:Port"] = "6900",
                ["Jdx:DhcpServer:Enabled"] = "true",
                ["Jdx:DhcpServer:Port"] = "6700",
                ["Jdx:Pop3Server:Enabled"] = "true",
                ["Jdx:Pop3Server:Port"] = "11000",
                ["Jdx:SmtpServer:Enabled"] = "true",
                ["Jdx:SmtpServer:Port"] = "2500",
                ["Jdx:ProxyServer:Enabled"] = "true",
                ["Jdx:ProxyServer:Port"] = "8081",
                ["Jdx:Logging:LogLevel"] = "Information"
            })
            .Build();

        var environment = new MockWebHostEnvironment();
        return new SettingsService(configuration, environment);
    }

    private void CleanupAppSettingsFile()
    {
        if (File.Exists(TestAppSettingsPath))
        {
            File.Delete(TestAppSettingsPath);
        }
    }

    private class MockWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = "/tmp";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "Jdx.WebUI";
        public string ContentRootPath { get; set; } = "/tmp";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
