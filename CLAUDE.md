# JumboDogX

.NET 9で構築されたマルチプロトコルサーバーソリューション。

## ドキュメント

技術ドキュメントは `/docs` 配下を参照してください。

### プロジェクト情報

- JumboDogX プロジェクト概要 @docs/00_project/01_project_overview.md
- 技術スタック @docs/00_project/02_tech_stack.md

### 開発ドキュメント

- アーキテクチャ設計 @docs/01_development_docs/01_architecture_design.md
- 開発環境セットアップ @docs/01_development_docs/02_development_setup.md
- テスト戦略 @docs/01_development_docs/03_test_strategy.md
- ロギング設計 @docs/01_development_docs/04_logging_design.md

### サーバープロトコル

- HTTPサーバー @docs/02_server_protocols/01_http_server.md
- DNSサーバー @docs/02_server_protocols/02_dns_server.md
- SMTPサーバー @docs/02_server_protocols/03_smtp_server.md
- POP3サーバー @docs/02_server_protocols/04_pop3_server.md
- FTPサーバー @docs/02_server_protocols/05_ftp_server.md
- TFTPサーバー @docs/02_server_protocols/06_tftp_server.md
- DHCPサーバー @docs/02_server_protocols/07_dhcp_server.md
- プロキシサーバー @docs/02_server_protocols/08_proxy_server.md

### Web UI

- Web UI 概要 @docs/03_web_ui/01_web_ui_overview.md
- コンポーネント設計 @docs/03_web_ui/02_components.md

## クイックスタート

```bash
# ビルド
dotnet build

# 実行
dotnet run --project src/Jdx.WebUI

# テスト
dotnet test
```

## プロジェクト構造

```
jumbodogx/
├── src/                    # ソースコード
│   ├── Jdx.Core/          # コアライブラリ
│   ├── Jdx.WebUI/         # Web管理UI (Blazor)
│   └── Jdx.Servers.*/     # 各サーバー実装
├── tests/                  # テストコード
├── docs/                   # 技術ドキュメント
└── benchmarks/            # ベンチマーク
```
