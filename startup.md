# JumboDogX 起動・動作確認手順書

## 前提条件

### 必須環境
- **オペレーティングシステム**: macOS, Windows, または Linux
- **.NET 9 SDK**: バージョン 9.0.100 以上

### .NET 9 SDK のインストール確認

```bash
dotnet --version
```

期待される出力: `9.0.xxx` (例: `9.0.112`)

### .NET 9 SDK がインストールされていない場合

**macOS (Homebrew):**
```bash
brew install dotnet@9
```

**Windows/その他:**
https://dotnet.microsoft.com/download/dotnet/9.0 からダウンロードしてインストール

---

## プロジェクトのビルド

### 1. プロジェクトディレクトリに移動

```bash
cd jumbodogx
```

### 2. ソリューション全体をビルド

```bash
dotnet build Jdx.sln
```

**期待される出力:**
```
ビルドに成功しました。
    0 個の警告
    0 エラー
```

### 3. ビルドエラーが発生した場合

#### .NET 9 SDK が見つからない場合
```bash
# Homebrew でインストールした場合の dotnet パスを確認
which dotnet

# Homebrew の dotnet@9 を使用する場合
/opt/homebrew/Cellar/dotnet@9/9.0.112/bin/dotnet build Jdx.sln
```

#### NuGet パッケージの復元エラー
```bash
dotnet restore Jdx.sln
dotnet build Jdx.sln
```

---

## サーバーの起動

JumboDogX は2つの起動方法があります：

### 方法1: CLI版（コマンドラインのみ）

コマンドラインでサーバーを管理する方法です。

```bash
dotnet run --project src/Jdx.Host
```

**期待される出力:**
```
JumboDogX - Multi-Server Application
================================

info: Jdx.Servers.Dns.DnsServer[0]
      DNS record added: example.com -> 192.0.2.1
info: Jdx.Servers.Dns.DnsServer[0]
      DNS record added: jdx.local -> 127.0.0.1
info: Jdx.Servers.Dns.DnsServer[0]
      DNS record added: test.local -> 192.168.1.100
info: Jdx.Servers.Http.HttpServer[0]
      Starting server HttpServer on port 8080
info: Jdx.Servers.Http.HttpServer[0]
      HTTP Server listening on http://localhost:8080
info: Jdx.Servers.Http.HttpServer[0]
      Server HttpServer started successfully
info: Jdx.Servers.Dns.DnsServer[0]
      Starting server DnsServer on port 5300
info: Jdx.Servers.Dns.DnsServer[0]
      DNS Server listening on port 5300

All servers started. Press Ctrl+C to stop.
  - HTTP Server: http://localhost:8080
  - DNS Server: port 5300 (use: dig @localhost -p 5300 example.com)

info: Jdx.Servers.Dns.DnsServer[0]
      Server DnsServer started successfully
```

**特徴:**
- ✅ シンプルで軽量
- ✅ サーバーが自動的に起動
- ❌ ブラウザでの管理不可
- ❌ サーバーの個別起動/停止不可

### 方法2: Web UI版（ブラウザで管理）**推奨**

ブラウザからサーバーを管理する方法です。

```bash
dotnet run --project src/Jdx.WebUI --urls "http://localhost:5001"
```

**期待される出力:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Jdx.Servers.Dns.DnsServer[0]
      DNS record added: example.com -> 192.0.2.1
info: Jdx.Servers.Dns.DnsServer[0]
      DNS record added: jdx.local -> 127.0.0.1
info: Jdx.Servers.Dns.DnsServer[0]
      DNS record added: test.local -> 192.168.1.100
info: Jdx.WebUI.Services.ServerManager[0]
      ServerManager initialized with 2 servers
```

**起動後、ブラウザで以下にアクセス:**
```
http://localhost:5001
```

**特徴:**
- ✅ ブラウザで直感的に操作
- ✅ サーバーの個別起動/停止が可能
- ✅ リアルタイムでログ確認
- ✅ DNS レコード管理が簡単
- ❌ Web UI のオーバーヘッド

### Homebrew の .NET 9 を使用する場合

```bash
# CLI版
/opt/homebrew/Cellar/dotnet@9/9.0.112/bin/dotnet run --project src/Jdx.Host

# Web UI版
/opt/homebrew/Cellar/dotnet@9/9.0.112/bin/dotnet run --project src/Jdx.WebUI --urls "http://localhost:5001"
```

---

## 動作確認

サーバーが起動したら、**別のターミナル**を開いて以下のテストを実行してください。

### HTTP サーバーの動作確認

#### 1. Welcome ページのテスト

```bash
curl http://localhost:8080/
```

**期待される出力:**
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>JumboDogX HTTP Server</title>
</head>
<body>
    <h1>Welcome to JumboDogX HTTP Server!</h1>
    <p>This is a simple HTTP server built with .NET 9</p>
    <p>Server Status: Running</p>
    <hr>
    <p><a href="/stats">View Statistics</a></p>
</body>
</html>
```

#### 2. 統計情報ページのテスト

```bash
curl http://localhost:8080/stats
```

**期待される出力:**
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>JumboDogX Server Statistics</title>
</head>
<body>
    <h1>Server Statistics</h1>
    <ul>
        <li>Active Connections: 1</li>
        <li>Total Connections: 2</li>
        <li>Total Requests: 2</li>
        <li>Total Bytes Sent: 391 bytes</li>
        <li>Total Errors: 0</li>
        <li>Uptime: 00:00:15</li>
    </ul>
    <hr>
    <p><a href="/">Back to Home</a></p>
</body>
</html>
```

#### 3. ブラウザでのテスト

ブラウザで以下の URL を開く:
- http://localhost:8080/
- http://localhost:8080/stats

#### 4. 404 エラーのテスト

```bash
curl http://localhost:8080/notfound
```

**期待される出力:**
```html
<html><body><h1>404 Not Found</h1></body></html>
```

### DNS サーバーの動作確認

#### 1. dig コマンドのインストール確認

```bash
dig -v
```

dig がインストールされていない場合:
```bash
# macOS
brew install bind

# Ubuntu/Debian
sudo apt-get install dnsutils
```

#### 2. 登録済みドメインのテスト

```bash
# example.com のテスト
dig @127.0.0.1 -p 5300 example.com +short
# 期待される出力: 192.0.2.1

# jdx.local のテスト
dig @127.0.0.1 -p 5300 jdx.local +short
# 期待される出力: 127.0.0.1

# test.local のテスト
dig @127.0.0.1 -p 5300 test.local +short
# 期待される出力: 192.168.1.100
```

#### 3. 未登録ドメインのテスト

```bash
dig @127.0.0.1 -p 5300 unknown.domain +short
# 期待される出力: 0.0.0.0 (デフォルトIP)
```

#### 4. 詳細な DNS レスポンスの確認

```bash
dig @127.0.0.1 -p 5300 example.com
```

**期待される出力:**
```
;; QUESTION SECTION:
;example.com.                   IN      A

;; ANSWER SECTION:
example.com.            300     IN      A       192.0.2.1
```

### サーバーログの確認

サーバーを起動しているターミナルで、リクエストのログが表示されていることを確認:

```
info: Jdx.Servers.Http.HttpServer[0]
      HTTP request from 127.0.0.1:xxxxx
info: Jdx.Servers.Http.HttpServer[0]
      HTTP GET / from 127.0.0.1:xxxxx
info: Jdx.Servers.Http.HttpServer[0]
      HTTP 200 GET /

info: Jdx.Servers.Dns.DnsServer[0]
      DNS query for example.com (type=1) from 127.0.0.1:xxxxx
info: Jdx.Servers.Dns.DnsServer[0]
      DNS response sent: example.com -> 192.0.2.1
```

---

## Web UI の使用方法

Web UI版（`Jdx.WebUI`）を起動している場合、ブラウザから直感的にサーバーを管理できます。

### アクセス方法

ブラウザで以下の URL を開く:
```
http://localhost:5001
```

### 利用可能なページ

#### 1. Dashboard（トップページ）

**URL:** http://localhost:5001/

**機能:**
- サーバー一覧表示（HTTP, DNS）
- 各サーバーの状態表示
  - 🟢 Running（稼働中）
  - ⚫ Stopped（停止中）
  - 🟡 Starting/Stopping（起動中/停止中）
  - 🔴 Error（エラー）
- リアルタイム統計情報
  - アクティブ接続数 / 総接続数
  - リクエスト数
  - 送信バイト数
  - エラー数
  - 稼働時間（hh:mm:ss形式）
- サーバー操作ボタン
  - ▶️ Start（個別起動）
  - ⏹️ Stop（個別停止）
  - ▶️ Start All（全サーバー起動）
  - ⏹️ Stop All（全サーバー停止）
  - 🔄 Refresh（手動更新）

**使用例:**
1. ブラウザで http://localhost:5001 にアクセス
2. HTTP サーバーの「▶️ Start」ボタンをクリック
3. サーバーが起動し、統計情報がリアルタイムで更新される
4. http://localhost:8080 で HTTP サーバーにアクセス可能になる

**自動更新:**
- 統計情報は1秒ごとに自動更新されます

#### 2. Logs（ログ表示）

**URL:** http://localhost:5001/logs

**機能:**
- リアルタイムログストリーム
- ログレベルフィルタリング
  - All（全て）
  - Info（情報）
  - Warning（警告）
  - Error（エラー）
- ログクリア機能
- カラーコーディング
  - ❌ Error/Critical（赤）
  - ⚠️ Warning（黄）
  - ℹ️ Information（青）
  - 🔍 Debug/Trace（グレー）

**使用例:**
1. http://localhost:5001/logs にアクセス
2. 「Error」フィルタを選択してエラーログのみ表示
3. サーバーの動作をリアルタイムで監視

**自動更新:**
- ログは500msごとに自動更新されます
- 最新100件のログが表示されます

#### 3. DNS Records（DNS レコード管理）

**URL:** http://localhost:5001/dns

**機能:**
- DNS レコード一覧表示
- レコード追加
  - ドメイン名入力
  - IP アドレス入力
  - バリデーション（IP形式チェック）
- レコード削除
- テストコマンド表示

**使用例:**
1. http://localhost:5001/dns にアクセス
2. 「Domain Name」に `mytest.local` を入力
3. 「IP Address」に `192.168.1.200` を入力
4. 「Add Record」ボタンをクリック
5. ターミナルで動作確認:
   ```bash
   dig @127.0.0.1 -p 5300 mytest.local +short
   # 期待される出力: 192.168.1.200
   ```

**初期レコード:**
- example.com → 192.0.2.1
- jdx.local → 127.0.0.1
- test.local → 192.168.1.100

### Web UI での動作確認フロー

**推奨テスト手順:**

1. **Web UI を起動**
   ```bash
   dotnet run --project src/Jdx.WebUI --urls "http://localhost:5001"
   ```

2. **Dashboard でサーバーを起動**
   - ブラウザで http://localhost:5001 にアクセス
   - HTTP サーバーの「▶️ Start」をクリック
   - DNS サーバーの「▶️ Start」をクリック

3. **HTTP サーバーをテスト**
   - 新しいタブで http://localhost:8080 にアクセス
   - Welcome ページが表示されることを確認
   - http://localhost:8080/stats で統計情報を確認

4. **Dashboard で統計情報を確認**
   - Dashboard に戻る
   - HTTP サーバーのカードに統計が表示されていることを確認
   - Total Requests が増えていることを確認

5. **Logs でログを確認**
   - http://localhost:5001/logs にアクセス
   - HTTP リクエストのログが表示されることを確認

6. **DNS レコードを追加**
   - http://localhost:5001/dns にアクセス
   - 新しいレコードを追加
   - ターミナルで dig コマンドでテスト

7. **サーバーを停止**
   - Dashboard に戻る
   - 「⏹️ Stop All」をクリック
   - 全サーバーが停止することを確認

---

## サーバーの停止

サーバーを起動しているターミナルで、**Ctrl+C** を押す。

**期待される出力:**
```
Shutdown signal received. Stopping servers...
Shutting down...
All servers stopped.
```

---

## トラブルシューティング

### ポートがすでに使用されている

**エラーメッセージ:**
```
System.Net.Sockets.SocketException: Address already in use
```

**対処法:**

#### HTTP サーバー（ポート 8080）が使用中の場合
```bash
# macOS/Linux
lsof -i :8080
kill -9 <PID>

# Windows
netstat -ano | findstr :8080
taskkill /PID <PID> /F
```

#### DNS サーバー（ポート 5300）が使用中の場合
```bash
# macOS/Linux
lsof -i :5300
kill -9 <PID>

# Windows
netstat -ano | findstr :5300
taskkill /PID <PID> /F
```

#### Web UI（ポート 5001）が使用中の場合
```bash
# macOS/Linux
lsof -i :5001
kill -9 <PID>

# Windows
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# または別のポートで起動
dotnet run --project src/Jdx.WebUI --urls "http://localhost:5002"
```

### Web UI にアクセスできない

**症状:**
- ブラウザで http://localhost:5001 にアクセスできない
- 「接続できません」エラーが表示される

**対処法:**

1. **Web UI が起動しているか確認**
   ```bash
   # ターミナルで以下のメッセージを確認
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: http://localhost:5001
   ```

2. **別のターミナルで確認**
   ```bash
   curl http://localhost:5001/
   ```
   - 成功すれば HTML が返ってくる
   - 失敗すれば Web UI が起動していない

3. **ポートが正しいか確認**
   - 起動時に `--urls "http://localhost:5001"` を指定しているか確認
   - 指定していない場合、デフォルトポート（5000または5001）で起動している可能性

4. **ファイアウォールを確認**
   - macOS: システム環境設定 → セキュリティとプライバシー → ファイアウォール
   - ローカルホスト接続を許可する

5. **ブラウザのキャッシュをクリア**
   - ハードリロード（Cmd+Shift+R / Ctrl+Shift+R）を試す

### .NET 9 SDK が見つからない

**エラーメッセージ:**
```
A compatible .NET SDK was not found.
Requested SDK version: 9.0.100
```

**対処法:**

1. .NET 9 SDK がインストールされているか確認:
```bash
dotnet --list-sdks
```

2. インストールされていない場合、インストール:
```bash
# macOS
brew install dotnet@9

# Windows/その他
https://dotnet.microsoft.com/download/dotnet/9.0
```

3. global.json を一時的に変更（既存の SDK を使用）:
```bash
# global.json を開いて、version を既存の SDK に変更
# 例: "version": "7.0.309" または "version": "6.0.415"
```

### curl / dig コマンドが見つからない

#### curl がない場合
```bash
# macOS (通常はプリインストール済み)
brew install curl

# Ubuntu/Debian
sudo apt-get install curl

# Windows
# PowerShell の Invoke-WebRequest を使用
Invoke-WebRequest -Uri http://localhost:8080/
```

#### dig がない場合
```bash
# macOS
brew install bind

# Ubuntu/Debian
sudo apt-get install dnsutils

# Windows
# nslookup を使用（dig の代替）
nslookup example.com 127.0.0.1
```

### DNS クエリがタイムアウトする

**症状:**
```
;; connection timed out; no servers could be reached
```

**原因と対処法:**

1. **localhost の名前解決の問題**
   - `@localhost` の代わりに `@127.0.0.1` を使用
   ```bash
   dig @127.0.0.1 -p 5300 example.com +short
   ```

2. **ファイアウォール**
   - macOS: システム環境設定 → セキュリティとプライバシー → ファイアウォール
   - ローカル接続を許可する設定を確認

3. **DNS サーバーが起動していない**
   - サーバーのログで "DNS Server listening on port 5300" を確認

---

## 現在の実装状況

### 実装済み機能

#### HTTP サーバー
- ✅ 基本的な HTTP/1.1 対応
- ✅ ルーティング機能（/, /stats）
- ✅ Welcome ページ
- ✅ 統計情報ページ
- ✅ 404 Not Found 応答
- ❌ 静的ファイル配信（未実装）
- ❌ POST リクエスト処理（未実装）

#### DNS サーバー
- ✅ UDP ベースの DNS プロトコル
- ✅ A レコードクエリ対応
- ✅ レコード管理（追加/削除）
- ✅ デフォルト IP 応答（未登録ドメイン）
- ❌ キャッシュ機能（未実装）
- ❌ AAAA/MX/CNAME レコード（未実装）

#### アーキテクチャ
- ✅ IServer 統一インターフェース
- ✅ ServerBase 抽象クラス（Template Method パターン）
- ✅ TCP/UDP 両対応のリスナー実装
- ✅ 構造化ロギング
- ✅ 統計情報の収集
- ✅ 複数サーバーの同時実行

#### Web UI（Blazor Server）
- ✅ Dashboard ページ
  - サーバー一覧表示
  - リアルタイム統計情報
  - サーバー起動/停止ボタン
  - 全サーバー一括操作
  - 1秒ごとの自動更新
- ✅ Logs ページ
  - リアルタイムログ表示
  - ログレベルフィルタリング
  - ログクリア機能
  - 500msごとの自動更新
- ✅ DNS Records ページ
  - レコード一覧表示
  - レコード追加/削除機能
  - テストコマンド表示
- ✅ ServerManager サービス
  - サーバーライフサイクル管理
  - イベント通知
- ✅ LogService
  - ログ収集・配信

### 未実装機能
- Generic Host への移行（CLI版）
- appsettings.json による設定管理
- 静的ファイル配信（HTTP サーバー）
- SMTP, POP3, FTP サーバー
- Proxy サーバー群
- テストコード
- Web UI 認証機能

---

## 次のステップ

### 短期（Phase C 完成）
1. **静的ファイル配信の実装** - HTTP サーバーで HTML/CSS/JS ファイルを配信
2. **Generic Host への移行** - Microsoft.Extensions.Hosting を使用した起動方式（CLI版）
3. **appsettings.json 対応** - 設定ファイルからポート番号等を読み込み
4. **Web UI 認証機能** - 管理画面へのアクセス制御
5. **テストコード実装** - xUnit による単体テスト・統合テスト

### 中期（Phase B）
6. **SMTP サーバーの実装** - メール送受信機能
7. **POP3 サーバーの実装** - メール取得機能
8. **Web UI の拡張** - 各サーバーの詳細設定画面

### 長期（Phase A）
9. **FTP サーバーの実装** - ファイル転送機能
10. **Proxy サーバー群の実装** - HTTP/FTP/SMTP/POP3/Telnet プロキシ
11. **パフォーマンス最適化** - プロファイリング、ボトルネック改善

詳細は `tmp/todo.md` を参照してください。

---

## 参考情報

### プロジェクト構造
```
jdx/
├── src/
│   ├── Jdx.Core/              # コア機能（IServer, ServerBase等）
│   ├── Jdx.Servers.Http/      # HTTP サーバー実装
│   ├── Jdx.Servers.Dns/       # DNS サーバー実装
│   ├── Jdx.Host/              # CLI版ホストアプリケーション
│   └── Jdx.WebUI/             # Blazor Web UI（実装済み）
│       ├── Services/           # ServerManager, LogService
│       └── Components/Pages/   # Dashboard, Logs, DnsRecords
├── tests/
│   ├── Jdx.Tests.Core/
│   └── Jdx.Tests.Servers.Http/
├── docs/                        # 技術設計ドキュメント
└── tmp/
    ├── todo.md                  # タスク管理
    └── instructions_history.md  # 作業履歴
```

### 関連ドキュメント
- `README.md` - プロジェクト概要
- `tmp/todo.md` - 詳細なタスクリスト
- `docs/01_technical/` - 技術設計書

### ビルド設定
- **ターゲットフレームワーク**: .NET 9 (net9.0)
- **C# バージョン**: 13.0
- **Nullable 参照型**: 有効
- **暗黙的 using**: 有効

---

## 起動方法まとめ

### CLI版（シンプル・自動起動）
```bash
dotnet run --project src/Jdx.Host
```
- HTTP: http://localhost:8080
- DNS: port 5300

### Web UI版（ブラウザ管理・推奨）
```bash
dotnet run --project src/Jdx.WebUI --urls "http://localhost:5001"
```
- Web UI: http://localhost:5001
- サーバーは Web UI から起動/停止

---

**最終更新**: 2026/01/16 - Web UI 実装完了
