# HTTP/HTTPSサーバー

JumboDogXのHTTP/HTTPSサーバーは、静的ファイルの配信、CGI/SSI実行、WebDAV、Virtual Hostなど、多彩な機能を提供する高機能Webサーバーです。

## 主な機能

### 基本機能
- **静的ファイル配信** - HTML、CSS、JavaScript、画像などのファイルをブラウザに配信
- **HTTP/1.1対応** - Keep-Alive、Range Requests、ETagなど
- **カスタマイズ可能** - サーバーヘッダー、エンコーディング、MIMEタイプ設定

### セキュリティ機能
- **HTTPS/SSL/TLS** - 暗号化通信に対応
- **認証機能** - Basic認証、Digest認証
- **アクセス制御（ACL）** - IPアドレスベースのアクセス制限

### 高度な機能
- **Virtual Host** - 複数のドメインを1台のサーバーでホスティング
- **CGI/SSI** - 動的コンテンツの生成
- **WebDAV** - Webベースのファイル管理
- **ディレクトリリスティング** - フォルダ内のファイル一覧表示

## クイックスタート

HTTP/HTTPSサーバーをすぐに使い始めたい方は、[クイックスタートガイド](getting-started.md)をご覧ください。

## ドキュメント一覧

### 基本設定
- [クイックスタート](getting-started.md) - 最初の設定と起動方法
- [詳細設定](configuration.md) - 各種設定項目の詳細説明

### 機能別ガイド
- [Virtual Host設定](virtual-hosts.md) - 複数ドメインの運用方法
- [SSL/TLS設定](ssl-tls.md) - HTTPS化の手順と証明書設定
- [CGI/SSI設定](cgi-ssi.md) - 動的コンテンツの設定
- [WebDAV設定](webdav.md) - Webベースファイル管理の設定

### トラブルシューティング
- [トラブルシューティング](troubleshooting.md) - よくある問題と解決方法

## 設定例

### シンプルな静的サイト

```json
{
  "HttpServer": {
    "Enabled": true,
    "Protocol": "HTTP",
    "Port": 8080,
    "BindAddress": "0.0.0.0",
    "DocumentRoot": "/var/www/html",
    "WelcomeFileName": "index.html"
  }
}
```

### HTTPS対応サイト

```json
{
  "HttpServer": {
    "Enabled": true,
    "Protocol": "HTTPS",
    "Port": 443,
    "BindAddress": "0.0.0.0",
    "DocumentRoot": "/var/www/html",
    "CertificateFile": "/path/to/certificate.pfx",
    "CertificatePassword": "your-password"
  }
}
```

### Virtual Hostを使った複数ドメイン運用

```json
{
  "HttpServer": {
    "VirtualHosts": [
      {
        "Host": "example.com:443",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTPS",
          "DocumentRoot": "/var/www/example.com",
          "CertificateFile": "/path/to/example.pfx"
        }
      },
      {
        "Host": "blog.example.com:443",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTPS",
          "DocumentRoot": "/var/www/blog",
          "CertificateFile": "/path/to/blog.pfx"
        }
      }
    ]
  }
}
```

## Web UI

JumboDogXは、Webブラウザから設定できる管理画面を提供しています。

### アクセス方法
1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **HTTP/HTTPS** を選択

### 設定画面
- **General** - 基本設定とSSL/TLS設定
- **Virtual Hosts** - Virtual Hostの一覧と管理
- **ACL** - アクセス制御リスト
- **Document** - ドキュメントルートとファイル設定
- **CGI** - CGI/SSI設定
- **SSI** - SSI詳細設定
- **WebDAV** - WebDAV設定
- **Alias & MIME** - エイリアスとMIMEタイプ設定
- **Authentication** - 認証設定

## ユースケース

### 個人サイトのホスティング
静的HTMLサイトやブログを手軽に公開できます。

### 開発環境のローカルサーバー
Webアプリケーション開発時のテストサーバーとして利用できます。

### 社内イントラネットサーバー
社内文書の共有や情報ポータルとして活用できます。

### 複数サイトの統合管理
Virtual Host機能により、1台のサーバーで複数のWebサイトを運用できます。

## 関連リンク

- [インストール手順](../common/installation.md)
- [ACL設定](../common/acl-configuration.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)

## 技術仕様

- **プロトコル**: HTTP/1.1, HTTPS (TLS 1.2/1.3)
- **対応証明書**: PFX形式
- **最大接続数**: 設定可能（デフォルト100）
- **タイムアウト**: 設定可能（デフォルト3秒）
- **Keep-Alive**: 対応
- **Range Requests**: 対応
- **圧縮**: 未対応（今後の実装予定）

## サポート

問題が発生した場合は、以下を参照してください：
- [トラブルシューティング](troubleshooting.md)
- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
