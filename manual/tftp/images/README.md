# TFTPサーバー スクリーンショット撮影ガイド

このディレクトリには、TFTPサーバーのクイックスタートガイドで使用するスクリーンショットを配置します。

## 必要なスクリーンショット一覧

### 1. dashboard-top.png
**撮影対象**: JumboDogXのダッシュボード画面（トップページ）
**撮影タイミング**: アプリケーション起動直後
**URL**: `http://localhost:5001/`
**説明**:
- ダッシュボードのトップ画面全体
- サイドメニュー、ヘッダーを含む
- TFTP Serverセクションが表示されている状態

---

### 2. tftp-general-menu.png
**撮影対象**: サイドメニューのTFTP > General
**撮影タイミング**: Settings > TFTPを展開した状態
**説明**:
- サイドメニューが展開された状態
- TFTPセクションが展開され、Generalが表示されている状態
- ハイライト表示でGeneralが選択されている

---

### 3. tftp-general-settings.png
**撮影対象**: TFTP General設定画面
**撮影タイミング**: TFTP General設定画面を開いた状態
**URL**: `http://localhost:5001/settings/tftp/general`
**説明**:
- Basic Settingsセクション:
  - Enable TFTP Server: チェックあり
  - Bind Address: "0.0.0.0"
  - Port: "69"
  - Root Directory: "C:\\TftpRoot"
  - Timeout: "5"
  - Read Only Mode: チェックなし
- "Save Settings"ボタンが表示されている

---

### 4. tftp-readonly-mode.png
**撮影対象**: Read Only Mode設定
**撮影タイミング**: Read Only Modeにチェックを入れた状態
**URL**: `http://localhost:5001/settings/tftp/general`
**説明**:
- Read Only Modeチェックボックスにチェックが入っている
- 情報メッセージ（青色）に読み取り専用モードの説明が表示されている
- "Save Settings"ボタンが表示されている

---

### 5. tftp-acl-menu.png
**撮影対象**: サイドメニューからTFTP > ACLを選択
**撮影タイミング**: Settings > TFTPを展開した状態
**説明**:
- サイドメニューが展開された状態
- TFTPセクションの下にACLメニューが表示されている
- ハイライト表示でACLが選択されている

---

### 6. tftp-acl-mode.png
**撮影対象**: ACL Mode選択画面
**撮影タイミング**: ACL設定画面を開いた状態
**URL**: `http://localhost:5001/settings/tftp/acl`
**説明**:
- ACL Modeセクション
- Allow Mode（ラジオボタン選択）
- Deny Mode（ラジオボタン未選択）
- 現在のモードの説明（青色の情報メッセージ）

---

### 7. tftp-acl-add.png
**撮影対象**: ACLエントリを追加する画面
**撮影タイミング**: Add ACL Entryボタンをクリックした後
**URL**: `http://localhost:5001/settings/tftp/acl`
**説明**:
- Add ACL Entry フォームが表示されている:
  - IP Address / Range: "192.168.1.0/24"
- "Add"ボタンが表示されている

---

### 8. tftp-acl-entries.png
**撮影対象**: ACLエントリを追加した状態
**撮影タイミング**: 複数のACLエントリを追加した後
**URL**: `http://localhost:5001/settings/tftp/acl`
**説明**:
- Access Control Listテーブルに以下のエントリが表示されている:
  - Entry 1: "192.168.1.0/24"
  - Entry 2: "10.0.0.100"
  - Entry 3: "172.16.0.0/16"
- 各エントリにDeleteボタンが表示されている
- "Save Settings"ボタンが表示されている

---

### 9. tftp-root-directory.png
**撮影対象**: ルートディレクトリの内容
**撮影タイミング**: ファイルエクスプローラーでルートディレクトリを開いた状態
**説明**:
- ファイルエクスプローラー（またはFinder）のスクリーンショット
- ルートディレクトリ（C:\TftpRoot）の内容が表示されている:
```
TftpRoot/
├── firmware/
│   ├── router.bin
│   └── switch.bin
├── pxe/
│   ├── pxelinux.0
│   └── vmlinuz
└── test.txt
```

---

### 10. tftp-client-test-windows.png
**撮影対象**: WindowsのTFTPクライアントでファイルをダウンロードした結果
**撮影タイミング**: コマンドプロンプトでtftp GETコマンドを実行した後
**説明**:
- コマンドプロンプトのスクリーンショット
- 以下のコマンドと結果が表示されている:
```
tftp -i localhost GET test.txt
Transfer successful: 1024 bytes in 1 second
```
- ダウンロードされたファイルが表示されている

---

### 11. tftp-client-test-curl.png
**撮影対象**: curlコマンドでファイルをダウンロードした結果
**撮影タイミング**: ターミナルでcurlコマンドを実行した後（macOS/Linux）
**説明**:
- ターミナルのスクリーンショット
- 以下のコマンドと結果が表示されている:
```
curl -o test.txt tftp://localhost/test.txt
  % Total    % Received % Xferd  Average Speed   Time    Time     Time  Current
                                 Dload  Upload   Total   Spent    Left  Speed
100  1024  100  1024    0     0  51200      0 --:--:-- --:--:-- --:--:-- 51200
```

---

### 12. tftp-dashboard.png
**撮影対象**: DashboardのTFTP Serverセクション
**撮影タイミング**: TFTPサーバーが起動している状態
**URL**: `http://localhost:5001/`
**説明**:
- TFTP Serverセクション
- サーバーのステータス（Running）
- ポート番号（69）
- Bind Address（0.0.0.0）
- Root Directory（C:\TftpRoot）
- Read Only Mode（Yes/No）
- 現在の転送数などの統計情報

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

---

## 撮影の進め方

以下の順序で撮影すると効率的です：

### 第1段階: JumboDogXの設定
1. JumboDogXを起動 → **dashboard-top.png**
2. Settings > TFTP > Generalを開く → **tftp-general-menu.png**, **tftp-general-settings.png**
3. Read Only Modeにチェック → **tftp-readonly-mode.png**
4. Settings > TFTP > ACLを開く → **tftp-acl-menu.png**, **tftp-acl-mode.png**
5. Add ACL Entryで192.168.1.0/24を追加（撮影用にフォームを開いた状態） → **tftp-acl-add.png**
6. 複数のACLエントリを追加 → **tftp-acl-entries.png**
7. Dashboardに戻る → **tftp-dashboard.png**

### 第2段階: ルートディレクトリの準備
8. ファイルエクスプローラーでルートディレクトリを開く → **tftp-root-directory.png**

### 第3段階: クライアント側のテスト
9. Windows: コマンドプロンプトでtftp GETを実行 → **tftp-client-test-windows.png**
10. macOS/Linux: curlコマンドを実行 → **tftp-client-test-curl.png**

---

## テストデータの準備

スクリーンショット撮影用に、以下のテストデータを準備してください：

### TFTPサーバー設定

**General設定**:
```
Bind Address: 0.0.0.0
Port: 69
Root Directory: C:\TftpRoot（Windows）または /tmp/TftpRoot（macOS/Linux）
Timeout: 5
Read Only Mode: Yes（読み取り専用モード有効）
```

### ACLエントリ

```
Entry 1: 192.168.1.0/24
Entry 2: 10.0.0.100
Entry 3: 172.16.0.0/16
```

### ルートディレクトリの作成

**Windows**:
```cmd
mkdir C:\TftpRoot
mkdir C:\TftpRoot\firmware
mkdir C:\TftpRoot\pxe

echo This is a test file > C:\TftpRoot\test.txt
echo Router firmware > C:\TftpRoot\firmware\router.bin
echo Switch firmware > C:\TftpRoot\firmware\switch.bin
echo PXE boot loader > C:\TftpRoot\pxe\pxelinux.0
```

**macOS/Linux**:
```bash
mkdir -p /tmp/TftpRoot/firmware
mkdir -p /tmp/TftpRoot/pxe

echo "This is a test file" > /tmp/TftpRoot/test.txt
echo "Router firmware" > /tmp/TftpRoot/firmware/router.bin
echo "Switch firmware" > /tmp/TftpRoot/firmware/switch.bin
echo "PXE boot loader" > /tmp/TftpRoot/pxe/pxelinux.0
```

### TFTPクライアントの準備

**Windows**:
1. コントロールパネル → プログラムと機能
2. Windowsの機能の有効化または無効化
3. 「TFTPクライアント」にチェック
4. OKをクリックして再起動

**macOS/Linux**:
- 標準のtftpコマンドまたはcurlを使用

---

## スクリーンショット配置後

すべてのスクリーンショットを配置した後、以下を確認してください：

- [ ] ファイル名が正しいか
- [ ] PNG形式であるか
- [ ] 画像が鮮明で読みやすいか
- [ ] 個人情報や機密情報が含まれていないか
- [ ] マニュアルで正しく表示されるか

マニュアルを確認する場合は、Markdownビューアーまたはブラウザで`getting-started.md`を開いてください。

---

## スクリーンショット一覧（チェックリスト）

- [ ] dashboard-top.png
- [ ] tftp-general-menu.png
- [ ] tftp-general-settings.png
- [ ] tftp-readonly-mode.png
- [ ] tftp-acl-menu.png
- [ ] tftp-acl-mode.png
- [ ] tftp-acl-add.png
- [ ] tftp-acl-entries.png
- [ ] tftp-root-directory.png
- [ ] tftp-client-test-windows.png
- [ ] tftp-client-test-curl.png
- [ ] tftp-dashboard.png

全て揃ったらこのチェックリストを完了させてください。
