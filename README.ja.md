# JumboDogX

[English](README.md) | 日本語

**JumboDogX** は、BlackJumboDog (BJD) を.NET 9で完全に書き直した次世代マルチプラットフォーム統合サーバーソフトウェアです。JumboDogXは、日本で愛されてきたBJDの進化版として、最新のクロスプラットフォーム機能を提供します。

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
- 各種Proxyサーバー
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
- HTTP/HTTPS サーバー実装済み（Range Requests、Keep-Alive、Virtual Host、SSL/TLS構造）
- DNS サーバー実装済み（A、AAAA、CNAME、MX、NS、PTR、SOAレコード）
- SMTP サーバー実装済み（認証付きメール送信）
- POP3 サーバー実装済み（認証付きメール受信）
- FTP サーバー実装済み（ファイル転送、ユーザー管理、ACL）
- DHCP サーバー実装済み（IPアドレス割り当て、リース管理）
- TFTP サーバー実装済み（簡易ファイル転送）
- Proxy サーバー実装済み（HTTPプロキシ、キャッシュ、URLフィルタリング）

### Web UI（Blazor Server）
- Dashboard実装済み（サーバー状態監視）
- Logs実装済み（リアルタイムログ表示）
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
- SSL/TLS完全統合（実際のSSL通信）は未実装
- AttackDb ACL自動追加機能は未実装

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
