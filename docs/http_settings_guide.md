# HTTP/HTTPS Server Settings Guide

JumboDogX Web UIでのHTTP/HTTPSサーバー設定の完全ガイド

## 目次

1. [アクセス方法](#アクセス方法)
2. [基本設定](#基本設定)
3. [ドキュメント設定](#ドキュメント設定)
4. [CGI設定](#cgi設定)
5. [SSI設定](#ssi設定)
6. [WebDAV設定](#webdav設定)
7. [Alias & MIME設定](#alias--mime設定)
8. [認証設定](#認証設定)
9. [テンプレート設定](#テンプレート設定)
10. [ACL設定](#acl設定)
11. [設定の保存と確認](#設定の保存と確認)
12. [トラブルシューティング](#トラブルシューティング)

---

## アクセス方法

### Web UI起動

```bash
cd /path/to/jumbodogx
dotnet run --project src/Jdx.WebUI --urls "http://localhost:5001"
```

### 設定画面へのアクセス

ブラウザで以下のURLにアクセス:
```
http://localhost:5001/settings
```

左側のナビゲーションメニューから「Settings」をクリック。

---

## 基本設定

HTTP Serverの基本的な動作設定。

### 設定項目

| 項目 | 説明 | デフォルト値 | 設定範囲 |
|------|------|------------|---------|
| **Enabled** | サーバーの有効/無効 | ON | ON/OFF |
| **Protocol** | 通信プロトコル | HTTP | HTTP/HTTPS |
| **Port** | ポート番号 | 8080 | 1-65535 |
| **Bind Address** | バインドアドレス | 0.0.0.0 | IPv4アドレス |
| **Timeout** | タイムアウト（秒） | 3 | 1-300 |
| **Max Connections** | 最大同時接続数 | 100 | 1-10000 |
| **Use Reverse DNS** | ホスト名逆引き | OFF | ON/OFF |

### 確認方法

1. **設定画面で確認**
   - 右側の「Current Configuration」パネルでリアルタイム表示

2. **appsettings.jsonで確認**
   ```bash
   cat src/Jdx.WebUI/appsettings.json | grep -A 10 "HttpServer"
   ```

3. **動作確認**
   ```bash
   # ポート番号を変更した場合
   curl http://localhost:[Port]/

   # 期待する結果: HTTPステータス 200
   ```

### 注意事項

- ポート番号変更後はサーバーの再起動が必要
- 1024未満のポート（特権ポート）は管理者権限が必要
- `0.0.0.0`は全インターフェースにバインド、`127.0.0.1`はローカルのみ

---

## ドキュメント設定

静的ファイル配信に関する設定。

### 設定項目

| 項目 | 説明 | デフォルト値 |
|------|------|------------|
| **Document Root** | ドキュメントルートディレクトリ | (空) |
| **Welcome File Names** | インデックスファイル名 | index.html |
| **Allow Hidden Files** | 隠しファイルへのアクセス許可 | OFF |
| **Allow ".." in URL** | 親ディレクトリアクセス許可 | OFF |
| **Directory Listing** | ディレクトリ一覧表示 | OFF |
| **Server Header** | Server:ヘッダの内容 | BlackJumboDog Version $v |
| **Use ETag** | ETagヘッダの使用 | OFF |
| **Server Admin** | 管理者メールアドレス | (空) |

### 確認方法

1. **Document Root設定確認**
   ```bash
   # Document Rootにファイルを配置
   echo "<h1>Test Page</h1>" > /path/to/document_root/test.html

   # ブラウザでアクセス
   curl http://localhost:8080/test.html
   ```

2. **Welcome File確認**
   ```bash
   # インデックスファイル作成
   echo "<h1>Index Page</h1>" > /path/to/document_root/index.html

   # ルートにアクセス
   curl http://localhost:8080/
   ```

3. **Directory Listing確認**
   - Directory Listing: ONに設定
   - ディレクトリにアクセス
   - ファイル一覧が表示されることを確認

### 注意事項

- **セキュリティ**: Allow ".." in URLは通常OFFにすべき
- **Hidden Files**: セキュリティ上の理由から通常OFFを推奨
- **Server Header**: `$v`は自動的にバージョン番号に置換される

---

## CGI設定

Common Gateway Interface (CGI) 実行機能の設定。

### 設定項目

| 項目 | 説明 | デフォルト値 |
|------|------|------------|
| **Enable CGI** | CGI機能の有効/無効 | OFF |
| **CGI Timeout** | CGI実行タイムアウト（秒） | 10 |
| **CGI Commands** | 拡張子→プログラムマッピング | (空) |
| **CGI Paths** | パス→ディレクトリマッピング | (空) |

### CGI Commands設定例

```
.pl  → /usr/bin/perl
.cgi → /usr/bin/perl
.php → /usr/bin/php-cgi
.py  → /usr/bin/python3
```

### 確認方法

1. **CGIスクリプト配置**
   ```bash
   # test.cgiを作成
   cat > /path/to/cgi-bin/test.cgi << 'EOF'
   #!/usr/bin/perl
   print "Content-Type: text/html\n\n";
   print "<h1>CGI Test</h1>\n";
   print "<p>CGI is working!</p>\n";
   EOF

   chmod +x /path/to/cgi-bin/test.cgi
   ```

2. **動作確認**
   ```bash
   curl http://localhost:8080/cgi-bin/test.cgi

   # 期待する結果: HTMLコンテンツが返される
   ```

### 注意事項

- **セキュリティ**: CGIは適切に設定しないとセキュリティリスクがある
- **パス設定**: CGIプログラムへのフルパスを指定
- **実行権限**: CGIスクリプトに実行権限が必要

---

## SSI設定

Server Side Includes (SSI) 機能の設定。

### 設定項目

| 項目 | 説明 | デフォルト値 |
|------|------|------------|
| **Enable SSI** | SSI機能の有効/無効 | OFF |
| **SSI Extensions** | SSI認識拡張子 | html,htm |
| **Allow SSI exec** | exec directive許可 | OFF |

### SSI Extensions設定

カンマ区切りで複数指定:
```
html,htm,shtml
```

### 確認方法

1. **SSIファイル作成**
   ```bash
   cat > /path/to/document_root/test.html << 'EOF'
   <!DOCTYPE html>
   <html>
   <body>
   <h1>SSI Test</h1>
   <p>Current Date: <!--#echo var="DATE_LOCAL" --></p>
   <p>Document Name: <!--#echo var="DOCUMENT_NAME" --></p>
   </body>
   </html>
   EOF
   ```

2. **動作確認**
   ```bash
   curl http://localhost:8080/test.html

   # 期待する結果: <!--#echo-->が実際の値に置換される
   ```

### 利用可能なSSIディレクティブ

- `<!--#echo var="変数名" -->`  - 変数表示
- `<!--#include file="ファイル名" -->` - ファイル挿入
- `<!--#exec cmd="コマンド" -->` - コマンド実行（Allow SSI exec=ON必要）

### 注意事項

- **exec directive**: セキュリティリスクがあるため、必要な場合のみON
- **拡張子**: SSIを使用するファイルは指定した拡張子にする

---

## WebDAV設定

Web-based Distributed Authoring and Versioning (WebDAV) の設定。

### 設定項目

| 項目 | 説明 | デフォルト値 |
|------|------|------------|
| **Enable WebDAV** | WebDAV機能の有効/無効 | OFF |
| **WebDAV Paths** | パス→ディレクトリマッピング | (空) |

### WebDAV Paths設定例

```
/dav → /path/to/webdav_root (書き込み許可: ON)
```

### 確認方法

1. **WebDAVクライアントでアクセス**
   ```bash
   # macOSの場合
   # Finderで「サーバへ接続」
   # http://localhost:8080/dav

   # Linuxの場合
   cadaver http://localhost:8080/dav
   ```

2. **curlでテスト**
   ```bash
   # PROPFINDリクエスト
   curl -X PROPFIND http://localhost:8080/dav/

   # 期待する結果: XMLレスポンス
   ```

### 注意事項

- **書き込み許可**: 必要な場合のみON
- **認証**: WebDAVは認証設定と組み合わせて使用を推奨
- **セキュリティ**: 外部公開する場合はHTTPSを使用

---

## Alias & MIME設定

仮想パスとMIMEタイプのマッピング。

### Aliases設定

| 項目 | 説明 |
|------|------|
| **仮想パス** | URLパス |
| **実際のディレクトリ** | ファイルシステムパス |

### 設定例

```
/images → /var/www/images
/docs   → /usr/share/doc
```

### MIME Types設定

| 拡張子 | MIMEタイプ | デフォルト設定 |
|--------|-----------|--------------|
| txt | text/plain | ✓ |
| html | text/html | ✓ |
| css | text/css | ✓ |
| js | text/javascript | ✓ |
| json | application/json | ✓ |
| png | image/png | ✓ |
| jpg | image/jpeg | ✓ |
| pdf | application/pdf | ✓ |

### 確認方法

1. **Alias確認**
   ```bash
   # エイリアス先にファイル配置
   echo "Test" > /var/www/images/test.txt

   # エイリアスパスでアクセス
   curl http://localhost:8080/images/test.txt
   ```

2. **MIME Type確認**
   ```bash
   # レスポンスヘッダー確認
   curl -I http://localhost:8080/test.html

   # 期待する結果: Content-Type: text/html
   ```

### 注意事項

- デフォルトMIMEタイプは自動的に設定される
- カスタムMIMEタイプを追加可能
- Reset to Defaultで初期MIMEリストに戻る

---

## 認証設定

HTTPベーシック認証の設定。

### 設定項目

- **Auth Definitions**: ディレクトリ→認証ルールマッピング
- **Users**: ユーザー名→パスワード（ハッシュ化）
- **Groups**: グループ→ユーザーマッピング

### 設定例

**Auth Definitions:**
```
/private → "Restricted Area" → user alice bob
/admin   → "Admin Only" → group admins
```

**Users:**
```
alice → [SHA256ハッシュ]
bob   → [SHA256ハッシュ]
admin → [SHA256ハッシュ]
```

**Groups:**
```
admins → admin
admins → alice
```

### 確認方法

1. **認証なしでアクセス**
   ```bash
   curl http://localhost:8080/private/

   # 期待する結果: 401 Unauthorized
   ```

2. **認証ありでアクセス**
   ```bash
   curl -u alice:password http://localhost:8080/private/

   # 期待する結果: 200 OK
   ```

### 注意事項

- パスワードはSHA256でハッシュ化して保存
- HTTPSを使用しない場合、パスワードは平文で送信される
- グループ機能で複数ユーザーを一括管理可能

---

## テンプレート設定

エラーページとディレクトリ一覧のテンプレート。

### 設定項目

| 項目 | 説明 | デフォルト値 |
|------|------|------------|
| **Character Encoding** | 文字エンコーディング | UTF-8 |
| **Index Document Template** | ディレクトリ一覧テンプレート | (空) |
| **Error Document Template** | エラーページテンプレート | (空) |

### 利用可能な変数

**Index Document:**
- `$URI` - リクエストURI
- `$HREF` - リンクURL
- `$NAME` - ファイル/ディレクトリ名
- `$DATE` - 更新日時
- `$SIZE` - ファイルサイズ
- `$SERVER` - サーバー名
- `$VER` - バージョン

**Error Document:**
- `$CODE` - HTTPステータスコード
- `$MSG` - エラーメッセージ
- `$URI` - リクエストURI
- `$SERVER` - サーバー名
- `$VER` - バージョン

### テンプレート例

**Error Document:**
```html
<html>
<head><title>Error $CODE</title></head>
<body>
<h1>Error $CODE: $MSG</h1>
<p>The requested URL <code>$URI</code> was not found on this server.</p>
<hr>
<small>$SERVER $VER</small>
</body>
</html>
```

### 確認方法

1. **エラーページ確認**
   ```bash
   curl http://localhost:8080/nonexistent

   # カスタムエラーページが表示されることを確認
   ```

2. **ディレクトリ一覧確認**
   - Directory Listing: ON
   - Index Document Templateに設定
   - ディレクトリにアクセスして確認

---

## ACL設定

Access Control List (ACL) によるアクセス制御。

### 設定項目

| 項目 | 説明 | デフォルト値 |
|------|------|------------|
| **Auto ACL** | 自動アクセス拒否 | OFF |
| **Apache Killer Protection** | Apache Killer攻撃対策 | OFF |
| **ACL Mode** | Allow (ホワイトリスト) / Deny (ブラックリスト) | Allow |
| **ACL Rules** | ACL名→アドレスパターン | (空) |

### ACL Mode

- **Allow (Whitelist)**: リストに含まれるアドレスのみ許可
- **Deny (Blacklist)**: リストに含まれるアドレスを拒否

### ACL Rules設定例

```
localhost → 127.0.0.1
localnet  → 192.168.1.0/24
blocked   → 10.0.0.50
```

### 確認方法

1. **ACL設定確認**
   ```bash
   # 許可されたIPからアクセス
   curl --interface 127.0.0.1 http://localhost:8080/

   # 期待する結果: 200 OK
   ```

2. **ブロック確認**
   ```bash
   # ブロックされたIPからアクセス（シミュレーション）
   # 期待する結果: 403 Forbidden
   ```

### 注意事項

- **Auto ACL**: 不審なアクセスパターンを自動検出して拒否
- **Apache Killer**: Apache Killer攻撃に対する保護
- **CIDR表記**: ネットワーク範囲指定可能 (例: 192.168.1.0/24)

---

## 設定の保存と確認

### 設定保存手順

1. **設定変更**
   - 各設定項目を変更

2. **保存**
   - 画面下部の「💾 Save Settings」ボタンをクリック
   - 成功メッセージ表示: "Settings saved successfully!"

3. **サーバー再起動** （必要な場合）
   - ポート番号変更
   - Enabled/Disabled切り替え
   - その他サーバー設定変更

### 設定確認方法

1. **Web UI上で確認**
   - 右側の「Current Configuration」パネル
   - リアルタイムで現在の設定を表示

2. **設定ファイルで確認**
   ```bash
   # appsettings.json を確認
   cat src/Jdx.WebUI/appsettings.json

   # または jq で整形表示
   jq '.Jdx.HttpServer' src/Jdx.WebUI/appsettings.json
   ```

3. **デフォルトにリセット**
   - 「🔄 Reset to Default」ボタンをクリック
   - すべての設定がデフォルト値に戻る
   - 保存するまで反映されない

4. **再読み込み**
   - 「↻ Reload」ボタンをクリック
   - appsettings.jsonから最新の設定を読み込む

---

## トラブルシューティング

### 設定が保存されない

**原因**: appsettings.jsonへの書き込み権限がない

**解決方法**:
```bash
# 権限確認
ls -l src/Jdx.WebUI/appsettings.json

# 権限付与
chmod 644 src/Jdx.WebUI/appsettings.json
```

### ポート変更後にアクセスできない

**原因**: サーバーが再起動されていない

**解決方法**:
```bash
# サーバー停止
Ctrl+C

# 新しいポートで起動
dotnet run --project src/Jdx.WebUI --urls "http://localhost:[新しいポート]"
```

### CGIが動作しない

**原因1**: CGI機能が無効
- Enable CGI: ONに設定

**原因2**: CGIスクリプトに実行権限がない
```bash
chmod +x /path/to/cgi-script
```

**原因3**: CGI Commands設定が間違っている
- 拡張子とプログラムパスを正しく設定

### SSIが動作しない

**原因1**: SSI機能が無効
- Enable SSI: ONに設定

**原因2**: 拡張子が一致しない
- SSI Extensionsに使用する拡張子を追加

**原因3**: SSI構文エラー
- ディレクティブの構文を確認
- スペース、ダブルハイフンの位置に注意

### WebDAVが動作しない

**原因1**: WebDAV機能が無効
- Enable WebDAV: ONに設定

**原因2**: パスマッピングが設定されていない
- WebDAV Pathsを設定

**原因3**: 書き込み権限がない
- ディレクトリの書き込み権限を確認
- WebDAV Pathsで書き込み許可をON

### 認証が機能しない

**原因1**: Auth Definitionsが設定されていない
- 保護するディレクトリと認証ルールを設定

**原因2**: ユーザーが登録されていない
- Usersにユーザーを追加

**原因3**: パスワードが間違っている
- パスワードはSHA256ハッシュで保存されることに注意

### ACLでアクセスがブロックされる

**原因**: ACL Mode設定が間違っている

**解決方法**:
- Allowモード: 許可するIPをリストに追加
- Denyモード: ブロックするIPをリストに追加
- localhost (127.0.0.1) は通常許可する

---

## 付録: 設定項目完全リスト

### 基本設定 (7項目)
- Enabled, Protocol, Port, BindAddress, UseResolve, TimeOut, MaxConnections

### ドキュメント設定 (8項目)
- DocumentRoot, WelcomeFileName, UseHidden, UseDot, UseDirectoryEnum, ServerHeader, UseEtag, ServerAdmin

### CGI設定 (4項目)
- UseCgi, CgiCommands, CgiTimeout, CgiPaths

### SSI設定 (3項目)
- UseSsi, SsiExt, UseExec

### WebDAV設定 (2項目)
- UseWebDav, WebDavPaths

### Alias設定 (1項目)
- Aliases

### MIME設定 (1項目)
- MimeTypes (14種類のデフォルト設定)

### 認証設定 (3項目)
- AuthList, UserList, GroupList

### テンプレート設定 (3項目)
- Encode, IndexDocument, ErrorDocument

### ACL設定 (4項目)
- UseAutoAcl, AutoAclApacheKiller, EnableAcl, AclList

**合計: 36設定項目 + テーブル形式データ9種類**

---

## まとめ

JumboDogXのHTTP/HTTPSサーバー設定は、元のBJD5の全機能をカバーしています。

✅ **実装完了機能**:
- 基本サーバー設定
- 静的ファイル配信設定
- CGI実行機能
- SSI機能
- WebDAV機能
- 仮想パス (Alias)
- MIMEタイプ管理
- HTTPベーシック認証
- テンプレートカスタマイズ
- アクセス制御 (ACL)

📝 **設定手順**:
1. Web UI (http://localhost:5001/settings) にアクセス
2. 各セクションで設定を変更
3. 「Save Settings」で保存
4. 必要に応じてサーバー再起動

🔒 **セキュリティ推奨設定**:
- HTTPSを使用（本番環境）
- Allow ".." in URL: OFF
- Allow Hidden Files: OFF
- CGI/SSI execは必要な場合のみON
- ACLで適切なアクセス制御
- 認証でセンシティブなパスを保護

---

**Document Version**: 1.0
**Last Updated**: 2026/01/16
**JumboDogX Version**: 9.0.0-dev
