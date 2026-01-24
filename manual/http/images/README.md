# HTTPサーバー スクリーンショット撮影ガイド

このディレクトリには、HTTPサーバーのクイックスタートガイドで使用するスクリーンショットを配置します。

## 必要なスクリーンショット一覧

### 1. dashboard-top.png
**撮影対象**: JumboDogXのダッシュボード画面（トップページ）
**撮影タイミング**: アプリケーション起動直後
**URL**: `http://localhost:5001/`
**説明**:
- ダッシュボードのトップ画面全体
- サイドメニュー、ヘッダーを含む

---

### 2. virtualhost-menu.png
**撮影対象**: サイドメニューのHTTP/HTTPS > Virtual Hosts
**撮影タイミング**: Settings > HTTP/HTTPSを展開した状態
**説明**:
- サイドメニューが展開された状態
- HTTP/HTTPSセクションが展開され、Virtual Hostsが表示されている状態

---

### 3. virtualhost-list.png
**撮影対象**: Virtual Hosts設定画面（初期状態）
**撮影タイミング**: Virtual Hostを1つも作成していない状態
**URL**: `http://localhost:5001/settings/http/virtualhost`
**説明**:
- "No virtual hosts configured. Click 'Add Virtual Host' to create one."メッセージが表示されている
- "Add Virtual Host"ボタンが表示されている

---

### 4. virtualhost-add.png
**撮影対象**: Virtual Hostを追加し、Host名を設定した状態
**撮影タイミング**: Add Virtual Hostボタンをクリックし、Host名を"sample.com"に設定した後
**URL**: `http://localhost:5001/settings/http/virtualhost`
**説明**:
- Virtual Host #1が表示されている
- Hostフィールドに"sample.com:8080"が入力されている状態
- "Save Settings"ボタンが表示されている

---

### 5. general-settings.png
**撮影対象**: Virtual HostのGeneral設定画面
**撮影タイミング**: Virtual Host作成後、General設定画面を開いた状態
**URL**: `http://localhost:5001/settings/http/0/general`
**説明**:
- Basic Settingsセクション（Enable Server, Bind Address, Port, Document Root）
- SSL/TLS Settingsセクション
- 各フィールドにデフォルト値が表示されている状態

---

### 6. ssl-settings.png
**撮影対象**: SSL/TLS Settingsセクション
**撮影タイミング**: General設定画面のSSL/TLS Settingsセクション
**URL**: `http://localhost:5001/settings/http/0/general`
**説明**:
- Enable HTTPSスイッチ
- Certificate File入力フィールド
- Certificate Password入力フィールド
- セキュリティ警告メッセージ

---

### 7. acl-menu.png
**撮影対象**: サイドメニューから仮想ホスト > ACLを選択
**撮影タイミング**: Settings > HTTP/HTTPS > sample.comを展開した状態
**説明**:
- サイドメニューが展開された状態
- sample.comの下にACLメニューが表示されている

---

### 8. acl-settings.png
**撮影対象**: ACL設定画面（初期状態）
**撮影タイミング**: ACLエントリを1つも追加していない状態
**URL**: `http://localhost:5001/settings/http/0/acl`
**説明**:
- ACL Modeセクション（Allow Mode選択状態）
- "No ACL entries configured. Click 'Add ACL Entry' to create one."メッセージ
- "Add ACL Entry"ボタン
- ACL Configuration Guideセクション

---

### 9. acl-localhost.png
**撮影対象**: 127.0.0.1を追加した状態
**撮影タイミング**: Add ACL Entryで127.0.0.1を追加した後
**URL**: `http://localhost:5001/settings/http/0/acl`
**説明**:
- ACL Entriesテーブルに以下のエントリが表示されている:
  - Name: "Localhost"
  - IP Address / Range: "127.0.0.1"
- "Save Settings"ボタンが表示されている

---

### 10. dashboard-virtualhost.png
**撮影対象**: DashboardのHTTP/HTTPS Virtual Hostsセクション
**撮影タイミング**: Virtual Hostを作成し、サーバーが起動している状態
**URL**: `http://localhost:5001/`
**説明**:
- HTTP/HTTPS Virtual Hostsセクション
- sample.comのVirtual Hostカードが表示されている
- ステータス（Running）、ポート、Bind Address、統計情報、Access URLが表示されている

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

1. JumboDogXを起動 → **dashboard-top.png**
2. Settings > HTTP/HTTPS > Virtual Hostsを開く → **virtualhost-menu.png**, **virtualhost-list.png**
3. Add Virtual Hostで"sample.com"を追加 → **virtualhost-add.png**
4. sample.com > Generalを開く → **general-settings.png**, **ssl-settings.png**
5. sample.com > ACLを開く → **acl-menu.png**, **acl-settings.png**
6. 127.0.0.1を追加 → **acl-localhost.png**
7. Dashboardに戻る → **dashboard-virtualhost.png**

---

## スクリーンショット配置後

すべてのスクリーンショットを配置した後、以下を確認してください：

- [ ] ファイル名が正しいか
- [ ] PNG形式であるか
- [ ] 画像が鮮明で読みやすいか
- [ ] 個人情報や機密情報が含まれていないか
- [ ] マニュアルで正しく表示されるか

マニュアルを確認する場合は、Markdownビューアーまたはブラウザで`getting-started.md`を開いてください。
