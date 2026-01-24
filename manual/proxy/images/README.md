# Proxyサーバー スクリーンショット撮影ガイド

このディレクトリには、Proxyサーバーのクイックスタートガイドで使用するスクリーンショットを配置します。

## 必要なスクリーンショット一覧

### 1. dashboard-top.png
**撮影対象**: JumboDogXのダッシュボード画面（トップページ）
**撮影タイミング**: アプリケーション起動直後
**URL**: `http://localhost:5001/`
**説明**:
- ダッシュボードのトップ画面全体
- サイドメニュー、ヘッダーを含む
- Proxy Serverセクションが表示されている状態

---

### 2. proxy-general-menu.png
**撮影対象**: サイドメニューのProxy > General
**撮影タイミング**: Settings > Proxyを展開した状態
**説明**:
- サイドメニューが展開された状態
- Proxyセクションが展開され、Generalが表示されている状態
- ハイライト表示でGeneralが選択されている

---

### 3. proxy-general-settings.png
**撮影対象**: Proxy General設定画面
**撮影タイミング**: Proxy General設定画面を開いた状態
**URL**: `http://localhost:5001/settings/proxy/general`
**説明**:
- Basic Settingsセクション:
  - Enable Proxy Server: チェックあり
  - Bind Address: "0.0.0.0"
  - Port: "8080"
- "Save Settings"ボタンが表示されている

---

### 4. proxy-cache-menu.png
**撮影対象**: サイドメニューからProxy > Cacheを選択
**撮影タイミング**: Settings > Proxyを展開した状態
**説明**:
- サイドメニューが展開された状態
- Proxyセクションの下にCacheメニューが表示されている
- ハイライト表示でCacheが選択されている

---

### 5. proxy-cache-settings.png
**撮影対象**: Proxy Cache設定画面
**撮影タイミング**: Cache設定画面を開いた状態
**URL**: `http://localhost:5001/settings/proxy/cache`
**説明**:
- Cache Settingsセクション:
  - Enable Cache: チェックあり
  - Cache Directory: "C:\\ProxyCache"
  - Max Cache Size: "1073741824" (1GB)
  - Cache Expiration: "3600" (1時間)
- "Save Settings"ボタンが表示されている

---

### 6. proxy-cache-clear.png
**撮影対象**: キャッシュクリア画面
**撮影タイミング**: Clear Cacheボタンをクリックした後
**URL**: `http://localhost:5001/settings/proxy/cache`
**説明**:
- 確認ダイアログが表示されている
- "すべてのキャッシュを削除しますか？"というメッセージ
- "OK"と"Cancel"ボタンが表示されている

---

### 7. proxy-url-filter-menu.png
**撮影対象**: サイドメニューからProxy > URL Filterを選択
**撮影タイミング**: Settings > Proxyを展開した状態
**説明**:
- サイドメニューが展開された状態
- Proxyセクションの下にURL Filterメニューが表示されている
- ハイライト表示でURL Filterが選択されている

---

### 8. proxy-url-filter-mode.png
**撮影対象**: URL Filter Mode選択画面
**撮影タイミング**: URL Filter設定画面を開いた状態
**URL**: `http://localhost:5001/settings/proxy/url-filter`
**説明**:
- Filter Modeセクション
- Allow Mode（ラジオボタン選択）
- Deny Mode（ラジオボタン未選択）
- 現在のモードの説明（青色の情報メッセージ）

---

### 9. proxy-url-filter-add.png
**撮影対象**: URLフィルタエントリを追加する画面
**撮影タイミング**: Add Entryボタンをクリックした後
**URL**: `http://localhost:5001/settings/proxy/url-filter`
**説明**:
- Add URL Filter Entry フォームが表示されている:
  - URL Pattern: "*.facebook.com"
- "Add"ボタンが表示されている

---

### 10. proxy-url-filter-entries.png
**撮影対象**: URLフィルタエントリを追加した状態
**撮影タイミング**: 複数のURLフィルタエントリを追加した後
**URL**: `http://localhost:5001/settings/proxy/url-filter`
**説明**:
- URL Filter Listテーブルに以下のエントリが表示されている:
  - Entry 1: "*.facebook.com"
  - Entry 2: "*.twitter.com"
  - Entry 3: "*.youtube.com"
- 各エントリにDeleteボタンが表示されている
- "Save Settings"ボタンが表示されている

---

### 11. browser-proxy-settings-windows.png
**撮影対象**: Windowsのプロキシ設定画面
**撮影タイミング**: Windowsの設定でプロキシを設定した状態
**説明**:
- Windows設定のプロキシ設定画面
- "手動プロキシセットアップ"セクション
- アドレス: "localhost"
- ポート: "8080"
- "保存"ボタンが表示されている

---

### 12. browser-proxy-settings-macos.png
**撮影対象**: macOSのプロキシ設定画面
**撮影タイミング**: macOSのシステム設定でプロキシを設定した状態
**説明**:
- macOSシステム設定のネットワーク設定
- "Webプロキシ(HTTP)"にチェック
- Webプロキシサーバー: "localhost:8080"
- "OK"ボタンが表示されている

---

### 13. curl-proxy-test.png
**撮影対象**: curlコマンドでプロキシ経由でアクセスした結果
**撮影タイミング**: ターミナルでcurlコマンドを実行した後
**説明**:
- ターミナルのスクリーンショット
- 以下のコマンドと結果が表示されている:
```
curl -x http://localhost:8080 http://example.com
<!doctype html>
<html>
<head>
    <title>Example Domain</title>
...
```

---

### 14. proxy-dashboard.png
**撮影対象**: DashboardのProxy Serverセクション
**撮影タイミング**: Proxyサーバーが起動している状態
**URL**: `http://localhost:5001/`
**説明**:
- Proxy Serverセクション
- サーバーのステータス（Running）
- ポート番号（8080）
- Bind Address（0.0.0.0）
- Cache Enabled（Yes/No）
- URL Filter Mode（Allow/Deny）
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

---

## 撮影の進め方

以下の順序で撮影すると効率的です：

### 第1段階: JumboDogXの設定
1. JumboDogXを起動 → **dashboard-top.png**
2. Settings > Proxy > Generalを開く → **proxy-general-menu.png**, **proxy-general-settings.png**
3. Settings > Proxy > Cacheを開く → **proxy-cache-menu.png**, **proxy-cache-settings.png**
4. Clear Cacheボタンをクリック（撮影用に確認ダイアログを開いた状態） → **proxy-cache-clear.png**
5. Settings > Proxy > URL Filterを開く → **proxy-url-filter-menu.png**, **proxy-url-filter-mode.png**
6. Add Entryで*.facebook.comを追加（撮影用にフォームを開いた状態） → **proxy-url-filter-add.png**
7. 複数のURLフィルタエントリを追加 → **proxy-url-filter-entries.png**
8. Dashboardに戻る → **proxy-dashboard.png**

### 第2段階: ブラウザのプロキシ設定
9. Windows: 設定でプロキシを設定 → **browser-proxy-settings-windows.png**
10. macOS: システム設定でプロキシを設定 → **browser-proxy-settings-macos.png**

### 第3段階: クライアント側のテスト
11. ターミナルでcurlコマンドを実行 → **curl-proxy-test.png**

---

## テストデータの準備

スクリーンショット撮影用に、以下のテストデータを準備してください：

### Proxyサーバー設定

**General設定**:
```
Bind Address: 0.0.0.0
Port: 8080
```

**Cache設定**:
```
Enable Cache: Yes
Cache Directory: C:\ProxyCache（Windows）または /tmp/ProxyCache（macOS/Linux）
Max Cache Size: 1073741824（1GB）
Cache Expiration: 3600（1時間）
```

**URL Filter設定**:
```
Mode: Allow Mode
Entries:
  - *.facebook.com
  - *.twitter.com
  - *.youtube.com
```

### キャッシュディレクトリの作成

**Windows**:
```cmd
mkdir C:\ProxyCache
```

**macOS/Linux**:
```bash
mkdir -p /tmp/ProxyCache
```

### ブラウザのプロキシ設定

**Windows**:
1. 設定 → ネットワークとインターネット → プロキシ
2. "手動プロキシセットアップ"を有効化
3. アドレス: localhost
4. ポート: 8080

**macOS**:
1. システム設定 → ネットワーク
2. 詳細 → プロキシ
3. "Webプロキシ(HTTP)"にチェック
4. Webプロキシサーバー: localhost:8080

**Linux**:
1. ネットワーク設定 → プロキシ
2. 手動プロキシを選択
3. HTTPプロキシ: localhost:8080

### curlでのテスト

```bash
# プロキシ経由でアクセス
curl -x http://localhost:8080 http://example.com

# プロキシ経由でHTTPSアクセス
curl -x http://localhost:8080 https://example.com
```

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
- [ ] proxy-general-menu.png
- [ ] proxy-general-settings.png
- [ ] proxy-cache-menu.png
- [ ] proxy-cache-settings.png
- [ ] proxy-cache-clear.png
- [ ] proxy-url-filter-menu.png
- [ ] proxy-url-filter-mode.png
- [ ] proxy-url-filter-add.png
- [ ] proxy-url-filter-entries.png
- [ ] browser-proxy-settings-windows.png
- [ ] browser-proxy-settings-macos.png
- [ ] curl-proxy-test.png
- [ ] proxy-dashboard.png

全て揃ったらこのチェックリストを完了させてください。
