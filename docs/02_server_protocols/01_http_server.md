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
      "Port": 80,
      "Protocol": "HTTP"
    }
  ]
}
```

**Protocol属性:**
- `"HTTP"`: HTTP接続のみ（デフォルト）
- `"HTTPS"`: HTTPS接続（SSL/TLS有効化）

### 3.3 SSL/TLS証明書設定

```json
{
  "Http": {
    "CertificatePath": "./certs/server.pfx",
    "CertificatePassword": "password"
  }
}
```

**証明書検証:**
- Web UIでHTTPSを有効化する際、証明書の自動検証を実施
- 証明書ファイルの存在確認
- パスワードの検証
- 有効期限チェック（NotBefore/NotAfter）
- 検証失敗時はHTTPSを有効化できない

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

## 更新履歴

- 2026-01-24: HTTPS有効化時の証明書検証機能を実装
- 2026-01-24: Virtual HostにProtocol属性を追加しHTTP/HTTPS制御を改善
