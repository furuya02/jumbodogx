# JumboDogX

[English](README.md) | 日本語

**JumboDogX** は、今まで多くの方にご利用いただいていた[BlackJumboDog (BJD)](https://forest.watch.impress.co.jp/library/software/blackjmbdog/) の次期バージョンとして .NET 9（クロスプラットフォーム）へ移植されたものです。
（2026.01現在、開発中です）

**⚠️ 重要**: このソフトウェアは**ローカル環境でのテスト用**に設計されています。本番環境や公開サーバーとしての使用は想定していません。開発・テスト環境でのみ使用してください。

## 概要

- **元プロジェクト**: BlackJumboDog Ver6.1.9
- **ライセンス**: MIT License
- **用途**: ローカルテスト環境専用（本番環境・公開サーバーとしての使用は非推奨）
- **対応プラットフォーム**: Windows、macOS、Linux
- **フレームワーク**: .NET 9
- **言語**: C# 13

## 主要機能

- HTTP/HTTPS サーバー
- DNS サーバー
- SMTP/POP3 メールサーバー
- FTP サーバー
- DHCP サーバー
- TFTP サーバー
- Proxyサーバー
- Web UI管理コンソール（Blazor Server）

## プロジェクト構成

```
jdx/
├── src/
│   ├── Jdx.Core/              # コアライブラリ
│   ├── Jdx.Servers.Http/      # HTTPサーバー実装
│   ├── Jdx.Servers.Dns/       # DNSサーバー実装
│   ├── Jdx.Host/              # ホストアプリケーション
│   └── Jdx.WebUI/             # Web UI（Blazor Server）
├── tests/
│   ├── Jdx.Tests.Core/        # Coreのユニットテスト
│   └── Jdx.Tests.Servers.Http/ # HTTPサーバーのテスト
└── docs/                        # 設計ドキュメント
```

## 開発状況

### コア基盤
- .NET 9 SDKインストール済み
- プロジェクト構造作成済み
- ソリューション設定完了
- コア機能実装完了（ServerBase、IServer抽象化）
- 共通基盤実装完了（NetworkHelper、ConnectionLimiter、NetworkExceptionHandler）
- サーバーリファクタリング Phase 1-3 完了（PR #13, #14）

### サーバー実装
- HTTP/HTTPS サーバー実装済み（Range Requests、Keep-Alive、Virtual Host、SSL/TLS、ACL、証明書検証）
- DNS サーバー実装済み（A、AAAA、CNAME、MX、NS、PTR、SOAレコード、ACL）
- SMTP サーバー実装済み（認証付きメール送信）
- POP3 サーバー実装済み（認証付きメール受信、ACL）
- FTP サーバー実装済み（ファイル転送、ユーザー管理、ACL）
- DHCP サーバー実装済み（IPアドレス割り当て、リース管理、MACベースACL）
- TFTP サーバー実装済み（簡易ファイル転送、ACL）
- Proxy サーバー実装済み（HTTPプロキシ、キャッシュ、URLフィルタリング）

### Web UI（Blazor Server）
- Dashboard実装済み（サーバー状態監視）
- Logs実装済み（IPアドレス追跡機能付きリアルタイムログ表示）
- 全サーバーの設定ページ実装済み（100%カバレッジ）
  - HTTP/HTTPS: General、Document、CGI、SSI、WebDAV、Alias & MIME、Authentication、Template、ACL、Virtual Hosts、SSL/TLS、Advanced
  - DNS: General、Recordsマネージメント
  - SMTP、POP3、FTP、DHCP、TFTP、Proxy: 各種設定ページ

### 高度な機能
- Apache Killer対策実装済み（DoS攻撃防御）
- AttackDb実装済み（時間窓方式の攻撃検出）
- Range Requests実装済み（部分コンテンツ配信 - RFC 7233）
- Keep-Alive実装済み（HTTP持続的接続）
- Virtual Host実装済み（Hostヘッダーベースのルーティング）
- SSL/TLS基本構造実装済み（証明書管理）
- SSL/TLS完全統合完了（実際のSSL通信 - PR #20）
- SSL/TLS証明書検証実装済み（自動証明書検証）
- AttackDb ACL自動追加完了（PR #18）
- ACL（アクセス制御リスト）実装済み（HTTP、DNS、POP3、FTP、TFTP、DHCPの全主要サーバー）

### Phase 2: 次世代機能とエンタープライズサポート

**Phase 1 完了**: Phase 1の全27項目を2026年1月19日に完了しました！
Phase 2では、操作性の向上、品質のさらなる強化、新機能の追加に焦点を当てています。

**進捗**: 4/18項目完了（22.2%）

#### 完了項目（重要度: 高）
- メトリクス収集機能（PR #26）
- ログローテーション機能（PR #27）
- Serilogによる構造化ログ（PR #27）
- 設定インポート/エクスポート機能（PR #28）

## 必要環境

- .NET 9 SDK以上
- Visual Studio 2022 / JetBrains Rider / VS Code

## ビルド方法

```bash
# 依存関係のリストア
dotnet restore

# ビルド
dotnet build

# テスト実行
dotnet test
```

## 実行方法

### CLI版（ホストアプリケーション）

```bash
cd src/Jdx.Host
dotnet run
```

### Web UI版

```bash
cd src/Jdx.WebUI
dotnet run
```

その後、ブラウザで http://localhost:5001 にアクセスしてください。

詳細は [startup.md](startup.md) を参照してください。

## ドキュメント

[manual](manual/README.md) ディレクトリに包括的なユーザーマニュアルがあります：

- **[ユーザーマニュアル](manual/README.md)** - サーバー別のガイドとチュートリアル
  - [HTTP/HTTPSサーバー クイックスタート](manual/http/getting-started.md)
  - DNS、FTP、SMTP、POP3、DHCP、TFTP、Proxyサーバーガイド
  - 共通ガイド（インストール、ACL設定、ロギング、セキュリティ）

## メトリクス

JumboDogXは、サーバーパフォーマンスを監視するためのPrometheus互換メトリクスを提供します。

### メトリクスへのアクセス

メトリクスはHTTPサーバーの `/metrics` エンドポイントから利用できます：

```bash
curl http://localhost:2001/metrics
```

### 利用可能なメトリクス

- **Total Connections**: 受信した接続数
- **Active Connections**: 現在アクティブな接続数
- **Total Requests**: 処理されたリクエスト数
- **Total Errors**: 発生したエラー数
- **Bytes Received/Sent**: ネットワークトラフィック統計
- **Server Uptime**: 各サーバーの稼働時間

### Prometheusとの統合

Prometheus設定ファイル（`prometheus.yml`）に以下を追加してください：

```yaml
scrape_configs:
  - job_name: 'jumbodogx'
    static_configs:
      - targets: ['localhost:2001']
```

### Grafanaダッシュボード

メトリクスは以下の手順でGrafanaで可視化できます：
1. Prometheusをデータソースとして追加
2. 以下のようなクエリでダッシュボードを作成：
   - `jumbodogx_http_total_connections`
   - `jumbodogx_http_active_connections`
   - `rate(jumbodogx_http_total_requests[5m])`

## ログ

JumboDogXは、機械可読で検索可能なログを提供するため、Serilogによる構造化ログを使用しています。

### ログ形式

すべてのログは**コンパクトJSON形式**で出力され、ログ集約ツールで簡単に解析できます。

### ログ出力先

ログは2つの出力先に書き込まれます：

1. **コンソール**: JSON形式のリアルタイムログ出力
2. **ファイル**: 自動ローテーション付きの永続的なログファイル

### ログファイル

- **ホストアプリケーション**: `logs/jumbodogx-host20260119.log`
- **WebUIアプリケーション**: `logs/jumbodogx-webui20260119.log`

注: Serilogは日次ローテーションが有効な場合、ファイル名と拡張子の間に日付（YYYYMMDD）を自動的に追加します。

### ログローテーション

ログファイルは以下の基準で自動的にローテーションされます：

- **日次ローテーション**: 毎日新しいログファイルを作成
- **サイズベースローテーション**: ファイルが10MBを超えた場合
- **保持期間**: 過去30日分のログを保持

### ログ構造

各ログエントリには以下が含まれます：

```json
{
  "@t": "2026-01-19T10:30:45.123Z",
  "@mt": "HTTP request received",
  "@l": "Information",
  "Application": "JumboDogX.Host",
  "SourceContext": "Jdx.Servers.Http.HttpServer",
  "Method": "GET",
  "Path": "/index.html"
}
```

### ログの解析

`jq`などのツールを使用してJSONログを解析できます：

```bash
# すべてのエラーログを表示
cat logs/jumbodogx-host*.log | jq 'select(.["@l"] == "Error")'

# メソッド別にリクエスト数をカウント
cat logs/jumbodogx-host*.log | jq -r '.Method' | sort | uniq -c

# タイムスタンプとメッセージを抽出
cat logs/jumbodogx-host*.log | jq -r '"\(.["@t"]) - \(.["@mt"])"'
```

## テスト

### ユニットテストの実行

すべてのテストを実行：
```bash
dotnet test
```

特定のプロジェクトのテストを実行：
```bash
dotnet test tests/Jdx.Core.Tests
dotnet test tests/Jdx.Servers.Http.Tests
dotnet test tests/Jdx.Servers.Dns.Tests
```

カバレッジ付きでテストを実行：
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### ベンチマークの実行

すべてのベンチマークを実行：
```bash
dotnet run --project benchmarks/Jdx.Benchmarks -c Release
```

特定のベンチマークを実行：
```bash
dotnet run --project benchmarks/Jdx.Benchmarks -c Release -- --filter *HttpRequestBenchmark*
```

詳細は [benchmarks/Jdx.Benchmarks/README.md](benchmarks/Jdx.Benchmarks/README.md) を参照してください。

## 設定

詳細な設定例については、[docs/configuration-guide.md](docs/configuration-guide.md) を参照してください。

### 設定のバックアップ・復元

JumboDogXは、簡単なバックアップと移行のための設定インポート/エクスポート機能を提供します。

#### 設定のエクスポート

1. `http://localhost:5001` でWeb UIにアクセス
2. **Settings** → **Backup & Restore** に移動
3. **Export Settings** ボタンをクリック
4. ダウンロードされたJSONファイル（例：`jumbodogx-settings-20260119-120000.json`）を保存

エクスポートされたファイルには、HTTP、DNS、FTP、SMTP、POP3、DHCP、TFTP、Proxyの全サーバー設定が含まれます。

#### 設定のインポート

1. `http://localhost:5001` でWeb UIにアクセス
2. **Settings** → **Backup & Restore** に移動
3. 以前にエクスポートしたJSONファイルを選択
4. **Import Settings** ボタンをクリック
5. インポートした設定を適用するためにアプリケーションを再起動

**注意**: 設定をインポートした後、変更を有効にするためにアプリケーションを再起動する必要があります。

#### ユースケース

- **バックアップ**: 変更前に現在の設定を保存
- **マイグレーション**: 異なるインスタンス間で設定を移行
- **テンプレート**: 開発チーム内で設定を共有
- **リカバリー**: 以前の動作する設定に復元

## 設計ドキュメント

詳細な設計ドキュメントは `docs/` ディレクトリに格納されています：

- 移植計画: `docs/00_migration/`
- 技術設計: `docs/01_technical/`

## セキュリティに関する注意

このソフトウェアは**ローカル開発・テスト環境専用**に設計されています。以下の点にご注意ください：

- インターネットに直接公開しないでください
- 本番環境での使用は推奨されません
- セキュリティ監査は実施されていません
- あくまで開発・テスト・学習目的での使用を想定しています

## セキュリティ設定

JumboDogXを使用する前に、`src/Jdx.WebUI/appsettings.json` のセキュリティ設定を確認・更新してください：

### 重要なセキュリティ設定

1. **ユーザーパスワード**（高優先度）
   - デフォルトパスワードハッシュ: `REPLACE_WITH_SECURE_SHA256_HASH`
   - ⚠️ 使用前に**必ず**変更してください
   - 強力なパスワードを生成し、SHA-256ハッシュを作成：
     ```bash
     echo -n 'YourStrongPassword' | shasum -a 256
     ```
   - 生成されたハッシュでプレースホルダーを置き換えてください

2. **ネットワークバインディング**
   - デフォルト: `"BindAddress": "127.0.0.1"`（ローカルホストのみ）
   - ネットワークアクセスが必要な場合は `"0.0.0.0"`（全インターフェース）に変更
   - ⚠️ 必要な場合のみ変更し、ファイアウォールの内側で使用してください

3. **CGI実行**（中優先度）
   - デフォルト: `"UseCgi": false`（無効）
   - CGIスクリプトが必要な場合のみ有効化
   - ⚠️ 有効化時はコマンドインジェクションのリスクがあります
   - CGI使用時は適切な入力検証を確保してください

4. **WebDAV書き込みアクセス**（中優先度）
   - デフォルト: `"AllowWrite": false`（読み取り専用）
   - 必要な場合のみ書き込みアクセスを有効化
   - ⚠️ 不正なファイルアップロードを防ぐため、認証と併用してください

### 推奨セキュリティプラクティス

- すべてのユーザーアカウントに強力で一意なパスワードを使用
- ネットワークアクセスが不要な場合はBindAddressを `127.0.0.1` に維持
- CGIとWebDAV書き込みアクセスは絶対に必要な場合のみ有効化
- セキュリティ設定を定期的に確認・更新
- 不審なアクティビティがないかサーバーログを監視

セキュリティ脆弱性の報告については、[SECURITY.md](SECURITY.md) を参照してください。

## ライセンス

MIT License

Copyright (c) 2026 Hirauchi Shinichi (SIN)

詳細は [LICENSE](LICENSE) ファイルを参照してください。

## 関連リンク

- 元プロジェクト: BlackJumboDog Ver6.1.9
- .NET 9: https://dotnet.microsoft.com/
