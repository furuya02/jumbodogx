# SMTPサーバー - 認証設定

JumboDogXのSMTPサーバーにおける認証機能の詳細設定について説明します。

## SMTP認証とは

SMTP認証（SMTP AUTH）は、メール送信者がSMTPサーバーにアクセスする際に、ユーザー名とパスワードで認証を行う仕組みです。これにより、不正なメール送信（スパム）を防ぐことができます。

## サポートされている認証メカニズム

JumboDogXのSMTPサーバーは、以下の認証メカニズムをサポートしています：

### PLAIN認証

ユーザー名とパスワードをBase64エンコードして送信する最もシンプルな認証方式です。

**特徴**:
- シンプルで実装が容易
- Base64エンコードのみ（暗号化ではない）
- TLS使用を推奨（平文での送信は危険）

**SMTP通信例**:
```
AUTH PLAIN AGFsaWNlAHBhc3N3b3Jk
```
（`alice`と`password`をBase64エンコード）

---

### LOGIN認証

ユーザー名とパスワードを段階的にBase64エンコードして送信する認証方式です。

**特徴**:
- PLAINより若干複雑
- ユーザー名とパスワードを分けて送信
- TLS使用を推奨

**SMTP通信例**:
```
AUTH LOGIN
334 VXNlcm5hbWU6
YWxpY2U=
334 UGFzc3dvcmQ6
cGFzc3dvcmQ=
```

---

## ユーザーアカウントの設定

### 基本的なユーザー設定

```json
{
  "SmtpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 25,
    "Users": [
      {
        "UserName": "alice@example.com",
        "Password": "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
        "HashType": "SHA256"
      }
    ]
  }
}
```

**設定項目**:
- **UserName**: ユーザー名（メールアドレス形式を推奨）
- **Password**: パスワードのハッシュ値
- **HashType**: ハッシュアルゴリズム（SHA256またはSHA512）

---

### 複数ユーザーの設定

```json
{
  "SmtpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 25,
    "Users": [
      {
        "UserName": "alice@example.com",
        "Password": "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
        "HashType": "SHA256"
      },
      {
        "UserName": "bob@example.com",
        "Password": "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        "HashType": "SHA512"
      },
      {
        "UserName": "charlie@example.com",
        "Password": "d74ff0ee8da3b9806b18c877dbf29bbde50b5bd8e4dad7a3a725000feb82e8f1",
        "HashType": "SHA256"
      }
    ]
  }
}
```

---

## パスワードハッシュの生成

### SHA256ハッシュの生成

**macOS/Linux**:
```bash
echo -n "password" | sha256sum
# または
echo -n "password" | shasum -a 256
```

**Windows (PowerShell)**:
```powershell
$password = "password"
$hash = [System.Security.Cryptography.SHA256]::Create()
$bytes = $hash.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($password))
[BitConverter]::ToString($bytes).Replace("-", "").ToLower()
```

**オンラインツール**:
- https://emn178.github.io/online-tools/sha256.html

---

### SHA512ハッシュの生成

**macOS/Linux**:
```bash
echo -n "password" | sha512sum
# または
echo -n "password" | shasum -a 512
```

**Windows (PowerShell)**:
```powershell
$password = "password"
$hash = [System.Security.Cryptography.SHA512]::Create()
$bytes = $hash.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($password))
[BitConverter]::ToString($bytes).Replace("-", "").ToLower()
```

**オンラインツール**:
- https://emn178.github.io/online-tools/sha512.html

---

## ハッシュタイプの選択

### SHA256（推奨）

**メリット**:
- セキュアなハッシュアルゴリズム
- 処理速度が速い
- ハッシュ値が短い（64文字）

**デメリット**:
- SHA512より若干セキュリティレベルが低い

---

### SHA512（より安全）

**メリット**:
- より高いセキュリティレベル
- 将来的な攻撃への耐性が高い

**デメリット**:
- ハッシュ値が長い（128文字）
- 処理速度がSHA256より若干遅い

---

## Web UIでのユーザー管理

### ユーザーの追加

1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **SMTP** → **Users** を選択
4. **Add User** ボタンをクリック
5. 各フィールドに値を入力:
   - **Username**: ユーザー名（例: `alice@example.com`）
   - **Password**: パスワード（平文で入力、自動的にハッシュ化されます）
   - **Hash Type**: `SHA256`または`SHA512`を選択
6. **Add** ボタンをクリック
7. **Save Settings** ボタンをクリック

![ユーザー追加画面](images/smtp-user-add.png)

**注意**: Web UIでパスワードを入力すると、自動的にハッシュ化されます。手動でハッシュ値を入力する必要はありません。

---

### ユーザーの編集

1. Users画面でユーザー一覧を表示
2. 編集したいユーザーの **Edit** ボタンをクリック
3. パスワードを変更する場合は、新しいパスワードを入力
4. **Update** ボタンをクリック
5. **Save Settings** ボタンをクリック

![ユーザー編集画面](images/smtp-user-edit.png)

---

### ユーザーの削除

1. Users画面でユーザー一覧を表示
2. 削除したいユーザーの **Delete** ボタンをクリック
3. 確認ダイアログで **OK** をクリック
4. **Save Settings** ボタンをクリック

---

## 認証のテスト

### telnetでの認証テスト

**Base64エンコード**:
```bash
# ユーザー名をエンコード
echo -n "alice@example.com" | base64
# 結果: YWxpY2VAZXhhbXBsZS5jb20=

# パスワードをエンコード
echo -n "password" | base64
# 結果: cGFzc3dvcmQ=
```

**SMTP通信**:
```bash
telnet localhost 25

# サーバーからの応答
220 JumboDogX SMTP Server Ready

# AUTH LOGINコマンド
AUTH LOGIN
334 VXNlcm5hbWU6

# Base64エンコードされたユーザー名
YWxpY2VAZXhhbXBsZS5jb20=
334 UGFzc3dvcmQ6

# Base64エンコードされたパスワード
cGFzc3dvcmQ=
235 Authentication successful

QUIT
221 Bye
```

---

### Pythonでの認証テスト

```python
import smtplib

# SMTPサーバーに接続
server = smtplib.SMTP('localhost', 25)
server.set_debuglevel(1)  # デバッグ出力を有効化

try:
    # 認証
    server.login('alice@example.com', 'password')
    print("Authentication successful!")
except smtplib.SMTPAuthenticationError as e:
    print(f"Authentication failed: {e}")
finally:
    server.quit()
```

---

### Node.jsでの認証テスト

```javascript
const nodemailer = require('nodemailer');

const transporter = nodemailer.createTransport({
  host: 'localhost',
  port: 25,
  auth: {
    user: 'alice@example.com',
    pass: 'password'
  }
});

transporter.verify((error, success) => {
  if (error) {
    console.log('Authentication failed:', error);
  } else {
    console.log('Authentication successful!');
  }
});
```

---

## セキュリティのベストプラクティス

### 1. 強力なパスワードを使用

**推奨**:
- 最低12文字以上
- 大文字、小文字、数字、記号を混在
- 辞書に載っている単語を避ける

**例**:
- 良い: `K9#mX$7wP@2qL`
- 悪い: `password123`

---

### 2. SHA512を使用（可能な場合）

より高いセキュリティが必要な場合は、SHA512を使用してください。

```json
{
  "UserName": "alice@example.com",
  "Password": "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
  "HashType": "SHA512"
}
```

---

### 3. TLS/SSLの使用（リレー経由時）

外部SMTPサーバー（Gmail等）経由でメールを送信する場合は、必ずTLSを使用してください。

```json
{
  "SmtpServer": {
    "RelayServer": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "UseTls": true,  # ← TLSを有効化
      "Username": "your-email@gmail.com",
      "Password": "your-app-password"
    }
  }
}
```

詳細は[リレー設定ガイド](relay-configuration.md)を参照してください。

---

### 4. ローカルネットワークのみで使用

外部からのアクセスを防ぐため、ローカルネットワークのみでSMTPサーバーを使用してください。

```json
{
  "SmtpServer": {
    "BindAddress": "127.0.0.1",  # ← ローカルホストのみ
    "Port": 25
  }
}
```

---

### 5. 定期的なパスワード変更

セキュリティを維持するため、定期的にパスワードを変更してください。

---

## トラブルシューティング

### 認証が失敗する

**原因1: パスワードハッシュが間違っている**

**解決方法**: ハッシュ値を再生成して設定してください

```bash
# SHA256ハッシュの生成
echo -n "password" | sha256sum
```

---

**原因2: HashTypeが間違っている**

**解決方法**: ハッシュアルゴリズムと一致するHashTypeを設定してください

- SHA256ハッシュ → `"HashType": "SHA256"`
- SHA512ハッシュ → `"HashType": "SHA512"`

---

**原因3: ユーザー名が間違っている**

**解決方法**: 大文字小文字を含め、正確なユーザー名を入力してください

---

### Web UIでパスワードが保存できない

**原因: ブラウザのキャッシュ**

**解決方法**: ブラウザのキャッシュをクリアして再試行してください

---

### メールクライアントで認証エラー

**原因: 認証メカニズムの不一致**

**解決方法**: メールクライアントの認証設定を確認してください

- 認証方式: `PLAIN`または`LOGIN`
- ユーザー名: 設定したユーザー名
- パスワード: 平文のパスワード（ハッシュ値ではない）

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [リレー設定](relay-configuration.md)
- [トラブルシューティング](troubleshooting.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
