# SMTPサーバー - リレー設定

JumboDogXのSMTPサーバーにおける外部SMTPサーバーへのリレー設定について説明します。

## SMTPリレーとは

SMTPリレーは、JumboDogXのSMTPサーバーが受信したメールを、外部のSMTPサーバー（Gmail、Outlook.com等）経由で実際に送信する仕組みです。

### リレーを使用する理由

1. **実際のメール送信**: ローカルSMTPサーバーから直接送信するのではなく、信頼性の高い外部SMTPサーバー経由で送信
2. **スパムフィルター回避**: 外部SMTPサーバーの認証を使用することで、スパムとして扱われにくい
3. **開発環境でのテスト**: 開発中のアプリケーションから、実際にメールを送信してテスト可能

---

## 基本的なリレー設定

### 設定例

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

**設定項目**:
- **Host**: リレー先のSMTPサーバーのホスト名
- **Port**: リレー先のSMTPサーバーのポート番号
- **UseTls**: TLS/SSL暗号化の使用（trueを推奨）
- **Username**: リレー先の認証ユーザー名
- **Password**: リレー先の認証パスワード（平文）

---

## 主要メールサービスの設定

### Gmail

Gmailをリレーサーバーとして使用する場合、アプリパスワードを生成する必要があります。

**リレー設定**:
```json
{
  "RelayServer": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseTls": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

**アプリパスワードの生成**:
1. Googleアカウントにログイン
2. https://myaccount.google.com/apppasswords にアクセス
3. 「アプリを選択」→「その他（カスタム名）」→「JumboDogX」と入力
4. 「生成」をクリック
5. 表示された16桁のパスワードをコピーして設定

**ポート**:
- `587`: STARTTLS（推奨）
- `465`: SSL/TLS

---

### Outlook.com / Hotmail

```json
{
  "RelayServer": {
    "Host": "smtp-mail.outlook.com",
    "Port": 587,
    "UseTls": true,
    "Username": "your-email@outlook.com",
    "Password": "your-password"
  }
}
```

**ポート**:
- `587`: STARTTLS（推奨）

---

### Yahoo Mail

```json
{
  "RelayServer": {
    "Host": "smtp.mail.yahoo.com",
    "Port": 587,
    "UseTls": true,
    "Username": "your-email@yahoo.com",
    "Password": "your-app-password"
  }
}
```

**注意**: Yahoo Mailもアプリパスワードの生成が必要です。

---

### SendGrid

```json
{
  "RelayServer": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "UseTls": true,
    "Username": "apikey",
    "Password": "your-api-key"
  }
}
```

**注意**: UsernameはAPIキーではなく、文字列`apikey`を使用します。

---

### Amazon SES

```json
{
  "RelayServer": {
    "Host": "email-smtp.us-east-1.amazonaws.com",
    "Port": 587,
    "UseTls": true,
    "Username": "your-smtp-username",
    "Password": "your-smtp-password"
  }
}
```

**注意**: リージョンに応じてホスト名が異なります（`us-east-1`の部分）。

---

## TLS/SSL設定

### TLS（STARTTLS）とSSL/TLSの違い

| 項目 | TLS (STARTTLS) | SSL/TLS |
|------|----------------|---------|
| ポート | 587 | 465 |
| 接続方法 | 平文で接続後、TLSにアップグレード | 最初から暗号化 |
| 推奨度 | ◎ 推奨 | ○ 利用可能 |

### TLSの有効化

```json
{
  "RelayServer": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseTls": true  # ← TLSを有効化
  }
}
```

**重要**: `UseTls: true`を設定すると、ポート587ではSTARTTLS、ポート465では暗号化されたSSL/TLS接続が使用されます。

---

## リレー設定のテスト

### telnetでのテスト

**Gmail経由の場合**:
```bash
# 接続テスト
telnet smtp.gmail.com 587

# サーバーからの応答
220 smtp.gmail.com ESMTP ...

EHLO localhost
250-smtp.gmail.com at your service
250-STARTTLS
...

QUIT
```

---

### Pythonでのテスト

```python
import smtplib
from email.mime.text import MIMEText

# メッセージ作成
msg = MIMEText('This is a test email.')
msg['Subject'] = 'Test from JumboDogX'
msg['From'] = 'test@example.com'
msg['To'] = 'recipient@example.com'

# JumboDogXのSMTPサーバーに接続
with smtplib.SMTP('localhost', 25) as server:
    server.set_debuglevel(1)  # デバッグ出力を有効化

    # JumboDogXで認証
    server.login('test@example.com', 'password')

    # メール送信（JumboDogXがGmail経由で送信）
    server.send_message(msg)

print("Email sent successfully!")
```

---

### curlでのテスト

```bash
curl --url "smtp://localhost:25" \
  --mail-from "test@example.com" \
  --mail-rcpt "recipient@example.com" \
  --user "test@example.com:password" \
  --upload-file - <<EOF
From: test@example.com
To: recipient@example.com
Subject: Test from JumboDogX

This is a test email.
EOF
```

---

## Web UIでのリレー設定

### リレーサーバーの設定

1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **SMTP** → **Relay** を選択
4. 各フィールドに値を入力:
   - **Host**: リレー先のSMTPサーバー（例: `smtp.gmail.com`）
   - **Port**: ポート番号（例: `587`）
   - **Use TLS**: チェックを入れる
   - **Username**: 認証ユーザー名（例: `your-email@gmail.com`）
   - **Password**: 認証パスワード（例: アプリパスワード）
5. **Test Connection** ボタンをクリックして接続をテスト（オプション）
6. **Save Settings** ボタンをクリック

![リレー設定画面](images/smtp-relay-settings.png)

---

## セキュリティのベストプラクティス

### 1. TLSを必ず使用

平文での認証情報の送信を防ぐため、必ずTLSを使用してください。

```json
{
  "RelayServer": {
    "UseTls": true  # ← 必須
  }
}
```

---

### 2. アプリパスワードの使用

Gmailなどでは、アカウントのパスワードではなく、アプリパスワードを使用してください。

**メリット**:
- アカウントのパスワードを直接使用しない
- アプリパスワードは個別に無効化可能
- より安全

---

### 3. パスワードの保護

設定ファイルにパスワードを平文で保存する場合、ファイルのアクセス権限を適切に設定してください。

**macOS/Linux**:
```bash
chmod 600 appsettings.json
```

**Windows**:
```cmd
icacls appsettings.json /grant:r %USERNAME%:RW /inheritance:r
```

---

### 4. 環境変数の使用（推奨）

パスワードをハードコードする代わりに、環境変数を使用することを検討してください。

**設定例**:
```json
{
  "RelayServer": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseTls": true,
    "Username": "${SMTP_RELAY_USERNAME}",
    "Password": "${SMTP_RELAY_PASSWORD}"
  }
}
```

**環境変数の設定**:
```bash
# macOS/Linux
export SMTP_RELAY_USERNAME="your-email@gmail.com"
export SMTP_RELAY_PASSWORD="your-app-password"

# Windows (PowerShell)
$env:SMTP_RELAY_USERNAME = "your-email@gmail.com"
$env:SMTP_RELAY_PASSWORD = "your-app-password"
```

**注意**: 現在のJumboDogXバージョンでは、環境変数の自動展開はサポートされていません。将来のバージョンで追加予定です。

---

## トラブルシューティング

### リレーサーバーに接続できない

**症状**: `Connection refused`や`Timeout`エラー

**原因と解決方法**:

**原因1: ファイアウォールでポートがブロックされている**

**解決方法**: ファイアウォールでポート587または465を許可してください

**macOS**:
```bash
# ファイアウォール設定を確認
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --getglobalstate
```

**Windows**:
```powershell
# ポート587の送信を許可
New-NetFirewallRule -DisplayName "SMTP 587" -Direction Outbound -Protocol TCP -RemotePort 587 -Action Allow
```

---

**原因2: ホスト名またはポート番号が間違っている**

**解決方法**: 設定を確認してください

| サービス | ホスト | ポート |
|----------|--------|--------|
| Gmail | `smtp.gmail.com` | 587または465 |
| Outlook | `smtp-mail.outlook.com` | 587 |
| Yahoo | `smtp.mail.yahoo.com` | 587 |

---

### 認証エラー

**症状**: `Authentication failed`や`535 5.7.8 Username and Password not accepted`

**原因と解決方法**:

**原因1: アプリパスワードが間違っている**

**解決方法**: Gmailのアプリパスワードを再生成して設定してください

---

**原因2: 2段階認証が有効になっていない（Gmail）**

**解決方法**: Googleアカウントで2段階認証を有効にしてください

1. https://myaccount.google.com/security にアクセス
2. 「2段階認証プロセス」を有効化
3. アプリパスワードを生成

---

**原因3: UsernameまたはPasswordが間違っている**

**解決方法**: 認証情報を確認してください

- Gmail: メールアドレスとアプリパスワード
- SendGrid: `apikey`とAPIキー
- Amazon SES: SMTP認証情報（IAM認証情報ではない）

---

### TLS/SSLエラー

**症状**: `SSL handshake failed`や`Certificate verification failed`

**原因と解決方法**:

**原因: TLS設定が間違っている**

**解決方法**: ポート番号に応じてTLS設定を確認してください

- ポート587: `UseTls: true`（STARTTLS）
- ポート465: `UseTls: true`（暗号化された接続）
- ポート25: `UseTls: false`（平文、推奨しません）

---

### メールが送信されない

**症状**: エラーは出ないが、メールが届かない

**原因と解決方法**:

**原因1: 送信者アドレスが不正**

**解決方法**: リレーサーバーで許可されている送信者アドレスを使用してください

Gmailの場合、`your-email@gmail.com`または設定したエイリアスのみが送信者として使用できます。

---

**原因2: 受信者アドレスが間違っている**

**解決方法**: 受信者のメールアドレスを確認してください

---

**原因3: スパムフィルターでブロック**

**解決方法**: 受信者の迷惑メールフォルダを確認してください

---

## ログの確認

リレー関連のエラーは、ログファイルに記録されます。

```bash
# ログファイルの確認
tail -f logs/jumbodogx.log

# SMTPに関するログのみを表示
grep SMTP logs/jumbodogx.log

# リレーに関するログのみを表示
grep Relay logs/jumbodogx.log
```

詳細は[ロギング設定ガイド](../common/logging.md)を参照してください。

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [認証設定](authentication.md)
- [トラブルシューティング](troubleshooting.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
- [ロギング設定](../common/logging.md)
