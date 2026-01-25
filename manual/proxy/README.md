# Proxyサーバー

JumboDogXのProxyサーバーは、HTTPプロキシ機能を提供します。
Webアクセスのキャッシュやフィルタリング、ローカル開発環境でのプロキシテストに最適です。

## 主な機能

### 基本機能
- **HTTP/HTTPS対応** - Webトラフィックのプロキシ処理
- **透過的プロキシ** - クライアント設定で簡単に利用可能
- **高速なリレー** - 効率的なデータ転送

### キャッシュ機能
- **キャッシュ機能** - レスポンスをローカルに保存して高速化
- **キャッシュサイズ制限** - ディスク使用量の制御
- **キャッシュクリア** - 手動でのキャッシュクリア

### フィルタリング機能
- **URLフィルタリング** - 特定のURLへのアクセス制御
- **ドメインフィルタリング** - ドメイン単位でのアクセス制御
- **Allow/Denyリスト** - 柔軟なフィルタリングルール

### 高度な機能
- **柔軟なバインド設定** - 特定のIPアドレスやポートで待ち受け
- **タイムアウト設定** - 接続タイムアウトの制御
- **カスタムヘッダー** - リクエスト/レスポンスヘッダーの追加

## クイックスタート

Proxyサーバーをすぐに使い始めたい方は、[クイックスタートガイド](getting-started.md)をご覧ください。

## ドキュメント一覧

### 基本設定
- [クイックスタート](getting-started.md) - 最初の設定と起動方法
- [キャッシュ設定](cache-configuration.md) - キャッシュ機能の詳細設定
- [URLフィルタリング](url-filtering.md) - フィルタリングルールの設定

### トラブルシューティング
- [トラブルシューティング](troubleshooting.md) - よくある問題と解決方法

## 設定例

### シンプルなProxyサーバー

```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "TimeOut": 30
  }
}
```

### キャッシュ機能付きProxyサーバー

```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "TimeOut": 30,
    "Cache": {
      "Enabled": true,
      "CacheDirectory": "C:\\ProxyCache",
      "MaxCacheSize": 1073741824,
      "CacheExpiration": 3600
    }
  }
}
```

### URLフィルタリング付きProxyサーバー

```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "TimeOut": 30,
    "UrlFilter": {
      "Mode": "Deny",
      "Entries": [
        "*.facebook.com",
        "*.twitter.com",
        "example.com/blocked/*"
      ]
    }
  }
}
```

### Allowリスト（ホワイトリスト）設定

```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "TimeOut": 30,
    "UrlFilter": {
      "Mode": "Allow",
      "Entries": [
        "*.google.com",
        "*.github.com",
        "localhost/*"
      ]
    }
  }
}
```

**フィルタリングモード**:
- `Allow`: 許可リストモード（リストにあるURLのみ許可）
- `Deny`: 拒否リストモード（リストにあるURL以外を許可）

## Web UI

JumboDogXは、Webブラウザから設定できる管理画面を提供しています。

### アクセス方法
1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **Proxy** を選択

### 設定画面
- **General** - 基本設定（ポート、タイムアウト）
- **Cache** - キャッシュ機能の設定
- **URL Filter** - URLフィルタリングの設定

## ユースケース

### ローカル開発環境
開発中のアプリケーションのHTTPトラフィックを監視。

### キャッシュプロキシ
頻繁にアクセスするリソースをキャッシュして高速化。

### コンテンツフィルタリング
特定のWebサイトへのアクセスを制御。

### トラフィック分析
Webアクセスのログを記録して分析。

### 学習・教育
HTTPプロキシの仕組みを理解するための実験環境。

## クライアントからの利用

### ブラウザ設定

**Google Chrome**:
1. 設定 → 詳細設定 → システム
2. 「パソコンのプロキシ設定を開く」
3. プロキシサーバー: `localhost`、ポート: `8080`

**Firefox**:
1. 設定 → ネットワーク設定
2. 「手動でプロキシを設定する」
3. HTTPプロキシ: `localhost`、ポート: `8080`

**Microsoft Edge**:
1. 設定 → システムとパフォーマンス
2. 「パソコンのプロキシ設定を開く」
3. プロキシサーバー: `localhost`、ポート: `8080`

### OSレベルのプロキシ設定

**Windows**:
1. 設定 → ネットワークとインターネット → プロキシ
2. 「プロキシサーバーを使う」をオン
3. アドレス: `localhost`、ポート: `8080`

**macOS**:
1. システム設定 → ネットワーク → 詳細
2. プロキシ → Webプロキシ（HTTP）
3. サーバー: `localhost`、ポート: `8080`

**Linux**:
```bash
export http_proxy=http://localhost:8080
export https_proxy=http://localhost:8080
```

### curlでの利用

```bash
curl -x http://localhost:8080 https://example.com
```

### Pythonでの利用

```python
import requests

proxies = {
    'http': 'http://localhost:8080',
    'https': 'http://localhost:8080'
}

response = requests.get('https://example.com', proxies=proxies)
print(response.text)
```

### Node.jsでの利用

```javascript
const axios = require('axios');

axios.get('https://example.com', {
  proxy: {
    host: 'localhost',
    port: 8080
  }
}).then(response => {
  console.log(response.data);
});
```

## キャッシュディレクトリ構造

キャッシュは`CacheDirectory`で指定したディレクトリに保存されます。

```
CacheDirectory/
├── cache_index.db
├── 1a2b3c4d.cache
├── 5e6f7g8h.cache
└── ...
```

キャッシュファイルはハッシュ値を基にした名前で保存され、`cache_index.db`でインデックス管理されます。

## 関連リンク

- [インストール手順](../common/installation.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)

## 技術仕様

- **プロトコル**: HTTP/1.1, HTTPS (CONNECT)
- **標準ポート**: 8080
- **タイムアウト**: 設定可能（デフォルト30秒）
- **キャッシュ**: ファイルベースキャッシュ
- **デフォルトキャッシュサイズ**: 1GB
- **デフォルトキャッシュ有効期限**: 1時間

## セキュリティ上の注意

### オープンプロキシ
外部からのアクセスを許可する場合、オープンプロキシとして悪用されないよう注意してください。

### HTTPS検査
HTTPSトラフィックの内容を検査する場合、中間者攻撃（MITM）と同等の構成となるため、証明書の適切な管理が必要です。

### ログの保護
プロキシログにはURLや一部のヘッダー情報が含まれるため、適切に保護してください。

## サポート

問題が発生した場合は、以下を参照してください：
- [トラブルシューティング](troubleshooting.md)
- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
