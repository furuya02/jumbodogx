using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Jdx.Core.Abstractions;
using Jdx.Core.Constants;
using Jdx.Core.Helpers;
using Jdx.Core.Metrics;
using Jdx.Core.Network;
using Jdx.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Jdx.Servers.Http;

/// <summary>
/// HTTPサーバー
/// </summary>
public class HttpServer : ServerBase
{
    private readonly ISettingsService? _settingsService;
    private readonly Action<LogLevel, string, string, string?>? _logCallback;
    private readonly ReaderWriterLockSlim _settingsLock = new ReaderWriterLockSlim();
    private int _port;
    private string _bindAddress = "0.0.0.0";
    private string _name = "HttpServer";
    private ServerTcpListener? _listener;
    private HttpTarget? _target;
    private HttpContentType? _contentType;
    private HttpFileHandler? _fileHandler;
    private HttpAuthenticator? _authenticator;
    private HttpAclFilter? _aclFilter;
    private HttpCgiHandler? _cgiHandler;
    private HttpWebDavHandler? _webDavHandler;
    private HttpAttackDb? _attackDb;
    private HttpVirtualHostManager? _virtualHostManager;
    private HttpSslManager? _sslManager;

    // VirtualHost専用モード
    private readonly bool _isVirtualHostMode;
    private readonly VirtualHostEntry? _virtualHostEntry;
    private HttpServerSettings? _virtualHostSettings;

    // デフォルトタイムアウト（タイムアウト設定が0の場合に使用）
    private const int DefaultTimeoutSeconds = 30;

    /// <summary>
    /// 通常モードのコンストラクタ（ISettingsServiceから全体設定を読み込む）
    /// </summary>
    public HttpServer(ILogger<HttpServer> logger, ISettingsService settingsService, Action<LogLevel, string, string, string?>? logCallback = null) : base(logger)
    {
        _settingsService = settingsService;
        _logCallback = logCallback;
        _isVirtualHostMode = false;

        // 初期設定を取得
        var settings = _settingsService.GetSettings().HttpServer;
        _port = settings.Port;
        _bindAddress = settings.BindAddress;
        _name = "HttpServer";

        // コンポーネントを初期化
        InitializeComponents(settings);

        // 設定変更イベントを購読
        _settingsService.SettingsChanged += OnSettingsChanged;
    }

    /// <summary>
    /// VirtualHost専用モードのコンストラクタ
    /// </summary>
    public HttpServer(ILogger<HttpServer> logger, VirtualHostEntry virtualHostEntry, HttpServerSettings parentSettings, ISettingsService? settingsService = null, Action<LogLevel, string, string, string?>? logCallback = null) : base(logger)
    {
        _settingsService = settingsService;
        _logCallback = logCallback;
        _isVirtualHostMode = true;
        _virtualHostEntry = virtualHostEntry;

        // VirtualHostの情報を設定
        _port = virtualHostEntry.GetPort();
        _bindAddress = virtualHostEntry.BindAddress ?? "0.0.0.0";
        _name = $"HttpServer ({virtualHostEntry.GetHostName()})";

        // VirtualHost用の設定を作成（親設定とVirtualHost設定をマージ）
        _virtualHostSettings = CreateVirtualHostSettings(parentSettings, virtualHostEntry);

        // コンポーネントを初期化
        InitializeComponents(_virtualHostSettings);

        // 設定変更イベントを購読（VirtualHostモードでもSettingsServiceがあれば購読）
        if (_settingsService != null)
        {
            _settingsService.SettingsChanged += OnSettingsChanged;
        }
    }

    public override string Name => _name;
    public override ServerType Type => ServerType.Http;
    public override int Port => _port;
    public override string BindAddress => _bindAddress;

    private void InitializeComponents(HttpServerSettings settings)
    {
        _target = new HttpTarget(settings, Logger);
        _contentType = new HttpContentType(settings);
        _fileHandler = new HttpFileHandler(Logger, _contentType);
        _authenticator = new HttpAuthenticator(settings, Logger);
        _aclFilter = new HttpAclFilter(settings, Logger);
        _cgiHandler = new HttpCgiHandler(settings, Logger);
        _webDavHandler = new HttpWebDavHandler(settings, Logger);

        // Virtual Host初期化（通常モードの場合のみ）
        if (!_isVirtualHostMode)
        {
            _virtualHostManager = new HttpVirtualHostManager(settings, Logger);
            if (settings.VirtualHosts != null && settings.VirtualHosts.Count > 0)
            {
                Logger.LogInformation("Virtual Host enabled: {Count} hosts configured", settings.VirtualHosts.Count);
            }
        }
        else
        {
            // VirtualHostモードではHttpVirtualHostManagerは不要
            _virtualHostManager = null;
        }

        // SSL/TLS初期化
        _sslManager = new HttpSslManager(settings.Protocol, settings.CertificateFile, settings.CertificatePassword, Logger);
        if (_sslManager.IsEnabled)
        {
            Logger.LogInformation("HTTPS/SSL enabled with certificate: {Subject}", _sslManager.Certificate?.Subject ?? "unknown");
        }

        // AttackDb初期化（UseAutoAclが有効な場合）
        if (settings.UseAutoAcl)
        {
            _attackDb = new HttpAttackDb(Logger, timeWindowSeconds: 120, maxAttempts: 1);
            Logger.LogInformation("AttackDb enabled: timeWindow=120s, maxAttempts=1");
        }
        else
        {
            _attackDb = null;
        }

        // SSIプロセッサを初期化してFileHandlerに設定
        var ssiProcessor = new HttpSsiProcessor(settings, Logger);
        _fileHandler.SetSsiProcessor(ssiProcessor);

        // AttackDb攻撃検出コールバックを設定
        if (_attackDb != null)
        {
            _fileHandler.SetAttackDetectedCallback((remoteIp) =>
            {
                // Apache Killer攻撃検出時の処理
                if (_attackDb.IsInjustice(false, remoteIp))
                {
                    Logger.LogWarning("Attack detected from {RemoteIp}, attempting to add to ACL", remoteIp);

                    // ACL自動追加機能（バックグラウンドで実行）
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // 書き込みロックを取得して設定の読み取り・変更・保存を原子的に実行
                            // これにより複数の攻撃が同時検出された場合の競合を防止
                            _settingsLock.EnterWriteLock();
                            try
                            {
                                var currentSettings = _settingsService.GetSettings();
                                var httpSettings = currentSettings.HttpServer;

                                // EnableAcl設定がDenyMode(1)であることを確認
                                if (httpSettings.EnableAcl != 1)
                                {
                                    Logger.LogWarning("ACL is not in Deny mode (current: {Mode}). Auto-ACL requires Deny mode to block attackers. Skipping auto-add.",
                                        httpSettings.EnableAcl);
                                    return;
                                }

                                // 既にACLリストに存在するかチェック
                                if (httpSettings.AclList.Any(acl => acl.Address == remoteIp))
                                {
                                    Logger.LogInformation("IP {RemoteIp} is already in ACL list, skipping duplicate entry", remoteIp);
                                    return;
                                }

                                // IPアドレス形式の検証
                                if (!IPAddress.TryParse(remoteIp, out _))
                                {
                                    Logger.LogWarning("Invalid IP address format: {RemoteIp}, skipping ACL add", remoteIp);
                                    return;
                                }

                                // 新しいACLエントリを追加
                                var newAclEntry = new AclEntry
                                {
                                    Name = $"AutoBlock_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}",
                                    Address = remoteIp
                                };

                                httpSettings.AclList.Add(newAclEntry);

                                // 設定を保存（ロック内で実行）
                                await _settingsService.SaveSettingsAsync(currentSettings);

                                Logger.LogWarning("Auto-blocked IP {RemoteIp} added to ACL (Entry name: {Name})",
                                    remoteIp, newAclEntry.Name);
                            }
                            finally
                            {
                                _settingsLock.ExitWriteLock();
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Failed to add IP {RemoteIp} to ACL automatically", remoteIp);
                        }
                    });
                }
            });
        }
    }

    /// <summary>
    /// VirtualHost用の設定を作成（親設定とVirtualHost設定をマージ）
    /// </summary>
    private HttpServerSettings CreateVirtualHostSettings(HttpServerSettings parentSettings, VirtualHostEntry virtualHostEntry)
    {
        var vhostSettings = virtualHostEntry.Settings;

        return new HttpServerSettings
        {
            // サーバー基本設定（VirtualHostで個別化）
            Enabled = virtualHostEntry.Enabled,
            Protocol = vhostSettings.Protocol,  // VirtualHostの設定を使用
            Port = virtualHostEntry.GetPort(),    // VirtualHostのポート
            BindAddress = virtualHostEntry.BindAddress ?? parentSettings.BindAddress,  // VirtualHostで指定がなければ親を使用
            UseResolve = parentSettings.UseResolve,
            TimeOut = parentSettings.TimeOut,
            MaxConnections = parentSettings.MaxConnections,

            // ドキュメント設定（VirtualHostの設定を優先）
            DocumentRoot = vhostSettings.DocumentRoot,
            WelcomeFileName = vhostSettings.WelcomeFileName,
            UseHidden = vhostSettings.UseHidden,
            UseDot = vhostSettings.UseDot,
            UseDirectoryEnum = vhostSettings.UseDirectoryEnum,
            UseExpansion = vhostSettings.UseExpansion,
            ServerHeader = vhostSettings.ServerHeader,
            UseEtag = vhostSettings.UseEtag,
            ServerAdmin = vhostSettings.ServerAdmin,

            // CGI設定
            UseCgi = vhostSettings.UseCgi,
            CgiCommands = vhostSettings.CgiCommands,
            CgiTimeout = vhostSettings.CgiTimeout,
            CgiPaths = vhostSettings.CgiPaths,

            // SSI設定
            UseSsi = vhostSettings.UseSsi,
            SsiExt = vhostSettings.SsiExt,
            UseExec = vhostSettings.UseExec,

            // WebDAV設定
            UseWebDav = vhostSettings.UseWebDav,
            WebDavPaths = vhostSettings.WebDavPaths,

            // Alias & MIME設定
            Aliases = vhostSettings.Aliases,
            MimeTypes = vhostSettings.MimeTypes,

            // 認証設定
            AuthList = vhostSettings.AuthList,
            UserList = vhostSettings.UserList,
            GroupList = vhostSettings.GroupList,
            Encode = vhostSettings.Encode,
            IndexDocument = vhostSettings.IndexDocument,
            ErrorDocument = vhostSettings.ErrorDocument,

            // ACL設定
            EnableAcl = vhostSettings.EnableAcl,
            AclList = vhostSettings.AclList,
            UseAutoAcl = vhostSettings.UseAutoAcl,
            AutoAclApacheKiller = vhostSettings.AutoAclApacheKiller,

            // SSL/TLS設定（VirtualHostの設定を優先）
            CertificateFile = vhostSettings.CertificateFile,
            CertificatePassword = vhostSettings.CertificatePassword,

            // 高度な設定（親設定を継承）
            UseKeepAlive = vhostSettings.UseKeepAlive,
            KeepAliveTimeout = vhostSettings.KeepAliveTimeout,
            MaxKeepAliveRequests = vhostSettings.MaxKeepAliveRequests,
            UseRangeRequests = vhostSettings.UseRangeRequests,
            MaxRangeCount = vhostSettings.MaxRangeCount,

            // VirtualHosts設定は空（このHttpServerインスタンスは単一VirtualHostを表す）
            VirtualHosts = new List<VirtualHostEntry>()
        };
    }

    private void OnSettingsChanged(object? sender, ApplicationSettings settings)
    {
        _settingsLock.EnterWriteLock();
        try
        {
            var httpSettings = settings.HttpServer;

            // VirtualHostモードでない場合のみポート変更を適用
            if (!_isVirtualHostMode && _port != httpSettings.Port)
            {
                _port = httpSettings.Port;
                Logger.LogInformation("HTTP Server port changed to {Port}", _port);
            }

            // VirtualHostモードの場合は、VirtualHost設定を再生成
            if (_isVirtualHostMode && _virtualHostEntry != null)
            {
                _virtualHostSettings = CreateVirtualHostSettings(httpSettings, _virtualHostEntry);
                InitializeComponents(_virtualHostSettings);
            }
            else
            {
                // 通常モードの場合は、直接httpSettingsを使用
                InitializeComponents(httpSettings);
            }

            Logger.LogInformation("HTTP Server settings updated");
        }
        finally
        {
            _settingsLock.ExitWriteLock();
        }
    }

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        HttpServerSettings settings;
        _settingsLock.EnterReadLock();
        try
        {
            // VirtualHostモードの場合は_virtualHostSettingsを使用
            settings = _isVirtualHostMode ? _virtualHostSettings! : _settingsService!.GetSettings().HttpServer;
        }
        finally
        {
            _settingsLock.ExitReadLock();
        }

        _listener = await CreateTcpListenerAsync(_port, settings.BindAddress, cancellationToken);

        var displayAddress = settings.BindAddress ?? "0.0.0.0";
        Logger.LogInformation("HTTP Server listening on http://{Address}:{Port}", displayAddress, _port);

        // クライアント接続を受け入れるループ
        // 注: ServerBase.RunTcpAcceptLoopAsync()は使用していません。
        // 理由: 最大接続数到達時に503 Service Unavailableレスポンスを送信する必要があるため。
        // ConnectionLimiterではレスポンス送信前に接続を拒否してしまうため、
        // カスタムループで接続受け入れ後に503を返す実装が必要です。
        _ = Task.Run(async () =>
        {
            while (!StopCts.Token.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await _listener.AcceptAsync(StopCts.Token);
                    Statistics.TotalConnections++;

                    // 最大接続数チェック
                    HttpServerSettings currentSettings;
                    _settingsLock.EnterReadLock();
                    try
                    {
                        // VirtualHostモードの場合は_virtualHostSettingsを使用
                        currentSettings = _isVirtualHostMode ? _virtualHostSettings! : _settingsService!.GetSettings().HttpServer;
                    }
                    finally
                    {
                        _settingsLock.ExitReadLock();
                    }

                    if (currentSettings.MaxConnections > 0 && Statistics.ActiveConnections >= currentSettings.MaxConnections)
                    {
                        Logger.LogWarning("Max connections ({MaxConnections}) reached, rejecting connection from {RemoteEndPoint}",
                            currentSettings.MaxConnections, clientSocket.RemoteEndPoint);

                        // 503 Service Unavailable を送信してクローズ
                        try
                        {
                            var errorResponse = HttpResponseBuilder.BuildErrorResponse(503, "Service Unavailable", currentSettings);
                            var errorBytes = errorResponse.ToBytes();
                            await clientSocket.SendAsync(errorBytes, SocketFlags.None, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            // エラーレスポンス送信に失敗してもログのみ出力
                            Logger.LogDebug(ex, "Failed to send error response to client");
                        }
                        finally
                        {
                            clientSocket.Close();
                            clientSocket.Dispose();
                        }

                        Statistics.TotalErrors++;
                        Metrics.IncrementErrors();
                        continue;
                    }

                    // 接続を受け入れる（最大接続数チェック後）
                    Statistics.ActiveConnections++;
                    Metrics.IncrementConnections();

                    // クライアント処理を非同期で実行
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await HandleClientAsync(clientSocket, StopCts.Token);
                        }
                        finally
                        {
                            Statistics.ActiveConnections--;
                            Metrics.DecrementActiveConnections();
                        }
                    }, StopCts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    NetworkExceptionHandler.LogNetworkException(ex, Logger, "HTTP client accept");
                }
            }
        }, StopCts.Token);
    }

    protected override async Task StopListeningAsync(CancellationToken cancellationToken)
    {
        await StopTcpListenerAsync();
        Logger.LogInformation("HTTP Server stopped");
    }

    protected override async Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        var remoteEndPoint = clientSocket.RemoteEndPoint?.ToString() ?? "unknown";
        Logger.LogDebug("HTTP connection from {RemoteEndPoint}", remoteEndPoint);

        HttpServerSettings settings;
        HttpAclFilter aclFilter;
        HttpSslManager? sslManager;
        _settingsLock.EnterReadLock();
        try
        {
            // VirtualHostモードの場合は_virtualHostSettingsを使用
            settings = _isVirtualHostMode ? _virtualHostSettings! : _settingsService!.GetSettings().HttpServer;
            aclFilter = _aclFilter!; // Capture component reference inside lock
            sslManager = _sslManager; // Capture SSL manager reference
        }
        finally
        {
            _settingsLock.ExitReadLock();
        }

        // ACLチェック（接続元IPアドレス）
        var remoteIp = clientSocket.RemoteEndPoint is IPEndPoint ipEndPoint
            ? ipEndPoint.Address.ToString()
            : "";
        if (!string.IsNullOrEmpty(remoteIp) && !aclFilter.IsAllowed(remoteIp))
        {
            // ACL denied - log already output in HttpAclFilter
            // 403 Forbidden を送信してクローズ（ベストエフォート、短いタイムアウト）
            try
            {
                using var errorCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var forbiddenResponse = HttpResponseBuilder.BuildErrorResponse(403, "Forbidden", settings);
                var forbiddenBytes = forbiddenResponse.ToBytes();
                await clientSocket.SendAsync(forbiddenBytes, SocketFlags.None, errorCts.Token);
            }
            catch (Exception ex)
            {
                // エラーレスポンス送信に失敗してもログのみ出力
                Logger.LogDebug(ex, "Failed to send 403 Forbidden response");
            }
            finally
            {
                clientSocket.Dispose();
            }

            // LogServiceにACL拒否ログを送信
            _logCallback?.Invoke(
                LogLevel.Warning,
                _name,
                "ACL denied connection",
                remoteIp);

            Statistics.TotalErrors++;
            Metrics.IncrementErrors();
            return;
        }

        // NetworkStream/SslStreamを作成
        NetworkStream? networkStream = null;
        Stream? stream = null;
        try
        {
            networkStream = new NetworkStream(clientSocket, ownsSocket: false);
            stream = networkStream;

            // SSL/TLSハンドシェイク（SSL有効時）
            if (sslManager?.IsEnabled == true)
            {
                var sslStream = sslManager.CreateServerStream(stream);
                await sslManager.AuthenticateAsServerAsync(sslStream, cancellationToken);
                stream = sslStream;
                Logger.LogDebug("HTTPS/SSL connection established from {RemoteEndPoint}", remoteEndPoint);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "SSL/TLS handshake failed for {RemoteEndPoint}", remoteEndPoint);
            stream?.Dispose();
            networkStream?.Dispose();
            clientSocket.Dispose();
            Statistics.TotalErrors++;
            Metrics.IncrementErrors();
            return;
        }

        // Keep-Alive用の変数
        var keepAlive = settings.UseKeepAlive;
        var requestCount = 0;

        try
        {
            // Keep-Aliveループ：接続を維持しながら複数のリクエストを処理
            while (keepAlive && !cancellationToken.IsCancellationRequested)
            {
                requestCount++;

                // 最大リクエスト数チェック
                if (settings.MaxKeepAliveRequests > 0 && requestCount > settings.MaxKeepAliveRequests)
                {
                    Logger.LogDebug("Max keep-alive requests ({Max}) reached for {RemoteEndPoint}",
                        settings.MaxKeepAliveRequests, remoteEndPoint);
                    break;
                }

                // Keep-Aliveタイムアウト設定
                using var keepAliveCts = new CancellationTokenSource();
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, keepAliveCts.Token);

                var timeout = requestCount == 1 ? settings.TimeOut : settings.KeepAliveTimeout;
                // タイムアウトが0の場合はデフォルト値を使用（無限待機を防ぐ）
                if (timeout <= 0)
                {
                    timeout = DefaultTimeoutSeconds;
                }
                keepAliveCts.CancelAfter(TimeSpan.FromSeconds(timeout));

                try
                {
                    // リクエストを読み取る
                    var buffer = new byte[NetworkConstants.Http.MaxLineLength];
                    var bytesRead = await stream.ReadAsync(buffer, linkedCts.Token);

                    if (bytesRead == 0)
                    {
                        // クライアントが接続を閉じた
                        Logger.LogDebug("Client closed connection: {RemoteEndPoint}", remoteEndPoint);
                        break;
                    }

                    var requestText = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var lines = requestText.Split("\r\n");

                    if (lines.Length == 0)
                    {
                        break;
                    }

                    // リクエスト全体をパース（ヘッダー含む）
                    var request = HttpRequest.ParseFull(lines);
                    Statistics.TotalRequests++;
                    Metrics.IncrementRequests();
                    Metrics.AddBytesReceived(bytesRead);

                    // リクエストボディサイズチェック（DoS対策、RFC 7230準拠）
                    if (request.Headers.TryGetValue("Content-Length", out var contentLengthStr))
                    {
                        if (!long.TryParse(contentLengthStr, out var contentLength) || contentLength < 0)
                        {
                            // 無効または負のContent-Lengthヘッダー
                            Logger.LogWarning("Invalid Content-Length header '{ContentLength}' from {RemoteEndPoint}",
                                contentLengthStr, remoteEndPoint);

                            var errorResponse = HttpResponseBuilder.BuildErrorResponse(400, "Bad Request", settings);
                            await errorResponse.SendAsync(stream, linkedCts.Token);
                            Statistics.TotalErrors++;
                            Metrics.IncrementErrors();
                            // 接続を即座にクローズ（不正なリクエストのため）
                            keepAlive = false;
                            break;
                        }

                        if (contentLength > NetworkConstants.Http.MaxRequestBodySize)
                        {
                            Logger.LogWarning("Request body size {Size} exceeds limit {Max} from {RemoteEndPoint}",
                                contentLength, NetworkConstants.Http.MaxRequestBodySize, remoteEndPoint);

                            // 413 Payload Too Large を返す
                            var errorResponse = HttpResponseBuilder.BuildErrorResponse(413, "Payload Too Large", settings);
                            await errorResponse.SendAsync(stream, linkedCts.Token);
                            Statistics.TotalErrors++;
                            Metrics.IncrementErrors();
                            // 接続を即座にクローズ（巨大なペイロードを読み取らないため）
                            keepAlive = false;
                            break;
                        }
                    }

                    Logger.LogDebug("HTTP {Method} {Path} from {RemoteEndPoint} (request #{Count})",
                        request.Method, request.Path, remoteEndPoint, requestCount);

                    // Keep-Aliveの判定
                    keepAlive = ShouldKeepAlive(request, settings);

                    // レスポンスを生成
                    var response = await GenerateResponseAsync(request, linkedCts.Token, remoteIp);

                    // Connection ヘッダーを設定
                    if (keepAlive && requestCount < settings.MaxKeepAliveRequests)
                    {
                        response.Headers["Connection"] = "keep-alive";
                        response.Headers["Keep-Alive"] = $"timeout={settings.KeepAliveTimeout}, max={settings.MaxKeepAliveRequests - requestCount}";
                    }
                    else
                    {
                        response.Headers["Connection"] = "close";
                        keepAlive = false;
                    }

                    // レスポンスを送信（ストリーム対応）
                    await response.SendAsync(stream, linkedCts.Token);
                    var bytesSent = response.ContentLength ?? response.ToBytes().Length;
                    Statistics.TotalBytesSent += bytesSent;
                    Metrics.AddBytesSent(bytesSent);

                    Logger.LogDebug("HTTP {StatusCode} {Method} {Path} - Keep-Alive: {KeepAlive}",
                        response.StatusCode, request.Method, request.Path, keepAlive);

                    // LogServiceにリクエストログを送信
                    _logCallback?.Invoke(
                        response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information,
                        _name,
                        $"{request.Method} {request.Path} - {response.StatusCode} ({bytesSent} bytes)",
                        remoteIp);
                }
                catch (OperationCanceledException) when (keepAliveCts.Token.IsCancellationRequested)
                {
                    // Keep-Aliveタイムアウト
                    if (requestCount == 1)
                    {
                        Logger.LogWarning("HTTP request from {RemoteEndPoint} timed out after {TimeOut} seconds",
                            remoteEndPoint, timeout);
                        Statistics.TotalErrors++;
                        Metrics.IncrementErrors();

                        // タイムアウトエラーレスポンスを送信（ベストエフォート、短いタイムアウト）
                        try
                        {
                            using var errorCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                            var timeoutResponse = HttpResponseBuilder.BuildErrorResponse(408, "Request Timeout", settings);
                            await timeoutResponse.SendAsync(stream, errorCts.Token);
                        }
                        catch (Exception ex)
                        {
                            // エラーレスポンス送信に失敗してもログのみ出力
                            Logger.LogDebug(ex, "Failed to send error response to client");
                        }
                    }
                    else
                    {
                        Logger.LogDebug("Keep-alive timeout for {RemoteEndPoint} after {Count} requests",
                            remoteEndPoint, requestCount - 1);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    NetworkExceptionHandler.LogNetworkException(ex, Logger, "HTTP request handling");
                    Statistics.TotalErrors++;
                    Metrics.IncrementErrors();
                    break;
                }
            }

            Logger.LogDebug("HTTP connection closed: {RemoteEndPoint} ({Count} requests processed)",
                remoteEndPoint, requestCount - 1);
        }
        finally
        {
            stream.Dispose();
            clientSocket.Dispose();
        }
    }

    /// <summary>
    /// Keep-Aliveを維持するかどうかを判定
    /// </summary>
    private bool ShouldKeepAlive(HttpRequest request, HttpServerSettings settings)
    {
        if (!settings.UseKeepAlive)
        {
            return false;
        }

        // HTTP/1.0 の場合、Connection: Keep-Alive ヘッダーが必要
        if (request.Version == "HTTP/1.0")
        {
            if (request.Headers.TryGetValue("Connection", out var connection))
            {
                return connection.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        // HTTP/1.1 の場合、デフォルトでKeep-Alive（Connection: close で明示的に無効化）
        if (request.Headers.TryGetValue("Connection", out var connectionHeader))
        {
            return !connectionHeader.Equals("close", StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    private async Task<HttpResponse> GenerateResponseAsync(HttpRequest request, CancellationToken cancellationToken, string? remoteIp = null)
    {
        // /metrics エンドポイント（Prometheus形式のメトリクス）
        if (request.Path.Equals("/metrics", StringComparison.OrdinalIgnoreCase))
        {
            var metricsText = MetricsCollector.Instance.ToPrometheusFormat();

            var response = new HttpResponse
            {
                StatusCode = 200,
                StatusText = "OK",
                Body = metricsText
            };
            response.Headers["Content-Type"] = "text/plain; version=0.0.4; charset=utf-8";

            return response;
        }

        HttpServerSettings settings;
        HttpVirtualHostManager? virtualHostManager;
        HttpAuthenticator authenticator;
        HttpWebDavHandler webDavHandler;
        HttpCgiHandler cgiHandler;
        HttpTarget target;
        HttpFileHandler fileHandler;
        _settingsLock.EnterReadLock();
        try
        {
            // VirtualHostモードの場合は_virtualHostSettingsを使用
            settings = _isVirtualHostMode ? _virtualHostSettings! : _settingsService!.GetSettings().HttpServer;
            // Capture all component references inside lock to ensure thread-safe access
            virtualHostManager = _virtualHostManager;
            authenticator = _authenticator!;
            webDavHandler = _webDavHandler!;
            cgiHandler = _cgiHandler!;
            target = _target!;
            fileHandler = _fileHandler!;
        }
        finally
        {
            _settingsLock.ExitReadLock();
        }

        // Virtual Host解決
        string? documentRoot = null;
        if (virtualHostManager != null && request.Headers.TryGetValue("Host", out var hostHeader))
        {
            // ローカルアドレス・ポートの取得（TODO: 実際の値を取得）
            var localAddress = settings.BindAddress;
            var localPort = _port;

            documentRoot = virtualHostManager.ResolveDocumentRoot(hostHeader, localAddress, localPort);
            if (documentRoot != settings.DocumentRoot)
            {
                Logger.LogDebug("Virtual host resolved: {Host} -> {DocumentRoot}", hostHeader, documentRoot);
            }
        }

        // 組み込みルート（/stats）
        if (request.Path == "/stats")
        {
            return GenerateStatsResponse();
        }

        // 認証チェック
        var authResult = authenticator.Authenticate(request.Path, request);

        if (authResult.IsUnauthorized)
        {
            // 401 Unauthorized
            return new HttpResponse
            {
                StatusCode = 401,
                StatusText = "Unauthorized",
                Body = "<html><body><h1>401 Unauthorized</h1><p>Authentication required</p></body></html>",
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "text/html; charset=utf-8",
                    ["WWW-Authenticate"] = $"Basic realm=\"{authResult.Realm}\"",
                    ["Server"] = settings.ServerHeader.Replace("$v", GetVersion()),
                    ["Date"] = DateTime.UtcNow.ToString("R")
                }
            };
        }

        if (authResult.IsForbidden)
        {
            // 403 Forbidden
            return HttpResponseBuilder.BuildErrorResponse(403, "Forbidden", settings);
        }

        // WebDAVチェック
        if (webDavHandler.IsWebDavRequest(request.Path, request.Method, out var webDavPhysicalPath, out var allowWrite))
        {
            return await webDavHandler.HandleWebDavAsync(request.Method, request.Path, webDavPhysicalPath, allowWrite, request, cancellationToken);
        }

        // CGIチェック
        if (cgiHandler.IsCgiRequest(request.Path, out var scriptPath, out var interpreter))
        {
            return await cgiHandler.ExecuteCgiAsync(scriptPath, interpreter, request, remoteIp ?? "127.0.0.1", cancellationToken);
        }

        // ターゲット解決（Virtual Host対応）
        var targetInfo = target.ResolveTarget(request.Path, documentRoot);

        if (!targetInfo.IsValid)
        {
            // エラーレスポンス
            var statusCode = targetInfo.ErrorMessage.Contains("Not Found") ? 404 :
                           targetInfo.ErrorMessage.Contains("Forbidden") || targetInfo.ErrorMessage.Contains("Access denied") ? 403 : 500;
            var statusText = targetInfo.ErrorMessage.Contains("Not Found") ? "Not Found" :
                            targetInfo.ErrorMessage.Contains("Forbidden") || targetInfo.ErrorMessage.Contains("Access denied") ? "Forbidden" : "Internal Server Error";

            return HttpResponseBuilder.BuildErrorResponse(statusCode, statusText, settings);
        }

        // ファイルハンドラで処理
        if (targetInfo.Type == TargetType.StaticFile || targetInfo.Type == TargetType.Directory)
        {
            return await fileHandler.HandleFileAsync(targetInfo, request, settings, cancellationToken, remoteIp);
        }

        // その他（本来ここには来ないはず）
        return HttpResponseBuilder.BuildErrorResponse(500, "Internal Server Error", settings);
    }

    private HttpResponse GenerateStatsResponse()
    {
        var stats = GetStatistics();
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>JumboDogX Server Statistics</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        h1 {{ color: #333; border-bottom: 2px solid #333; padding-bottom: 10px; }}
        ul {{ list-style: none; padding: 0; }}
        li {{ padding: 8px; margin: 5px 0; background-color: #f5f5f5; border-left: 4px solid #4CAF50; }}
        .value {{ font-weight: bold; color: #4CAF50; }}
    </style>
</head>
<body>
    <h1>JumboDogX HTTP Server Statistics</h1>
    <ul>
        <li>Active Connections: <span class=""value"">{stats.ActiveConnections}</span></li>
        <li>Total Connections: <span class=""value"">{stats.TotalConnections}</span></li>
        <li>Total Requests: <span class=""value"">{stats.TotalRequests}</span></li>
        <li>Total Bytes Sent: <span class=""value"">{stats.TotalBytesSent:N0}</span> bytes</li>
        <li>Total Errors: <span class=""value"">{stats.TotalErrors}</span></li>
        <li>Uptime: <span class=""value"">{stats.Uptime:hh\:mm\:ss}</span></li>
    </ul>
    <hr>
    <p><a href=""/"">Back to Home</a></p>
</body>
</html>";
        return HttpResponse.Ok(html);
    }

    private string GetVersion()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "9.0.0";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from settings changes before disposing the lock to prevent race conditions
            _settingsService.SettingsChanged -= OnSettingsChanged;
            _settingsLock?.Dispose();
        }
        base.Dispose(disposing);
    }
}
