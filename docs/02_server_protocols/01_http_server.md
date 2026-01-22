# HTTPサーバー

## 1. 概要

JumboDogX HTTPサーバーは、開発・テスト環境向けの軽量なWebサーバーです。

### 1.1 主要機能

- HTTP/1.1対応
- HTTPS (SSL/TLS)
- Virtual Host
- Range Requests (RFC 7233)
- Keep-Alive接続
- WebDAV
- CGI
- SSI (Server Side Includes)
- Basic/Digest認証
- ACL (アクセス制御)

## 2. プロジェクト構造

```
Jdx.Servers.Http/
├── HttpServer.cs              # メインサーバークラス
├── HttpProtocol.cs            # HTTPプロトコル処理
├── HttpRequest.cs             # リクエストパーサー
├── HttpResponse.cs            # レスポンス生成
├── VirtualHost/               # 仮想ホスト
├── Handlers/                  # リクエストハンドラー
│   ├── StaticFileHandler.cs
│   ├── CgiHandler.cs
│   ├── WebDavHandler.cs
│   └── SsiHandler.cs
├── Authentication/            # 認証
│   ├── BasicAuth.cs
│   └── DigestAuth.cs
├── Ssl/                       # SSL/TLS
└── Acl/                       # アクセス制御
```

## 3. 設定

### 3.1 基本設定

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 80 | 待ち受けポート |
| SSL Port | 443 | HTTPS用ポート |
| Document Root | ./www | ドキュメントルート |
| Index Files | index.html, index.htm | デフォルトドキュメント |

### 3.2 Virtual Host設定

```json
{
  "VirtualHosts": [
    {
      "HostName": "example.local",
      "DocumentRoot": "./www/example",
      "Port": 80
    }
  ]
}
```

## 4. 機能詳細

### 4.1 Range Requests

部分ダウンロード対応（RFC 7233）。

```
GET /largefile.zip HTTP/1.1
Range: bytes=0-1023
```

### 4.2 Keep-Alive

HTTP永続接続のサポート。

```
Connection: keep-alive
Keep-Alive: timeout=5, max=100
```

### 4.3 WebDAV

Web分散オーサリング対応メソッド：
- PROPFIND
- PROPPATCH
- MKCOL
- COPY
- MOVE
- LOCK
- UNLOCK

### 4.4 CGI

外部スクリプト実行：
- .exe, .bat (Windows)
- シェルスクリプト (Linux/macOS)
- 環境変数の受け渡し

### 4.5 認証

Basic認証とDigest認証をサポート。

## 5. アクセス制御

### 5.1 ACLルール

```json
{
  "Acl": {
    "Rules": [
      {
        "Path": "/admin/*",
        "Allow": ["192.168.1.0/24"],
        "Deny": ["*"]
      }
    ]
  }
}
```

## 6. テスト

テストプロジェクト: `tests/Jdx.Servers.Http.Tests/`

```bash
dotnet test tests/Jdx.Servers.Http.Tests
```
