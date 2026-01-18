# サーバー実装共通化リファクタリング計画

## 分析日時
2026/01/18

## 概要
全8サーバー（SMTP, POP3, FTP, DHCP, TFTP, DNS, HTTP, Proxy）の実装を分析し、共通化可能な箇所を特定。
合計78ファイル、約3,190行のサーバーコードを対象としたリファクタリング計画。

## 現状分析

### サーバー一覧と行数
| サーバー | 行数 | プロトコル | 特徴 |
|---------|------|----------|------|
| FTP | 191 | TCP | ファイル転送 |
| POP3 | 241 | TCP | メール受信 |
| DNS | 257 | UDP | 名前解決 |
| SMTP | 308 | TCP | メール送信 |
| DHCP | 385 | UDP | IP割り当て |
| TFTP | 455 | UDP | 簡易ファイル転送 |
| HTTP | 574 | TCP | Webサーバー |
| Proxy | 779 | TCP | プロキシ |

### 現在の共通基盤
- **ServerBase抽象クラス**: すべてのサーバーの基底
  - Start/Stop管理
  - ステータス管理
  - 統計情報管理
  - ヘルスチェック
- **ServerTcpListener**: TCP接続管理（FTP, HTTP, POP3, Proxy, SMTP）
- **ServerUdpListener**: UDP受信管理（DHCP, DNS, TFTP）

---

## 共通化可能なパターン

### 🔴 優先度: 高

#### 1. 接続制限セマフォパターン
**対象サーバー**: DHCP, POP3, SMTP, TFTP（4/8サーバー）

**現状の重複コード**:
```csharp
// 各サーバーで同じパターンが繰り返されている
private readonly SemaphoreSlim _connectionSemaphore;

public Server(...)
{
    _connectionSemaphore = new SemaphoreSlim(settings.MaxConnections, settings.MaxConnections);
}

// 接続処理
await _connectionSemaphore.WaitAsync(cancellationToken);
try
{
    await HandleClientAsync(...);
}
finally
{
    _connectionSemaphore.Release();
}

// Dispose
_connectionSemaphore?.Dispose();
```

**共通化案**:
```csharp
// Jdx.Core/Network/ConnectionLimiter.cs (新規)
public class ConnectionLimiter : IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    public ConnectionLimiter(int maxConnections)
    {
        _semaphore = new SemaphoreSlim(maxConnections, maxConnections);
    }

    public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        return new ReleaseHandle(_semaphore);
    }

    private class ReleaseHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        public ReleaseHandle(SemaphoreSlim semaphore) => _semaphore = semaphore;
        public void Dispose() => _semaphore.Release();
    }

    public void Dispose() => _semaphore?.Dispose();
}

// 使用例
private readonly ConnectionLimiter _connectionLimiter;

public Server(...)
{
    _connectionLimiter = new ConnectionLimiter(settings.MaxConnections);
}

// 接続処理
using (await _connectionLimiter.AcquireAsync(cancellationToken))
{
    await HandleClientAsync(...);
}
```

**効果**:
- 重複コード削減: 約40行 × 4サーバー = 160行削減
- 保守性向上: セマフォロジックの一元管理
- 将来的な拡張性: レート制限、優先度制御などの追加が容易

---

#### 2. エラーハンドリングパターン
**対象サーバー**: 全サーバー（8/8サーバー）

**現状の重複コード**:
```csharp
// 各サーバーで類似のエラーハンドリングが繰り返されている
catch (OperationCanceledException)
{
    Logger.LogDebug("Client cancelled");
}
catch (IOException ex) when (ex.InnerException is SocketException)
{
    Logger.LogDebug(ex, "Connection closed (network error)");
}
catch (SocketException ex)
{
    Logger.LogDebug(ex, "Socket error");
}
catch (Exception ex)
{
    Logger.LogWarning(ex, "Unexpected error");
}
```

**共通化案**:
```csharp
// Jdx.Core/Helpers/ExceptionHandler.cs (新規)
public static class NetworkExceptionHandler
{
    public static void LogNetworkException(
        Exception ex,
        ILogger logger,
        string context,
        params object[] args)
    {
        switch (ex)
        {
            case OperationCanceledException:
                logger.LogDebug($"{context} cancelled", args);
                break;

            case IOException ioEx when ioEx.InnerException is SocketException:
                logger.LogDebug(ioEx, $"{context} connection closed (network error)", args);
                break;

            case SocketException sockEx:
                logger.LogDebug(sockEx, $"{context} socket error", args);
                break;

            default:
                logger.LogWarning(ex, $"{context} unexpected error", args);
                break;
        }
    }

    public static bool ShouldBreakLoop(Exception ex)
    {
        return ex is OperationCanceledException;
    }
}

// 使用例
catch (Exception ex)
{
    NetworkExceptionHandler.LogNetworkException(ex, Logger, "Client handling");
    if (NetworkExceptionHandler.ShouldBreakLoop(ex))
        break;
}
```

**効果**:
- 重複コード削減: 約20行 × 8サーバー × 2-3箇所 = 320-480行削減
- 一貫性向上: すべてのサーバーで統一されたエラーログ
- 保守性向上: ログレベルやメッセージの変更が一箇所で完結

---

#### 3. BindAddress解析パターン
**対象サーバー**: 全サーバー（8/8サーバー）

**現状の重複コード**:
```csharp
// 各サーバーで同様のBindAddress解析が繰り返されている
IPAddress bindAddress;
if (string.IsNullOrWhiteSpace(_settings.BindAddress) || _settings.BindAddress == "0.0.0.0")
{
    bindAddress = IPAddress.Any;
}
else if (!IPAddress.TryParse(_settings.BindAddress, out bindAddress))
{
    Logger.LogWarning("Invalid bind address '{Address}', using Any", _settings.BindAddress);
    bindAddress = IPAddress.Any;
}
```

**共通化案**:
```csharp
// Jdx.Core/Helpers/NetworkHelper.cs (新規)
public static class NetworkHelper
{
    public static IPAddress ParseBindAddress(
        string? bindAddress,
        ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(bindAddress) || bindAddress == "0.0.0.0")
        {
            return IPAddress.Any;
        }

        if (IPAddress.TryParse(bindAddress, out var result))
        {
            return result;
        }

        logger?.LogWarning("Invalid bind address '{Address}', using Any", bindAddress);
        return IPAddress.Any;
    }
}

// 使用例
var bindAddress = NetworkHelper.ParseBindAddress(_settings.BindAddress, Logger);
_listener = new ServerTcpListener(_port, bindAddress, Logger);
```

**効果**:
- 重複コード削減: 約10行 × 8サーバー = 80行削減
- 一貫性向上: すべてのサーバーで同じ解析ロジック
- テスト容易性: 単一メソッドのテストで全体をカバー

---

### 🟡 優先度: 中

#### 4. リスナー初期化・停止パターン
**対象サーバー**: 全サーバー（8/8サーバー）

**現状の重複コード**:
```csharp
// StartListeningAsync
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

_listener = new ServerTcpListener(_port, bindAddress, Logger);
await _listener.StartAsync(cancellationToken);

// StopListeningAsync
if (_listener != null)
{
    await _listener.StopAsync(cancellationToken);
    _listener.Dispose();
    _listener = null;
}
```

**共通化案**:
```csharp
// ServerBase に追加
protected async Task<ServerTcpListener> CreateTcpListenerAsync(
    int port,
    string? bindAddress,
    CancellationToken cancellationToken)
{
    await StopExistingListenerAsync(_tcpListener);

    var ipAddress = NetworkHelper.ParseBindAddress(bindAddress, Logger);
    var listener = new ServerTcpListener(port, ipAddress, Logger);
    await listener.StartAsync(cancellationToken);

    _tcpListener = listener;
    return listener;
}

protected async Task<ServerUdpListener> CreateUdpListenerAsync(
    int port,
    string? bindAddress,
    CancellationToken cancellationToken)
{
    await StopExistingListenerAsync(_udpListener);

    var ipAddress = NetworkHelper.ParseBindAddress(bindAddress, Logger);
    var listener = new ServerUdpListener(port, ipAddress, Logger);
    await listener.StartAsync(cancellationToken);

    _udpListener = listener;
    return listener;
}

private async Task StopExistingListenerAsync(IDisposable? listener)
{
    if (listener == null) return;

    try
    {
        if (listener is ServerTcpListener tcp)
            await tcp.StopAsync(CancellationToken.None);
        else if (listener is ServerUdpListener udp)
            await udp.StopAsync(CancellationToken.None);

        listener.Dispose();
    }
    catch (Exception ex)
    {
        Logger.LogWarning(ex, "Error stopping existing listener");
    }
}
```

**効果**:
- 重複コード削減: 約15行 × 8サーバー = 120行削減
- 一貫性向上: リスナーライフサイクル管理の統一
- バグリスク低減: エラーハンドリングの漏れ防止

---

#### 5. Accept/Receiveループパターン
**対象サーバー**: 全サーバー（8/8サーバー）

**現状の重複コード**:
```csharp
// TCPサーバーの場合
while (!cancellationToken.IsCancellationRequested)
{
    try
    {
        var client = await _listener.AcceptAsync(cancellationToken);
        _ = Task.Run(async () =>
        {
            await _connectionSemaphore.WaitAsync(cancellationToken);
            try
            {
                await HandleClientAsync(client, cancellationToken);
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }, cancellationToken);
    }
    catch (OperationCanceledException)
    {
        break;
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error accepting client");
    }
}

// UDPサーバーの場合
while (!cancellationToken.IsCancellationRequested)
{
    try
    {
        var (data, remoteEndPoint) = await _listener.ReceiveAsync(cancellationToken);
        _ = Task.Run(async () =>
        {
            await _connectionSemaphore.WaitAsync(cancellationToken);
            try
            {
                await HandleRequestAsync(data, remoteEndPoint, cancellationToken);
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }, cancellationToken);
    }
    catch (OperationCanceledException)
    {
        break;
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error receiving request");
    }
}
```

**共通化案**:
```csharp
// ServerBase に追加
protected async Task RunTcpAcceptLoopAsync(
    ServerTcpListener listener,
    Func<Socket, CancellationToken, Task> handler,
    ConnectionLimiter? limiter,
    CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            var client = await listener.AcceptAsync(cancellationToken);

            _ = Task.Run(async () =>
            {
                IDisposable? limitHandle = null;
                try
                {
                    if (limiter != null)
                        limitHandle = await limiter.AcquireAsync(cancellationToken);

                    await handler(client, cancellationToken);
                }
                catch (Exception ex)
                {
                    NetworkExceptionHandler.LogNetworkException(ex, Logger, "Client handling");
                }
                finally
                {
                    limitHandle?.Dispose();
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error accepting client");
        }
    }
}

protected async Task RunUdpReceiveLoopAsync(
    ServerUdpListener listener,
    Func<byte[], EndPoint, CancellationToken, Task> handler,
    ConnectionLimiter? limiter,
    CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            var (data, remoteEndPoint) = await listener.ReceiveAsync(cancellationToken);

            _ = Task.Run(async () =>
            {
                IDisposable? limitHandle = null;
                try
                {
                    if (limiter != null)
                        limitHandle = await limiter.AcquireAsync(cancellationToken);

                    await handler(data, remoteEndPoint, cancellationToken);
                }
                catch (Exception ex)
                {
                    NetworkExceptionHandler.LogNetworkException(ex, Logger, "Request handling");
                }
                finally
                {
                    limitHandle?.Dispose();
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error receiving request");
        }
    }
}

// 使用例
protected override async Task StartListeningAsync(CancellationToken cancellationToken)
{
    var listener = await CreateTcpListenerAsync(_port, _settings.BindAddress, cancellationToken);
    _ = Task.Run(() => RunTcpAcceptLoopAsync(listener, HandleClientAsync, _connectionLimiter, StopCts.Token));
}
```

**効果**:
- 重複コード削減: 約30行 × 8サーバー = 240行削減
- バグリスク低減: キャンセル処理、エラーハンドリングの統一
- 保守性向上: ループロジックの一元管理

---

### 🟢 優先度: 低

#### 6. 設定変更通知パターン
**対象サーバー**: HTTP, Proxy（実装済み）、他サーバーへの展開

**現状**:
- HTTP, Proxyのみが`ISettingsService.SettingsChanged`イベントを購読
- 他のサーバーは設定変更時に再起動が必要

**共通化案**:
```csharp
// ServerBase に追加
protected virtual void OnSettingsChanged(ApplicationSettings settings)
{
    // 派生クラスでオーバーライド可能
    // デフォルト実装は何もしない
}

protected void SubscribeToSettingsChanges(ISettingsService settingsService)
{
    settingsService.SettingsChanged += (sender, settings) =>
    {
        OnSettingsChanged(settings);
    };
}

// 各サーバーでの使用例
public SmtpServer(ILogger<SmtpServer> logger, ISettingsService settingsService, SmtpServerSettings settings)
    : base(logger)
{
    _settings = settings;
    SubscribeToSettingsChanges(settingsService);
}

protected override void OnSettingsChanged(ApplicationSettings settings)
{
    var newSettings = settings.SmtpServer;

    // 設定を更新（必要に応じて再起動）
    if (_settings.Port != newSettings.Port)
    {
        // ポート変更時は再起動が必要
        Logger.LogInformation("Port changed, restart required");
    }

    _settings = newSettings;
}
```

**効果**:
- 一貫性向上: すべてのサーバーで設定変更対応
- ユーザビリティ向上: 再起動なしの設定変更が可能
- 将来的な拡張性: ホットリロード機能の基盤

---

## 実装優先順位と影響範囲

### Phase 1: 基盤整備（影響: 低、効果: 高）
1. **NetworkHelper.ParseBindAddress** 作成
   - 新規ファイル: `Jdx.Core/Helpers/NetworkHelper.cs`
   - 影響: なし（新規追加）
   - 効果: 即座に全サーバーで使用可能

2. **ConnectionLimiter** 作成
   - 新規ファイル: `Jdx.Core/Network/ConnectionLimiter.cs`
   - 影響: なし（新規追加）
   - 効果: 接続制限ロジックの一元化

3. **NetworkExceptionHandler** 作成
   - 新規ファイル: `Jdx.Core/Helpers/NetworkExceptionHandler.cs`
   - 影響: なし（新規追加）
   - 効果: エラーハンドリングの統一

### Phase 2: ServerBase拡張（影響: 中、効果: 高）
4. **リスナー管理メソッド** を ServerBase に追加
   - 修正ファイル: `Jdx.Core/Abstractions/ServerBase.cs`
   - 影響: 既存コードへの影響なし（新規メソッド追加）
   - 効果: リスナーライフサイクル管理の統一

5. **Accept/Receiveループメソッド** を ServerBase に追加
   - 修正ファイル: `Jdx.Core/Abstractions/ServerBase.cs`
   - 影響: 既存コードへの影響なし（新規メソッド追加）
   - 効果: ループロジックの一元化

### Phase 3: 各サーバーのリファクタリング（影響: 高、効果: 高）
6. **各サーバーで新しいヘルパーを使用**
   - 修正対象: 全8サーバー
   - 影響: 大（各サーバーのコード変更）
   - 効果: 重複コード削減、保守性向上

### Phase 4: オプショナル機能（影響: 低、効果: 中）
7. **設定変更通知** の全サーバー展開
   - 修正対象: SMTP, POP3, FTP, DHCP, TFTP, DNS
   - 影響: 中（各サーバーのコンストラクタ変更）
   - 効果: ホットリロード機能の基盤

---

## リスク分析

### 低リスク
- **Phase 1（基盤整備）**: 新規ファイル追加のみ、既存コードへの影響なし
- テスト: 単体テストで完全カバレッジ可能

### 中リスク
- **Phase 2（ServerBase拡張）**: 新規メソッド追加のみ、既存の動作に影響なし
- テスト: 統合テストで検証

### 高リスク
- **Phase 3（各サーバーのリファクタリング）**: 既存コードの大幅な変更
- リスク軽減策:
  - サーバーごとに段階的に適用
  - 各サーバーで既存の動作テストを実施
  - ロールバック可能な状態を維持

---

## 期待効果

### コード削減
- **Phase 1完了時**: 約240行削減（NetworkHelper + エラーハンドリング）
- **Phase 2完了時**: 約360行削減（リスナー管理 + ループ）
- **Phase 3完了時**: 約900-1,100行削減（全サーバー適用）
- **合計**: 約1,500行削減（現在の約47%削減）

### 品質向上
- バグリスク低減: エラーハンドリング、リソース管理の統一
- 保守性向上: 共通ロジックの一元管理
- テスト容易性: 共通部分のテストで全体をカバー

### 開発効率
- 新規サーバー追加時の開発時間短縮
- バグ修正時の影響範囲の明確化
- コードレビューの効率化

---

## 実装例: SMTP サーバーのBefore/After

### Before（現状）
```csharp
public class SmtpServer : ServerBase
{
    private readonly SmtpServerSettings _settings;
    private ServerTcpListener? _tcpListener;
    private readonly SemaphoreSlim _connectionSemaphore;

    public SmtpServer(ILogger<SmtpServer> logger, SmtpServerSettings settings)
        : base(logger)
    {
        _settings = settings;
        _connectionSemaphore = new SemaphoreSlim(settings.MaxConnections, settings.MaxConnections);
    }

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        var bindAddress = string.IsNullOrWhiteSpace(_settings.BindAddress) || _settings.BindAddress == "0.0.0.0"
            ? IPAddress.Any
            : IPAddress.Parse(_settings.BindAddress);

        _tcpListener = new ServerTcpListener(_settings.Port, bindAddress, Logger);
        await _tcpListener.StartAsync(cancellationToken);

        Logger.LogInformation("SMTP Server started on {Address}:{Port} (Domain: {Domain})",
            _settings.BindAddress, _settings.Port, _settings.DomainName);

        _ = Task.Run(() => AcceptLoopAsync(StopCts.Token), StopCts.Token);
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        if (_tcpListener == null)
            return;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var clientSocket = await _tcpListener.AcceptAsync(cancellationToken);
                var client = new TcpClient { Client = clientSocket };

                _ = Task.Run(async () =>
                {
                    await _connectionSemaphore.WaitAsync(cancellationToken);
                    try
                    {
                        await HandleClientInternalAsync(client, cancellationToken);
                    }
                    finally
                    {
                        _connectionSemaphore.Release();
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error accepting SMTP client");
            }
        }
    }

    private async Task HandleClientInternalAsync(TcpClient client, CancellationToken cancellationToken)
    {
        // ... 300行のクライアント処理 ...

        catch (OperationCanceledException)
        {
            Logger.LogDebug("SMTP client cancelled");
        }
        catch (IOException ex) when (ex.InnerException is SocketException)
        {
            Logger.LogDebug(ex, "SMTP client connection closed (network error)");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unexpected error handling SMTP client");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tcpListener?.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
            _connectionSemaphore?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

### After（リファクタリング後）
```csharp
public class SmtpServer : ServerBase
{
    private readonly SmtpServerSettings _settings;
    private ServerTcpListener? _tcpListener;
    private readonly ConnectionLimiter _connectionLimiter;

    public SmtpServer(ILogger<SmtpServer> logger, SmtpServerSettings settings)
        : base(logger)
    {
        _settings = settings;
        _connectionLimiter = new ConnectionLimiter(settings.MaxConnections);
    }

    protected override async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        _tcpListener = await CreateTcpListenerAsync(
            _settings.Port,
            _settings.BindAddress,
            cancellationToken);

        Logger.LogInformation("SMTP Server started on {Address}:{Port} (Domain: {Domain})",
            _settings.BindAddress, _settings.Port, _settings.DomainName);

        _ = Task.Run(() => RunTcpAcceptLoopAsync(
            _tcpListener,
            HandleClientAsync,
            _connectionLimiter,
            StopCts.Token));
    }

    protected override async Task HandleClientAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        var client = new TcpClient { Client = clientSocket };

        try
        {
            // ... 300行のクライアント処理（変更なし） ...
        }
        catch (Exception ex)
        {
            NetworkExceptionHandler.LogNetworkException(ex, Logger, "SMTP client handling");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connectionLimiter?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

**削減内容**:
- AcceptLoopAsync メソッド（約40行）→ 削除（ServerBaseに集約）
- エラーハンドリング（約15行）→ 1行に簡略化
- BindAddress解析（約10行）→ 削除（NetworkHelperに集約）
- セマフォ管理（約10行）→ 削除（ConnectionLimiterに集約）
- **合計: 約75行削減（約24%削減）**

---

## テスト戦略

### Phase 1: 単体テスト
```csharp
// NetworkHelperTest.cs
[Fact]
public void ParseBindAddress_ShouldReturnAny_WhenNullOrEmpty()
{
    var result = NetworkHelper.ParseBindAddress(null);
    Assert.Equal(IPAddress.Any, result);
}

[Fact]
public void ParseBindAddress_ShouldReturnParsedAddress_WhenValid()
{
    var result = NetworkHelper.ParseBindAddress("192.168.1.1");
    Assert.Equal(IPAddress.Parse("192.168.1.1"), result);
}

// ConnectionLimiterTest.cs
[Fact]
public async Task AcquireAsync_ShouldRespectMaxConnections()
{
    var limiter = new ConnectionLimiter(2);

    using (await limiter.AcquireAsync(CancellationToken.None))
    using (await limiter.AcquireAsync(CancellationToken.None))
    {
        var task = limiter.AcquireAsync(CancellationToken.None);
        await Task.Delay(100);
        Assert.False(task.IsCompleted); // 3つ目は待機状態
    }
}
```

### Phase 2: 統合テスト
```csharp
// ServerBaseTest.cs
[Fact]
public async Task RunTcpAcceptLoopAsync_ShouldHandleClients()
{
    var server = new TestServer(logger);
    var listener = new ServerTcpListener(port, IPAddress.Any, logger);
    await listener.StartAsync(CancellationToken.None);

    var cts = new CancellationTokenSource();
    var loopTask = server.RunTcpAcceptLoopAsync(
        listener,
        (socket, ct) => Task.CompletedTask,
        null,
        cts.Token);

    // クライアント接続テスト
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, port);

    cts.Cancel();
    await loopTask;
}
```

### Phase 3: 既存動作テスト
各サーバーの既存機能テストを実施:
- SMTP: メール送信テスト
- POP3: メール受信テスト
- HTTP: Webリクエストテスト
- など

---

## 次のステップ

### 即座に実施可能
1. **NetworkHelper.cs** の作成と単体テスト
2. **ConnectionLimiter.cs** の作成と単体テスト
3. **NetworkExceptionHandler.cs** の作成と単体テスト

### 段階的に実施
4. SMTPサーバーで試験的に適用（最もシンプル）
5. POP3サーバーで適用（SMTPと類似）
6. 他のサーバーに順次展開

### レビューポイント
- Phase 1完了後: 基盤クラスのコードレビュー
- 各サーバー適用後: 動作確認とパフォーマンステスト
- Phase 3完了後: 全体の統合テストとドキュメント更新

---

## 補足: 他の共通化候補

### 定数管理
各サーバーで定義されている定数（タイムアウト、バッファサイズなど）の一部は共通化可能:
```csharp
// Jdx.Core/Constants/NetworkConstants.cs
public static class NetworkConstants
{
    public const int DefaultTimeoutSeconds = 30;
    public const int DefaultBufferSize = 8192;
    public const int MaxLineLength = 8192; // HTTP/SMTP/POP3など
}
```

### ログメッセージのテンプレート化
```csharp
// Jdx.Core/Helpers/LogTemplates.cs
public static class ServerLogTemplates
{
    public static void LogServerStarted(ILogger logger, string serverName, int port)
    {
        logger.LogInformation("{ServerName} started on port {Port}", serverName, port);
    }

    public static void LogClientConnected(ILogger logger, string serverName, EndPoint? endpoint)
    {
        logger.LogInformation("{ServerName} client connected from {RemoteEndPoint}", serverName, endpoint);
    }
}
```

---

## 結論

このリファクタリング計画により、以下の成果が期待できます:

1. **約1,500行（47%）のコード削減**
2. **保守性の大幅向上** - 共通ロジックの一元管理
3. **バグリスクの低減** - エラーハンドリング、リソース管理の統一
4. **開発効率の向上** - 新規サーバー追加時の工数削減
5. **テスト容易性** - 共通部分のテストで広範囲をカバー

リスクを最小化するため、段階的なアプローチ（Phase 1→2→3）を推奨します。
各Phaseで十分なテストを実施し、問題があれば即座にロールバック可能な状態を維持します。
