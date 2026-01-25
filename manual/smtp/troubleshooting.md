# SMTPサーバー - トラブルシューティング

JumboDogXのSMTPサーバーで発生する可能性のある問題と解決方法を説明します。

## 一般的な問題

### SMTPサーバーが起動しない

#### 症状
- JumboDogXを起動してもSMTPサーバーが動作しない
- ダッシュボードでSMTPサーバーのステータスが"Stopped"または"Error"

#### 原因と解決方法

**原因1: ポート25が既に使用されている**

多くのシステムでは、ポート25が既に使用されています。

**解決方法**:

**macOS**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :25
```

**Windows**:
```cmd
# 使用中のプロセスを確認
netstat -ano | findstr :25
```

**Linux**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :25

# Postfixなどが動作している場合、停止
sudo systemctl stop postfix
```

**代替ポートの使用**（推奨）:
```json
{
  "SmtpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 2525,  # ← 代替ポートを使用
    "Users": [...]
  }
}
```

クライアント側でポート2525を指定:
```python
server = smtplib.SMTP('localhost', 2525)
```

---

**原因2: 管理者権限が不足している**

ポート25（1024未満の特権ポート）を使用する場合、管理者権限が必要です。

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
  "SmtpServer": {
    "Enabled": true,  # ← trueに設定
    "BindAddress": "0.0.0.0",
    "Port": 25
  }
}
```

---

### メール送信ができない

#### 症状
- SMTPサーバーには接続できるが、メールが送信できない
- タイムアウトエラーやエラーメッセージが表示される

#### 原因と解決方法

**原因1: 認証が失敗している**

**症状例**:
```
535 5.7.8 Username and Password not accepted
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
  "UserName": "test@example.com",
  "Password": "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
  "HashType": "SHA256"  # ← SHA256ハッシュの場合
}
```

詳細は[認証設定ガイド](authentication.md)を参照してください。

---

**原因2: ユーザーが設定されていない**

**解決方法**: 少なくとも1つのユーザーアカウントを設定してください

```json
{
  "SmtpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 25,
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

---

**原因3: ファイアウォールでポートがブロックされている**

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
New-NetFirewallRule -DisplayName "JumboDogX SMTP" -Direction Inbound -Protocol TCP -LocalPort 25 -Action Allow
```

**Linux (ufw)**:
```bash
# ポート25を許可
sudo ufw allow 25/tcp
```

---

## リレー関連の問題

### リレーサーバーに接続できない

#### 症状
- `Connection refused`または`Timeout`エラー
- メールが送信できない

#### 原因と解決方法

**原因1: リレーサーバーの設定が間違っている**

**解決方法**: ホスト名とポート番号を確認してください

| サービス | ホスト | ポート |
|----------|--------|--------|
| Gmail | `smtp.gmail.com` | 587または465 |
| Outlook | `smtp-mail.outlook.com` | 587 |
| Yahoo | `smtp.mail.yahoo.com` | 587 |
| SendGrid | `smtp.sendgrid.net` | 587 |

```json
{
  "RelayServer": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseTls": true
  }
}
```

---

**原因2: ネットワーク接続の問題**

**解決方法**: telnetでリレーサーバーへの接続をテストしてください

```bash
telnet smtp.gmail.com 587

# 接続成功の場合
220 smtp.gmail.com ESMTP ...

# 接続失敗の場合
Connection refused
```

接続できない場合、ファイアウォールやプロキシ設定を確認してください。

---

**原因3: TLS設定が間違っている**

**解決方法**: ポート番号に応じてTLS設定を確認してください

- ポート587: `UseTls: true`（STARTTLS）
- ポート465: `UseTls: true`（暗号化された接続）

```json
{
  "RelayServer": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseTls": true  # ← 必須
  }
}
```

---

### リレーサーバーの認証が失敗する

#### 症状
- `535 5.7.8 Username and Password not accepted`エラー
- Gmail: `535 5.7.8 Error: authentication failed`

#### 原因と解決方法

**原因1: アプリパスワードが間違っている（Gmail）**

**解決方法**: Gmailのアプリパスワードを再生成してください

1. https://myaccount.google.com/apppasswords にアクセス
2. 「アプリを選択」→「その他（カスタム名）」→「JumboDogX」
3. 「生成」をクリック
4. 16桁のパスワードをコピーして設定

```json
{
  "RelayServer": {
    "Username": "your-email@gmail.com",
    "Password": "abcd efgh ijkl mnop"  # ← アプリパスワード（スペースなしでも可）
  }
}
```

---

**原因2: 2段階認証が有効になっていない（Gmail）**

**解決方法**: Googleアカウントで2段階認証を有効にしてください

1. https://myaccount.google.com/security にアクセス
2. 「2段階認証プロセス」を有効化
3. アプリパスワードを生成

---

**原因3: UsernameまたはPasswordが間違っている**

**解決方法**: 認証情報を再確認してください

- Gmail: メールアドレスとアプリパスワード
- SendGrid: Username=`apikey`、Password=APIキー
- Amazon SES: SMTP認証情報（IAM認証情報ではない）

---

### メールが届かない（リレー経由）

#### 症状
- エラーは出ないが、メールが届かない
- 送信は成功するが、受信者に届いていない

#### 原因と解決方法

**原因1: 送信者アドレスが不正**

Gmailなどでは、リレーサーバーで許可されている送信者アドレスのみが使用できます。

**解決方法**: `MAIL FROM`で指定する送信者アドレスを確認してください

Gmail: `your-email@gmail.com`または設定したエイリアスのみ

```python
msg['From'] = 'your-email@gmail.com'  # ← Gmailのアドレスを使用
```

---

**原因2: スパムフィルターでブロックされている**

**解決方法**: 受信者の迷惑メールフォルダを確認してください

---

**原因3: 受信者アドレスが間違っている**

**解決方法**: メールアドレスのタイプミスを確認してください

---

## 認証の問題

### telnetで認証できない

#### 症状
- `AUTH LOGIN`コマンドを送信してもエラーが発生する

#### 原因と解決方法

**原因: Base64エンコードが間違っている**

**解決方法**: Base64エンコードを正しく行ってください

```bash
# ユーザー名をエンコード
echo -n "test@example.com" | base64
# 結果: dGVzdEBleGFtcGxlLmNvbQ==

# パスワードをエンコード
echo -n "password" | base64
# 結果: cGFzc3dvcmQ=
```

**SMTP通信**:
```
telnet localhost 25
220 JumboDogX SMTP Server Ready

AUTH LOGIN
334 VXNlcm5hbWU6

dGVzdEBleGFtcGxlLmNvbQ==
334 UGFzc3dvcmQ6

cGFzc3dvcmQ=
235 Authentication successful
```

---

### Pythonでログインできない

#### 症状
- `smtplib.SMTPAuthenticationError`が発生する

#### 原因と解決方法

**原因: 平文のパスワードを使用していない**

**解決方法**: `server.login()`にはハッシュ値ではなく、平文のパスワードを指定してください

```python
# 正しい
server.login('test@example.com', 'password')

# 間違い（ハッシュ値は使用できない）
server.login('test@example.com', '5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8')
```

---

## パフォーマンスの問題

### メール送信が遅い

#### 症状
- メール送信に時間がかかる
- タイムアウトが頻繁に発生する

#### 原因と解決方法

**原因1: リレーサーバーの応答が遅い**

**解決方法**: タイムアウト値を増やしてください

```json
{
  "SmtpServer": {
    "TimeOut": 60  # ← 60秒に増加（デフォルトは30秒）
  }
}
```

---

**原因2: ネットワーク遅延**

**解決方法**: ネットワーク遅延を測定してください

```bash
# リレーサーバーへのping
ping smtp.gmail.com

# traceroute
traceroute smtp.gmail.com
```

---

**原因3: リレーサーバーのレート制限**

Gmailなどでは、1日あたりの送信数に制限があります。

**Gmail**:
- 無料アカウント: 500通/日
- Google Workspace: 2000通/日

**解決方法**: 送信数を制限するか、複数のリレーサーバーを使用してください

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

# SMTPに関するログのみを表示
grep SMTP logs/jumbodogx.log

# エラーのみを表示
grep ERROR logs/jumbodogx.log

# リレーに関するログのみを表示
grep Relay logs/jumbodogx.log
```

詳細は[ロギング設定ガイド](../common/logging.md)を参照してください。

---

### telnetでのテスト

`telnet`コマンドでSMTPサーバーをテストできます。

**基本的な使用方法**:
```bash
telnet localhost 25

# サーバーからの応答
220 JumboDogX SMTP Server Ready

# EHLOコマンド
EHLO localhost
250-JumboDogX
250-AUTH PLAIN LOGIN
250 OK

# QUITコマンド
QUIT
221 Bye
```

---

### Pythonでのデバッグ

```python
import smtplib

server = smtplib.SMTP('localhost', 25)
server.set_debuglevel(1)  # デバッグ出力を有効化

try:
    server.login('test@example.com', 'password')
    print("Login successful!")
except Exception as e:
    print(f"Error: {e}")
finally:
    server.quit()
```

---

## よくある質問（FAQ）

### Q1: ポート25以外を使用できますか？

**A**: はい、設定ファイルで任意のポートを指定できます。

```json
{
  "SmtpServer": {
    "Port": 2525  # ← 2525番ポートを使用
  }
}
```

クライアント側でポートを指定してください:
```python
server = smtplib.SMTP('localhost', 2525)
```

---

### Q2: TLS/SSLに対応していますか？

**A**: 現在、JumboDogX→クライアント間のTLS/SSLには対応していません。ただし、JumboDogX→リレーサーバー間のTLS/SSLには対応しています。

```json
{
  "RelayServer": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseTls": true  # ← リレーサーバーへの接続でTLS使用
  }
}
```

---

### Q3: 複数のリレーサーバーを設定できますか？

**A**: 現在のバージョンでは、1つのリレーサーバーのみ設定可能です。複数のリレーサーバーのサポートは将来のバージョンで検討中です。

---

### Q4: メール送信数に制限はありますか？

**A**: JumboDogX自体には制限はありませんが、リレーサーバー（Gmail等）には制限があります。

| サービス | 送信制限 |
|----------|----------|
| Gmail（無料） | 500通/日 |
| Gmail（Workspace） | 2000通/日 |
| Outlook.com | 300通/日 |
| SendGrid（無料） | 100通/日 |

---

### Q5: SMTPサーバーを外部に公開できますか？

**A**: 技術的には可能ですが、セキュリティ上のリスクがあるため推奨しません。外部に公開する場合は、必ず認証を有効にし、TLS/SSLを使用してください。

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
- [リレー設定](relay-configuration.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
