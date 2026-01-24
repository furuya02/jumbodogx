# SMTPサーバー

JumboDogXのSMTPサーバーは、メール送信機能を提供します。
RFC 5321に準拠し、ローカル開発環境やテストネットワークでのメール送信テストに最適です。

## 主な機能

### 基本機能
- **RFC 5321準拠** - 標準的なSMTPプロトコルに対応
- **認証機能** - PLAIN、LOGIN認証メカニズム
- **リレー設定** - 外部SMTPサーバー経由での送信

### セキュリティ機能
- **ユーザー認証** - ユーザー名とパスワードによる認証
- **パスワードハッシュ** - SHA256/SHA512によるパスワード保護
- **タイムアウト設定** - 不正な接続への対策

### 高度な機能
- **外部SMTPリレー** - GmailなどのSMTPサーバーを経由した送信
- **複数アカウント** - 複数のユーザーアカウントを管理
- **柔軟なバインド設定** - 特定のIPアドレスやポートで待ち受け

## クイックスタート

SMTPサーバーをすぐに使い始めたい方は、[クイックスタートガイド](getting-started.md)をご覧ください。

## ドキュメント一覧

### 基本設定
- [クイックスタート](getting-started.md) - 最初の設定と起動方法
- [認証設定](authentication.md) - ユーザー認証の詳細設定
- [リレー設定](relay-configuration.md) - 外部SMTPサーバーの設定

### トラブルシューティング
- [トラブルシューティング](troubleshooting.md) - よくある問題と解決方法

## 設定例

### シンプルなSMTPサーバー

```json
{
  "SmtpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 25,
    "TimeOut": 30,
    "Users": [
      {
        "UserName": "test@example.com",
        "Password": "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
        "HashType": "SHA256"
      }
    ]
  }
}
```

### Gmail経由でメールを送信

```json
{
  "SmtpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 25,
    "RelayServer": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "UseTls": true,
      "Username": "your-email@gmail.com",
      "Password": "your-app-password"
    },
    "Users": [
      {
        "UserName": "test@example.com",
        "Password": "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
        "HashType": "SHA256"
      }
    ]
  }
}
```

**認証タイプ**:
- `PLAIN`: プレーンテキスト認証
- `LOGIN`: LOGIN認証メカニズム

**ハッシュタイプ**:
- `SHA256`: SHA-256ハッシュ（推奨）
- `SHA512`: SHA-512ハッシュ（より安全）

## Web UI

JumboDogXは、Webブラウザから設定できる管理画面を提供しています。

### アクセス方法
1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **SMTP** を選択

### 設定画面
- **General** - 基本設定（ポート、タイムアウト）
- **Users** - ユーザーアカウント管理
- **Relay** - 外部SMTPリレー設定

## ユースケース

### ローカル開発環境
開発中のアプリケーションからメール送信機能をテスト。

### メール送信テスト
本番環境にデプロイする前にメール送信フローを検証。

### 外部SMTPリレー
Gmailなどの外部SMTPサーバーを経由してメールを送信。

### 学習・教育
SMTPプロトコルの仕組みを理解するための実験環境。

## クライアントからの利用

### telnetコマンド

**Windows/macOS/Linux**:
```bash
telnet localhost 25
```

SMTPコマンド例:
```
EHLO localhost
AUTH LOGIN
(Base64エンコードされたユーザー名)
(Base64エンコードされたパスワード)
MAIL FROM:<sender@example.com>
RCPT TO:<recipient@example.com>
DATA
Subject: Test

This is a test email.
.
QUIT
```

### Pythonでのメール送信

```python
import smtplib
from email.mime.text import MIMEText

msg = MIMEText('This is a test email.')
msg['Subject'] = 'Test'
msg['From'] = 'sender@example.com'
msg['To'] = 'recipient@example.com'

with smtplib.SMTP('localhost', 25) as server:
    server.login('test@example.com', 'password')
    server.send_message(msg)
```

### Node.jsでのメール送信

```javascript
const nodemailer = require('nodemailer');

const transporter = nodemailer.createTransport({
  host: 'localhost',
  port: 25,
  auth: {
    user: 'test@example.com',
    pass: 'password'
  }
});

const mailOptions = {
  from: 'sender@example.com',
  to: 'recipient@example.com',
  subject: 'Test',
  text: 'This is a test email.'
};

transporter.sendMail(mailOptions);
```

## 関連リンク

- [インストール手順](../common/installation.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)

## 技術仕様

- **プロトコル**: SMTP (RFC 5321)
- **認証メカニズム**: PLAIN, LOGIN
- **標準ポート**: 25（SMTP）、587（Submission）
- **暗号化**: TLS（リレー経由時）
- **タイムアウト**: 設定可能（デフォルト30秒）
- **パスワードハッシュ**: SHA256, SHA512

## サポート

問題が発生した場合は、以下を参照してください：
- [トラブルシューティング](troubleshooting.md)
- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
