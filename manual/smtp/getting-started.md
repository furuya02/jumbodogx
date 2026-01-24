# SMTPサーバー クイックスタートガイド

このガイドでは、JumboDogXのSMTPサーバーを最短で起動し、メール送信を行う方法を説明します。

## 前提条件

- JumboDogXがインストールされていること
- .NET 9 Runtimeがインストールされていること

詳細は[インストール手順](../common/installation.md)を参照してください。

## SMTPサーバーとは？

SMTP（Simple Mail Transfer Protocol）サーバーは、メールを送信するためのサービスです。

### 用途

- **ローカル開発環境**: アプリケーションからのメール送信テスト
- **テスト環境**: メール機能のデバッグ
- **学習・教育**: メール送信の仕組みを理解する

**注意**: JumboDogXのSMTPサーバーは、ローカルテスト環境専用です。本番環境での使用は推奨されません。

## ステップ1: JumboDogXの起動

### 起動方法

```bash
cd /path/to/jumbodogx
dotnet run --project src/Jdx.WebUI
```

### 起動確認

ターミナルに以下のようなメッセージが表示されれば成功です：

```
Now listening on: http://localhost:5001
Application started. Press Ctrl+C to shut down.
```

## ステップ2: Web管理画面にアクセス

ブラウザで以下のURLを開きます：

```
http://localhost:5001
```

![ダッシュボード画面](images/dashboard-top.png)
*JumboDogXのダッシュボード画面*

## ステップ3: SMTP基本設定

### 3-1. SMTP設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **SMTP** セクションを展開
3. **General** をクリック

![SMTP設定画面](images/smtp-general-menu.png)
*サイドメニューからSMTP > Generalを選択*

### 3-2. 基本設定を入力

以下の項目を設定します：

| 項目 | 設定値 | 説明 |
|------|--------|------|
| Enable Server | ✓ ON | SMTPサーバーを有効化 |
| Bind Address | `0.0.0.0` または `127.0.0.1` | すべてのネットワークインターフェースでリッスン（`0.0.0.0`）またはローカルのみ（`127.0.0.1`） |
| Port | `25` | SMTP標準ポート（テスト環境では587や2525も可） |
| Timeout | `60` | コマンドのタイムアウト時間（秒） |
| Max Message Size | `10485760` | 最大メッセージサイズ（バイト、10MB） |

**ポートについて**:
- **25番**: SMTP標準ポート（管理者権限が必要な場合あり）
- **587番**: SMTP Submission ポート（推奨）
- **2525番**: テスト用代替ポート

![SMTP基本設定](images/smtp-general-settings.png)
*SMTP Generalの設定画面*

### 3-3. 認証設定（推奨）

セキュリティのため、認証を有効にすることを推奨します。

1. **Authentication** セクションで以下を設定：
   - **Require Authentication**: ✓ ON
   - **Authentication Method**: `PLAIN` または `LOGIN`

2. **ユーザーの追加**:
   - **Add User** ボタンをクリック
   - **Username**: `testuser`
   - **Password**: `testpass123` （強力なパスワードを設定）

![SMTP認証設定](images/smtp-authentication.png)
*SMTP認証設定*

### 3-4. 設定を保存

1. **Save Settings** ボタンをクリック
2. 成功メッセージが表示されることを確認
3. **設定は即座に反映されます**（再起動不要）

## ステップ4: メール送信のテスト

### 4-1. telnetコマンドでテスト

SMTPサーバーが正しく動作しているか、telnetコマンドで確認します。

#### Windows の場合

まず、Telnetクライアントを有効化します：

1. **コントロールパネル** > **プログラムと機能** > **Windowsの機能の有効化または無効化**
2. **Telnetクライアント** にチェック > **OK**

PowerShellまたはコマンドプロンプトで実行：

```cmd
telnet localhost 25
```

#### macOS / Linux の場合

ターミナルで実行：

```bash
telnet localhost 25
```

または、`nc`（netcat）コマンドを使用：

```bash
nc localhost 25
```

#### SMTPコマンドを送信

Telnet接続後、以下のコマンドを順番に入力します：

```
EHLO localhost
MAIL FROM:<sender@example.com>
RCPT TO:<recipient@example.com>
DATA
Subject: Test Email
From: sender@example.com
To: recipient@example.com

This is a test email from JumboDogX SMTP server.
.
QUIT
```

**期待される応答**:
```
220 JumboDogX SMTP Server Ready
250-localhost
250-AUTH PLAIN LOGIN
250 OK
250 OK
250 OK
354 Start mail input; end with <CRLF>.<CRLF>
250 OK: Message accepted
221 Bye
```

![telnet SMTPテスト](images/smtp-telnet-test.png)
*telnetコマンドでのSMTPテスト*

### 4-2. Pythonスクリプトでテスト

Pythonを使用してメールを送信するテストスクリプトです。

**smtp_test.py**:
```python
import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart

# SMTP設定
smtp_host = 'localhost'
smtp_port = 25

# メール内容
sender = 'sender@example.com'
recipient = 'recipient@example.com'
subject = 'Test Email from JumboDogX'
body = 'This is a test email sent from JumboDogX SMTP server.'

# メッセージを作成
message = MIMEMultipart()
message['From'] = sender
message['To'] = recipient
message['Subject'] = subject
message.attach(MIMEText(body, 'plain'))

try:
    # SMTPサーバーに接続
    server = smtplib.SMTP(smtp_host, smtp_port)
    server.set_debuglevel(1)  # デバッグ出力を有効化

    # 認証が必要な場合（設定に応じて）
    # server.login('testuser', 'testpass123')

    # メールを送信
    server.send_message(message)
    print('メール送信成功！')

    # 接続を閉じる
    server.quit()
except Exception as e:
    print(f'エラー: {e}')
```

実行：

```bash
python smtp_test.py
```

### 4-3. Node.jsスクリプトでテスト

Node.jsの`nodemailer`を使用したテストスクリプトです。

まず、nodemailerをインストール：

```bash
npm install nodemailer
```

**smtp_test.js**:
```javascript
const nodemailer = require('nodemailer');

// SMTP設定
const transporter = nodemailer.createTransport({
    host: 'localhost',
    port: 25,
    secure: false, // TLS未使用
    // 認証が必要な場合
    // auth: {
    //     user: 'testuser',
    //     pass: 'testpass123'
    // }
});

// メール内容
const mailOptions = {
    from: 'sender@example.com',
    to: 'recipient@example.com',
    subject: 'Test Email from JumboDogX',
    text: 'This is a test email sent from JumboDogX SMTP server.',
    html: '<p>This is a <strong>test email</strong> sent from JumboDogX SMTP server.</p>'
};

// メールを送信
transporter.sendMail(mailOptions, (error, info) => {
    if (error) {
        console.log('エラー:', error);
    } else {
        console.log('メール送信成功:', info.response);
    }
});
```

実行：

```bash
node smtp_test.js
```

## ステップ5: 送信済みメールの確認

JumboDogXは、送信されたメールをファイルとして保存します。

### 5-1. メールボックスの場所

デフォルトでは、以下のディレクトリに保存されます：

```
jumbodogx/
└── data/
    └── smtp/
        └── mailbox/
            └── <recipient@example.com>/
                └── <message-id>.eml
```

### 5-2. メールファイルの確認

送信されたメールは、EML形式で保存されます。

```bash
# メールボックスを確認（Linux/macOS）
ls -la data/smtp/mailbox/

# メールの内容を表示
cat data/smtp/mailbox/recipient@example.com/*.eml
```

**Windows**:
```powershell
# メールボックスを確認
dir data\smtp\mailbox\

# メールの内容を表示
type data\smtp\mailbox\recipient@example.com\*.eml
```

EMLファイルは、メールクライアント（Outlook、Thunderbirdなど）で開くこともできます。

![メールボックスの確認](images/smtp-mailbox.png)
*送信済みメールの確認*

## ステップ6: リレー設定（外部SMTPサーバー経由）

JumboDogXのSMTPサーバーを、外部のSMTPサーバー経由でメールを送信するように設定できます。

### 6-1. Relay設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **SMTP** セクションを展開
3. **Relay Configuration** をクリック

### 6-2. リレー設定を入力

| 項目 | 設定値 | 説明 |
|------|--------|------|
| Enable Relay | ✓ ON | リレーを有効化 |
| Relay Host | `smtp.gmail.com` | 外部SMTPサーバーのホスト名 |
| Relay Port | `587` | 外部SMTPサーバーのポート |
| Use TLS | ✓ ON | TLS/SSL接続を使用 |
| Username | `your-email@gmail.com` | 外部SMTPサーバーのユーザー名 |
| Password | `your-password` | 外部SMTPサーバーのパスワード |

**例: Gmailを使用する場合**:
- Relay Host: `smtp.gmail.com`
- Relay Port: `587`
- Use TLS: ON
- Username: あなたのGmailアドレス
- Password: アプリパスワード（Gmailの2段階認証が必要）

![SMTPリレー設定](images/smtp-relay-config.png)
*SMTPリレー設定画面*

### 6-3. 設定を保存

1. **Save Settings** ボタンをクリック
2. テストメールを送信して、リレーが正しく動作することを確認

**注意**: Gmail などの外部サービスを使用する場合、アプリパスワードの作成や2段階認証の設定が必要になる場合があります。

## ステップ7: ログの確認

送信されたメールのログを確認できます。

### 7-1. Web UIでログを確認

1. サイドメニューから **Logs** をクリック
2. **Category** ドロップダウンから **Jdx.Servers.Smtp** を選択
3. メール送信のログが表示されます

![SMTPログ](images/smtp-logs.png)
*SMTPサーバーのログ*

### 7-2. ログファイルで確認

```bash
# SMTPサーバーのログを確認
cat logs/jumbodogx-*.log | jq 'select(.SourceContext == "Jdx.Servers.Smtp.SmtpServer")'

# 送信成功のログのみ表示
cat logs/jumbodogx-*.log | jq 'select(.["@mt"] | test("Message sent"))'
```

## よくある問題と解決方法

### SMTPサーバーに接続できない

**症状**: telnetやメールクライアントで接続できない

**原因1: サーバーが起動していない**

**解決策**:
1. Dashboard > SMTP Serverでステータスを確認
2. Settings > SMTP > Generalで "Enable Server" がONか確認

**原因2: ポートが使用中**

```bash
# ポートの使用状況を確認（macOS/Linux）
sudo lsof -i :25

# ポートの使用状況を確認（Windows）
netstat -ano | findstr :25
```

**解決策**: 別のポート番号（例：587、2525）を使用する

**原因3: ファイアウォールでブロックされている**

**解決策**: ファイアウォール設定でSMTPポートを許可

### 認証エラー

**症状**: "Authentication failed" エラー

**原因**: ユーザー名またはパスワードが間違っている

**解決策**:
1. Settings > SMTP > General > Authentication を確認
2. ユーザー名とパスワードが正しいか確認
3. 認証方式（PLAIN, LOGIN）を確認

### メールが送信されない

**症状**: SMTPコマンドは成功するが、メールが届かない

**原因1: メールボックスが作成されていない**

**解決策**:
```bash
# メールボックスディレクトリを確認
ls -la data/smtp/mailbox/
```

**原因2: リレー設定が間違っている**

**解決策**:
1. Settings > SMTP > Relay Configuration を確認
2. Relay Host, Port, Username, Passwordが正しいか確認
3. ログでエラーメッセージを確認

### "Permission denied" エラー（ポート25使用時）

**症状**: ポート25でサーバーが起動できない

**原因**: ポート1024未満は管理者権限が必要

**解決策**:

**Windows**: 管理者としてPowerShellを起動
```powershell
# PowerShellを管理者として実行
cd C:\JumboDogX
.\Jdx.WebUI.exe
```

**macOS / Linux**: sudoで実行
```bash
sudo dotnet run --project src/Jdx.WebUI
```

または、高いポート番号（587、2525など）を使用する。

## 次のステップ

基本的なSMTPサーバーが動作したら、以下のドキュメントで更に機能を拡張できます：

- [認証設定](authentication.md) - SMTP認証の詳細設定
- [リレー設定](relay-configuration.md) - 外部SMTPサーバー経由での送信
- [トラブルシューティング](troubleshooting.md) - よくある問題の解決方法

## 実用的な使用例

### 例1: Webアプリケーションのメール送信テスト

Webアプリケーションからのメール送信機能をテストする：

**設定**:
- Port: `2525` （開発環境で使いやすいポート）
- Require Authentication: OFF （ローカルテストのみ）
- Bind Address: `127.0.0.1` （ローカルのみ）

**アプリケーション設定**:
```javascript
// Node.js Express アプリの例
const transporter = nodemailer.createTransport({
    host: 'localhost',
    port: 2525,
    secure: false
});
```

### 例2: CI/CDパイプラインの通知

テスト完了時にメール通知を送信：

```bash
# テスト完了後にメール送信
python <<EOF
import smtplib
from email.mime.text import MIMEText

msg = MIMEText('Tests completed successfully!')
msg['Subject'] = 'CI/CD Build Status'
msg['From'] = 'ci@example.com'
msg['To'] = 'developer@example.com'

s = smtplib.SMTP('localhost', 25)
s.send_message(msg)
s.quit()
EOF
```

### 例3: バックアップ通知

バックアップ完了時に通知メールを送信：

```python
import smtplib
from email.mime.text import MIMEText
import datetime

def send_backup_notification(status):
    msg = MIMEText(f'Backup {status} at {datetime.datetime.now()}')
    msg['Subject'] = f'Backup {status}'
    msg['From'] = 'backup@example.com'
    msg['To'] = 'admin@example.com'

    s = smtplib.SMTP('localhost', 25)
    s.send_message(msg)
    s.quit()

# バックアップ処理
try:
    # バックアップ処理...
    send_backup_notification('SUCCESS')
except Exception as e:
    send_backup_notification(f'FAILED: {e}')
```

## まとめ

このガイドでは、以下の手順を完了しました：

✓ JumboDogXの起動とWeb管理画面へのアクセス
✓ SMTP基本設定（ポート、Bind Address）
✓ 認証設定（推奨）
✓ メール送信のテスト（telnet、Python、Node.js）
✓ 送信済みメールの確認
✓ リレー設定（オプション）
✓ ログの確認

これで、JumboDogXのSMTPサーバーが起動し、メール送信ができるようになりました！

### 重要なポイント

- ポート25を使用するには管理者権限が必要（または587、2525を使用）
- 認証を有効化することを推奨（セキュリティ向上）
- 送信されたメールはdata/smtp/mailbox/に保存される
- リレー設定で外部SMTPサーバー経由での送信が可能
- 設定は即座に反映されるため、再起動は不要

### セキュリティの注意

- JumboDogXのSMTPサーバーは、ローカルテスト環境専用です
- インターネットに直接公開しないでください
- 認証を必ず有効化してください（外部アクセスを許可する場合）
- 強力なパスワードを使用してください

さらに詳しい設定は、各マニュアルを参照してください。
