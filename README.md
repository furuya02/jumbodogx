# BJD9 - BlackJumboDog 9

BJD9は、BlackJumboDog (BJD5)を.NET 9へ移植したクロスプラットフォーム統合サーバーソフトウェアです。

## 概要

- **元プロジェクト**: BlackJumboDog Ver6.1.9
- **ライセンス**: Apache License 2.0
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
bjd9/
├── src/
│   ├── Bjd9.Core/              # コアライブラリ
│   ├── Bjd9.Servers.Http/      # HTTPサーバー実装
│   ├── Bjd9.Host/              # ホストアプリケーション
│   └── Bjd9.WebUI/             # Web UI（Blazor Server）
├── tests/
│   ├── Bjd9.Tests.Core/        # Coreのユニットテスト
│   └── Bjd9.Tests.Servers.Http/ # HTTPサーバーのテスト
└── docs/                        # 設計ドキュメント

```

## 開発状況

現在、プロジェクト骨格の作成が完了しました。

- [x] .NET 9 SDKインストール
- [x] プロジェクト構造作成
- [x] ソリューション設定
- [ ] コア機能実装（進行中）
- [ ] HTTPサーバー実装
- [ ] Web UI実装

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

```bash
cd src/Bjd9.Host
dotnet run
```

## 設計ドキュメント

詳細な設計ドキュメントは `../docs/` ディレクトリに格納されています：

- 移植計画: `docs/00_migration/`
- 技術設計: `docs/01_technical/`

## ライセンス

Apache License 2.0

## 関連リンク

- 元プロジェクト: BlackJumboDog Ver6.1.9
- .NET 9: https://dotnet.microsoft.com/
