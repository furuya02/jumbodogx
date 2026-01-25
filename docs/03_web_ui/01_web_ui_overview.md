# Web UI 概要

## 1. はじめに

JumboDogX Web UIは、Blazor Serverで構築された管理インターフェースです。

### 1.1 主要機能

- サーバーの起動/停止
- リアルタイムステータス監視
- 各サーバーの設定管理
- ログビューア
- DNSレコード管理

## 2. アクセス

### 2.1 URL

```
http://localhost:5000
```

### 2.2 デフォルトポート

| 環境 | ポート |
|------|--------|
| Development | 5000 |
| Production | 5000 |

## 3. 画面構成

### 3.1 ダッシュボード

- サーバーステータス一覧
- クイック起動/停止ボタン
- 統計情報サマリー

### 3.2 設定ページ

各サーバーの詳細設定（階層構造対応）：
- HTTP設定
  - Virtual Hosts（階層的に各仮想ホストの詳細設定にアクセス可能）
- DNS設定
  - Domains（階層的に各ドメインの詳細設定にアクセス可能）
- SMTP設定
- POP3設定
- FTP設定
- TFTP設定
- DHCP設定
- プロキシ設定

### 3.3 ログビューア

- リアルタイムログ表示
- レベルフィルタ（Info, Warning, Error）
- カテゴリフィルタ
- テキスト検索
- 時刻フォーマット選択

### 3.4 DNSレコード管理

- レコードの追加/編集/削除
- レコードタイプ別表示

## 4. プロジェクト構造

```
Jdx.WebUI/
├── Components/
│   ├── App.razor              # アプリケーションルート
│   ├── Routes.razor           # ルーティング
│   ├── _Imports.razor         # グローバル using
│   ├── Layout/
│   │   ├── MainLayout.razor   # メインレイアウト
│   │   └── NavMenu.razor      # ナビゲーション
│   ├── Pages/
│   │   ├── Dashboard.razor    # ダッシュボード
│   │   ├── Logs.razor         # ログビューア
│   │   ├── DnsRecords.razor   # DNSレコード
│   │   ├── Settings/          # 設定ページ群
│   │   └── Error.razor        # エラーページ
│   └── Models/                # Razorモデル
├── Services/
│   ├── ServerManager.cs       # サーバー管理
│   ├── LogService.cs          # ログサービス
│   └── SettingsService.cs     # 設定サービス
├── wwwroot/                   # 静的ファイル
│   ├── css/
│   ├── js/
│   └── lib/                   # Bootstrap等
└── Program.cs                 # エントリポイント
```

## 5. デザインシステム

### 5.1 Modern Chicデザイン

- クリーンでモダンなUI
- レスポンシブ対応
- Bootstrap 5ベース
- カスタムCSS変数

### 5.2 CSS変数

```css
:root {
  --text-primary: #2D3748;
  --text-secondary: #718096;
  --btn-primary: #2D3748;
  --card-border: #E2E8F0;
  --z-index-modal: 1000;
}
```

## 6. サービス

### 6.1 ServerManager

サーバーのライフサイクル管理。

```csharp
public class ServerManager
{
    Task StartServerAsync(ServerType type);
    Task StopServerAsync(ServerType type);
    ServerStatus GetStatus(ServerType type);
}
```

### 6.2 LogService

ログの収集・管理。

```csharp
public class LogService
{
    event EventHandler<LogEntry>? LogAdded;
    IEnumerable<LogEntry> GetRecentLogs(int count);
    void ClearLogs();
}
```

## 7. リアルタイム更新

### 7.1 Blazor Server SignalR

- サーバーステータスの自動更新
- ログのリアルタイム表示
- 設定変更の即時反映

### 7.2 タイマーベース更新

```csharp
// 500msごとの自動更新
refreshTimer = new Timer(_ =>
{
    InvokeAsync(() =>
    {
        LoadData();
        StateHasChanged();
    });
}, null, 500, 500);
```
