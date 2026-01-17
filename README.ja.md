# JumboDogX

[English](README.md) | 日本語

**JumboDogX** は、BlackJumboDog (BJD) を.NET 9で完全に書き直した次世代マルチプラットフォーム統合サーバーソフトウェアです。JumboDogXは、日本で愛されてきたBJDの進化版として、最新のクロスプラットフォーム機能を提供します。

**⚠️ 重要**: このソフトウェアは**ローカル環境でのテスト用サーバー**です。グローバルネットワークへの公開を想定していません。開発・テスト環境でのみ使用してください。

## 概要

- **元プロジェクト**: BlackJumboDog Ver6.1.9
- **ライセンス**: MIT License
- **用途**: ローカルテスト環境専用（本番環境・公開サーバーとしての使用は非推奨）
- **対応プラットフォーム**: Windows、macOS、Linux
- **フレームワーク**: .NET 9
- **言語**: C# 13

## 主要機能（予定）

- HTTP/HTTPS サーバー
- DNS サーバー
- SMTP/POP3 メールサーバー
- FTP サーバー
- DHCP サーバー
- 各種Proxyサーバー
- Web UI管理画面（Blazor Server）

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

- [x] .NET 9 SDKインストール
- [x] プロジェクト構造作成
- [x] ソリューション設定
- [x] コア機能実装（ServerBase、IServer抽象化）
- [x] HTTPサーバー実装（基本機能）
- [x] DNSサーバー実装（基本機能）
- [x] Web UI実装（Dashboard、Logs、DNS Records管理）
- [ ] SMTP/POP3サーバー実装
- [ ] FTPサーバー実装
- [ ] DHCPサーバー実装
- [ ] Proxyサーバー実装

## 必要環境

- .NET 9 SDK以上
- Visual Studio 2022 / JetBrains Rider / VS Code

## ビルド方法

```bash
# リストア
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

このソフトウェアは**ローカル開発環境でのテスト用**に設計されています。以下の点にご注意ください：

- インターネットに直接公開しないでください
- 本番環境での使用は推奨されません
- セキュリティ監査は実施されていません
- あくまで開発・テスト・学習目的での使用を想定しています

## ライセンス

MIT License

Copyright (c) 2026 Hirauchi Shinichi (SIN)

詳細は [LICENSE](LICENSE) ファイルを参照してください。

## 関連リンク

- 元プロジェクト: BlackJumboDog Ver6.1.9
- .NET 9: https://dotnet.microsoft.com/
