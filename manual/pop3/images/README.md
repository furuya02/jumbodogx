# POP3サーバー スクリーンショット撮影ガイド

このディレクトリには、POP3サーバーのクイックスタートガイドで使用するスクリーンショットを配置します。

## 必要なスクリーンショット一覧

### 1. dashboard-top.png
**撮影対象**: JumboDogXのダッシュボード画面（トップページ）
**撮影タイミング**: アプリケーション起動直後
**URL**: `http://localhost:5001/`
**説明**:
- ダッシュボードのトップ画面全体
- サイドメニュー、ヘッダーを含む
- POP3 Serverセクションが表示されている状態

---

### 2. pop3-general-menu.png
**撮影対象**: サイドメニューのPOP3 > General
**撮影タイミング**: Settings > POP3を展開した状態
**説明**:
- サイドメニューが展開された状態
- POP3セクションが展開され、Generalが表示されている状態
- ハイライト表示でGeneralが選択されている

---

### 3. pop3-general-settings.png
**撮影対象**: POP3 General設定画面
**撮影タイミング**: POP3 General設定画面を開いた状態
**URL**: `http://localhost:5001/settings/pop3/general`
**説明**:
- Basic Settingsセクション（Enable POP3 Server, Bind Address, Port, Timeout, Mail Directory）
- 各フィールドにデフォルト値が入力されている状態
- Enable POP3 Serverがオンになっている
- Mail Directory: "C:\\JumboDogX\\Mailboxes"

---

### 4. pop3-users-menu.png
**撮影対象**: サイドメニューからPOP3 > Usersを選択
**撮影タイミング**: Settings > POP3を展開した状態
**説明**:
- サイドメニューが展開された状態
- POP3セクションの下にUsersメニューが表示されている
- ハイライト表示でUsersが選択されている

---

### 5. pop3-user-add.png
**撮影対象**: POP3ユーザーを追加する画面
**撮影タイミング**: Add Userボタンをクリックした後
**URL**: `http://localhost:5001/settings/pop3/users`
**説明**:
- Add User フォームが表示されている:
  - Username: "user1@example.com"
  - Password: "********"（マスクされた状態）
  - Hash Type: ドロップダウンで"SHA256"を選択
- "Add"ボタンが表示されている

---

### 6. pop3-users-list.png
**撮影対象**: POP3ユーザー一覧画面
**撮影タイミング**: ユーザーを2-3個追加した後
**URL**: `http://localhost:5001/settings/pop3/users`
**説明**:
- POP3 Usersテーブルに複数のユーザーが表示されている:
  - User 1:
    - Username: "user1@example.com"
    - Password: "********"（マスクされた状態）
    - Hash Type: "SHA256"
  - User 2:
    - Username: "user2@example.com"
    - Password: "********"
    - Hash Type: "SHA512"
- 各ユーザーにEdit, Deleteボタンが表示されている
- "Save Settings"ボタンが表示されている

---

### 7. pop3-hash-type.png
**撮影対象**: Hash Typeドロップダウン
**撮影タイミング**: Add User画面でHash Typeドロップダウンを開いた状態
**URL**: `http://localhost:5001/settings/pop3/users`
**説明**:
- Hash Typeドロップダウンが展開されている
- 以下のハッシュタイプが表示されている:
  - SHA256
  - SHA512

---

### 8. pop3-mailbox-structure.png
**撮影対象**: メールボックスディレクトリ構造
**撮影タイミング**: ファイルエクスプローラーでメールボックスディレクトリを開いた状態
**説明**:
- ファイルエクスプローラー（またはFinder）のスクリーンショット
- 以下のディレクトリ構造が表示されている:
```
Mailboxes/
├── user1@example.com/
│   ├── 1.eml
│   ├── 2.eml
│   └── 3.eml
└── user2@example.com/
    └── 1.eml
```

---

### 9. pop3-eml-file.png
**撮影対象**: EMLファイルの内容
**撮影タイミング**: テキストエディタでEMLファイルを開いた状態
**説明**:
- テキストエディタのスクリーンショット
- EMLファイルの内容が表示されている:
```
From: sender@example.com
To: user1@example.com
Subject: Test Email
Date: Wed, 24 Jan 2026 12:00:00 +0900
Content-Type: text/plain; charset=utf-8

This is a test email body.
```

---

### 10. pop3-terminal-hash.png
**撮影対象**: ターミナルでパスワードハッシュを生成した結果
**撮影タイミング**: ターミナルでsha256sumコマンドを実行した後
**説明**:
- ターミナルまたはコマンドプロンプトのスクリーンショット
- 以下のコマンドと結果が表示されている:
```
echo -n "password" | sha256sum
5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8  -
```

---

### 11. pop3-python-test.png
**撮影対象**: PythonでPOP3サーバーに接続してメール受信をテストした結果
**撮影タイミング**: Pythonスクリプトを実行した後
**説明**:
- ターミナルまたはIDEのスクリーンショット
- Pythonスクリプトのコードとその実行結果が表示されている:
```python
import poplib

pop = poplib.POP3('localhost', 110)
pop.user('user1@example.com')
pop.pass_('password')

num_messages = len(pop.list()[1])
print(f'Total messages: {num_messages}')

msg = pop.retr(1)
print(b'\n'.join(msg[1]).decode('utf-8'))

pop.quit()
```
- 実行結果: "Total messages: 3"とメール内容が表示されている

---

### 12. pop3-thunderbird-settings.png
**撮影対象**: Thunderbirdのサーバー設定画面
**撮影タイミング**: ThunderbirdでPOP3アカウントを設定した状態
**説明**:
- Thunderbirdのアカウント設定画面
- 以下の設定が表示されている:
  - サーバー名: localhost
  - ポート: 110
  - セキュリティ: なし
  - 認証方式: 通常のパスワード認証
  - ユーザー名: user1@example.com

---

### 13. pop3-dashboard.png
**撮影対象**: DashboardのPOP3 Serverセクション
**撮影タイミング**: POP3サーバーが起動している状態
**URL**: `http://localhost:5001/`
**説明**:
- POP3 Serverセクション
- サーバーのステータス（Running）
- ポート番号（110）
- Bind Address（0.0.0.0）
- Mail Directory
- 現在の接続数などの統計情報

---

## スクリーンショット撮影時の注意事項

### 推奨設定
- **ブラウザ**: Chrome、Firefox、Safari等の最新版
- **画面サイズ**: 1920x1080以上推奨
- **ブラウザウィンドウ**: できるだけ広く（フルスクリーンまたは最大化）
- **ズーム**: 100%（デフォルト）

### 撮影方法
- **macOS**: `Command + Shift + 4` → スペースキー → ウィンドウをクリック
- **Windows**: `Alt + PrintScreen` または Snipping Tool
- **Linux**: GNOME Screenshot、Spectacle等

### ファイル形式
- **推奨**: PNG形式
- **品質**: 圧縮なし、または可逆圧縮
- **ファイル名**: 上記の一覧に記載された名前を使用（小文字、ハイフン区切り）

### 画像の編集
- 個人情報や機密情報が含まれていないか確認
- 必要に応じて、特定の部分をぼかしまたは黒塗り
- 画像サイズが大きすぎる場合は、適切にリサイズ（幅1200px程度推奨）
- パスワードフィールドは必ずマスク表示（`********`）で撮影

---

## 撮影の進め方

以下の順序で撮影すると効率的です：

### 第1段階: JumboDogXの設定
1. JumboDogXを起動 → **dashboard-top.png**
2. Settings > POP3 > Generalを開く → **pop3-general-menu.png**, **pop3-general-settings.png**
3. Settings > POP3 > Usersを開く → **pop3-users-menu.png**
4. Add Userでuser1@example.comを追加（撮影用にフォームを開いた状態） → **pop3-user-add.png**
5. Hash Typeドロップダウンを開く → **pop3-hash-type.png**
6. user1@example.comとuser2@example.comを追加 → **pop3-users-list.png**
7. Dashboardに戻る → **pop3-dashboard.png**

### 第2段階: メールボックスの準備
8. ファイルエクスプローラーでメールボックスディレクトリを開く → **pop3-mailbox-structure.png**
9. テキストエディタでEMLファイルを開く → **pop3-eml-file.png**

### 第3段階: ターミナルでの操作
10. ターミナルまたはコマンドプロンプトを開く
11. echo -n "password" | sha256sumを実行 → **pop3-terminal-hash.png**

### 第4段階: Pythonでのテスト
12. Pythonスクリプトを作成・実行 → **pop3-python-test.png**

### 第5段階: メールクライアントの設定
13. Thunderbirdをインストール・設定 → **pop3-thunderbird-settings.png**

---

## テストデータの準備

スクリーンショット撮影用に、以下のテストデータを準備してください：

### POP3ユーザー

**ユーザー1**:
```
Username: user1@example.com
Password: password
Hash Type: SHA256
Hash Value: 5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8
```

**ユーザー2**:
```
Username: user2@example.com
Password: user2pass
Hash Type: SHA512
Hash Value: ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff
```

### メールボックスディレクトリの作成

```bash
# macOS/Linux
mkdir -p C:/JumboDogX/Mailboxes/user1@example.com
mkdir -p C:/JumboDogX/Mailboxes/user2@example.com

# Windows
mkdir C:\JumboDogX\Mailboxes\user1@example.com
mkdir C:\JumboDogX\Mailboxes\user2@example.com
```

### EMLファイルの作成

**1.eml**:
```
From: sender@example.com
To: user1@example.com
Subject: Test Email 1
Date: Wed, 24 Jan 2026 12:00:00 +0900
Content-Type: text/plain; charset=utf-8

This is the first test email.
```

**2.eml**:
```
From: sender@example.com
To: user1@example.com
Subject: Test Email 2
Date: Wed, 24 Jan 2026 13:00:00 +0900
Content-Type: text/plain; charset=utf-8

This is the second test email.
```

**3.eml**:
```
From: sender@example.com
To: user1@example.com
Subject: Test Email 3
Date: Wed, 24 Jan 2026 14:00:00 +0900
Content-Type: text/plain; charset=utf-8

This is the third test email.
```

これらのファイルを`C:\JumboDogX\Mailboxes\user1@example.com\`に配置してください。

### パスワードハッシュの生成

```bash
# macOS/Linux
echo -n "password" | sha256sum
echo -n "user2pass" | sha512sum

# Windows (PowerShell)
$password = "password"
$hash = [System.Security.Cryptography.SHA256]::Create()
$bytes = $hash.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($password))
[BitConverter]::ToString($bytes).Replace("-", "").ToLower()
```

---

## スクリーンショット配置後

すべてのスクリーンショットを配置した後、以下を確認してください：

- [ ] ファイル名が正しいか
- [ ] PNG形式であるか
- [ ] 画像が鮮明で読みやすいか
- [ ] 個人情報や機密情報が含まれていないか
- [ ] パスワードがマスク表示（`********`）されているか
- [ ] マニュアルで正しく表示されるか

マニュアルを確認する場合は、Markdownビューアーまたはブラウザで`getting-started.md`を開いてください。

---

## スクリーンショット一覧（チェックリスト）

- [ ] dashboard-top.png
- [ ] pop3-general-menu.png
- [ ] pop3-general-settings.png
- [ ] pop3-users-menu.png
- [ ] pop3-user-add.png
- [ ] pop3-users-list.png
- [ ] pop3-hash-type.png
- [ ] pop3-mailbox-structure.png
- [ ] pop3-eml-file.png
- [ ] pop3-terminal-hash.png
- [ ] pop3-python-test.png
- [ ] pop3-thunderbird-settings.png
- [ ] pop3-dashboard.png

全て揃ったらこのチェックリストを完了させてください。
