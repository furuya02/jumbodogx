# プロキシサーバー

## 1. 概要

JumboDogX プロキシサーバーは、HTTPプロキシ機能を提供します。

### 1.1 主要機能

- HTTPプロキシ
- HTTPS (CONNECTメソッド)
- アクセス制御
- キャッシュ（基本的）
- ロギング

## 2. プロジェクト構造

```
Jdx.Servers.Proxy/
├── ProxyServer.cs             # メインサーバークラス
├── ProxyProtocol.cs           # プロキシプロトコル処理
├── ProxySession.cs            # セッション管理
├── HttpProxy/                 # HTTPプロキシ
│   ├── HttpProxyHandler.cs
│   └── ConnectHandler.cs
├── Cache/                     # キャッシュ
└── Acl/                       # アクセス制御
```

## 3. 設定

### 3.1 基本設定

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 8080 | 待ち受けポート |
| Timeout | 60 | 接続タイムアウト（秒） |

### 3.2 アクセス制御

```json
{
  "Proxy": {
    "AllowedClients": ["192.168.1.0/24"],
    "BlockedDomains": ["example.com"],
    "MaxConnections": 100
  }
}
```

## 4. プロキシ動作

### 4.1 HTTPプロキシ

```
Client          Proxy           Server
   |--- GET --->|               |
   |            |--- GET ------>|
   |            |<-- Response --|
   |<-- Resp ---|               |
```

### 4.2 HTTPS (CONNECT)

```
Client          Proxy           Server
   |-- CONNECT ->|              |
   |<- 200 OK ---|              |
   |             |<-- TCP ----->|
   |<--- TLS Tunnel ----------->|
```

## 5. HTTPプロキシリクエスト

### 5.1 通常のGET

```http
GET http://example.com/path HTTP/1.1
Host: example.com
Proxy-Connection: keep-alive
```

### 5.2 CONNECTメソッド

```http
CONNECT example.com:443 HTTP/1.1
Host: example.com:443
```

## 6. アクセス制御

### 6.1 クライアント制限

特定のIPアドレス/ネットワークからのみアクセスを許可。

### 6.2 宛先制限

特定のドメイン/URLへのアクセスをブロック。

```json
{
  "BlockedDomains": [
    "*.blocked.com",
    "malware.example.org"
  ]
}
```

## 7. 認証

### 7.1 Proxy-Authorization

```http
GET http://example.com/ HTTP/1.1
Proxy-Authorization: Basic dXNlcjpwYXNz
```

## 8. 使用例

### 8.1 ブラウザ設定

- プロキシサーバー: localhost
- ポート: 8080

### 8.2 curlでの使用

```bash
# HTTPプロキシ
curl -x localhost:8080 http://example.com

# HTTPSプロキシ
curl -x localhost:8080 https://example.com
```

### 8.3 環境変数

```bash
export http_proxy=http://localhost:8080
export https_proxy=http://localhost:8080
```

## 9. ログ

プロキシアクセスログ：
- クライアントIP
- リクエストURL
- レスポンスコード
- 転送バイト数
- 処理時間

## 10. 注意事項

- 開発・テスト環境向け
- 本番環境での使用は推奨しない
- 大量トラフィックには未対応
