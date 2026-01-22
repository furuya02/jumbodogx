# 証明書ファイルのセットアップ

## 1. 概要

JumboDogXでは、以下のサーバーでTLS/SSL暗号化通信を使用できます。

| サーバー | 用途 |
|----------|------|
| HTTP | HTTPS (ポート443) |
| SMTP | STARTTLS / SMTPS |
| POP3 | POP3S (ポート995) |
| FTP | FTPS |

これらの機能を使用するには、証明書ファイル（PFX形式推奨）が必要です。

## 2. 証明書ファイルの作成

### 2.1 .NET開発証明書を使用（最も簡単）

開発環境で最も簡単な方法です。

```bash
# 開発用HTTPS証明書の作成
dotnet dev-certs https --export-path ./certificate.pfx --password yourpassword

# 証明書を信頼済みとして登録（オプション）
dotnet dev-certs https --trust
```

### 2.2 OpenSSLを使用

より詳細な設定が必要な場合に使用します。

```bash
# 秘密鍵と証明書を作成（有効期限365日）
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes \
  -subj "/CN=localhost"

# PFX形式に変換（.NETで使用しやすい形式）
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem -password pass:yourpassword
```

#### カスタムSAN（Subject Alternative Name）を含める場合

複数のホスト名やIPアドレスに対応する証明書を作成できます。

```bash
# 設定ファイルを作成
cat > san.cnf << EOF
[req]
distinguished_name = req_distinguished_name
x509_extensions = v3_req
prompt = no

[req_distinguished_name]
CN = localhost

[v3_req]
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
DNS.2 = *.local
IP.1 = 127.0.0.1
IP.2 = 192.168.1.100
EOF

# SANを含む証明書を作成
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes \
  -config san.cnf

# PFX形式に変換
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem -password pass:yourpassword
```

### 2.3 PowerShellを使用（Windows）

Windows環境ではPowerShellで簡単に作成できます。

```powershell
# 自己署名証明書を作成
$cert = New-SelfSignedCertificate `
  -DnsName "localhost", "*.local" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -NotAfter (Get-Date).AddYears(1) `
  -KeyAlgorithm RSA `
  -KeyLength 4096

# PFXファイルにエクスポート
$password = ConvertTo-SecureString -String "yourpassword" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "./certificate.pfx" -Password $password
```

## 3. 証明書ファイル形式

| 形式 | 拡張子 | 説明 | 推奨 |
|------|--------|------|------|
| PFX/PKCS#12 | .pfx, .p12 | 秘密鍵と証明書を含む | ○ |
| PEM | .pem, .crt | Base64エンコード | - |
| DER | .der, .cer | バイナリ形式 | - |

JumboDogXでは **PFX形式** を推奨します。

## 4. 証明書の配置

作成した証明書ファイルをプロジェクトルートまたは任意のディレクトリに配置します。

```
jumbodogx/
├── certificate.pfx    # 証明書ファイル
├── src/
└── ...
```

## 5. 設定例

### 5.1 appsettings.json

```json
{
  "Ssl": {
    "CertificatePath": "./certificate.pfx",
    "CertificatePassword": "yourpassword"
  }
}
```

### 5.2 環境変数での指定

パスワードを環境変数で管理することを推奨します。

```bash
export SSL_CERTIFICATE_PATH="./certificate.pfx"
export SSL_CERTIFICATE_PASSWORD="yourpassword"
```

## 6. 注意事項

### 6.1 自己署名証明書の制限

- 自己署名証明書は **開発・テスト環境専用** です
- ブラウザやクライアントで警告が表示されます
- 本番環境では使用しないでください

### 6.2 「保護されていない通信」警告について

自己署名証明書を使用してHTTPSサーバーにアクセスすると、ブラウザで以下のような警告が表示されます。

#### 警告の表示例

| ブラウザ | 警告メッセージ |
|----------|----------------|
| Chrome | 「この接続ではプライバシーが保護されません」 |
| Firefox | 「警告: 潜在的なセキュリティリスクあり」 |
| Safari | 「この接続はプライベートではありません」 |
| Edge | 「この接続ではプライバシーが保護されません」 |

#### 警告が表示される理由

自己署名証明書は、信頼された認証局（CA）によって発行されていないため、ブラウザは以下の点を検証できません：

1. **身元の確認**: サーバーが本当に主張している相手かどうか
2. **第三者による検証**: 信頼できる認証局がサーバーの身元を確認していない
3. **証明書チェーン**: ルート認証局までの信頼チェーンが存在しない

```
正規の証明書:
  ブラウザ → 中間CA → ルートCA（信頼済み）✓

自己署名証明書:
  ブラウザ → 自己署名証明書（信頼されていない）✗
```

**重要**: 通信自体は暗号化されており、盗聴からは保護されています。警告は「サーバーの身元が第三者によって確認されていない」ことを示しています。

#### 開発環境での警告回避方法

##### 方法1: .NET開発証明書を信頼済みとして登録

```bash
dotnet dev-certs https --trust
```

##### 方法2: 手動で証明書を信頼済みストアに追加

**Windows:**
```powershell
# 証明書をエクスポート（秘密鍵なし）
openssl pkcs12 -in certificate.pfx -clcerts -nokeys -out cert.crt -passin pass:yourpassword

# 信頼済みルート証明機関に追加
Import-Certificate -FilePath "cert.crt" -CertStoreLocation "Cert:\CurrentUser\Root"
```

**macOS:**
```bash
# 証明書をエクスポート
openssl pkcs12 -in certificate.pfx -clcerts -nokeys -out cert.crt -passin pass:yourpassword

# キーチェーンに追加して信頼
sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain cert.crt
```

**Linux (Chrome/Chromium):**
```bash
# 証明書をエクスポート
openssl pkcs12 -in certificate.pfx -clcerts -nokeys -out cert.crt -passin pass:yourpassword

# NSS証明書データベースに追加
certutil -d sql:$HOME/.pki/nssdb -A -t "C,," -n "localhost" -i cert.crt
```

##### 方法3: ブラウザで一時的に許可

開発中は、警告画面で「詳細設定」→「（安全でないことを理解した上で）続行」をクリックして一時的にアクセスできます。

### 6.3 本番環境での証明書取得

本番環境では、認証局（CA）から正規の証明書を取得してください。

| サービス | 費用 | 特徴 |
|----------|------|------|
| Let's Encrypt | 無料 | 90日有効、自動更新可能 |
| ZeroSSL | 無料/有料 | 90日有効（無料プラン） |
| DigiCert | 有料 | エンタープライズ向け |

### 6.4 セキュリティ

- 証明書ファイルとパスワードは安全に管理してください
- 証明書ファイルをGitリポジトリにコミットしないでください
- `.gitignore` に証明書ファイルを追加することを推奨します

```gitignore
# 証明書ファイル
*.pfx
*.p12
*.pem
*.key
```

## 7. トラブルシューティング

### 7.1 証明書が読み込めない

- ファイルパスが正しいか確認
- パスワードが正しいか確認
- ファイルの読み取り権限を確認

### 7.2 証明書の有効期限切れ

```bash
# 証明書の有効期限を確認
openssl pkcs12 -in certificate.pfx -nodes -passin pass:yourpassword | openssl x509 -noout -dates
```

### 7.3 クライアントで接続エラー

自己署名証明書の場合、クライアント側で証明書を信頼する設定が必要な場合があります。

## 8. 関連ドキュメント

- [HTTPサーバー](../02_server_protocols/01_http_server.md)
- [SMTPサーバー](../02_server_protocols/03_smtp_server.md)
- [POP3サーバー](../02_server_protocols/04_pop3_server.md)
- [FTPサーバー](../02_server_protocols/05_ftp_server.md)
