# HTTP設定の保存・読み込みテスト手順

## テスト目的
- appsettings.jsonから設定が正しく読み込まれること
- WebUIで設定を変更して保存できること
- 保存した設定がappsettings.jsonに正しく反映されること
- アプリ再起動後も設定が保持されること

## テスト実施日
2026/01/16

## 前提条件
- WebUIが http://localhost:5001 で起動していること
- appsettings.jsonにPhase 3-12の設定が含まれていること

## テスト手順

### 1. 設定読み込みテスト

#### 1.1 基本設定（Phase 1）の確認
1. ブラウザで http://localhost:5001/settings にアクセス
2. 「Basic Settings (HTTP Server)」セクションを確認
3. 以下の値が表示されていることを確認：
   - Enable HTTP Server: ✓ (チェック済み)
   - Protocol: HTTP
   - Port: 8080
   - Bind Address: 0.0.0.0
   - Timeout: 3
   - Max Connections: 100

#### 1.2 ドキュメント設定（Phase 2）の確認
1. 「Document Settings (HTTP Server)」セクションを確認
2. 以下の値が表示されていることを確認：
   - Welcome File: index.html
   - Server Header: BlackJumboDog Version $v
   - Use Hidden Files: ✗
   - Use Dot Files: ✗
   - Enable Directory Listing: ✗
   - Use ETag: ✗

#### 1.3 CGI設定（Phase 3）の確認
1. 「CGI Settings (HTTP Server)」セクションを確認
2. 以下の値が表示されていることを確認：
   - Enable CGI: ✓ (チェック済み)
   - CGI Timeout: 30
   - CGI Commands: 2 item(s) configured
   - CGI Paths: 1 item(s) configured

#### 1.4 SSI設定（Phase 4）の確認
1. 「SSI Settings (HTTP Server)」セクションを確認
2. 以下の値が表示されていることを確認：
   - Enable SSI: ✓ (チェック済み)
   - SSI Extensions: html,htm,shtml
   - Allow SSI exec: ✗

#### 1.5 WebDAV設定（Phase 5）の確認
1. 「WebDAV Settings (HTTP Server)」セクションを確認
2. 以下の値が表示されていることを確認：
   - Enable WebDAV: ✓ (チェック済み)
   - WebDAV Paths: 1 item(s) configured

#### 1.6 Alias・MIME設定（Phase 6-7）の確認
1. 「Alias & MIME Settings (HTTP Server)」セクションを確認
2. 以下の値が表示されていることを確認：
   - Aliases: 2 item(s) configured
   - MIME Types: 8 item(s) configured

#### 1.7 認証設定（Phase 8-9）の確認
1. 「Authentication Settings (HTTP Server)」セクションを確認
2. 以下の値が表示されていることを確認：
   - Auth List: 1 item(s) configured
   - User List: 1 item(s) configured
   - Group List: 1 item(s) configured

#### 1.8 テンプレート設定（Phase 10）の確認
1. 「Template Settings (HTTP Server)」セクションを確認
2. 以下の値が表示されていることを確認：
   - Character Encoding: UTF-8
   - Index Document Template: `<html><body><h1>Directory: $URI</h1><ul>$LIST</ul></body></html>`
   - Error Document Template: `<html><body><h1>Error $CODE</h1><p>$MSG</p></body></html>`

#### 1.9 ACL設定（Phase 11-12）の確認
1. 「ACL Settings (HTTP Server)」セクションを確認
2. 以下の値が表示されていることを確認：
   - Enable Auto ACL: ✓ (チェック済み)
   - Auto ACL Apache Killer: ✓ (チェック済み)
   - ACL Mode: Allow
   - ACL List: 1 item(s) configured

### 2. 設定変更・保存テスト

#### 2.1 基本設定の変更
1. Portを 8080 → 8090 に変更
2. Max Connectionsを 100 → 200 に変更
3. 「Save Settings」ボタンをクリック
4. 成功メッセージが表示されることを確認

#### 2.2 appsettings.json確認（ターミナルから）
```bash
cd /Users/hirauchi.shinichi/Downloads/MyTools/JumboDogX/jumbodogx/src/Jdx.WebUI
cat appsettings.json | grep -A 5 "Port"
```
- Port: 8090 と表示されることを確認
- MaxConnections: 200 と表示されることを確認

#### 2.3 CGI設定の変更
1. CGI Timeoutを 30 → 60 に変更
2. 「Save Settings」ボタンをクリック
3. 成功メッセージが表示されることを確認

#### 2.4 appsettings.json確認
```bash
cat appsettings.json | grep "CgiTimeout"
```
- CgiTimeout: 60 と表示されることを確認

#### 2.5 テンプレート設定の変更
1. Character Encodingを UTF-8 → SHIFT-JIS に変更
2. 「Save Settings」ボタンをクリック
3. 成功メッセージが表示されることを確認

#### 2.6 appsettings.json確認
```bash
cat appsettings.json | grep "Encode"
```
- Encode: "SHIFT-JIS" と表示されることを確認

### 3. 再起動後の設定保持テスト

#### 3.1 WebUIの再起動
```bash
# 現在のプロセスを停止（Ctrl+Cまたはkillコマンド）
ps aux | grep "dotnet run" | grep -v grep | awk '{print $2}' | xargs kill

# 再起動
cd /Users/hirauchi.shinichi/Downloads/MyTools/JumboDogX/jumbodogx/src/Jdx.WebUI
dotnet run --urls http://localhost:5001
```

#### 3.2 設定の確認
1. ブラウザで http://localhost:5001/settings にアクセス
2. 以下の変更した値が保持されていることを確認：
   - Port: 8090
   - Max Connections: 200
   - CGI Timeout: 60
   - Character Encoding: SHIFT-JIS

### 4. デフォルト設定リセットテスト

#### 4.1 リセット実行
1. 「Reset to Default」ボタンをクリック
2. リセットメッセージが表示されることを確認
3. 設定値がデフォルトに戻っていることを確認：
   - Port: 8080
   - Max Connections: 100
   - CGI Timeout: 10
   - Character Encoding: UTF-8

#### 4.2 保存確認
1. 「Save Settings」ボタンをクリック
2. appsettings.jsonを確認して、デフォルト値が保存されていることを確認

## テスト結果記録

### 実施日時
2026/01/16 19:40

### テスト結果サマリー

| テスト項目 | 結果 | 備考 |
|-----------|------|------|
| 1.1 基本設定の読み込み | ⬜ 未実施 | ユーザーによる確認が必要 |
| 1.2 ドキュメント設定の読み込み | ⬜ 未実施 | ユーザーによる確認が必要 |
| 1.3 CGI設定の読み込み | ⬜ 未実施 | ユーザーによる確認が必要 |
| 1.4 SSI設定の読み込み | ⬜ 未実施 | ユーザーによる確認が必要 |
| 1.5 WebDAV設定の読み込み | ⬜ 未実施 | ユーザーによる確認が必要 |
| 1.6 Alias・MIME設定の読み込み | ⬜ 未実施 | ユーザーによる確認が必要 |
| 1.7 認証設定の読み込み | ⬜ 未実施 | ユーザーによる確認が必要 |
| 1.8 テンプレート設定の読み込み | ⬜ 未実施 | ユーザーによる確認が必要 |
| 1.9 ACL設定の読み込み | ⬜ 未実施 | ユーザーによる確認が必要 |
| 2.1-2.2 基本設定の変更・保存 | ⬜ 未実施 | ユーザーによる確認が必要 |
| 2.3-2.4 CGI設定の変更・保存 | ⬜ 未実施 | ユーザーによる確認が必要 |
| 2.5-2.6 テンプレート設定の変更・保存 | ⬜ 未実施 | ユーザーによる確認が必要 |
| 3.1-3.2 再起動後の設定保持 | ⬜ 未実施 | ユーザーによる確認が必要 |
| 4.1-4.2 デフォルト設定リセット | ⬜ 未実施 | ユーザーによる確認が必要 |

### 自動確認可能な項目

以下のコマンドで、appsettings.jsonの内容を自動的に確認できます：

```bash
# すべてのHTTPサーバー設定を表示
cd /Users/hirauchi.shinichi/Downloads/MyTools/JumboDogX/jumbodogx/src/Jdx.WebUI
cat appsettings.json | grep -A 200 "HttpServer"

# 特定の設定項目を確認
cat appsettings.json | grep -E "UseCgi|CgiTimeout|UseSsi|UseWebDav|Encode|UseAutoAcl"
```

## 次のステップ

### 完了した項目
- ✅ すべての設定項目のデータモデル作成
- ✅ すべての設定項目のUI作成
- ✅ 設定の保存・読み込み機能の実装
- ✅ 設定確認手順のドキュメント作成（http_settings_guide.md）
- ✅ テスト手順のドキュメント作成（このファイル）

### 今後の実装が必要な項目

#### Phase A: 設定UI の改善
- テーブル形式データの詳細編集UI（CGI Commands、WebDAV Paths、Aliases、MIME Types、Auth、Users、Groups、ACL）
- 設定項目のバリデーション機能
- 設定変更時の確認ダイアログ

#### Phase B: HTTPサーバー機能の実装
- 基本的なHTTPリクエスト/レスポンス処理
- ドキュメントルートからの静的ファイル配信
- CGI実行機能
- SSI処理機能
- WebDAV機能
- Alias機能
- 基本認証機能
- ACL機能

#### Phase C: テスト強化
- 単体テスト（SettingsService、各機能のテスト）
- 統合テスト（実際のHTTPリクエストを使ったテスト）
- E2Eテスト（WebUIの操作テスト）

## 参考情報

- 設定項目の詳細: [http_settings_guide.md](./http_settings_guide.md)
- 元のBJD5実装: BJD5のソースコード参照
- ASP.NET Core設定: https://learn.microsoft.com/ja-jp/aspnet/core/fundamentals/configuration/
