# SSL/TLS設定ガイド

このガイドでは、JumboDogXのHTTPサーバーでHTTPS（SSL/TLS）を設定し、暗号化通信を実現する方法を説明します。

## 目次

- [SSL/TLSとは](#ssltlsとは)
- [証明書の準備](#証明書の準備)
- [HTTPS設定方法](#https設定方法)
- [自己署名証明書の作成](#自己署名証明書の作成)
- [Let's Encrypt証明書の使用](#lets-encrypt証明書の使用)
- [セキュリティベストプラクティス](#セキュリティベストプラクティス)
- [トラブルシューティング](#トラブルシューティング)

## SSL/TLSとは

### 概要

SSL（Secure Sockets Layer）/TLS（Transport Layer Security）は、インターネット上で安全な通信を行うための暗号化プロトコルです。

### 主な機能

1. **暗号化**: 通信内容を第三者から保護
2. **認証**: サーバーの身元を証明
3. **完全性**: データの改ざんを検知

### HTTPSのメリット

- **データ保護**: パスワード、個人情報、クレジットカード情報などを保護
- **SEO効果**: Googleなどの検索エンジンがHTTPSサイトを優遇
- **ユーザー信頼**: ブラウザのアドレスバーに鍵マークが表示
- **必須化の流れ**: 多くのブラウザがHTTPサイトに警告を表示

### 対応プロトコル

JumboDogXは以下のTLSバージョンに対応しています：

| プロトコル | 対応 | 推奨 |
|-----------|------|------|
| SSL 2.0 | ✗ | 非推奨（脆弱性あり） |
| SSL 3.0 | ✗ | 非推奨（脆弱性あり） |
| TLS 1.0 | ✗ | 非推奨 |
| TLS 1.1 | ✗ | 非推奨 |
| TLS 1.2 | ✓ | 推奨 |
| TLS 1.3 | ✓ | 最も推奨 |

## 証明書の準備

### 証明書形式

JumboDogXはPFX（PKCS#12）形式の証明書をサポートしています。

| 形式 | 拡張子 | 説明 |
|------|--------|------|
| PFX/PKCS#12 | .pfx, .p12 | 秘密鍵と証明書を含む（推奨） |
| PEM | .pem, .crt | テキスト形式（要変換） |
| DER | .der, .cer | バイナリ形式（要変換） |

### 証明書の種類

#### 1. 自己署名証明書

**用途**: 開発・テスト環境

**メリット**:
- 無料
- すぐに作成可能
- ローカル環境で使用可能

**デメリット**:
- ブラウザで警告が表示される
- 本番環境では使用不可
- 第三者による検証がない

#### 2. 正規の証明書（CA発行）

**用途**: 本番環境

**メリット**:
- ブラウザで警告が表示されない
- ユーザーに信頼される
- SEO効果がある

**デメリット**:
- 費用がかかる場合がある（Let's Encryptは無料）
- 取得に手間がかかる
- 定期的な更新が必要

### 証明書取得方法

| サービス | 費用 | 有効期限 | 特徴 |
|----------|------|----------|------|
| Let's Encrypt | 無料 | 90日 | 自動更新可能、ドメイン認証のみ |
| ZeroSSL | 無料/有料 | 90日/1年 | Let's Encrypt代替 |
| DigiCert | 有料 | 1-3年 | エンタープライズ向け、EV証明書 |
| Sectigo | 有料 | 1-3年 | ドメイン認証、企業認証 |

## HTTPS設定方法

### Web UIからの設定

#### ステップ1: 証明書ファイルの配置

証明書ファイル（.pfx）をサーバーに配置します：

```
/path/to/certificates/
├── example.com.pfx
└── blog.example.com.pfx
```

#### ステップ2: HTTPS設定画面を開く

1. ブラウザで `http://localhost:5001` にアクセス
2. **Settings** → **HTTP/HTTPS** → **General** を選択

#### ステップ3: HTTPS設定を入力

以下の項目を設定します：

| 項目 | 設定値 | 説明 |
|------|--------|------|
| Protocol | HTTPS | HTTPSを選択 |
| Port | 443 | HTTPS標準ポート |
| Certificate File | `/path/to/certificate.pfx` | 証明書ファイルのパス |
| Certificate Password | `your-password` | 証明書のパスワード |

#### ステップ4: 設定を保存してサーバーを再起動

1. **Save Settings** ボタンをクリック
2. JumboDogXを再起動

### appsettings.jsonでの設定

`src/Jdx.WebUI/appsettings.json` を編集します。

#### 基本的なHTTPS設定

```json
{
  "Jdx": {
    "HttpServer": {
      "Enabled": true,
      "Protocol": "HTTPS",
      "Port": 443,
      "BindAddress": "0.0.0.0",
      "DocumentRoot": "/var/www/html",
      "CertificateFile": "/path/to/certificate.pfx",
      "CertificatePassword": "your-password",
      "WelcomeFileName": "index.html"
    }
  }
}
```

#### HTTPとHTTPSの同時運用

HTTPとHTTPSを同時に運用する場合は、メインサーバーとVirtual Hostを組み合わせます：

```json
{
  "Jdx": {
    "HttpServer": {
      "Enabled": true,
      "Protocol": "HTTP",
      "Port": 80,
      "BindAddress": "0.0.0.0",
      "DocumentRoot": "/var/www/html",
      "VirtualHosts": [
        {
          "Host": "example.com:443",
          "Enabled": true,
          "BindAddress": "0.0.0.0",
          "Settings": {
            "Protocol": "HTTPS",
            "DocumentRoot": "/var/www/html",
            "CertificateFile": "/path/to/certificate.pfx",
            "CertificatePassword": "your-password"
          }
        }
      ]
    }
  }
}
```

#### 複数ドメインのHTTPS設定

Virtual Hostを使用して、複数のドメインでHTTPSを設定：

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
          "CertificateFile": "/certs/example.com.pfx",
          "CertificatePassword": "${EXAMPLE_CERT_PASSWORD}"
        }
      },
      {
        "Host": "blog.example.com:443",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTPS",
          "DocumentRoot": "/var/www/blog",
          "CertificateFile": "/certs/blog.example.com.pfx",
          "CertificatePassword": "${BLOG_CERT_PASSWORD}"
        }
      }
    ]
  }
}
```

## 自己署名証明書の作成

開発・テスト環境で使用する自己署名証明書の作成方法を説明します。

### 方法1: .NET開発証明書（最も簡単）

.NET SDKに含まれる開発用証明書を使用します。

```bash
# 開発用HTTPS証明書を作成
dotnet dev-certs https --export-path ./certificate.pfx --password yourpassword

# 証明書を信頼済みとして登録（推奨）
dotnet dev-certs https --trust
```

**メリット**:
- 1コマンドで作成可能
- 開発環境で信頼される（--trust使用時）
- .NETツールとの統合

**デメリット**:
- `localhost` のみ対応
- カスタムドメインには不向き

### 方法2: OpenSSL（柔軟性が高い）

OpenSSLを使用して、詳細な設定が可能な証明書を作成します。

#### シンプルな証明書

```bash
# 秘密鍵と証明書を作成（有効期限365日）
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes \
  -subj "/CN=localhost"

# PFX形式に変換
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem \
  -password pass:yourpassword
```

#### 複数ドメイン対応（SAN証明書）

```bash
# 設定ファイルを作成
cat > san.cnf << 'EOF'
[req]
distinguished_name = req_distinguished_name
x509_extensions = v3_req
prompt = no

[req_distinguished_name]
CN = example.com

[v3_req]
subjectAltName = @alt_names

[alt_names]
DNS.1 = example.com
DNS.2 = www.example.com
DNS.3 = blog.example.com
DNS.4 = *.example.com
IP.1 = 127.0.0.1
IP.2 = 192.168.1.100
EOF

# SANを含む証明書を作成
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes \
  -config san.cnf

# PFX形式に変換
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem \
  -password pass:yourpassword
```

### 方法3: PowerShell（Windows）

Windows環境ではPowerShellで簡単に作成できます。

```powershell
# 自己署名証明書を作成
$cert = New-SelfSignedCertificate `
  -DnsName "localhost", "example.com", "*.example.com" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -NotAfter (Get-Date).AddYears(1) `
  -KeyAlgorithm RSA `
  -KeyLength 4096 `
  -KeyUsage DigitalSignature, KeyEncipherment `
  -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")

# PFXファイルにエクスポート
$password = ConvertTo-SecureString -String "yourpassword" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "./certificate.pfx" -Password $password
```

### 自己署名証明書を信頼する

開発環境でブラウザの警告を回避するため、証明書を信頼済みストアに追加します。

#### macOS

```bash
# 証明書をエクスポート（秘密鍵なし）
openssl pkcs12 -in certificate.pfx -clcerts -nokeys -out cert.crt \
  -passin pass:yourpassword

# キーチェーンに追加して信頼
sudo security add-trusted-cert -d -r trustRoot \
  -k /Library/Keychains/System.keychain cert.crt
```

#### Windows

```powershell
# 証明書をエクスポート
openssl pkcs12 -in certificate.pfx -clcerts -nokeys -out cert.crt \
  -passin pass:yourpassword

# 信頼済みルート証明機関に追加
Import-Certificate -FilePath "cert.crt" `
  -CertStoreLocation "Cert:\CurrentUser\Root"
```

#### Linux (Ubuntu/Debian)

```bash
# 証明書をエクスポート
openssl pkcs12 -in certificate.pfx -clcerts -nokeys -out cert.crt \
  -passin pass:yourpassword

# システムの信頼ストアに追加
sudo cp cert.crt /usr/local/share/ca-certificates/
sudo update-ca-certificates

# Chrome/Chromium用（NSS証明書データベース）
certutil -d sql:$HOME/.pki/nssdb -A -t "C,," -n "localhost" -i cert.crt
```

## Let's Encrypt証明書の使用

本番環境では、Let's Encryptで無料のSSL/TLS証明書を取得できます。

### 前提条件

- ドメイン名を所有している
- ドメインがサーバーのIPアドレスに向いている
- ポート80と443が開放されている

### Certbotのインストール

#### macOS

```bash
brew install certbot
```

#### Ubuntu/Debian

```bash
sudo apt update
sudo apt install certbot
```

#### CentOS/RHEL

```bash
sudo yum install certbot
```

### 証明書の取得

#### スタンドアロンモード（推奨）

JumboDogXを一時停止して、Certbotの組み込みWebサーバーで認証します。

```bash
# JumboDogXを停止
# Ctrl+C または kill コマンド

# 証明書を取得
sudo certbot certonly --standalone -d example.com -d www.example.com

# 証明書の場所
# /etc/letsencrypt/live/example.com/fullchain.pem
# /etc/letsencrypt/live/example.com/privkey.pem
```

#### Webルートモード

JumboDogXを稼働させたまま認証します。

```bash
# 証明書を取得
sudo certbot certonly --webroot -w /var/www/html -d example.com -d www.example.com
```

### PFX形式に変換

Let's Encryptの証明書をJumboDogXで使用できるPFX形式に変換します。

```bash
# PFX形式に変換
sudo openssl pkcs12 -export \
  -out /etc/letsencrypt/live/example.com/certificate.pfx \
  -inkey /etc/letsencrypt/live/example.com/privkey.pem \
  -in /etc/letsencrypt/live/example.com/fullchain.pem \
  -password pass:yourpassword

# ファイルのアクセス権限を設定
sudo chmod 644 /etc/letsencrypt/live/example.com/certificate.pfx
```

### JumboDogXの設定

```json
{
  "HttpServer": {
    "Protocol": "HTTPS",
    "Port": 443,
    "CertificateFile": "/etc/letsencrypt/live/example.com/certificate.pfx",
    "CertificatePassword": "yourpassword"
  }
}
```

### 自動更新

Let's Encrypt証明書は90日間有効なので、定期的な更新が必要です。

#### 更新コマンド

```bash
# 証明書を更新
sudo certbot renew

# PFX形式に再変換
sudo openssl pkcs12 -export \
  -out /etc/letsencrypt/live/example.com/certificate.pfx \
  -inkey /etc/letsencrypt/live/example.com/privkey.pem \
  -in /etc/letsencrypt/live/example.com/fullchain.pem \
  -password pass:yourpassword

# JumboDogXを再起動
sudo systemctl restart jumbodogx
```

#### Cron/Systemdタイマーで自動化

**/etc/cron.d/certbot-renew**:

```cron
0 3 * * * root certbot renew --quiet && \
  openssl pkcs12 -export \
    -out /etc/letsencrypt/live/example.com/certificate.pfx \
    -inkey /etc/letsencrypt/live/example.com/privkey.pem \
    -in /etc/letsencrypt/live/example.com/fullchain.pem \
    -password pass:yourpassword && \
  systemctl restart jumbodogx
```

## セキュリティベストプラクティス

### 証明書パスワードの管理

#### 環境変数の使用（推奨）

証明書のパスワードをappsettings.jsonに直接書かず、環境変数で管理します。

**環境変数の設定**:

```bash
# ~/.bashrc または ~/.zshrc に追加
export JUMBODOGX_CERT_PASSWORD="your-secure-password"

# または systemd サービスファイルで設定
# /etc/systemd/system/jumbodogx.service
[Service]
Environment="JUMBODOGX_CERT_PASSWORD=your-secure-password"
```

**appsettings.json**:

```json
{
  "HttpServer": {
    "CertificatePassword": "${JUMBODOGX_CERT_PASSWORD}"
  }
}
```

#### シークレット管理ツールの使用

本番環境では、シークレット管理ツールの使用を検討してください。

- **Azure Key Vault**: Azure環境
- **AWS Secrets Manager**: AWS環境
- **HashiCorp Vault**: オンプレミス、マルチクラウド

### 証明書ファイルの保護

#### ファイルのアクセス権限

証明書ファイルは読み取り専用にし、必要最小限のユーザーのみアクセス可能にします。

```bash
# 証明書ファイルの権限を設定
chmod 600 /path/to/certificate.pfx

# 所有者をJumboDogXの実行ユーザーに設定
chown jumbodogx:jumbodogx /path/to/certificate.pfx
```

#### .gitignoreに追加

証明書ファイルをGitリポジトリにコミットしないよう、.gitignoreに追加します。

```gitignore
# 証明書ファイル
*.pfx
*.p12
*.pem
*.key
*.crt
*.cer

# 設定ファイル（パスワードを含む場合）
appsettings.Production.json
```

### TLSバージョンの設定

古いTLSバージョンを無効化し、セキュリティを強化します。

JumboDogXはデフォルトでTLS 1.2以上のみを有効にしていますが、.NETの設定で変更可能です。

```json
{
  "HttpServer": {
    "SslProtocols": "Tls12, Tls13"
  }
}
```

### HSTS（HTTP Strict Transport Security）

HTTPSを強制し、中間者攻撃を防ぎます。

```json
{
  "HttpServer": {
    "CustomHeaders": {
      "Strict-Transport-Security": "max-age=31536000; includeSubDomains; preload"
    }
  }
}
```

**効果**:
- ブラウザが常にHTTPSで接続
- HTTPアクセスを自動的にHTTPSにリダイレクト
- 証明書エラーを無視できない

### セキュリティヘッダーの追加

```json
{
  "HttpServer": {
    "CustomHeaders": {
      "Strict-Transport-Security": "max-age=31536000; includeSubDomains",
      "X-Content-Type-Options": "nosniff",
      "X-Frame-Options": "SAMEORIGIN",
      "X-XSS-Protection": "1; mode=block",
      "Content-Security-Policy": "default-src 'self'"
    }
  }
}
```

## トラブルシューティング

### 証明書が読み込めない

#### 問題1: ファイルが見つからない

**症状**: `Certificate file not found` エラー

**解決策**:

1. **パスを確認**

```bash
# 証明書ファイルの存在確認
ls -la /path/to/certificate.pfx

# 絶対パスを使用
# 相対パス: ./certificate.pfx ✗
# 絶対パス: /path/to/certificate.pfx ✓
```

2. **アクセス権限を確認**

```bash
# 読み取り権限があるか確認
cat /path/to/certificate.pfx > /dev/null
```

#### 問題2: パスワードが間違っている

**症状**: `Invalid certificate password` エラー

**解決策**:

```bash
# パスワードを確認（証明書の内容を表示）
openssl pkcs12 -in certificate.pfx -nodes -passin pass:yourpassword
```

#### 問題3: 証明書が破損している

**症状**: `Corrupted certificate file` エラー

**解決策**:

```bash
# 証明書の検証
openssl pkcs12 -in certificate.pfx -noout -passin pass:yourpassword

# 破損している場合は再作成
```

### ブラウザで警告が表示される

#### 問題1: 「この接続ではプライバシーが保護されません」

**原因**: 自己署名証明書を使用している

**解決策**:

1. **開発環境**: 証明書を信頼済みストアに追加（上記参照）
2. **本番環境**: Let's Encryptなどで正規の証明書を取得

#### 問題2: 「証明書のホスト名が一致しません」

**原因**: 証明書のCN/SANとアクセスしているドメインが一致していない

**解決策**:

証明書の内容を確認：

```bash
openssl pkcs12 -in certificate.pfx -nodes -passin pass:yourpassword | \
  openssl x509 -noout -text | grep -A1 "Subject:"

openssl pkcs12 -in certificate.pfx -nodes -passin pass:yourpassword | \
  openssl x509 -noout -text | grep -A3 "Subject Alternative Name"
```

正しいドメイン名で証明書を再作成します。

#### 問題3: 「証明書の有効期限が切れています」

**症状**: `Certificate has expired` エラー

**解決策**:

```bash
# 証明書の有効期限を確認
openssl pkcs12 -in certificate.pfx -nodes -passin pass:yourpassword | \
  openssl x509 -noout -dates

# 有効期限が切れている場合は、証明書を更新
certbot renew  # Let's Encryptの場合
```

### 接続エラー

#### 問題1: ブラウザで接続できない

**症状**: `Connection refused` または `Unable to connect`

**解決策**:

1. **サーバーが起動しているか確認**

```bash
# プロセスを確認
ps aux | grep JumboDogX

# ポートがリッスンしているか確認
lsof -i :443  # macOS/Linux
netstat -ano | findstr :443  # Windows
```

2. **ファイアウォールを確認**

```bash
# ファイアウォールでポート443を開放
# macOS
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --add /path/to/jumbodogx

# Linux (ufw)
sudo ufw allow 443/tcp

# Linux (firewalld)
sudo firewall-cmd --permanent --add-port=443/tcp
sudo firewall-cmd --reload
```

#### 問題2: 混在コンテンツの警告

**症状**: HTTPSページでHTTPリソースを読み込むと警告が表示される

**解決策**:

HTMLを修正してすべてのリソースをHTTPSで読み込みます：

```html
<!-- 修正前 -->
<script src="http://example.com/script.js"></script>
<link rel="stylesheet" href="http://example.com/style.css">
<img src="http://example.com/image.jpg">

<!-- 修正後: HTTPSを使用 -->
<script src="https://example.com/script.js"></script>
<link rel="stylesheet" href="https://example.com/style.css">
<img src="https://example.com/image.jpg">

<!-- または相対パスを使用 -->
<script src="/script.js"></script>
<link rel="stylesheet" href="/style.css">
<img src="/image.jpg">

<!-- またはプロトコル相対URL -->
<script src="//example.com/script.js"></script>
```

### パフォーマンス問題

#### 問題: HTTPS接続が遅い

**原因**: SSL/TLSハンドシェイクのオーバーヘッド

**解決策**:

1. **Keep-Aliveを有効化**

```json
{
  "HttpServer": {
    "UseKeepAlive": true,
    "KeepAliveTimeout": 5
  }
}
```

2. **HTTP/2の使用を検討**（今後の実装予定）

### ログの確認

問題が発生した場合は、ログを確認してください：

```bash
# HTTPサーバーログ
cat logs/http-server.log | grep -i ssl
cat logs/http-server.log | grep -i certificate
cat logs/http-server.log | grep -i error

# エラーログ
cat logs/error.log
```

## 設定例

### シンプルなHTTPS設定

```json
{
  "Jdx": {
    "HttpServer": {
      "Enabled": true,
      "Protocol": "HTTPS",
      "Port": 443,
      "BindAddress": "0.0.0.0",
      "DocumentRoot": "/var/www/html",
      "CertificateFile": "/etc/ssl/certs/example.com.pfx",
      "CertificatePassword": "${CERT_PASSWORD}",
      "UseKeepAlive": true
    }
  }
}
```

### HTTPからHTTPSへのリダイレクト

現在、JumboDogXは自動リダイレクトをサポートしていませんが、HTTPとHTTPSを両方設定し、HTMLでメタリダイレクトを使用できます。

**HTTP設定（ポート80）**:

```json
{
  "HttpServer": {
    "Protocol": "HTTP",
    "Port": 80,
    "DocumentRoot": "/var/www/redirect"
  }
}
```

**/var/www/redirect/index.html**:

```html
<!DOCTYPE html>
<html>
<head>
  <meta http-equiv="refresh" content="0; url=https://example.com/">
  <title>Redirecting...</title>
</head>
<body>
  <p>Redirecting to <a href="https://example.com/">HTTPS version</a>...</p>
</body>
</html>
```

### 本番環境の推奨設定

```json
{
  "Jdx": {
    "HttpServer": {
      "Enabled": true,
      "Protocol": "HTTPS",
      "Port": 443,
      "BindAddress": "0.0.0.0",
      "DocumentRoot": "/var/www/html",
      "CertificateFile": "/etc/letsencrypt/live/example.com/certificate.pfx",
      "CertificatePassword": "${CERT_PASSWORD}",
      "UseKeepAlive": true,
      "KeepAliveTimeout": 5,
      "UseEtag": true,
      "ServerHeader": "",
      "CustomHeaders": {
        "Strict-Transport-Security": "max-age=31536000; includeSubDomains",
        "X-Content-Type-Options": "nosniff",
        "X-Frame-Options": "SAMEORIGIN"
      }
    }
  }
}
```

## 関連ドキュメント

- [クイックスタート](getting-started.md) - 基本的な設定手順
- [詳細設定](configuration.md) - HTTPサーバーの詳細設定
- [Virtual Host設定](virtual-hosts.md) - 複数サイトの運用
- [証明書セットアップ](../../docs/01_development_docs/05_certificate_setup.md) - 証明書の詳細
- [トラブルシューティング](troubleshooting.md) - 問題解決

## まとめ

HTTPS（SSL/TLS）を設定することで：

- 通信内容を暗号化し、データを保護
- サーバーの身元を証明し、ユーザーに信頼を提供
- SEO効果とブラウザの信頼を獲得
- セキュリティベストプラクティスに準拠

開発環境では自己署名証明書、本番環境ではLet's Encryptなどの正規の証明書を使用し、安全なWebサイトを構築してください。
