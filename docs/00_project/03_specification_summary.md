# JumboDogX 仕様書

## 1. プロジェクト概要

JumboDogXは、オリジナルのBlackJumboDog（BJD）を.NET 9で完全にリライトした、マルチプロトコルサーバーソリューションです。開発・テスト環境向けの軽量なネットワークサーバー群を提供し、Windows環境で簡単にセットアップ可能なサーバー環境を実現します。モダンなWeb UIによる直感的な管理インターフェースを備えています。

### 1.1 主な特徴

- **マルチプロトコル対応**: HTTP, DNS, SMTP, POP3, FTP, TFTP, DHCP, Proxyの8プロトコルをサポート
- **モダンなアーキテクチャ**: .NET 9 / C# 13.0 / Blazor Server
- **Web UI**: ブラウザベースの管理画面（リアルタイム監視対応）
- **構造化ロギング**: Serilogによる高度なログ管理
- **モジュール設計**: 各サーバーが独立したパッケージとして構造化
- **ACL（アクセス制御リスト）**: 全サーバーでIPアドレスベースのアクセス制御をサポート
- **TLS/SSL対応**: HTTP, SMTP, POP3, FTPで暗号化通信をサポート

### 1.2 システム要件

| 項目 | 内容 |
|------|------|
| ランタイム | .NET 9.0 Runtime |
| 対応OS | Windows 10/11, Windows Server 2019以降, macOS, Linux（限定的サポート） |
| 開発環境 | .NET 9.0 SDK, Visual Studio 2022 または VS Code, C# 13.0 |

---

## 2. アーキテクチャ

### 2.1 全体構成

```
┌─────────────────────────────────────────────────────────────┐
│                      Jdx.Host (CLI)                         │
├─────────────────────────────────────────────────────────────┤
│                     Jdx.WebUI (Blazor)                      │
├─────────────────────────────────────────────────────────────┤
│  HTTP  │  DNS  │  SMTP  │  POP3  │  FTP  │ TFTP │DHCP│Proxy│
├─────────────────────────────────────────────────────────────┤
│                       Jdx.Core                              │
│  (ServerBase, Network, Logging, Metrics, Settings)          │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 プロジェクト構成

| プロジェクト | 説明 |
|-------------|------|
| Jdx.Core | コアライブラリ（サーバー基底クラス、ネットワーク、ロギング、メトリクス、設定管理） |
| Jdx.Host | コンソールホストアプリケーション |
| Jdx.WebUI | Blazor Server Web管理UI |
| Jdx.Servers.Http | HTTPサーバー |
| Jdx.Servers.Dns | DNSサーバー |
| Jdx.Servers.Smtp | SMTPサーバー |
| Jdx.Servers.Pop3 | POP3サーバー |
| Jdx.Servers.Ftp | FTPサーバー |
| Jdx.Servers.Tftp | TFTPサーバー |
| Jdx.Servers.Dhcp | DHCPサーバー |
| Jdx.Servers.Proxy | プロキシサーバー |

### 2.3 依存関係

```
Jdx.Host ─────┐
              │
Jdx.WebUI ────┼──► Jdx.Servers.* ──► Jdx.Core
              │
```

### 2.4 コアコンポーネント

#### IServer インターフェース

すべてのサーバーが実装する共通インターフェース:

```csharp
public interface IServer
{
    ServerType Type { get; }
    ServerStatus Status { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    ServerStatistics GetStatistics();
}
```

#### ServerBase 基底クラス

すべてのサーバーが継承する基底クラス:
- ライフサイクル管理（起動/停止）
- 統計情報収集
- ロギング統合
- 接続数制限

### 2.5 設計原則

- **SOLID原則**: 各サーバーは1つのプロトコルに特化、DIコンテナによる依存性注入
- **非同期処理**: async/awaitの徹底
- **null安全性**: Nullable enabled
- **構造化ロギング**: Serilog
- **設定の外部化**: ApplicationSettings

---

## 3. サポートプロトコル一覧

| プロトコル | デフォルトポート | トランスポート | 用途 |
|-----------|-----------------|---------------|------|
| HTTP/HTTPS | 80, 443 | TCP | Webサーバー |
| DNS | 53 | UDP | ドメイン名解決 |
| SMTP | 25, 587 | TCP | メール送信 |
| POP3 | 110, 995 | TCP | メール受信 |
| FTP | 21 | TCP | ファイル転送 |
| TFTP | 69 | UDP | シンプルファイル転送 |
| DHCP | 67, 68 | UDP | IP自動割当 |
| Proxy | 8080 | TCP | HTTPプロキシ |

---

## 4. HTTPサーバー仕様

### 4.1 主要機能

| 機能 | 説明 |
|------|------|
| HTTP/1.1 | HTTP/1.1プロトコル対応 |
| HTTPS | SSL/TLS暗号化通信（PFX証明書） |
| Virtual Host | 複数ドメインのホスティング |
| Range Requests | 部分ダウンロード対応（RFC 7233） |
| Keep-Alive | HTTP永続接続 |
| WebDAV | Web分散オーサリング（PROPFIND, PROPPATCH, MKCOL, COPY, MOVE, LOCK, UNLOCK） |
| CGI | 外部スクリプト実行（.exe, .bat, シェルスクリプト） |
| SSI | Server Side Includes |
| 認証 | Basic認証, Digest認証 |
| ACL | IPアドレスベースのアクセス制御 |

### 4.2 設定項目

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 80 | HTTP待ち受けポート |
| SSL Port | 443 | HTTPS用ポート |
| Document Root | ./www | ドキュメントルート |
| Index Files | index.html, index.htm | デフォルトドキュメント |

### 4.3 Virtual Host

各仮想ホストにホスト名、ドキュメントルート、ポート、プロトコル（HTTP/HTTPS）を設定可能。

### 4.4 SSL/TLS証明書

- PFX形式推奨
- Web UIでHTTPS有効化時に証明書の自動検証を実施（ファイル存在確認、パスワード検証、有効期限チェック）
- 検証失敗時はHTTPSを有効化できない

---

## 5. DNSサーバー仕様

### 5.1 主要機能

| 機能 | 説明 |
|------|------|
| Aレコード | IPv4アドレスマッピング |
| AAAAレコード | IPv6アドレスマッピング |
| CNAMEレコード | 別名（エイリアス） |
| MXレコード | メールサーバー指定 |
| NSレコード | ネームサーバー指定 |
| TXTレコード | テキスト情報 |
| SOAレコード | Start of Authority |
| PTRレコード | 逆引きレコード |
| 上位DNS転送 | 未登録ドメインの解決を上位DNSに転送 |
| ACL | IPアドレスベースのアクセス制御 |

### 5.2 設定項目

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 53 | 待ち受けポート（UDP） |
| TTL | 3600 | デフォルトTTL（秒） |

### 5.3 ドメイン管理

- ドメイン単位でのレコード管理
- Web UIでのドメイン・レコード追加/編集/削除
- SOA設定は各ドメインページで管理

---

## 6. SMTPサーバー仕様

### 6.1 主要機能

| 機能 | 説明 |
|------|------|
| SMTP | RFC 5321準拠 |
| ESMTP拡張 | EHLOコマンド対応 |
| TLS/STARTTLS | 暗号化通信 |
| AUTH | 認証（PLAIN, LOGIN） |
| ローカル保存 | メールボックスへの保存 |
| メール転送 | リレー機能 |

### 6.2 設定項目

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 25 | 標準SMTPポート |
| Submission Port | 587 | メール投稿用ポート |
| Max Message Size | 10MB | 最大メッセージサイズ |

### 6.3 サポートコマンド

HELO, EHLO, MAIL FROM, RCPT TO, DATA, QUIT, RSET, NOOP, STARTTLS, AUTH

---

## 7. POP3サーバー仕様

### 7.1 主要機能

| 機能 | 説明 |
|------|------|
| POP3 | RFC 1939準拠 |
| TLS/SSL | POP3S暗号化通信 |
| 認証 | USER/PASS, APOP |
| メールボックス管理 | メッセージ取得/削除 |
| ACL | IPアドレスベースのアクセス制御 |

### 7.2 設定項目

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 110 | 標準POP3ポート |
| SSL Port | 995 | POP3S用ポート |

### 7.3 サポートコマンド

- **認証状態**: USER, PASS, APOP, QUIT
- **トランザクション状態**: STAT, LIST, RETR, DELE, NOOP, RSET, TOP, UIDL, QUIT

### 7.4 SMTP連携

SMTPサーバーで受信したメールを保存・提供。

---

## 8. FTPサーバー仕様

### 8.1 主要機能

| 機能 | 説明 |
|------|------|
| FTP | RFC 959準拠 |
| パッシブモード | クライアントがサーバーに接続（PASV） |
| アクティブモード | サーバーがクライアントに接続（PORT） |
| FTPS | TLS/SSL暗号化通信 |
| 仮想ディレクトリ | 仮想フォルダマウント |
| ユーザー認証 | ユーザー名/パスワード認証 |
| ACL | IPアドレスベースのアクセス制御 |

### 8.2 設定項目

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Command Port | 21 | コマンドポート |
| Passive Port Range | 50000-50100 | パッシブモード用ポート範囲 |
| Root Directory | ./ftp | ルートディレクトリ |

### 8.3 サポートコマンド

- **認証**: USER, PASS
- **転送**: LIST, NLST, RETR, STOR, APPE
- **ディレクトリ**: PWD, CWD, CDUP, MKD, RMD
- **その他**: TYPE, PASV, PORT, DELE, RNFR/RNTO, SIZE, QUIT

### 8.4 ユーザー管理

ユーザーごとにホームディレクトリとパーミッション（read, write, delete）を設定可能。匿名アクセスの有効/無効設定あり。

---

## 9. TFTPサーバー仕様

### 9.1 主要機能

| 機能 | 説明 |
|------|------|
| TFTP | RFC 1350準拠 |
| ブロックサイズオプション | RFC 2348（8-65464バイト） |
| タイムアウトオプション | RFC 2349（1-255秒） |
| 転送サイズオプション | RFC 2349 |
| ACL | IPアドレスベースのアクセス制御 |

### 9.2 設定項目

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 69 | 待ち受けポート（UDP） |
| Root Directory | ./tftp | ルートディレクトリ |
| Block Size | 512 | デフォルトブロックサイズ |
| Timeout | 5 | タイムアウト（秒） |

### 9.3 用途

- ネットワークブート（PXE）
- ルーター/スイッチの設定バックアップ
- ファームウェア更新

### 9.4 パケットタイプ

| オペコード | タイプ | 説明 |
|-----------|--------|------|
| 1 | RRQ | 読み取り要求 |
| 2 | WRQ | 書き込み要求 |
| 3 | DATA | データ |
| 4 | ACK | 確認応答 |
| 5 | ERROR | エラー |
| 6 | OACK | オプション確認 |

---

## 10. DHCPサーバー仕様

### 10.1 主要機能

| 機能 | 説明 |
|------|------|
| DHCP | RFC 2131準拠 |
| 動的IP割り当て | アドレスプールからの自動割り当て |
| 静的IP予約 | MACアドレスによるIP固定割り当て |
| DHCPオプション | サブネットマスク、ゲートウェイ、DNS等 |
| リース管理 | リース期間の管理と更新 |
| ACL | MACアドレスベースのアクセス制御（ワイルドカード対応） |

### 10.2 設定項目

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 67（サーバー）/ 68（クライアント） | UDPポート |
| Lease Time | 86400 | デフォルトリース時間（秒） |

### 10.3 アドレスプール設定

開始/終了アドレス、サブネットマスク、デフォルトゲートウェイ、DNSサーバー、リース時間を設定可能。

### 10.4 DHCPメッセージフロー（DORA）

```
Client → DISCOVER → Server
Client ← OFFER    ← Server
Client → REQUEST  → Server
Client ← ACK      ← Server
```

### 10.5 DHCPオプション

| コード | 名前 | 説明 |
|--------|------|------|
| 1 | Subnet Mask | サブネットマスク |
| 3 | Router | デフォルトゲートウェイ |
| 6 | DNS Server | DNSサーバー |
| 12 | Hostname | ホスト名 |
| 15 | Domain Name | ドメイン名 |
| 51 | Lease Time | リース時間 |
| 53 | Message Type | DHCPメッセージタイプ |
| 54 | Server ID | DHCPサーバーID |

### 10.6 リース状態

Available（利用可能） → Offered（提案中） → Leased（リース中） → Expired（期限切れ）

---

## 11. プロキシサーバー仕様

### 11.1 主要機能

| 機能 | 説明 |
|------|------|
| HTTPプロキシ | HTTP通信の中継 |
| HTTPS（CONNECT） | CONNECTメソッドによるTLSトンネリング |
| アクセス制御 | クライアント制限、宛先制限 |
| キャッシュ | レスポンスキャッシュ（基本的） |
| 認証 | Proxy-Authorization（Basic認証） |
| ロギング | アクセスログ記録 |

### 11.2 設定項目

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 8080 | 待ち受けポート |
| Timeout | 60 | 接続タイムアウト（秒） |

### 11.3 アクセス制御

- **クライアント制限**: 特定のIP/ネットワークからのみアクセス許可
- **宛先制限**: 特定のドメイン/URLへのアクセスブロック（ワイルドカード対応）

### 11.4 ログ記録項目

クライアントIP、リクエストURL、レスポンスコード、転送バイト数、処理時間

---

## 12. Web UI仕様

### 12.1 概要

Blazor Serverで構築された管理インターフェース。Bootstrap 5ベースのModern Chicデザイン。

### 12.2 アクセス

| 環境 | URL |
|------|-----|
| Development | http://localhost:5000 |
| Production | http://localhost:5000 |

### 12.3 画面構成

#### ダッシュボード
- サーバーステータス一覧（リアルタイム更新）
- クイック起動/停止ボタン
- 統計情報サマリー

#### 設定ページ
各サーバーの詳細設定（階層構造対応）:

| サーバー | 設定項目 |
|---------|---------|
| HTTP | General, Document, SSL, VirtualHost, Authentication, CGI, SSI, WebDAV, AliasMime, Template, Advanced, ACL |
| DNS | General, Domains（各ドメイン: Records, Resources, SOA）, ACL |
| SMTP | General, Host, Esmtp, Aliases, AutoReception, Queue, Header, Relay, ACL |
| POP3 | General, ChangePassword, AutoDeny, ACL |
| FTP | General, User, VirtualFolder, ACL |
| TFTP | General, ACL |
| DHCP | General, MacAcl |
| Proxy | Basic, HigherProxy, Cache1, Cache2, LimitUrl, LimitContents, ACL |

#### ログビューア
- リアルタイムログ表示
- レベルフィルタ（Info, Warning, Error）
- カテゴリフィルタ
- テキスト検索
- 時刻フォーマット選択
- IP Addressカラム表示
- ログクリア（確認ダイアログ付き）

#### DNSレコード管理
- レコードの追加/編集/削除
- レコードタイプ別表示

### 12.4 リアルタイム更新

- Blazor Server SignalRによるサーバーステータス自動更新
- ログのリアルタイム表示
- 設定変更の即時反映
- 500msごとの自動更新タイマー

### 12.5 アクセシビリティ

- ARIA属性対応（role, aria-labelledby, aria-modal）
- キーボード操作（Escape, Tab, Enter）
- フォーカス状態の視覚的表示
- レスポンシブ対応

---

## 13. セキュリティ機能

### 13.1 ACL（アクセス制御リスト）

全サーバーでACLをサポート:
- **IP/CIDRベース**: HTTP, DNS, SMTP, POP3, FTP, TFTP, Proxy
- **MACアドレスベース**: DHCP（ワイルドカード対応）
- ルールは上から順に評価
- Allow/Denyルールの組み合わせ

### 13.2 TLS/SSL

| サーバー | 暗号化通信 |
|---------|-----------|
| HTTP | HTTPS（ポート443） |
| SMTP | STARTTLS / SMTPS |
| POP3 | POP3S（ポート995） |
| FTP | FTPS（AUTH TLS） |

- PFX形式証明書推奨
- 証明書自動検証（ファイル存在、パスワード、有効期限）
- 開発証明書・OpenSSL・PowerShellでの証明書作成をサポート

### 13.3 認証

| サーバー | 認証方式 |
|---------|---------|
| HTTP | Basic認証, Digest認証 |
| SMTP | AUTH（PLAIN, LOGIN） |
| POP3 | USER/PASS, APOP |
| FTP | ユーザー名/パスワード |
| Proxy | Proxy-Authorization（Basic） |

---

## 14. ロギング仕様

### 14.1 ロギングスタック

- **Serilog**: 構造化ロギングライブラリ
- **シンク**: Console, File（ローリング）, Web UI（LogService）
- **非同期出力**: Serilog.Sinks.Async

### 14.2 ログレベル

| レベル | 用途 |
|--------|------|
| Verbose | 最も詳細なデバッグ情報 |
| Debug | デバッグ情報 |
| Information | 通常の操作情報 |
| Warning | 警告（動作は継続） |
| Error | エラー（操作失敗） |
| Fatal | 致命的エラー |

### 14.3 Web UIログビューア

- レベルフィルタ
- カテゴリフィルタ
- テキスト検索
- 時刻フォーマット選択
- ログクリア（確認ダイアログ付き）

---

## 15. 技術スタック

### 15.1 フレームワーク・言語

| 項目 | バージョン |
|------|----------|
| .NET | 9.0 |
| C# | 13.0 |
| ASP.NET Core | 9.0 |
| Blazor Server | 9.0 |

### 15.2 主要ライブラリ

| ライブラリ | バージョン | 用途 |
|-----------|----------|------|
| Serilog | 4.2.0+ | 構造化ロギング |
| Serilog.Extensions.Hosting | 8.0.0+ | ホスト統合 |
| Serilog.Sinks.Console | 6.0.0+ | コンソール出力 |
| Serilog.Sinks.File | 6.0.0+ | ファイル出力 |
| Serilog.Sinks.Async | 2.1.0 | 非同期出力 |
| Bootstrap | 5 | Web UIフレームワーク |

### 15.3 テストフレームワーク

| ライブラリ | バージョン | 用途 |
|-----------|----------|------|
| xunit | 2.9.2 | テストフレームワーク |
| FluentAssertions | 6.12.2 | アサーション |
| Moq | 4.20.72 | モック |
| coverlet.collector | 6.0.2 | カバレッジ収集 |

### 15.4 コンパイル設定

- TargetFramework: net9.0
- LangVersion: 13.0
- Nullable: enable
- ImplicitUsings: enable
- EnforceCodeStyleInBuild: true

---

## 16. テスト仕様

### 16.1 テストピラミッド

```
        ┌─────┐
        │ E2E │  少数の統合テスト
       ─┴─────┴─
      /  統合   \  サーバー間連携テスト
    ─────────────
   /    単体     \  ユニットテスト（大多数）
  ───────────────
```

### 16.2 カバレッジ目標

| 対象 | 目標 |
|------|------|
| コアライブラリ | 80%以上 |
| サーバー実装 | 70%以上 |
| Web UI | 機能テスト中心 |

### 16.3 テストプロジェクト

| プロジェクト | 対象 |
|-------------|------|
| Jdx.Core.Tests | コア機能（Abstractions, Logging, Metrics, Network, Settings） |
| Jdx.Servers.Http.Tests | HTTPサーバー |
| Jdx.Servers.Dns.Tests | DNSサーバー |
| Jdx.Servers.Smtp.Tests | SMTPサーバー |
| Jdx.Servers.Pop3.Tests | POP3サーバー |
| Jdx.Servers.Ftp.Tests | FTPサーバー |
| Jdx.Servers.Tftp.Tests | TFTPサーバー |
| Jdx.Servers.Dhcp.Tests | DHCPサーバー |
| Jdx.E2E.Tests | E2Eテスト（HTTP, DNS, SMTP） |
| Jdx.WebUI.Tests | Web UI |

### 16.4 ベンチマーク

BenchmarkDotNetによるパフォーマンス計測:
- NetworkHelperBenchmark
- HttpRequestBenchmark
- DnsQueryBenchmark

---

## 17. クイックスタート

```bash
# リポジトリのクローン
git clone https://github.com/furuya02/jumbodogx.git
cd jumbodogx

# 依存関係の復元
dotnet restore

# ビルド
dotnet build

# 実行（Web UIを含むホストアプリケーション）
dotnet run --project src/Jdx.WebUI

# テスト実行
dotnet test

# ベンチマーク実行
dotnet run -c Release --project benchmarks/Jdx.Benchmarks
```

ブラウザで `http://localhost:5000` にアクセスしてWeb UIを使用。

---

## 18. ライセンス

MIT License
