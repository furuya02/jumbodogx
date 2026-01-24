# HTTPサーバー 詳細設定

このドキュメントでは、JumboDogX HTTPサーバーの各種設定項目について詳しく説明します。

## 設定方法

### Web UIから設定

1. ブラウザで `http://localhost:5001` にアクセス
2. **Settings** → **HTTP/HTTPS** → 各設定画面を選択
3. 設定を変更後、**Save Settings** をクリック
4. サーバーを再起動して設定を反映

### 設定ファイルから設定

`src/Jdx.WebUI/appsettings.json` を直接編集します。

## 基本設定 (General)

### サーバー有効化

```json
{
  "HttpServer": {
    "Enabled": true
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| Enabled | boolean | true | HTTPサーバーを有効化/無効化 |

### ネットワーク設定

```json
{
  "HttpServer": {
    "Protocol": "HTTP",
    "Port": 8080,
    "BindAddress": "0.0.0.0"
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| Protocol | string | "HTTP" | プロトコル（HTTP/HTTPS） |
| Port | number | 8080 | リッスンポート番号 |
| BindAddress | string | "0.0.0.0" | バインドIPアドレス |

**BindAddress の値**:
- `0.0.0.0` - すべてのネットワークインターフェース
- `127.0.0.1` - ローカルホストのみ
- `192.168.1.100` - 特定のIPアドレス

### 接続設定

```json
{
  "HttpServer": {
    "TimeOut": 3,
    "MaxConnections": 100
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| TimeOut | number | 3 | タイムアウト時間（秒） |
| MaxConnections | number | 100 | 最大同時接続数 |

## ドキュメント設定 (Document)

### ドキュメントルート

```json
{
  "HttpServer": {
    "DocumentRoot": "/var/www/html",
    "WelcomeFileName": "index.html"
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| DocumentRoot | string | "" | Webサイトのルートディレクトリ |
| WelcomeFileName | string | "index.html" | ディレクトリアクセス時のデフォルトファイル |

**WelcomeFileName の動作**:
- `http://example.com/` → `DocumentRoot/index.html`
- `http://example.com/about/` → `DocumentRoot/about/index.html`

### ファイル表示設定

```json
{
  "HttpServer": {
    "UseHidden": false,
    "UseDot": false,
    "UseDirectoryEnum": false
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| UseHidden | boolean | false | 隠しファイル（Hidden属性）を表示 |
| UseDot | boolean | false | ドットファイル（.で始まる）を表示 |
| UseDirectoryEnum | boolean | false | ディレクトリリスティングを有効化 |

**セキュリティ注意**:
- `UseDirectoryEnum: true` はファイル一覧を公開するため、本番環境では通常オフにします

### エンコーディング設定

```json
{
  "HttpServer": {
    "Encode": "UTF-8"
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| Encode | string | "UTF-8" | 文字エンコーディング |

**対応エンコーディング**:
- UTF-8（推奨）
- Shift_JIS
- EUC-JP
- ISO-8859-1

## HTTP詳細設定

### サーバーヘッダー

```json
{
  "HttpServer": {
    "ServerHeader": "JumboDogX Version $v",
    "UseExpansion": false
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| ServerHeader | string | "JumboDogX Version $v" | Serverヘッダーの値 |
| UseExpansion | boolean | false | 変数展開を有効化 |

**変数一覧**:
- `$v` - JumboDogXのバージョン
- `$p` - プロトコル名
- `$d` - ドメイン名

**セキュリティ注意**:
- サーバー情報を隠すには、ServerHeaderを空文字列に設定

### Keep-Alive設定

```json
{
  "HttpServer": {
    "UseKeepAlive": true,
    "KeepAliveTimeout": 5,
    "MaxKeepAliveRequests": 100
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| UseKeepAlive | boolean | true | HTTP Keep-Aliveを有効化 |
| KeepAliveTimeout | number | 5 | Keep-Aliveタイムアウト（秒） |
| MaxKeepAliveRequests | number | 100 | Keep-Alive最大リクエスト数 |

**Keep-Aliveの効果**:
- 接続の再利用により、ページ読み込みが高速化
- サーバーリソースの節約

### ETag設定

```json
{
  "HttpServer": {
    "UseEtag": false
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| UseEtag | boolean | false | ETagヘッダーを有効化 |

**ETagの効果**:
- ブラウザキャッシュの効率化
- 帯域幅の節約（304 Not Modified）

### Range Requests設定

```json
{
  "HttpServer": {
    "UseRangeRequests": true,
    "MaxRangeCount": 20
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| UseRangeRequests | boolean | true | Range Requestsを有効化 |
| MaxRangeCount | number | 20 | 最大Range数 |

**Range Requestsの用途**:
- 大きなファイルのダウンロード再開
- 動画のシーク機能
- PDFの部分読み込み

## エラーページ設定

### カスタムエラーページ

```json
{
  "HttpServer": {
    "ErrorDocument": "/errors/404.html",
    "IndexDocument": "/errors/index.html"
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| ErrorDocument | string | "" | エラーページのパス |
| IndexDocument | string | "" | インデックスページのパス |

**カスタムエラーページの例**:

```html
<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title>404 Not Found</title>
</head>
<body>
    <h1>ページが見つかりません</h1>
    <p>お探しのページは存在しないか、移動した可能性があります。</p>
    <a href="/">トップページへ戻る</a>
</body>
</html>
```

## 管理者設定

```json
{
  "HttpServer": {
    "ServerAdmin": "admin@example.com"
  }
}
```

| 設定項目 | 型 | デフォルト | 説明 |
|---------|-----|-----------|------|
| ServerAdmin | string | "" | 管理者のメールアドレス |

エラーページに表示される管理者連絡先として使用されます。

## パフォーマンスチューニング

### 推奨設定（小規模サイト）

```json
{
  "HttpServer": {
    "MaxConnections": 50,
    "TimeOut": 5,
    "KeepAliveTimeout": 3,
    "MaxKeepAliveRequests": 50
  }
}
```

### 推奨設定（中規模サイト）

```json
{
  "HttpServer": {
    "MaxConnections": 200,
    "TimeOut": 10,
    "KeepAliveTimeout": 5,
    "MaxKeepAliveRequests": 100
  }
}
```

### 推奨設定（大規模サイト）

```json
{
  "HttpServer": {
    "MaxConnections": 500,
    "TimeOut": 15,
    "KeepAliveTimeout": 10,
    "MaxKeepAliveRequests": 200
  }
}
```

## 設定例

### 静的サイト（最小構成）

```json
{
  "HttpServer": {
    "Enabled": true,
    "Protocol": "HTTP",
    "Port": 8080,
    "BindAddress": "0.0.0.0",
    "DocumentRoot": "/var/www/html",
    "WelcomeFileName": "index.html",
    "TimeOut": 3,
    "MaxConnections": 100
  }
}
```

### 高性能サイト（最適化済み）

```json
{
  "HttpServer": {
    "Enabled": true,
    "Protocol": "HTTP",
    "Port": 8080,
    "BindAddress": "0.0.0.0",
    "DocumentRoot": "/var/www/html",
    "WelcomeFileName": "index.html",
    "TimeOut": 10,
    "MaxConnections": 300,
    "UseKeepAlive": true,
    "KeepAliveTimeout": 5,
    "MaxKeepAliveRequests": 100,
    "UseEtag": true,
    "UseRangeRequests": true,
    "MaxRangeCount": 20,
    "ServerHeader": ""
  }
}
```

## 関連ドキュメント

- [クイックスタート](getting-started.md) - 基本的な設定手順
- [SSL/TLS設定](ssl-tls.md) - HTTPS化の設定
- [Virtual Host設定](virtual-hosts.md) - 複数サイトの運用
- [トラブルシューティング](troubleshooting.md) - 問題解決

## 設定変更後の確認

設定を変更した後は、以下を確認してください：

1. **サーバーの再起動**: 設定を反映
2. **ログの確認**: エラーが発生していないか
3. **動作テスト**: ブラウザでアクセスして確認
4. **パフォーマンステスト**: 負荷をかけて動作確認
