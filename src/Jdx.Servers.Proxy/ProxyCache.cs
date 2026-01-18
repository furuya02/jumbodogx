using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Proxy;

/// <summary>
/// Proxyキャッシュ管理
/// bjd5-master/ProxyHttpServer/Cache.cs に対応
/// </summary>
public class ProxyCache : IDisposable
{
    private readonly ProxyServerSettings _settings;
    private readonly ILogger _logger;
    private readonly string _cacheDir;
    private bool _isRunning;
    private bool _disposed;

    public ProxyCache(ProxyServerSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
        _cacheDir = settings.CacheDir;

        // キャッシュディレクトリの作成
        if (!string.IsNullOrWhiteSpace(_cacheDir))
        {
            try
            {
                if (!Directory.Exists(_cacheDir))
                {
                    Directory.CreateDirectory(_cacheDir);
                    _logger.LogInformation("Cache directory created: {CacheDir}", _cacheDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create cache directory: {CacheDir}", _cacheDir);
            }
        }
    }

    public void Start()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _logger.LogInformation("Proxy cache started");

        // TODO: キャッシュクリーニングタスクなどを開始
    }

    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _logger.LogInformation("Proxy cache stopped");

        // TODO: キャッシュクリーニングタスクなどを停止
    }

    /// <summary>
    /// キャッシュが有効かどうかをチェック
    /// </summary>
    public bool IsEnabled()
    {
        return _settings.UseCache && !string.IsNullOrWhiteSpace(_cacheDir);
    }

    /// <summary>
    /// 指定されたURLのキャッシュが存在するかチェック
    /// </summary>
    /// <param name="url">URL</param>
    /// <returns>true: キャッシュ存在, false: キャッシュなし</returns>
    public bool HasCache(string url)
    {
        if (!IsEnabled())
            return false;

        // TODO: キャッシュの存在チェックロジックを実装
        // - URLからキャッシュキーを生成
        // - メモリキャッシュをチェック
        // - ディスクキャッシュをチェック

        return false;
    }

    /// <summary>
    /// キャッシュからデータを取得
    /// </summary>
    /// <param name="url">URL</param>
    /// <returns>キャッシュデータ（nullの場合はキャッシュなし）</returns>
    public byte[]? GetCache(string url)
    {
        if (!IsEnabled())
            return null;

        // TODO: キャッシュ取得ロジックを実装
        // - メモリキャッシュから取得を試みる
        // - メモリになければディスクキャッシュから取得
        // - キャッシュの有効期限をチェック

        return null;
    }

    /// <summary>
    /// データをキャッシュに保存
    /// </summary>
    /// <param name="url">URL</param>
    /// <param name="data">保存するデータ</param>
    /// <returns>true: 保存成功, false: 保存失敗</returns>
    public bool SetCache(string url, byte[] data)
    {
        if (!IsEnabled() || data == null || data.Length == 0)
            return false;

        // サイズ制限チェック
        if (data.Length > _settings.MaxSize * 1024)
        {
            _logger.LogDebug("Data too large to cache: {Size} KB > {MaxSize} KB",
                data.Length / 1024, _settings.MaxSize);
            return false;
        }

        // ホスト・拡張子フィルタリングチェック
        if (!ShouldCache(url))
        {
            _logger.LogDebug("URL not eligible for caching: {Url}", url);
            return false;
        }

        // TODO: キャッシュ保存ロジックを実装
        // - メモリサイズ制限内ならメモリキャッシュに保存
        // - メモリ制限を超えたらディスクキャッシュに保存
        // - LRU（最も使われていない）アイテムを削除

        return false;
    }

    /// <summary>
    /// 指定されたURLをキャッシュすべきかチェック
    /// </summary>
    private bool ShouldCache(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            var uri = new Uri(url);

            // ホストフィルタリング
            if (_settings.EnableHost == 1) // 指定したホストのみ
            {
                if (_settings.CacheHostList == null || _settings.CacheHostList.Count == 0)
                    return false;

                var hostMatch = _settings.CacheHostList.Any(h =>
                    !string.IsNullOrWhiteSpace(h.Host) &&
                    uri.Host.Equals(h.Host, StringComparison.OrdinalIgnoreCase));

                if (!hostMatch)
                    return false;
            }

            // 拡張子フィルタリング
            if (_settings.EnableExt == 1) // 指定した拡張子のみ
            {
                if (_settings.CacheExtList == null || _settings.CacheExtList.Count == 0)
                    return false;

                var ext = Path.GetExtension(uri.AbsolutePath).TrimStart('.');
                if (string.IsNullOrWhiteSpace(ext))
                    return false;

                var extMatch = _settings.CacheExtList.Any(e =>
                    !string.IsNullOrWhiteSpace(e.Ext) &&
                    ext.Equals(e.Ext, StringComparison.OrdinalIgnoreCase));

                if (!extMatch)
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if URL should be cached: {Url}", url);
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _disposed = true;

        _logger.LogInformation("Proxy cache disposed");
    }
}
