# JumboDogX プロジェクト概要

## 1. プロジェクトについて

JumboDogXは、オリジナルのBlackJumboDog（BJD）を.NET 9で完全にリライトした、マルチプロトコルサーバーソリューションです。

### 1.1 目的

- 開発・テスト環境向けの軽量なネットワークサーバー群の提供
- Windows環境で簡単にセットアップ可能なサーバー環境の実現
- モダンなWeb UIによる直感的な管理インターフェース

### 1.2 主な特徴

- **マルチプロトコル対応**: HTTP, DNS, SMTP, POP3, FTP, TFTP, DHCP, Proxy
- **モダンなアーキテクチャ**: .NET 9, Blazor Server
- **Web UI**: ブラウザベースの管理画面
- **構造化ロギング**: Serilogによる高度なログ管理
- **モジュール設計**: 各サーバーが独立したパッケージとして構造化

## 2. システム要件

### 2.1 実行環境

- .NET 9.0 Runtime
- Windows 10/11, Windows Server 2019以降
- macOS, Linux（限定的サポート）

### 2.2 開発環境

- .NET 9.0 SDK
- Visual Studio 2022 または VS Code
- C# 13.0

## 3. サポートプロトコル

| プロトコル | デフォルトポート | 用途 |
|-----------|-----------------|------|
| HTTP/HTTPS | 80, 443 | Webサーバー |
| DNS | 53 | ドメイン名解決 |
| SMTP | 25, 587 | メール送信 |
| POP3 | 110, 995 | メール受信 |
| FTP | 21 | ファイル転送 |
| TFTP | 69 | シンプルファイル転送 |
| DHCP | 67, 68 | IP自動割当 |
| Proxy | 8080 | HTTPプロキシ |

## 4. ライセンス

MIT License

## 5. 関連ドキュメント

- [アーキテクチャ設計](../01_development_docs/01_architecture_design.md)
- [開発環境セットアップ](../01_development_docs/02_development_setup.md)
- [テスト戦略](../01_development_docs/03_test_strategy.md)
