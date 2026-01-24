# SMTPサーバー スクリーンショット撮影ガイド

このディレクトリには、SMTPサーバーのクイックスタートガイドで使用するスクリーンショットを配置します。

## 必要なスクリーンショット一覧

### 1. dashboard-top.png
**撮影対象**: JumboDogXのダッシュボード画面（トップページ）
**撮影タイミング**: アプリケーション起動直後
**URL**: `http://localhost:5001/`
**説明**:
- ダッシュボードのトップ画面全体
- サイドメニュー、ヘッダーを含む
- SMTP Serverセクションが表示されている状態

---

### 2. smtp-general-menu.png
**撮影対象**: サイドメニューのSMTP > General
**撮影タイミング**: Settings > SMTPを展開した状態
**説明**:
- サイドメニューが展開された状態
- SMTPセクションが展開され、Generalが表示されている状態
- ハイライト表示でGeneralが選択されている

---

### 3. smtp-general-settings.png
**撮影対象**: SMTP General設定画面
**撮影タイミング**: SMTP General設定画面を開いた状態
**URL**: `http://localhost:5001/settings/smtp/general`
**説明**:
- Basic Settingsセクション（Enable SMTP Server, Bind Address, Port, Timeout）
- 各フィールドにデフォルト値が入力されている状態
- Enable SMTP Serverがオンになっている

---

### 4. smtp-users-menu.png
**撮影対象**: サイドメニューからSMTP > Usersを選択
**撮影タイミング**: Settings > SMTPを展開した状態
**説明**:
- サイドメニューが展開された状態
- SMTPセクションの下にUsersメニューが表示されている
- ハイライト表示でUsersが選択されている

---

### 5. smtp-user-add.png
**撮影対象**: SMTPユーザーを追加する画面
**撮影タイミング**: Add Userボタンをクリックした後
**URL**: `http://localhost:5001/settings/smtp/users`
**説明**:
- Add User フォームが表示されている:
  - Username: "test@example.com"
  - Password: "********"（マスクされた状態）
  - Hash Type: ドロップダウンで"SHA256"を選択
- "Add"ボタンが表示されている

---

### 6. smtp-users-list.png
**撮影対象**: SMTPユーザー一覧画面
**撮影タイミング**: ユーザーを2-3個追加した後
**URL**: `http://localhost:5001/settings/smtp/users`
**説明**:
- SMTP Usersテーブルに複数のユーザーが表示されている:
  - User 1:
    - Username: "test@example.com"
    - Password: "********"（マスクされた状態）
    - Hash Type: "SHA256"
  - User 2:
    - Username: "alice@example.com"
    - Password: "********"
    - Hash Type: "SHA256"
- 各ユーザーにEdit, Deleteボタンが表示されている
- "Save Settings"ボタンが表示されている

---

### 7. smtp-relay-menu.png
**撮影対象**: サイドメニューからSMTP > Relayを選択
**撮影タイミング**: Settings > SMTPを展開した状態
**説明**:
- サイドメニューが展開された状態
- SMTPセクションの下にRelayメニューが表示されている
- ハイライト表示でRelayが選択されている

---

### 8. smtp-relay-settings.png
**撮影対象**: SMTPリレー設定画面
**撮影タイミング**: Relay設定画面を開き、Gmail設定を入力した状態
**URL**: `http://localhost:5001/settings/smtp/relay`
**説明**:
- Relay Server Settingsセクション:
  - Host: "smtp.gmail.com"
  - Port: "587"
  - Use TLS: チェックあり
  - Username: "your-email@gmail.com"
  - Password: "********"（マスクされた状態）
- "Test Connection"ボタン（オプション）
- "Save Settings"ボタンが表示されている

---

### 9. smtp-hash-type.png
**撮影対象**: Hash Typeドロップダウン
**撮影タイミング**: Add User画面でHash Typeドロップダウンを開いた状態
**URL**: `http://localhost:5001/settings/smtp/users`
**説明**:
- Hash Typeドロップダウンが展開されている
- 以下のハッシュタイプが表示されている:
  - SHA256
  - SHA512

---

### 10. smtp-terminal-hash.png
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

### 11. smtp-python-test.png
**撮影対象**: PythonでSMTPサーバーにメール送信をテストした結果
**撮影タイミング**: Pythonスクリプトを実行した後
**説明**:
- ターミナルまたはIDEのスクリーンショット
- Pythonスクリプトのコードとその実行結果が表示されている:
```python
import smtplib
from email.mime.text import MIMEText

msg = MIMEText('This is a test email.')
msg['Subject'] = 'Test'
msg['From'] = 'test@example.com'
msg['To'] = 'recipient@example.com'

with smtplib.SMTP('localhost', 25) as server:
    server.login('test@example.com', 'password')
    server.send_message(msg)

print("Email sent successfully!")
```
- 実行結果: "Email sent successfully!"

---

### 12. smtp-dashboard.png
**撮影対象**: DashboardのSMTP Serverセクション
**撮影タイミング**: SMTPサーバーが起動している状態
**URL**: `http://localhost:5001/`
**説明**:
- SMTP Serverセクション
- サーバーのステータス（Running）
- ポート番号（25）
- Bind Address（0.0.0.0）
- リレー設定の有無
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
2. Settings > SMTP > Generalを開く → **smtp-general-menu.png**, **smtp-general-settings.png**
3. Settings > SMTP > Usersを開く → **smtp-users-menu.png**
4. Add Userでtest@example.comを追加（撮影用にフォームを開いた状態） → **smtp-user-add.png**
5. Hash Typeドロップダウンを開く → **smtp-hash-type.png**
6. test@example.comとalice@example.comを追加 → **smtp-users-list.png**
7. Settings > SMTP > Relayを開く → **smtp-relay-menu.png**
8. Relay設定でGmailの情報を入力 → **smtp-relay-settings.png**
9. Dashboardに戻る → **smtp-dashboard.png**

### 第2段階: ターミナルでの操作
10. ターミナルまたはコマンドプロンプトを開く
11. echo -n "password" | sha256sumを実行 → **smtp-terminal-hash.png**

### 第3段階: Pythonでのテスト
12. Pythonスクリプトを作成・実行 → **smtp-python-test.png**

---

## テストデータの準備

スクリーンショット撮影用に、以下のテストデータを準備してください：

### SMTPユーザー

**ユーザー1**:
```
Username: test@example.com
Password: password
Hash Type: SHA256
Hash Value: 5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8
```

**ユーザー2**:
```
Username: alice@example.com
Password: alice123
Hash Type: SHA256
Hash Value: d74ff0ee8da3b9806b18c877dbf29bbde50b5bd8e4dad7a3a725000feb82e8f1
```

### リレー設定（Gmail）

```
Host: smtp.gmail.com
Port: 587
Use TLS: Yes
Username: your-email@gmail.com
Password: your-app-password（16桁のアプリパスワード）
```

**注意**: 実際のGmailアカウントとアプリパスワードを使用する場合、スクリーンショット撮影後にぼかし処理を行ってください。

### パスワードハッシュの生成

```bash
# macOS/Linux
echo -n "password" | sha256sum
echo -n "alice123" | sha256sum

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
- [ ] 個人情報や機密情報が含まれていないか（特にメールアドレスとパスワード）
- [ ] パスワードがマスク表示（`********`）されているか
- [ ] マニュアルで正しく表示されるか

マニュアルを確認する場合は、Markdownビューアーまたはブラウザで`getting-started.md`を開いてください。

---

## スクリーンショット一覧（チェックリスト）

- [ ] dashboard-top.png
- [ ] smtp-general-menu.png
- [ ] smtp-general-settings.png
- [ ] smtp-users-menu.png
- [ ] smtp-user-add.png
- [ ] smtp-users-list.png
- [ ] smtp-relay-menu.png
- [ ] smtp-relay-settings.png
- [ ] smtp-hash-type.png
- [ ] smtp-terminal-hash.png
- [ ] smtp-python-test.png
- [ ] smtp-dashboard.png

全て揃ったらこのチェックリストを完了させてください。
