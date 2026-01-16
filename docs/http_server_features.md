# HTTP Server - 実装機能一覧

## 実装日
2026-01-17

## 概要
JumboDogX HTTPサーバーの実装機能を説明します。

---

## 実装済み機能

### 1. 基本機能（優先度: 高）

#### 1.1 静的ファイル配信
- **状態**: ✅ 完全実装
- **説明**:
  - HTMLファイル、CSS、JavaScript、画像などの静的ファイルを配信
  - 大容量ファイル（10MB以上）はストリーミング配信
  - Content-Type自動判定（MIMEタイプ設定に基づく）
  - Last-Modified ヘッダーサポート
  - Conditional GET (If-Modified-Since) サポート

#### 1.2 Hidden/Dotファイルアクセス制御
- **状態**: ✅ 完全実装
- **設定項目**:
  - `UseHidden`: 隠しファイルへのアクセス許可 (デフォルト: false)
  - `UseDot`: ドットファイル（.から始まるファイル）へのアクセス許可 (デフォルト: false)
- **動作**:
  - `UseHidden=false`: ファイルシステムの隠し属性を持つファイルへのアクセスを拒否
  - `UseDot=false`: ファイル名が`.`で始まるファイル・ディレクトリへのアクセスを拒否
  - セキュリティ警告をログ出力

#### 1.3 ディレクトリ一覧表示
- **状態**: ✅ 完全実装
- **設定項目**:
  - `UseDirectoryEnum`: ディレクトリ一覧表示の有効/無効 (デフォルト: false)
  - `IndexDocument`: ディレクトリ一覧のテンプレートHTML
  - `WelcomeFileName`: インデックスファイル名（カンマ区切り、デフォルト: "index.html")
- **動作**:
  - Welcome fileが存在する場合は優先的に表示
  - `UseDirectoryEnum=true`: ディレクトリ内のファイル・フォルダ一覧をHTML形式で表示
  - ファイルサイズ、最終更新日時を表示
  - 親ディレクトリへのリンクを提供

#### 1.4 Alias機能
- **状態**: ✅ 完全実装
- **設定項目**: `Aliases` リスト
- **説明**:
  - URLパスを任意の物理ディレクトリにマッピング
  - 例: `/docs` → `/usr/share/doc`
  - 最長一致優先
  - セキュリティ検証（パストラバーサル防止）

**設定例**:
```json
"Aliases": [
  {
    "Name": "/docs",
    "Directory": "/usr/share/doc"
  },
  {
    "Name": "/images",
    "Directory": "/var/www/images"
  }
]
```

#### 1.5 テンプレート処理
- **状態**: ✅ 完全実装
- **設定項目**:
  - `IndexDocument`: ディレクトリ一覧表示用テンプレート
  - `ErrorDocument`: エラーページ用テンプレート
- **テンプレート変数**:
  - `$URI`: リクエストURI
  - `$LIST`: ディレクトリ一覧HTML（IndexDocumentのみ）
  - `$CODE`: HTTPステータスコード（ErrorDocumentのみ）
  - `$MSG`: エラーメッセージ（ErrorDocumentのみ）
  - `$SERVER`: サーバー名
  - `$VER`: バージョン番号

**設定例**:
```json
"IndexDocument": "<html><body><h1>Directory: $URI</h1><ul>$LIST</ul></body></html>",
"ErrorDocument": "<html><body><h1>Error $CODE</h1><p>$MSG</p></body></html>"
```

#### 1.6 ETag対応
- **状態**: ✅ 完全実装
- **設定項目**: `UseEtag` (デフォルト: false)
- **説明**:
  - ファイルのETagを自動生成（ファイルサイズ + 最終更新日時のハッシュ）
  - ブラウザキャッシュの効率化
  - 304 Not Modified レスポンスのサポート

#### 1.7 BindAddress対応
- **状態**: ✅ 完全実装
- **設定項目**: `BindAddress` (デフォルト: "127.0.0.1")
- **説明**:
  - 指定されたIPアドレスでリスニング
  - `127.0.0.1`: localhost のみ
  - `0.0.0.0`: すべてのネットワークインターフェース
  - 無効なアドレスの場合はデフォルト（Any）を使用

#### 1.8 タイムアウト処理
- **状態**: ✅ 完全実装
- **設定項目**: `TimeOut` (秒単位、デフォルト: 3)
- **説明**:
  - リクエスト処理全体（読み取り→処理→送信）のタイムアウト
  - タイムアウト発生時は408 Request Timeout レスポンス
  - タイムアウトエラーをログ出力

#### 1.9 最大接続数制限
- **状態**: ✅ 完全実装
- **設定項目**: `MaxConnections` (デフォルト: 100)
- **説明**:
  - 同時接続数が上限に達した場合、新規接続を拒否
  - 503 Service Unavailable レスポンスを送信
  - 警告ログ出力

---

### 2. セキュリティ機能（優先度: 中）

#### 2.1 Basic認証
- **状態**: ✅ 完全実装
- **設定項目**:
  - `AuthList`: 認証が必要なディレクトリのリスト
  - `UserList`: ユーザー名とパスワード（SHA-256ハッシュ）
  - `GroupList`: グループとユーザーの関連付け
- **動作**:
  - パス別の認証設定
  - SHA-256ハッシュによるパスワード検証
  - `Require` 条件サポート:
    - `user username1 username2`: 特定ユーザーのみ許可
    - `group groupname`: 特定グループのユーザーのみ許可
    - `valid-user`: 認証されたすべてのユーザーを許可
  - 401 Unauthorized レスポンス（WWW-Authenticate ヘッダー付き）
  - 403 Forbidden レスポンス（権限不足時）

**設定例**:
```json
"AuthList": [
  {
    "Directory": "/private",
    "AuthName": "Private Area",
    "Require": "user admin"
  }
],
"UserList": [
  {
    "UserName": "admin",
    "Password": "SHA256_HASH_HERE"
  }
],
"GroupList": [
  {
    "GroupName": "admins",
    "UserName": "admin"
  }
]
```

**パスワードハッシュ生成**:
```bash
echo -n 'YourPassword' | shasum -a 256
```

#### 2.2 ACL (Access Control List)
- **状態**: ✅ 基本実装完了
- **設定項目**:
  - `EnableAcl`: 0=無効, 1=許可リスト, 2=拒否リスト (デフォルト: 0)
  - `AclList`: IPアドレスまたはネットワーク範囲のリスト
- **サポート形式**:
  - 単一IPアドレス: `192.168.1.100`
  - CIDR notation: `192.168.1.0/24`
- **動作**:
  - `EnableAcl=1`: リストにあるIPのみ許可（ホワイトリスト）
  - `EnableAcl=2`: リストにあるIPを拒否（ブラックリスト）
  - 拒否時は403 Forbiddenレスポンス

**設定例**:
```json
"EnableAcl": 1,
"AclList": [
  {
    "Name": "LocalNetwork",
    "Address": "192.168.1.0/24"
  },
  {
    "Name": "SpecificIP",
    "Address": "203.0.113.100"
  }
]
```

#### 2.3 エンコーディング設定
- **状態**: ✅ 完全実装
- **設定項目**: `Encode` (デフォルト: "UTF-8")
- **説明**:
  - HTMLレスポンスのcharset指定
  - 現在はUTF-8で統一実装

---

### 3. 高度な機能（優先度: 低）

#### 3.1 CGI実行
- **状態**: ✅ 完全実装
- **設定項目**:
  - `UseCgi`: CGI機能の有効/無効 (デフォルト: false)
  - `CgiPaths`: CGIスクリプトのパスとディレクトリマッピング
  - `CgiCommands`: 拡張子とインタプリタのマッピング
  - `CgiTimeout`: CGIスクリプト実行タイムアウト（秒、デフォルト: 30）
- **動作**:
  - CGI/1.1 仕様に準拠
  - 環境変数の自動設定（GATEWAY_INTERFACE, SERVER_PROTOCOL, REQUEST_METHOD等）
  - POSTデータの標準入力への送信
  - 標準出力からのヘッダー・ボディ解析
  - タイムアウト処理（504 Gateway Timeout）
  - Python、Perl、Ruby等の各種スクリプト実行対応

**設定例**:
```json
"UseCgi": true,
"CgiTimeout": 30,
"CgiPaths": [
  {
    "Path": "/cgi-bin",
    "Directory": "/var/www/cgi-bin"
  }
],
"CgiCommands": [
  {
    "Extension": "py",
    "Program": "/usr/bin/python3"
  },
  {
    "Extension": "pl",
    "Program": "/usr/bin/perl"
  }
]
```

#### 3.2 SSI (Server Side Includes)
- **状態**: ✅ 完全実装
- **設定項目**:
  - `UseSsi`: SSI機能の有効/無効 (デフォルト: false)
  - `SsiExt`: SSI処理対象の拡張子（カンマ区切り、デフォルト: "html,htm,shtml"）
  - `UseExec`: exec ディレクティブの有効/無効 (デフォルト: false)
- **サポートディレクティブ**:
  - `<!--#include file="path" -->`: ファイルインクルード（相対パス）
  - `<!--#include virtual="/path" -->`: ファイルインクルード（ドキュメントルートからの絶対パス）
  - `<!--#echo var="VARIABLE" -->`: 変数の展開
    - DATE_LOCAL, DATE_GMT, LAST_MODIFIED, DOCUMENT_NAME, DOCUMENT_URI, SERVER_SOFTWARE
  - `<!--#exec cmd="command" -->`: コマンド実行（UseExec=trueの場合のみ）
  - `<!--#fsize file="path" -->`: ファイルサイズ表示
  - `<!--#flastmod file="path" -->`: 最終更新日時表示
- **セキュリティ**:
  - パストラバーサル攻撃の防止（DocumentRoot外へのアクセス拒否）
  - exec ディレクティブはデフォルト無効

**設定例**:
```json
"UseSsi": true,
"SsiExt": "html,htm,shtml",
"UseExec": false
```

**SSIファイル例**:
```html
<!DOCTYPE html>
<html>
<body>
  <h1>Server Info</h1>
  <p>Date: <!--#echo var="DATE_LOCAL" --></p>
  <p>Server: <!--#echo var="SERVER_SOFTWARE" --></p>
  <!--#include virtual="/common/header.html" -->
</body>
</html>
```

#### 3.3 WebDAV
- **状態**: ✅ 完全実装
- **設定項目**:
  - `UseWebDav`: WebDAV機能の有効/無効 (デフォルト: false)
  - `WebDavPaths`: WebDAVを許可するパスとディレクトリマッピング
    - `Path`: URLパス
    - `Directory`: 物理ディレクトリ
    - `AllowWrite`: 書き込み許可 (デフォルト: false)
- **サポートメソッド**:
  - `PROPFIND`: プロパティ取得（ディレクトリ一覧、ファイル情報）
  - `MKCOL`: コレクション（ディレクトリ）作成
  - `PUT`: ファイルアップロード
  - `DELETE`: ファイル/ディレクトリ削除
  - `COPY`: ファイル/ディレクトリコピー（基本実装）
  - `MOVE`: ファイル/ディレクトリ移動（基本実装）
  - `LOCK/UNLOCK`: リソースロック（Stub実装）
- **動作**:
  - RFC 4918 (WebDAV) 準拠
  - XML multi-status レスポンス (207 Multi-Status)
  - Depth ヘッダーサポート (0, 1, infinity)
  - 書き込み権限チェック（AllowWrite=falseの場合は403 Forbidden）

**設定例**:
```json
"UseWebDav": true,
"WebDavPaths": [
  {
    "Path": "/webdav",
    "Directory": "/var/www/webdav",
    "AllowWrite": true
  },
  {
    "Path": "/readonly",
    "Directory": "/var/www/public",
    "AllowWrite": false
  }
]
```

---

## 動作確認

### 基本動作
1. WebUIまたはHostアプリケーションを起動
2. HTTPサーバーが自動的に起動（デフォルト: http://127.0.0.1:8080）
3. ドキュメントルート: `/tmp/jumbodogx-www`

### テスト方法

#### 静的ファイル配信
```bash
curl http://localhost:8080/index.html
```

#### ディレクトリ一覧
```bash
# appsettings.json で UseDirectoryEnum: true に設定後
curl http://localhost:8080/
```

#### 統計情報
```bash
curl http://localhost:8080/stats
```

#### Basic認証
```bash
# 認証なし（401 Unauthorized）
curl -v http://localhost:8080/private/

# 認証あり
curl -u admin:password http://localhost:8080/private/
```

#### ETag
```bash
# 初回リクエスト（200 OK + ETag）
curl -v http://localhost:8080/index.html

# 2回目（If-None-Match + ETag → 304 Not Modified）
curl -v -H 'If-None-Match: "ETAG_VALUE"' http://localhost:8080/index.html
```

#### CGI実行
```bash
# appsettings.json で UseCgi: true に設定後

# Pythonスクリプトの実行
curl http://localhost:8080/cgi-bin/test.py

# POSTリクエスト
curl -X POST -d "name=test&value=123" http://localhost:8080/cgi-bin/form.py
```

**テスト用CGIスクリプト例** (`test.py`):
```python
#!/usr/bin/env python3
print("Content-Type: text/html")
print()
print("<html><body>")
print("<h1>CGI Test</h1>")
print("<p>Hello from CGI!</p>")
print("</body></html>")
```

#### SSI処理
```bash
# appsettings.json で UseSsi: true に設定後

# SSIを含むHTMLファイルにアクセス
curl http://localhost:8080/test.shtml
```

**テスト用SSIファイル例** (`test.shtml`):
```html
<!DOCTYPE html>
<html>
<body>
  <h1>SSI Test</h1>
  <p>Current Date: <!--#echo var="DATE_LOCAL" --></p>
  <p>Server: <!--#echo var="SERVER_SOFTWARE" --></p>
</body>
</html>
```

#### WebDAV操作
```bash
# appsettings.json で UseWebDav: true に設定後

# ディレクトリ一覧取得 (PROPFIND)
curl -X PROPFIND http://localhost:8080/webdav/

# ファイルアップロード (PUT)
curl -X PUT --data-binary @test.txt http://localhost:8080/webdav/test.txt

# ディレクトリ作成 (MKCOL)
curl -X MKCOL http://localhost:8080/webdav/newdir/

# ファイル削除 (DELETE)
curl -X DELETE http://localhost:8080/webdav/test.txt

# ディレクトリ削除
curl -X DELETE http://localhost:8080/webdav/newdir/
```

---

## 設定ファイル

設定は `src/Jdx.WebUI/appsettings.json` に記載されています。

### セキュリティ上の注意事項
1. **パスワードの変更**: デフォルトのパスワードハッシュを必ず変更してください
2. **BindAddress**: 外部アクセスが不要な場合は `127.0.0.1` を維持
3. **CGI実行**: セキュリティリスクが高いため、信頼できるスクリプトのみ実行してください（デフォルトで無効）
4. **SSI exec**: コマンド実行は特に危険です。`UseExec: false` を維持してください（デフォルトで無効）
5. **WebDAV書き込み**: `AllowWrite: false` を基本とし、必要な場合のみ有効化してください
6. **ACL**: 必要に応じてIPアドレス制限を設定してください

詳細は [SECURITY.md](../SECURITY.md) を参照してください。

---

## ログ

HTTPサーバーのログは以下の情報を出力します：
- リクエスト受信（メソッド、パス、接続元）
- レスポンス送信（ステータスコード）
- 認証成功/失敗
- ACL拒否
- エラー（ファイル未検出、権限エラー、タイムアウトなど）

---

## 今後の拡張予定
1. HTTPS/TLS サポート
2. HTTP/2 サポート
3. Range requests (部分ダウンロード)
4. Gzip圧縮
5. WebDAV COPY/MOVE/LOCK/UNLOCK の完全実装
6. CGI FastCGI サポート

---

## 関連ドキュメント
- [README.md](../README.md)
- [SECURITY.md](../SECURITY.md)
- [appsettings.json](../src/Jdx.WebUI/appsettings.json)
