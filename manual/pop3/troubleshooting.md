# POP3サーバー - トラブルシューティング

JumboDogXのPOP3サーバーで発生する可能性のある問題と解決方法を説明します。

## 一般的な問題

### POP3サーバーが起動しない

#### 症状
- JumboDogXを起動してもPOP3サーバーが動作しない
- ダッシュボードでPOP3サーバーのステータスが"Stopped"または"Error"

#### 原因と解決方法

**原因1: ポート110が既に使用されている**

多くのシステムでは、ポート110が既に使用されている場合があります。

**解決方法**:

**macOS**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :110
```

**Windows**:
```cmd
# 使用中のプロセスを確認
netstat -ano | findstr :110
```

**Linux**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :110

# Dovecotなどが動作している場合、停止
sudo systemctl stop dovecot
```

**代替ポートの使用**（推奨）:
```json
{
  "Pop3Server": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 1110,  # ← 代替ポートを使用
    "MailDir": "C:\\JumboDogX\\Mailboxes",
    "Users": [...]
  }
}
```

クライアント側でポート1110を指定:
```python
pop = poplib.POP3('localhost', 1110)
```

---

**原因2: 管理者権限が不足している**

ポート110（1024未満の特権ポート）を使用する場合、管理者権限が必要です。

**解決方法**:

**macOS/Linux**:
```bash
sudo dotnet run --project src/Jdx.WebUI
```

**Windows**:
- コマンドプロンプトまたはPowerShellを「管理者として実行」
- Visual Studioを管理者として実行

---

**原因3: `Enabled`が`false`になっている**

**解決方法**:
```json
{
  "Pop3Server": {
    "Enabled": true,  # ← trueに設定
    "BindAddress": "0.0.0.0",
    "Port": 110
  }
}
```

---

**原因4: MailDirディレクトリが存在しない**

**解決方法**: MailDirディレクトリを作成してください

```bash
# macOS/Linux
mkdir -p /path/to/MailDir

# Windows
mkdir C:\JumboDogX\Mailboxes
```

---

### メール受信ができない

#### 症状
- POP3サーバーには接続できるが、メールが受信できない
- `STAT`コマンドで`+OK 0 0`（メールなし）が返される

#### 原因と解決方法

**原因1: 認証が失敗している**

**症状例**:
```
-ERR Authentication failed
```

**解決方法1**: パスワードハッシュを確認してください

```bash
# SHA256ハッシュの生成
echo -n "password" | sha256sum
```

設定ファイルのハッシュ値と一致するか確認してください。

**解決方法2**: HashTypeを確認してください

```json
{
  "UserName": "user1@example.com",
  "Password": "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
  "HashType": "SHA256"  # ← SHA256ハッシュの場合
}
```

詳細は[認証設定ガイド](authentication.md)を参照してください。

---

**原因2: メールボックスにメールがない**

**解決方法**: メールボックスにEMLファイルを配置してください

```bash
# macOS/Linux
cp test.eml /path/to/MailDir/user1@example.com/1.eml

# Windows
copy test.eml C:\JumboDogX\Mailboxes\user1@example.com\1.eml
```

**EMLファイルの例**:
```
From: sender@example.com
To: user1@example.com
Subject: Test Email
Date: Wed, 24 Jan 2026 12:00:00 +0900

This is a test email body.
```

---

**原因3: メールボックスディレクトリが存在しない**

**解決方法**: ユーザー名と同じディレクトリを作成してください

```bash
# macOS/Linux
mkdir -p /path/to/MailDir/user1@example.com

# Windows
mkdir C:\JumboDogX\Mailboxes\user1@example.com
```

---

**原因4: ファイアウォールでポートがブロックされている**

**解決方法**:

**macOS**:
```bash
# ファイアウォール設定を確認
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --getglobalstate

# JumboDogXを許可リストに追加
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --add /path/to/jumbodogx
```

**Windows**:
```powershell
# ファイアウォールルールを追加
New-NetFirewallRule -DisplayName "JumboDogX POP3" -Direction Inbound -Protocol TCP -LocalPort 110 -Action Allow
```

**Linux (ufw)**:
```bash
# ポート110を許可
sudo ufw allow 110/tcp
```

---

## メール関連の問題

### メールが正しく表示されない

#### 症状
- メールクライアントでメールを開くとエラーが発生する
- メールの内容が文字化けしている

#### 原因と解決方法

**原因1: EMLファイルの形式が間違っている**

**解決方法**: EMLファイルがRFC 822形式であることを確認してください

**正しいEMLファイル**:
```
From: sender@example.com
To: user1@example.com
Subject: Test Email
Date: Wed, 24 Jan 2026 12:00:00 +0900
Content-Type: text/plain; charset=utf-8

This is a test email body.
```

**重要**:
- ヘッダーとボディの間に空行が必要です
- 日本語を含む場合は`Content-Type`にcharsetを指定してください

---

**原因2: 文字エンコーディングが間違っている**

**解決方法**: EMLファイルをUTF-8で保存してください

```bash
# macOS/Linux
file -I test.eml  # エンコーディングを確認

# UTF-8に変換
iconv -f SHIFT_JIS -t UTF-8 test.eml > test_utf8.eml
```

---

### メールが削除できない

#### 症状
- `DELE`コマンドでメールを削除しても、ファイルが残っている
- 次回接続時に削除したメールが再び表示される

#### 原因と解決方法

**原因: `QUIT`コマンドを実行していない**

POP3では、`QUIT`コマンドを実行した時点で削除が確定します。

**解決方法**: 必ず`QUIT`コマンドを実行してください

```
DELE 1
+OK Message 1 deleted

QUIT  # ← この時点で削除が確定
+OK Bye
```

**Pythonでの削除例**:
```python
import poplib

pop = poplib.POP3('localhost', 110)
pop.user('user1@example.com')
pop.pass_('password')

# メール1を削除
pop.dele(1)

# QUITで削除を確定
pop.quit()  # ← 重要
```

---

## メールボックス関連の問題

### メールボックスディレクトリのパーミッションエラー

#### 症状
- メールボックスにアクセスできない
- ログに"Permission denied"エラーが記録される

#### 原因と解決方法

**原因: ディレクトリのアクセス権限が不適切**

**解決方法**: ディレクトリの権限を設定してください

**macOS/Linux**:
```bash
# MailDirディレクトリの権限を設定
sudo chown -R $USER:$USER /path/to/MailDir
chmod 755 /path/to/MailDir
chmod 755 /path/to/MailDir/user1@example.com
chmod 644 /path/to/MailDir/user1@example.com/*.eml
```

**Windows**:
```cmd
# アクセス権限を設定
icacls C:\JumboDogX\Mailboxes /grant:r %USERNAME%:F /inheritance:r
```

---

### EMLファイルの番号が重複している

#### 症状
- 同じメールが複数回表示される
- メールの順序がおかしい

#### 原因と解決方法

**原因: ファイル名が重複している**

**解決方法**: ファイル名を連番で付け直してください

```bash
# macOS/Linux
cd /path/to/MailDir/user1@example.com
ls -1 *.eml | sort -n | awk '{printf "%d.eml\n", NR}'  # 連番を確認

# 重複を解消（手動でリネーム）
mv duplicate.eml 4.eml
```

---

## 認証の問題

### telnetで認証できない

#### 症状
- `USER`コマンドを送信してもエラーが発生する

#### 原因と解決方法

**原因1: ユーザーが設定されていない**

**解決方法**: 少なくとも1つのユーザーアカウントを設定してください

```json
{
  "Pop3Server": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 110,
    "MailDir": "C:\\JumboDogX\\Mailboxes",
    "Users": [
      {
        "UserName": "user1@example.com",
        "Password": "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
        "HashType": "SHA256"
      }
    ]
  }
}
```

---

**原因2: パスワードが間違っている**

**解決方法**: 平文のパスワードを確認してください

telnetでは平文のパスワードを送信します（ハッシュ値ではありません）。

```
USER user1@example.com
+OK

PASS password  # ← 平文のパスワード
+OK Logged in
```

---

### Pythonでログインできない

#### 症状
- `poplib.error_proto`が発生する

#### 原因と解決方法

**原因: 平文のパスワードを使用していない**

**解決方法**: `pop.pass_()`にはハッシュ値ではなく、平文のパスワードを指定してください

```python
# 正しい
pop.user('user1@example.com')
pop.pass_('password')

# 間違い（ハッシュ値は使用できない）
pop.pass_('5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8')
```

---

## メールクライアント関連の問題

### Thunderbirdで接続できない

#### 症状
- Thunderbirdでアカウントを設定してもエラーが発生する

#### 原因と解決方法

**原因: サーバー設定が間違っている**

**解決方法**: Thunderbirdの設定を確認してください

1. アカウント設定 → サーバー設定
2. サーバー名: `localhost`
3. ポート: `110`
4. セキュリティ: なし
5. 認証方式: 通常のパスワード認証

![Thunderbird設定](images/thunderbird-pop3-settings.png)

---

### Outlookで接続できない

#### 症状
- Outlookでアカウントを設定してもエラーが発生する

#### 原因と解決方法

**原因: SSL/TLS設定が間違っている**

**解決方法**: OutlookのSSL/TLS設定を無効にしてください

1. アカウント追加 → 手動設定
2. 受信サーバー: `localhost`
3. ポート: `110`
4. 暗号化: なし
5. 認証: 基本認証

**注意**: 現在のJumboDogXバージョンでは、POP3のSSL/TLSには対応していません。

---

## パフォーマンスの問題

### メール受信が遅い

#### 症状
- メールの受信に時間がかかる
- タイムアウトが頻繁に発生する

#### 原因と解決方法

**原因1: メールボックスに大量のメールがある**

**解決方法**: 古いメールを削除するか、アーカイブしてください

```bash
# macOS/Linux
cd /path/to/MailDir/user1@example.com

# 古いメールを削除（例：30日以上前）
find . -name "*.eml" -mtime +30 -delete

# またはアーカイブ
mkdir archive
find . -name "*.eml" -mtime +30 -exec mv {} archive/ \;
```

---

**原因2: タイムアウト値が短い**

**解決方法**: タイムアウト値を増やしてください

```json
{
  "Pop3Server": {
    "TimeOut": 60  # ← 60秒に増加（デフォルトは30秒）
  }
}
```

---

**原因3: ネットワーク遅延**

**解決方法**: ネットワーク遅延を測定してください

```bash
# ローカルホストへのping
ping localhost
```

---

## Web UI関連の問題

### Web UIで設定が保存できない

#### 症状
- "Save Settings"ボタンをクリックしてもエラーが発生する

#### 原因と解決方法

**原因: ファイルのアクセス権限**

**解決方法**: 設定ファイルに書き込み権限があるか確認してください

```bash
# macOS/Linux
ls -l appsettings.json
chmod 644 appsettings.json  # 必要に応じて権限を変更

# Windows
icacls appsettings.json
```

---

### Web UIでユーザーが表示されない

#### 症状
- 設定ファイルにユーザーが存在するが、Web UIに表示されない

#### 原因と解決方法

**原因: ブラウザのキャッシュ**

**解決方法**: ブラウザのキャッシュをクリアしてページを再読み込みしてください

- Chrome/Edge: `Ctrl+Shift+R`（Windows）、`Cmd+Shift+R`（macOS）
- Firefox: `Ctrl+F5`（Windows）、`Cmd+Shift+R`（macOS）

---

## デバッグ方法

### ログの確認

JumboDogXのログファイルを確認して、エラーの詳細を調べてください。

**ログファイルの場所**:
```
logs/jumbodogx.log
logs/jumbodogx-YYYYMMDD.log
```

**ログの確認**:
```bash
# 最新のログを表示
tail -f logs/jumbodogx.log

# POP3に関するログのみを表示
grep POP3 logs/jumbodogx.log

# エラーのみを表示
grep ERROR logs/jumbodogx.log
```

詳細は[ロギング設定ガイド](../common/logging.md)を参照してください。

---

### telnetでのテスト

`telnet`コマンドでPOP3サーバーをテストできます。

**基本的な使用方法**:
```bash
telnet localhost 110

# サーバーからの応答
+OK POP3 server ready

# USERコマンド
USER user1@example.com
+OK

# PASSコマンド
PASS password
+OK Logged in

# STATコマンド（メール数とサイズを確認）
STAT
+OK 3 12345

# LISTコマンド（メール一覧）
LIST
+OK
1 4567
2 3456
3 4322
.

# QUITコマンド
QUIT
+OK Bye
```

---

### Pythonでのデバッグ

```python
import poplib

# デバッグレベルを設定
poplib._MAXLINE = 20480  # 最大行長を増やす

try:
    pop = poplib.POP3('localhost', 110)
    pop.set_debuglevel(2)  # デバッグ出力を有効化

    pop.user('user1@example.com')
    pop.pass_('password')

    # メール数を確認
    num_messages = len(pop.list()[1])
    print(f'Total messages: {num_messages}')

except Exception as e:
    print(f"Error: {e}")
finally:
    pop.quit()
```

---

## よくある質問（FAQ）

### Q1: ポート110以外を使用できますか？

**A**: はい、設定ファイルで任意のポートを指定できます。

```json
{
  "Pop3Server": {
    "Port": 1110  # ← 1110番ポートを使用
  }
}
```

クライアント側でポートを指定してください:
```python
pop = poplib.POP3('localhost', 1110)
```

---

### Q2: SSL/TLSに対応していますか？

**A**: 現在のバージョンでは、POP3のSSL/TLSには対応していません。将来のバージョンで追加予定です。

---

### Q3: IMAP対応の予定はありますか？

**A**: 将来のバージョンでIMAP対応を検討していますが、現在のところ具体的なスケジュールは未定です。

---

### Q4: メールの送信はできますか？

**A**: POP3サーバーはメール受信専用です。メール送信にはSMTPサーバーを使用してください。

JumboDogXのSMTPサーバーと連携することで、完全なメールシステムを構築できます。

---

### Q5: POP3サーバーを外部に公開できますか？

**A**: 技術的には可能ですが、セキュリティ上のリスクがあるため推奨しません。外部に公開する場合は、必ず認証を有効にし、SSL/TLSを使用してください（将来のバージョン）。

詳細は[セキュリティベストプラクティス](../common/security-best-practices.md)を参照してください。

---

## サポート

問題が解決しない場合は、以下を参照してください：

- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
- [ディスカッション](https://github.com/furuya02/jumbodogx/discussions)

Issue報告時には、以下の情報を含めてください：
- JumboDogXのバージョン
- OS（Windows/macOS/Linux）とバージョン
- 設定ファイル（`appsettings.json`の関連部分）
- ログファイル（`logs/jumbodogx.log`の関連部分）
- エラーメッセージ

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [認証設定](authentication.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
