# FTPサーバー スクリーンショット撮影ガイド

このディレクトリには、FTPサーバーのクイックスタートガイドで使用するスクリーンショットを配置します。

## 必要なスクリーンショット一覧

### 1. dashboard-top.png
**撮影対象**: JumboDogXのダッシュボード画面（トップページ）
**撮影タイミング**: アプリケーション起動直後
**URL**: `http://localhost:5001/`
**説明**:
- ダッシュボードのトップ画面全体
- サイドメニュー、ヘッダーを含む

---

### 2. ftp-general-menu.png
**撮影対象**: サイドメニューのFTP > General
**撮影タイミング**: Settings > FTPを展開した状態
**説明**:
- サイドメニューが展開された状態
- FTPセクションが展開され、Generalが表示されている状態
- ハイライト表示でGeneralが選択されている

---

### 3. ftp-general-settings.png
**撮影対象**: FTP General設定画面
**撮影タイミング**: FTP General設定画面を開いた状態
**URL**: `http://localhost:5001/settings/ftp/general`
**説明**:
- Basic Settingsセクション（Enable FTP Server, Bind Address, Port, Timeout, Max Connections, Banner Message）
- FTPS Settingsセクションが表示されている
- 各フィールドにデフォルト値が入力されている状態

---

### 4. ftp-ftps-settings.png
**撮影対象**: FTPS（FTP over SSL/TLS）設定セクション
**撮影タイミング**: General設定画面のFTPS Settingsセクション
**URL**: `http://localhost:5001/settings/ftp/general`
**説明**:
- Enable FTPSスイッチ
- Certificate File入力フィールド
- Certificate Password入力フィールド
- セキュリティ警告メッセージ

---

### 5. ftp-user-menu.png
**撮影対象**: サイドメニューからFTP > Userを選択
**撮影タイミング**: Settings > FTPを展開した状態
**説明**:
- サイドメニューが展開された状態
- FTPセクションの下にUserメニューが表示されている
- ハイライト表示でUserが選択されている

---

### 6. ftp-user-add.png
**撮影対象**: ユーザーを追加した状態
**撮影タイミング**: Add Userボタンをクリックし、ユーザー情報を入力した後
**URL**: `http://localhost:5001/settings/ftp/user`
**説明**:
- FTP Usersテーブルにユーザーが表示されている:
  - Username: "testuser"
  - Password: "********"（マスクされた状態）
  - Home Directory: "/tmp/jumbodogx-ftp/testuser"
  - Access Control: "Full (Upload & Download)"
- "Save Settings"ボタンが表示されている

---

### 7. ftp-anonymous-user.png
**撮影対象**: 匿名ユーザーを追加した状態
**撮影タイミング**: Add Anonymous Userボタンをクリックした後
**URL**: `http://localhost:5001/settings/ftp/user`
**説明**:
- FTP Usersテーブルに匿名ユーザーが表示されている:
  - Username: "anonymous"
  - Password: （空）
  - Home Directory: "/tmp/jumbodogx-ftp"
  - Access Control: "Download Only"
- 情報メッセージ（青色）に匿名FTPの説明が表示されている

---

### 8. ftp-acl-menu.png
**撮影対象**: サイドメニューからFTP > ACLを選択
**撮影タイミング**: Settings > FTPを展開した状態
**説明**:
- サイドメニューが展開された状態
- FTPセクションの下にACLメニューが表示されている
- ハイライト表示でACLが選択されている

---

### 9. ftp-acl-mode.png
**撮影対象**: ACL Mode選択画面
**撮影タイミング**: ACL設定画面を開いた状態
**URL**: `http://localhost:5001/settings/ftp/acl`
**説明**:
- ACL Modeセクション
- Allow Mode（ラジオボタン選択）
- Deny Mode（ラジオボタン未選択）
- 現在のモードの説明（青色の情報メッセージ）

---

### 10. ftp-acl-entries.png
**撮影対象**: ACLエントリを追加した状態
**撮影タイミング**: Add ACL Entryで127.0.0.1とローカルネットワークを追加した後
**URL**: `http://localhost:5001/settings/ftp/acl`
**説明**:
- Access Control Listテーブルに以下のエントリが表示されている:
  - Entry 1:
    - Name: "Localhost"
    - IP Address / Range: "127.0.0.1"
  - Entry 2:
    - Name: "LocalNetwork"
    - IP Address / Range: "192.168.1.0/24"
- "Save Settings"ボタンが表示されている

---

### 11. ftp-filezilla-connect.png
**撮影対象**: FileZillaでFTPサーバーに接続した状態
**撮影タイミング**: FileZillaで接続成功後
**説明**:
- FileZillaのメインウィンドウ
- 上部の接続フィールド（ホスト、ユーザー名、パスワード、ポート）に値が入力されている
- 接続ログに"220 JumboDogX FTP Server Ready"などのメッセージが表示されている
- ディレクトリ一覧が表示されている状態

---

### 12. ftp-dashboard.png
**撮影対象**: DashboardのFTP Serverセクション
**撮影タイミング**: FTPサーバーが起動している状態
**URL**: `http://localhost:5001/`
**説明**:
- FTP Serverセクション
- サーバーのステータス（Running）
- ポート番号（2121）
- Bind Address（0.0.0.0）
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
2. Settings > FTP > Generalを開く → **ftp-general-menu.png**, **ftp-general-settings.png**, **ftp-ftps-settings.png**
3. Settings > FTP > Userを開く → **ftp-user-menu.png**
4. Add Userでtestuserを追加 → **ftp-user-add.png**
5. Add Anonymous Userで匿名ユーザーを追加 → **ftp-anonymous-user.png**
6. Settings > FTP > ACLを開く → **ftp-acl-menu.png**, **ftp-acl-mode.png**
7. Add ACL Entryで127.0.0.1とローカルネットワークを追加 → **ftp-acl-entries.png**
8. Dashboardに戻る → **ftp-dashboard.png**

### 第2段階: FTPクライアントの接続
9. FileZillaをインストール（まだの場合）
10. FileZillaで接続（ホスト: localhost, ポート: 2121, ユーザー名: testuser, パスワード: 設定したパスワード）
11. 接続成功後、ディレクトリが表示された状態で撮影 → **ftp-filezilla-connect.png**

---

## テストデータの準備

スクリーンショット撮影用に、以下のテストデータを準備してください：

### ユーザー情報
```
Username: testuser
Password: password123
Home Directory: /tmp/jumbodogx-ftp/testuser
Access Control: Full (Upload & Download)
```

### 匿名ユーザー
```
Username: anonymous
Password: （空）
Home Directory: /tmp/jumbodogx-ftp
Access Control: Download Only
```

### ACLエントリ
```
Entry 1:
  Name: Localhost
  Address: 127.0.0.1

Entry 2:
  Name: LocalNetwork
  Address: 192.168.1.0/24
```

### ホームディレクトリの作成
```bash
# macOS/Linux
mkdir -p /tmp/jumbodogx-ftp/testuser
echo "test file" > /tmp/jumbodogx-ftp/testuser/sample.txt

# Windows
mkdir C:\tmp\jumbodogx-ftp\testuser
echo test file > C:\tmp\jumbodogx-ftp\testuser\sample.txt
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
- [ ] ftp-general-menu.png
- [ ] ftp-general-settings.png
- [ ] ftp-ftps-settings.png
- [ ] ftp-user-menu.png
- [ ] ftp-user-add.png
- [ ] ftp-anonymous-user.png
- [ ] ftp-acl-menu.png
- [ ] ftp-acl-mode.png
- [ ] ftp-acl-entries.png
- [ ] ftp-filezilla-connect.png
- [ ] ftp-dashboard.png

全て揃ったらこのチェックリストを完了させてください。
