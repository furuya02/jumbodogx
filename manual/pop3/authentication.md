# POP3サーバー - 認証設定

JumboDogXのPOP3サーバーにおける認証機能の詳細設定について説明します。

## POP3認証とは

POP3認証は、メール受信者がPOP3サーバーにアクセスする際に、ユーザー名とパスワードで認証を行う仕組みです。これにより、各ユーザーのメールボックスが保護されます。

## 認証の仕組み

POP3では、USER/PASSコマンドを使用して認証を行います。

**POP3通信例**:
```
USER user1@example.com
+OK
PASS password
+OK Logged in
```

---

## ユーザーアカウントの設定

### 基本的なユーザー設定

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

**設定項目**:
- **UserName**: ユーザー名（メールアドレス形式を推奨）
- **Password**: パスワードのハッシュ値
- **HashType**: ハッシュアルゴリズム（SHA256またはSHA512）

---

### 複数ユーザーの設定

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
      },
      {
        "UserName": "user2@example.com",
        "Password": "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        "HashType": "SHA512"
      },
      {
        "UserName": "user3@example.com",
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
3. サイドメニューから **Settings** → **POP3** → **Users** を選択
4. **Add User** ボタンをクリック
5. 各フィールドに値を入力:
   - **Username**: ユーザー名（例: `user1@example.com`）
   - **Password**: パスワード（平文で入力、自動的にハッシュ化されます）
   - **Hash Type**: `SHA256`または`SHA512`を選択
6. **Add** ボタンをクリック
7. **Save Settings** ボタンをクリック

![ユーザー追加画面](images/pop3-user-add.png)

**注意**: Web UIでパスワードを入力すると、自動的にハッシュ化されます。手動でハッシュ値を入力する必要はありません。

---

### ユーザーの編集

1. Users画面でユーザー一覧を表示
2. 編集したいユーザーの **Edit** ボタンをクリック
3. パスワードを変更する場合は、新しいパスワードを入力
4. **Update** ボタンをクリック
5. **Save Settings** ボタンをクリック

![ユーザー編集画面](images/pop3-user-edit.png)

---

### ユーザーの削除

1. Users画面でユーザー一覧を表示
2. 削除したいユーザーの **Delete** ボタンをクリック
3. 確認ダイアログで **OK** をクリック
4. **Save Settings** ボタンをクリック

---

## メールボックスの管理

### メールボックスの構造

各ユーザーのメールボックスは、`MailDir`で指定したディレクトリ配下に作成されます。

```
MailDir/
├── user1@example.com/
│   ├── 1.eml
│   ├── 2.eml
│   └── 3.eml
├── user2@example.com/
│   ├── 1.eml
│   └── 2.eml
└── user3@example.com/
    └── 1.eml
```

**ファイル形式**:
- EML形式（RFC 822）
- ファイル名は連番（1.eml、2.eml、3.eml...）

---

### メールボックスへのメール配置

メールボックスにメールを配置するには、EML形式のファイルをユーザーのメールボックスディレクトリに配置します。

**EMLファイルの例**:
```
From: sender@example.com
To: user1@example.com
Subject: Test Email
Date: Wed, 24 Jan 2026 12:00:00 +0900

This is a test email body.
```

**配置方法**:
```bash
# macOS/Linux
cp test.eml /path/to/MailDir/user1@example.com/1.eml

# Windows
copy test.eml C:\JumboDogX\Mailboxes\user1@example.com\1.eml
```

**注意**: ファイル名は連番にする必要があります。既存のファイルがある場合は、次の番号を使用してください。

---

### SMTPサーバーとの連携

JumboDogXのSMTPサーバーと連携することで、受信したメールを自動的にPOP3メールボックスに配置できます。

**設定例**:
```json
{
  "SmtpServer": {
    "Enabled": true,
    "Port": 25,
    "MailDir": "C:\\JumboDogX\\Mailboxes",  # ← POP3と同じディレクトリ
    "Users": [...]
  },
  "Pop3Server": {
    "Enabled": true,
    "Port": 110,
    "MailDir": "C:\\JumboDogX\\Mailboxes",  # ← SMTPと同じディレクトリ
    "Users": [...]
  }
}
```

この設定により、SMTPサーバーで受信したメールがPOP3サーバーのメールボックスに自動配置されます。

---

## 認証のテスト

### telnetでの認証テスト

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

# QUITコマンド
QUIT
+OK Bye
```

---

### Pythonでの認証テスト

```python
import poplib

try:
    # POP3サーバーに接続
    pop = poplib.POP3('localhost', 110)

    # 認証
    pop.user('user1@example.com')
    pop.pass_('password')

    print("Authentication successful!")

    # メール数を確認
    num_messages = len(pop.list()[1])
    print(f'Total messages: {num_messages}')

except poplib.error_proto as e:
    print(f"Authentication failed: {e}")
finally:
    pop.quit()
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
  "UserName": "user1@example.com",
  "Password": "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
  "HashType": "SHA512"
}
```

---

### 3. ローカルネットワークのみで使用

外部からのアクセスを防ぐため、ローカルネットワークのみでPOP3サーバーを使用してください。

```json
{
  "Pop3Server": {
    "BindAddress": "127.0.0.1",  # ← ローカルホストのみ
    "Port": 110
  }
}
```

---

### 4. メールボックスディレクトリのアクセス権限

メールボックスディレクトリのアクセス権限を適切に設定してください。

**macOS/Linux**:
```bash
# メールボックスディレクトリの権限を設定
chmod 700 /path/to/MailDir
chmod 600 /path/to/MailDir/user1@example.com/*.eml
```

**Windows**:
```cmd
# アクセス権限を設定
icacls C:\JumboDogX\Mailboxes /grant:r %USERNAME%:F /inheritance:r
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

### メールボックスが作成されない

**原因: MailDirディレクトリが存在しない**

**解決方法**: MailDirディレクトリを作成してください

```bash
# macOS/Linux
mkdir -p /path/to/MailDir

# Windows
mkdir C:\JumboDogX\Mailboxes
```

---

### メールが表示されない

**原因1: EMLファイルが正しい場所にない**

**解決方法**: ユーザー名と同じディレクトリにEMLファイルを配置してください

```
MailDir/
└── user1@example.com/
    └── 1.eml  # ← ここに配置
```

---

**原因2: EMLファイルの形式が間違っている**

**解決方法**: EMLファイルがRFC 822形式であることを確認してください

**最小限のEMLファイル**:
```
From: sender@example.com
To: user1@example.com
Subject: Test
Date: Wed, 24 Jan 2026 12:00:00 +0900

Test email body.
```

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [トラブルシューティング](troubleshooting.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
