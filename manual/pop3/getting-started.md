# POP3サーバー クイックスタートガイド

このガイドでは、JumboDogXのPOP3サーバーを最短で起動し、メール受信を行う方法を説明します。

## 前提条件

- JumboDogXがインストールされていること
- .NET 9 Runtimeがインストールされていること

詳細は[インストール手順](../common/installation.md)を参照してください。

## POP3サーバーとは？

POP3（Post Office Protocol version 3）サーバーは、メールを受信するためのサービスです。

### 用途

- **ローカル開発環境**: アプリケーションからのメール受信テスト
- **テスト環境**: メール機能のデバッグ
- **学習・教育**: メール受信の仕組みを理解する

**注意**: JumboDogXのPOP3サーバーは、ローカルテスト環境専用です。本番環境での使用は推奨されません。

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

## ステップ3: POP3基本設定

### 3-1. POP3設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **POP3** セクションを展開
3. **General** をクリック

![POP3設定画面](images/pop3-general-menu.png)
*サイドメニューからPOP3 > Generalを選択*

### 3-2. 基本設定を入力

以下の項目を設定します：

| 項目 | 設定値 | 説明 |
|------|--------|------|
| Enable Server | ✓ ON | POP3サーバーを有効化 |
| Bind Address | `0.0.0.0` または `127.0.0.1` | すべてのネットワークインターフェースでリッスン（`0.0.0.0`）またはローカルのみ（`127.0.0.1`） |
| Port | `110` | POP3標準ポート（テスト環境では11110なども可） |
| Timeout | `60` | コマンドのタイムアウト時間（秒） |
| Max Connections | `10` | 最大同時接続数 |

**ポートについて**:
- **110番**: POP3標準ポート（管理者権限が必要な場合あり）
- **995番**: POP3S（SSL/TLS）ポート
- **11110番**: テスト用代替ポート

![POP3基本設定](images/pop3-general-settings.png)
*POP3 Generalの設定画面*

### 3-3. ユーザーの追加

POP3サーバーにアクセスするユーザーを追加します。

1. **Users** セクションで以下を設定：
   - **Add User** ボタンをクリック
   - **Username**: `testuser`
   - **Password**: `testpass123` （強力なパスワードを設定）
   - **Mailbox Path**: `data/pop3/mailbox/testuser` （メールボックスのパス）

2. 必要に応じて複数のユーザーを追加

![POP3ユーザー設定](images/pop3-users.png)
*POP3ユーザー設定*

### 3-4. 設定を保存

1. **Save Settings** ボタンをクリック
2. 成功メッセージが表示されることを確認
3. **設定は即座に反映されます**（再起動不要）

## ステップ4: ACL設定（アクセス制御）

**重要**: デフォルトでは、POP3サーバーは **ACLがAllow Mode（全てDeny）** になっています。
このままではlocalhostからもアクセスできないため、ACL設定が必要です。

### 4-1. ACL設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **POP3** セクションを展開
3. **ACL** をクリック

![POP3 ACL設定画面](images/pop3-acl-menu.png)
*サイドメニューからPOP3 > ACLを選択*

### 4-2. localhostからのアクセスを許可

1. **Add ACL Entry** ボタンをクリック
2. 追加されたエントリに以下を入力：
   - **Name**: `Localhost`
   - **IP Address / Range**: `127.0.0.1`
3. **Save Settings** ボタンをクリック

![ACLエントリ追加](images/pop3-acl-localhost.png)
*127.0.0.1を追加した状態*

### 4-3. ローカルネットワークからのアクセスを許可（オプション）

同じネットワーク内の他のマシンからもアクセスを許可する場合：

1. **Add ACL Entry** ボタンをクリック
2. 追加されたエントリに以下を入力：
   - **Name**: `Local Network`
   - **IP Address / Range**: `192.168.1.0/24`
3. **Save Settings** ボタンをクリック

詳細は[ACL設定ガイド](../common/acl-configuration.md)を参照してください。

## ステップ5: テストメールの作成

POP3サーバーからメールを受信できるように、テストメールを作成します。

### 5-1. メールボックスディレクトリの作成

```bash
# ユーザーのメールボックスディレクトリを作成
mkdir -p data/pop3/mailbox/testuser
```

### 5-2. テストメールファイルの作成

メールボックスに手動でテストメールを配置します。

**data/pop3/mailbox/testuser/001.eml**:
```
From: sender@example.com
To: testuser@localhost
Subject: Test Email
Date: Fri, 24 Jan 2026 10:00:00 +0900
Message-ID: <test001@localhost>

This is a test email for POP3 server.
```

または、以下のコマンドで作成：

**Linux / macOS**:
```bash
cat > data/pop3/mailbox/testuser/001.eml << 'EOF'
From: sender@example.com
To: testuser@localhost
Subject: Test Email
Date: Fri, 24 Jan 2026 10:00:00 +0900
Message-ID: <test001@localhost>

This is a test email for POP3 server.
EOF
```

**Windows (PowerShell)**:
```powershell
@"
From: sender@example.com
To: testuser@localhost
Subject: Test Email
Date: Fri, 24 Jan 2026 10:00:00 +0900
Message-ID: <test001@localhost>

This is a test email for POP3 server.
"@ | Out-File -FilePath data\pop3\mailbox\testuser\001.eml -Encoding ASCII
```

![テストメール作成](images/pop3-test-email.png)
*テストメールファイルの作成*

## ステップ6: メール受信のテスト

### 6-1. telnetコマンドでテスト

POP3サーバーが正しく動作しているか、telnetコマンドで確認します。

#### Windows の場合

まず、Telnetクライアントを有効化します（SMTP編を参照）。

PowerShellまたはコマンドプロンプトで実行：

```cmd
telnet localhost 110
```

#### macOS / Linux の場合

ターミナルで実行：

```bash
telnet localhost 110
```

または、`nc`（netcat）コマンドを使用：

```bash
nc localhost 110
```

#### POP3コマンドを送信

Telnet接続後、以下のコマンドを順番に入力します：

```
USER testuser
PASS testpass123
STAT
LIST
RETR 1
QUIT
```

**期待される応答**:
```
+OK POP3 server ready
+OK User accepted
+OK Pass accepted
+OK 1 150
+OK 1 messages (150 octets)
1 150
.
+OK 150 octets
From: sender@example.com
To: testuser@localhost
Subject: Test Email
...
.
+OK Bye
```

![telnet POP3テスト](images/pop3-telnet-test.png)
*telnetコマンドでのPOP3テスト*

### 6-2. Pythonスクリプトでテスト

Pythonの`poplib`を使用してメールを受信するテストスクリプトです。

**pop3_test.py**:
```python
import poplib
import email
from email.parser import Parser

# POP3設定
pop3_host = 'localhost'
pop3_port = 110
username = 'testuser'
password = 'testpass123'

try:
    # POP3サーバーに接続
    server = poplib.POP3(pop3_host, pop3_port)
    server.set_debuglevel(1)  # デバッグ出力を有効化

    # ログイン
    server.user(username)
    server.pass_(password)

    # メール数を取得
    num_messages = len(server.list()[1])
    print(f'メール数: {num_messages}')

    # すべてのメールを取得
    for i in range(num_messages):
        response, lines, octets = server.retr(i + 1)
        msg_content = b'\r\n'.join(lines).decode('utf-8')
        msg = Parser().parsestr(msg_content)

        print(f'\n--- メール {i + 1} ---')
        print(f'From: {msg["From"]}')
        print(f'To: {msg["To"]}')
        print(f'Subject: {msg["Subject"]}')
        print(f'Date: {msg["Date"]}')
        print(f'Body: {msg.get_payload()}')

    # 接続を閉じる
    server.quit()
except Exception as e:
    print(f'エラー: {e}')
```

実行：

```bash
python pop3_test.py
```

### 6-3. メールクライアントで確認

ThunderbirdやOutlookなどのメールクライアントでも確認できます。

#### Thunderbirdの設定例

1. **ツール** > **アカウント設定** > **アカウント操作** > **メールアカウントを追加**
2. 以下の情報を入力：
   - **名前**: Test User
   - **メールアドレス**: testuser@localhost
   - **パスワード**: testpass123

3. **手動設定**をクリック
4. **受信サーバー**:
   - **プロトコル**: POP3
   - **サーバーのホスト名**: localhost
   - **ポート番号**: 110
   - **SSL**: なし
   - **認証方式**: 通常のパスワード認証

5. **完了**をクリック
6. メールボックスを確認してテストメールが表示されることを確認

![Thunderbird設定](images/pop3-thunderbird.png)
*Thunderbirdでの設定例*

## ステップ7: SMTPと連携してメールを送受信

JumboDogXのSMTPサーバーと連携して、完全なメール送受信環境を構築できます。

### 7-1. SMTPサーバーの起動

[SMTPサーバー クイックスタート](../smtp/getting-started.md)を参照して、SMTPサーバーを設定します。

### 7-2. SMTPでメールを送信

SMTPサーバー経由でメールを送信すると、POP3のメールボックスに自動的に配信されます。

```bash
# Pythonでメールを送信
python <<EOF
import smtplib
from email.mime.text import MIMEText

msg = MIMEText('This is a test email.')
msg['Subject'] = 'Test from SMTP'
msg['From'] = 'sender@example.com'
msg['To'] = 'testuser@localhost'

s = smtplib.SMTP('localhost', 25)
s.send_message(msg)
s.quit()
print('メール送信完了')
EOF
```

### 7-3. POP3でメールを受信

```bash
# Pythonでメールを受信
python pop3_test.py
```

送信したメールがPOP3サーバーで受信できることを確認します。

![SMTP-POP3連携](images/pop3-smtp-integration.png)
*SMTPとPOP3の連携*

## ステップ8: ログの確認

受信されたメールのログを確認できます。

### 8-1. Web UIでログを確認

1. サイドメニューから **Logs** をクリック
2. **Category** ドロップダウンから **Jdx.Servers.Pop3** を選択
3. メール受信のログが表示されます

![POP3ログ](images/pop3-logs.png)
*POP3サーバーのログ*

### 8-2. ログファイルで確認

```bash
# POP3サーバーのログを確認
cat logs/jumbodogx-*.log | jq 'select(.SourceContext == "Jdx.Servers.Pop3.Pop3Server")'

# ログイン成功のログのみ表示
cat logs/jumbodogx-*.log | jq 'select(.["@mt"] | test("User logged in"))'
```

## よくある問題と解決方法

### POP3サーバーに接続できない

**症状**: telnetやメールクライアントで接続できない

**原因1: ACLで拒否されている**

POP3サーバーはデフォルトでACLがAllow Mode（全てDeny）になっているため、ACL設定が必要です。

**解決策**:
1. Settings > POP3 > ACL を開く
2. "Add ACL Entry"で`127.0.0.1`を追加
3. "Save Settings"をクリック

**原因2: サーバーが起動していない**

**解決策**:
1. Dashboard > POP3 Serverでステータスを確認
2. Settings > POP3 > Generalで "Enable Server" がONか確認

**原因3: ポートが使用中**

```bash
# ポートの使用状況を確認（macOS/Linux）
sudo lsof -i :110

# ポートの使用状況を確認（Windows）
netstat -ano | findstr :110
```

**解決策**: 別のポート番号（例：11110）を使用する

### ログインできない

**症状**: "Authentication failed" エラー

**原因**: ユーザー名またはパスワードが間違っている

**解決策**:
1. Settings > POP3 > General > Users を確認
2. ユーザー名とパスワードが正しいか確認
3. 大文字小文字を区別することに注意

### メールが表示されない

**症状**: `STAT` コマンドで "0 messages"

**原因1: メールボックスが空**

**解決策**:
```bash
# メールボックスディレクトリを確認
ls -la data/pop3/mailbox/testuser/

# テストメールを追加
cat > data/pop3/mailbox/testuser/001.eml << 'EOF'
From: sender@example.com
To: testuser@localhost
Subject: Test Email

This is a test email.
EOF
```

**原因2: メールボックスのパスが間違っている**

**解決策**:
1. Settings > POP3 > General > Users を確認
2. Mailbox Pathが正しいか確認

### "Permission denied" エラー（ポート110使用時）

**症状**: ポート110でサーバーが起動できない

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

または、高いポート番号（11110など）を使用する。

## 次のステップ

基本的なPOP3サーバーが動作したら、以下のドキュメントで更に機能を拡張できます：

- [認証設定](authentication.md) - POP3認証の詳細設定
- [トラブルシューティング](troubleshooting.md) - よくある問題の解決方法
- [SMTPサーバー](../smtp/getting-started.md) - メール送信サーバーとの連携
- [ACL設定ガイド](../common/acl-configuration.md) - アクセス制御の詳細設定

## 実用的な使用例

### 例1: Webアプリケーションのメール受信テスト

Webアプリケーションからのメール受信機能をテストする：

**設定**:
- Port: `11110` （開発環境で使いやすいポート）
- Bind Address: `127.0.0.1` （ローカルのみ）
- Users: `testuser` / `testpass123`

**アプリケーション設定**:
```python
# Python poplib の例
import poplib

server = poplib.POP3('localhost', 11110)
server.user('testuser')
server.pass_('testpass123')
```

### 例2: メール受信の自動テスト

CI/CDパイプラインでメール受信をテスト：

```python
import poplib
import time

def wait_for_email(subject, timeout=30):
    start = time.time()
    while time.time() - start < timeout:
        server = poplib.POP3('localhost', 110)
        server.user('testuser')
        server.pass_('testpass123')

        num_messages = len(server.list()[1])
        for i in range(num_messages):
            response, lines, octets = server.retr(i + 1)
            msg_content = b'\r\n'.join(lines).decode('utf-8')
            if subject in msg_content:
                server.quit()
                return True

        server.quit()
        time.sleep(1)

    return False

# テスト実行
if wait_for_email('Test Subject'):
    print('✓ メール受信成功')
else:
    print('✗ メール受信失敗')
```

## まとめ

このガイドでは、以下の手順を完了しました：

✓ JumboDogXの起動とWeb管理画面へのアクセス
✓ POP3基本設定（ポート、Bind Address）
✓ ユーザーの追加
✓ ACL設定（localhostからのアクセス許可）
✓ テストメールの作成
✓ メール受信のテスト（telnet、Python）
✓ SMTPサーバーとの連携
✓ ログの確認

これで、JumboDogXのPOP3サーバーが起動し、メール受信ができるようになりました！

### 重要なポイント

- **POP3サーバーはデフォルトでACLがAllow Mode（全てDeny）** - 127.0.0.1の追加が必須
- ポート110を使用するには管理者権限が必要（または11110を使用）
- ユーザー名とパスワードの設定が必須
- メールボックスのパスを正しく設定する必要がある
- 設定は即座に反映されるため、再起動は不要

### セキュリティの注意

- JumboDogXのPOP3サーバーは、ローカルテスト環境専用です
- インターネットに直接公開しないでください
- ACLで適切にアクセス制御してください
- 強力なパスワードを使用してください

さらに詳しい設定は、各マニュアルを参照してください。
