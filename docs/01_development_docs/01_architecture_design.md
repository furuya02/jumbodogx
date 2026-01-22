# アーキテクチャ設計

## 1. 全体構成

JumboDogXは、モジュール化されたマルチプロトコルサーバーアーキテクチャを採用しています。

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

## 2. プロジェクト構成

### 2.1 ソリューション構造

```
jumbodogx/
├── src/
│   ├── Jdx.Core/              # コアライブラリ
│   ├── Jdx.Host/              # ホストアプリケーション
│   ├── Jdx.WebUI/             # Web管理UI
│   ├── Jdx.Servers.Http/      # HTTPサーバー
│   ├── Jdx.Servers.Dns/       # DNSサーバー
│   ├── Jdx.Servers.Smtp/      # SMTPサーバー
│   ├── Jdx.Servers.Pop3/      # POP3サーバー
│   ├── Jdx.Servers.Ftp/       # FTPサーバー
│   ├── Jdx.Servers.Tftp/      # TFTPサーバー
│   ├── Jdx.Servers.Dhcp/      # DHCPサーバー
│   └── Jdx.Servers.Proxy/     # プロキシサーバー
├── tests/                      # テストプロジェクト
├── benchmarks/                 # ベンチマーク
└── docs/                       # ドキュメント
```

### 2.2 依存関係

```
Jdx.Host ─────┐
              │
Jdx.WebUI ────┼──► Jdx.Servers.* ──► Jdx.Core
              │
```

## 3. コアコンポーネント

### 3.1 Jdx.Core

共通機能を提供するコアライブラリ。

```
Jdx.Core/
├── Abstractions/          # 抽象クラス・インターフェース
│   ├── IServer.cs         # サーバーインターフェース
│   ├── ServerBase.cs      # サーバー基底クラス
│   ├── ServerType.cs      # サーバー種別
│   └── ServerStatus.cs    # サーバー状態
├── Network/               # ネットワーク関連
│   ├── ServerTcpListener.cs
│   ├── ServerUdpListener.cs
│   ├── ConnectionLimiter.cs
│   └── IpAddressMatcher.cs
├── Logging/               # ロギング
├── Metrics/               # メトリクス
└── Settings/              # 設定管理
```

### 3.2 IServer インターフェース

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

### 3.3 ServerBase 基底クラス

すべてのサーバーが継承する基底クラス。
- ライフサイクル管理
- 統計情報収集
- ロギング統合
- 接続数制限

## 4. データフロー

### 4.1 サーバー起動フロー

```
User Request (Web UI)
        │
        ▼
   ServerManager
        │
        ▼
   IServer.StartAsync()
        │
        ├──► TCP/UDP Listener Start
        │
        └──► Status: Running
```

### 4.2 リクエスト処理フロー

```
Client Request
        │
        ▼
   TCP/UDP Listener
        │
        ▼
   Connection Limiter (接続数チェック)
        │
        ▼
   Protocol Handler (プロトコル固有処理)
        │
        ▼
   Response Generation
        │
        ▼
   Client Response
```

## 5. 設計原則

### 5.1 SOLID原則

- **Single Responsibility**: 各サーバーは1つのプロトコルに特化
- **Open/Closed**: ServerBaseを拡張して新サーバーを追加可能
- **Liskov Substitution**: IServer実装は相互に置換可能
- **Interface Segregation**: 小さな専用インターフェース
- **Dependency Inversion**: DIコンテナによる依存性注入

### 5.2 その他の設計方針

- 非同期処理（async/await）の徹底
- null安全性（Nullable enabled）
- 構造化ロギング（Serilog）
- 設定の外部化（ApplicationSettings）
