# ロギング設計

## 1. 概要

JumboDogXは、Serilogを使用した構造化ロギングを採用しています。

## 2. ロギングスタック

### 2.1 使用ライブラリ

| ライブラリ | 用途 |
|-----------|------|
| Serilog | メインロギングライブラリ |
| Serilog.Extensions.Hosting | ホスト統合 |
| Serilog.Sinks.Console | コンソール出力 |
| Serilog.Sinks.File | ファイル出力 |
| Serilog.Sinks.Async | 非同期出力 |
| Serilog.Enrichers.* | コンテキスト情報付加 |

### 2.2 シンク構成

```
Log Event
    │
    ├──► Console Sink (開発環境)
    │
    ├──► File Sink (本番環境)
    │       └── ローリングファイル
    │
    └──► Web UI (LogService)
```

## 3. ログレベル

| レベル | 用途 |
|--------|------|
| Verbose | 最も詳細なデバッグ情報 |
| Debug | デバッグ情報 |
| Information | 通常の操作情報 |
| Warning | 警告（動作は継続） |
| Error | エラー（操作失敗） |
| Fatal | 致命的エラー |

## 4. 構造化ロギング

### 4.1 基本パターン

```csharp
// プロパティを構造化データとして記録
_logger.Information("Server {ServerType} started on port {Port}",
    serverType, port);

// 例外を含むログ
_logger.Error(ex, "Connection failed for {ClientAddress}",
    clientAddress);
```

### 4.2 コンテキスト情報

```csharp
using (LogContext.PushProperty("RequestId", requestId))
{
    // このスコープ内のすべてのログにRequestIdが付加される
    _logger.Information("Processing request");
}
```

## 5. Web UIログビューア

### 5.1 LogService

`Jdx.WebUI.Services.LogService` がログを収集・管理。

```csharp
public class LogService
{
    public event EventHandler<LogEntry>? LogAdded;
    public IEnumerable<LogEntry> GetRecentLogs(int count);
    public void ClearLogs();
}
```

### 5.2 ログビューア機能

- レベルフィルタ（Info, Warning, Error）
- カテゴリフィルタ
- テキスト検索
- 時刻フォーマット選択
- ログクリア（確認ダイアログ付き）

## 6. 設定

### 6.1 appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/jumbodogx-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## 7. パフォーマンス考慮事項

### 7.1 非同期ロギング

高スループット時はAsync Sinkを使用。

### 7.2 ログレベルチェック

```csharp
// 高コストな処理はレベルチェック後に実行
if (_logger.IsEnabled(LogEventLevel.Debug))
{
    var details = GenerateExpensiveDetails();
    _logger.Debug("Details: {Details}", details);
}
```

### 7.3 サンプリング

大量ログ発生時はサンプリングを検討。
