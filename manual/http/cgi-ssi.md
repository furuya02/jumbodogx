# CGI/SSI設定ガイド

このガイドでは、JumboDogXのHTTPサーバーでCGI（Common Gateway Interface）とSSI（Server Side Includes）を設定し、動的コンテンツを生成する方法を説明します。

## 目次

- [CGI/SSIとは](#cgissiとは)
- [CGI設定](#cgi設定)
- [SSI設定](#ssi設定)
- [セキュリティ注意事項](#セキュリティ注意事項)
- [トラブルシューティング](#トラブルシューティング)

## CGI/SSIとは

### CGI（Common Gateway Interface）とは

CGIは、Webサーバーが外部プログラムを実行して動的なコンテンツを生成する仕組みです。

**用途**:
- フォームの処理
- データベースへのアクセス
- 動的なHTMLページの生成
- アクセスカウンター
- 簡易的なAPI

**対応言語**:
- シェルスクリプト（Bash, sh）
- Python
- Perl
- Ruby
- PHP
- 実行可能バイナリ（.exe, .bat）

### SSI（Server Side Includes）とは

SSIは、HTMLファイル内に特別なコメント形式のディレクティブを記述し、サーバー側で動的に内容を埋め込む機能です。

**用途**:
- ヘッダー・フッターの共通化
- 最終更新日時の表示
- ファイルのインクルード
- 環境変数の表示
- 簡易的な条件分岐

**メリット**:
- CGIより軽量
- HTMLファイル内で直接使用可能
- サーバーサイドでの処理

## CGI設定

### Web UIからの設定

#### ステップ1: CGI設定画面を開く

1. ブラウザで `http://localhost:5001` にアクセス
2. **Settings** → **HTTP/HTTPS** → **CGI** を選択

#### ステップ2: CGI設定を入力

以下の項目を設定します：

| 項目 | 説明 | 例 |
|------|------|-----|
| Enable CGI | CGIを有効化 | ✓ ON |
| CGI Extension | CGI拡張子 | `.cgi`, `.pl`, `.py` |
| CGI Directory | CGIスクリプトのディレクトリ | `/cgi-bin/` |
| Timeout | スクリプトのタイムアウト（秒） | 30 |

#### ステップ3: サーバーを再起動

設定を反映するため、JumboDogXを再起動します。

### appsettings.jsonでの設定

`src/Jdx.WebUI/appsettings.json` を編集します。

#### 基本的なCGI設定

```json
{
  "Jdx": {
    "HttpServer": {
      "Cgi": {
        "Enabled": true,
        "Extensions": [".cgi", ".pl", ".py", ".rb", ".sh"],
        "Directory": "/cgi-bin/",
        "Timeout": 30
      }
    }
  }
}
```

#### 詳細なCGI設定

```json
{
  "Jdx": {
    "HttpServer": {
      "Cgi": {
        "Enabled": true,
        "Extensions": [".cgi", ".pl", ".py", ".rb", ".sh", ".exe", ".bat"],
        "Directory": "/cgi-bin/",
        "Timeout": 30,
        "MaxContentLength": 10485760,
        "Interpreters": {
          ".py": "/usr/bin/python3",
          ".rb": "/usr/bin/ruby",
          ".pl": "/usr/bin/perl"
        }
      }
    }
  }
}
```

### ディレクトリ構成

CGIスクリプトは専用のディレクトリに配置することを推奨します。

```
/var/www/
├── html/              # 静的ファイル
│   └── index.html
└── cgi-bin/           # CGIスクリプト
    ├── hello.cgi
    ├── form.py
    └── counter.sh
```

### CGIスクリプトの例

#### 例1: シンプルなHello World（Bash）

**/var/www/cgi-bin/hello.cgi**:

```bash
#!/bin/bash

# Content-Typeヘッダーを出力（必須）
echo "Content-Type: text/html"
echo ""

# HTMLコンテンツを出力
cat << EOF
<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title>Hello CGI</title>
</head>
<body>
    <h1>Hello, CGI World!</h1>
    <p>現在時刻: $(date)</p>
</body>
</html>
EOF
```

**実行権限の付与**:

```bash
chmod +x /var/www/cgi-bin/hello.cgi
```

**アクセス方法**:

```
http://localhost:8080/cgi-bin/hello.cgi
```

#### 例2: 環境変数の表示（Bash）

**/var/www/cgi-bin/env.cgi**:

```bash
#!/bin/bash

echo "Content-Type: text/html"
echo ""

cat << EOF
<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title>CGI Environment Variables</title>
    <style>
        table { border-collapse: collapse; }
        th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }
        th { background: #f0f0f0; }
    </style>
</head>
<body>
    <h1>CGI Environment Variables</h1>
    <table>
        <tr><th>Variable</th><th>Value</th></tr>
        <tr><td>SERVER_SOFTWARE</td><td>${SERVER_SOFTWARE}</td></tr>
        <tr><td>SERVER_NAME</td><td>${SERVER_NAME}</td></tr>
        <tr><td>GATEWAY_INTERFACE</td><td>${GATEWAY_INTERFACE}</td></tr>
        <tr><td>SERVER_PROTOCOL</td><td>${SERVER_PROTOCOL}</td></tr>
        <tr><td>SERVER_PORT</td><td>${SERVER_PORT}</td></tr>
        <tr><td>REQUEST_METHOD</td><td>${REQUEST_METHOD}</td></tr>
        <tr><td>PATH_INFO</td><td>${PATH_INFO}</td></tr>
        <tr><td>QUERY_STRING</td><td>${QUERY_STRING}</td></tr>
        <tr><td>REMOTE_ADDR</td><td>${REMOTE_ADDR}</td></tr>
        <tr><td>REMOTE_HOST</td><td>${REMOTE_HOST}</td></tr>
        <tr><td>CONTENT_TYPE</td><td>${CONTENT_TYPE}</td></tr>
        <tr><td>CONTENT_LENGTH</td><td>${CONTENT_LENGTH}</td></tr>
    </table>
</body>
</html>
EOF
```

#### 例3: フォーム処理（Python）

**/var/www/cgi-bin/form.py**:

```python
#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import cgi
import cgitb
import os

# デバッグモード（開発時のみ）
cgitb.enable()

# フォームデータの取得
form = cgi.FieldStorage()
name = form.getvalue('name', 'Guest')
email = form.getvalue('email', '')

# Content-Typeヘッダーの出力
print("Content-Type: text/html; charset=utf-8")
print()

# HTMLコンテンツの出力
html = f"""<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title>Form Result</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            max-width: 600px;
            margin: 50px auto;
            padding: 20px;
        }}
        .result {{
            background: #f0f0f0;
            padding: 20px;
            border-radius: 8px;
        }}
    </style>
</head>
<body>
    <h1>Form Result</h1>
    <div class="result">
        <p><strong>Name:</strong> {cgi.escape(name)}</p>
        <p><strong>Email:</strong> {cgi.escape(email)}</p>
        <p><strong>Request Method:</strong> {os.environ.get('REQUEST_METHOD', 'N/A')}</p>
        <p><strong>Remote Address:</strong> {os.environ.get('REMOTE_ADDR', 'N/A')}</p>
    </div>
    <p><a href="/form.html">Back to form</a></p>
</body>
</html>
"""

print(html)
```

**フォームHTML** (**/var/www/html/form.html**):

```html
<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title>Contact Form</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            max-width: 600px;
            margin: 50px auto;
            padding: 20px;
        }
        input, textarea {
            width: 100%;
            padding: 8px;
            margin: 8px 0;
            box-sizing: border-box;
        }
        button {
            background: #4299E1;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }
    </style>
</head>
<body>
    <h1>Contact Form</h1>
    <form action="/cgi-bin/form.py" method="POST">
        <label>Name:</label>
        <input type="text" name="name" required>

        <label>Email:</label>
        <input type="email" name="email" required>

        <button type="submit">Submit</button>
    </form>
</body>
</html>
```

#### 例4: アクセスカウンター（Python）

**/var/www/cgi-bin/counter.py**:

```python
#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import os
import fcntl

COUNTER_FILE = '/var/www/data/counter.txt'

def get_count() -> int:
    """カウンターファイルから現在のカウントを取得"""
    if not os.path.exists(COUNTER_FILE):
        os.makedirs(os.path.dirname(COUNTER_FILE), exist_ok=True)
        return 0

    try:
        with open(COUNTER_FILE, 'r') as f:
            return int(f.read().strip())
    except (ValueError, FileNotFoundError):
        return 0

def increment_count() -> int:
    """カウントをインクリメントして新しい値を返す"""
    # ファイルロックを使用して排他制御
    lock_file = COUNTER_FILE + '.lock'

    with open(lock_file, 'w') as lock:
        fcntl.flock(lock.fileno(), fcntl.LOCK_EX)

        count = get_count() + 1

        with open(COUNTER_FILE, 'w') as f:
            f.write(str(count))

        fcntl.flock(lock.fileno(), fcntl.LOCK_UN)

    return count

# Content-Typeヘッダーの出力
print("Content-Type: text/html; charset=utf-8")
print()

# カウントをインクリメント
count = increment_count()

# HTMLコンテンツの出力
html = f"""<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title>Access Counter</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            text-align: center;
            padding: 50px;
        }}
        .counter {{
            font-size: 48px;
            font-weight: bold;
            color: #4299E1;
            background: #f0f0f0;
            padding: 20px;
            border-radius: 8px;
            display: inline-block;
        }}
    </style>
</head>
<body>
    <h1>Access Counter</h1>
    <div class="counter">{count:,}</div>
    <p>You are visitor number {count:,}</p>
</body>
</html>
"""

print(html)
```

**ディレクトリ作成**:

```bash
mkdir -p /var/www/data
chmod 755 /var/www/data
```

#### 例5: JSON APIエンドポイント（Python）

**/var/www/cgi-bin/api.py**:

```python
#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import json
import os
import sys
from datetime import datetime

def get_server_info() -> dict:
    """サーバー情報を取得"""
    return {
        'timestamp': datetime.now().isoformat(),
        'server': os.environ.get('SERVER_SOFTWARE', 'Unknown'),
        'protocol': os.environ.get('SERVER_PROTOCOL', 'Unknown'),
        'method': os.environ.get('REQUEST_METHOD', 'Unknown'),
        'remote_addr': os.environ.get('REMOTE_ADDR', 'Unknown'),
        'query_string': os.environ.get('QUERY_STRING', ''),
    }

# Content-Typeヘッダーの出力（JSON）
print("Content-Type: application/json; charset=utf-8")
print()

# JSONレスポンスの出力
response = {
    'status': 'success',
    'data': get_server_info()
}

print(json.dumps(response, indent=2, ensure_ascii=False))
```

**アクセス方法**:

```bash
curl http://localhost:8080/cgi-bin/api.py
```

**レスポンス例**:

```json
{
  "status": "success",
  "data": {
    "timestamp": "2026-01-24T10:30:00.123456",
    "server": "JumboDogX Version 1.0",
    "protocol": "HTTP/1.1",
    "method": "GET",
    "remote_addr": "127.0.0.1",
    "query_string": ""
  }
}
```

### CGI環境変数

JumboDogXは、以下のCGI標準環境変数をスクリプトに渡します：

| 環境変数 | 説明 | 例 |
|----------|------|-----|
| SERVER_SOFTWARE | サーバーソフトウェア名 | `JumboDogX/1.0` |
| SERVER_NAME | サーバーのホスト名 | `example.com` |
| GATEWAY_INTERFACE | CGIバージョン | `CGI/1.1` |
| SERVER_PROTOCOL | HTTPプロトコル | `HTTP/1.1` |
| SERVER_PORT | サーバーポート | `80` |
| REQUEST_METHOD | リクエストメソッド | `GET`, `POST` |
| PATH_INFO | パス情報 | `/path/to/resource` |
| PATH_TRANSLATED | 変換されたパス | `/var/www/path/to/resource` |
| SCRIPT_NAME | スクリプト名 | `/cgi-bin/script.cgi` |
| QUERY_STRING | クエリ文字列 | `name=value&foo=bar` |
| REMOTE_ADDR | クライアントIPアドレス | `192.168.1.100` |
| REMOTE_HOST | クライアントホスト名 | `client.example.com` |
| AUTH_TYPE | 認証タイプ | `Basic` |
| REMOTE_USER | 認証ユーザー名 | `john` |
| CONTENT_TYPE | リクエストのContent-Type | `application/x-www-form-urlencoded` |
| CONTENT_LENGTH | リクエストのContent-Length | `1024` |

## SSI設定

### Web UIからの設定

#### ステップ1: SSI設定画面を開く

1. ブラウザで `http://localhost:5001` にアクセス
2. **Settings** → **HTTP/HTTPS** → **SSI** を選択

#### ステップ2: SSI設定を入力

以下の項目を設定します：

| 項目 | 説明 | 例 |
|------|------|-----|
| Enable SSI | SSIを有効化 | ✓ ON |
| SSI Extension | SSI拡張子 | `.shtml`, `.shtm` |

#### ステップ3: サーバーを再起動

設定を反映するため、JumboDogXを再起動します。

### appsettings.jsonでの設定

```json
{
  "Jdx": {
    "HttpServer": {
      "Ssi": {
        "Enabled": true,
        "Extensions": [".shtml", ".shtm", ".html"]
      }
    }
  }
}
```

**注意**: `.html` をSSI対象にすると、すべてのHTMLファイルがSSI処理されるため、パフォーマンスに影響する可能性があります。通常は `.shtml` のみを推奨します。

### SSIディレクティブ

#### 基本構文

SSIディレクティブは、HTMLコメント形式で記述します：

```html
<!--#command parameter="value" -->
```

#### include - ファイルのインクルード

外部ファイルを埋め込みます。

```html
<!--#include file="header.html" -->
<!--#include virtual="/includes/footer.html" -->
```

**違い**:
- `file`: 現在のディレクトリからの相対パス
- `virtual`: ドキュメントルートからの絶対パス

**例** (**/var/www/html/index.shtml**):

```html
<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title>SSI Example</title>
</head>
<body>
    <!--#include virtual="/includes/header.html" -->

    <main>
        <h1>Welcome to my site</h1>
        <p>This is the main content.</p>
    </main>

    <!--#include virtual="/includes/footer.html" -->
</body>
</html>
```

**/var/www/html/includes/header.html**:

```html
<header>
    <nav>
        <a href="/">Home</a> |
        <a href="/about.html">About</a> |
        <a href="/contact.html">Contact</a>
    </nav>
</header>
```

**/var/www/html/includes/footer.html**:

```html
<footer>
    <p>&copy; 2026 My Website. All rights reserved.</p>
    <p>Last modified: <!--#echo var="LAST_MODIFIED" --></p>
</footer>
```

#### echo - 変数の表示

環境変数やSSI変数を表示します。

```html
<!--#echo var="DATE_LOCAL" -->
<!--#echo var="LAST_MODIFIED" -->
<!--#echo var="DOCUMENT_NAME" -->
<!--#echo var="DOCUMENT_URI" -->
```

**利用可能な変数**:

| 変数 | 説明 | 例 |
|------|------|-----|
| DATE_LOCAL | 現在の日時（ローカル） | `Friday, 24-Jan-2026 10:30:00 JST` |
| DATE_GMT | 現在の日時（GMT） | `Friday, 24-Jan-2026 01:30:00 GMT` |
| LAST_MODIFIED | ファイルの最終更新日時 | `Thursday, 23-Jan-2026 15:00:00 JST` |
| DOCUMENT_NAME | ドキュメント名 | `index.shtml` |
| DOCUMENT_URI | ドキュメントのURI | `/index.shtml` |
| QUERY_STRING | クエリ文字列 | `id=123&page=2` |
| REMOTE_ADDR | クライアントIPアドレス | `192.168.1.100` |
| SERVER_NAME | サーバーのホスト名 | `example.com` |
| SERVER_PORT | サーバーポート | `80` |

**例**:

```html
<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title>SSI Variables</title>
</head>
<body>
    <h1>SSI Variables Example</h1>
    <ul>
        <li>Current Date: <!--#echo var="DATE_LOCAL" --></li>
        <li>Last Modified: <!--#echo var="LAST_MODIFIED" --></li>
        <li>Document Name: <!--#echo var="DOCUMENT_NAME" --></li>
        <li>Your IP: <!--#echo var="REMOTE_ADDR" --></li>
    </ul>
</body>
</html>
```

#### exec - コマンドの実行

外部コマンドを実行して結果を埋め込みます（セキュリティリスクあり）。

```html
<!--#exec cmd="date" -->
<!--#exec cgi="/cgi-bin/script.cgi" -->
```

**セキュリティ警告**: `exec` ディレクティブは任意のコマンド実行を許可するため、セキュリティリスクが高く、通常は無効化すべきです。

#### config - SSI設定

SSIの動作を設定します。

```html
<!--#config timefmt="%Y-%m-%d %H:%M:%S" -->
<!--#config errmsg="[Error occurred]" -->
```

**例**:

```html
<!--#config timefmt="%Y年%m月%d日 %H:%M:%S" -->
<p>最終更新: <!--#echo var="LAST_MODIFIED" --></p>
```

#### fsize - ファイルサイズの表示

ファイルのサイズを表示します。

```html
<!--#fsize file="document.pdf" -->
<!--#fsize virtual="/downloads/software.zip" -->
```

#### flastmod - 最終更新日時の表示

ファイルの最終更新日時を表示します。

```html
<!--#flastmod file="index.shtml" -->
<!--#flastmod virtual="/about.html" -->
```

### SSI実用例

#### 例1: 共通ヘッダー・フッター

複数のページで共通のヘッダーとフッターを使用します。

**/var/www/html/template.shtml**:

```html
<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title><!--#echo var="PAGE_TITLE" default="My Website" --></title>
    <link rel="stylesheet" href="/css/style.css">
</head>
<body>
    <!--#include virtual="/includes/header.html" -->

    <main>
        <!-- ページ固有のコンテンツ -->
    </main>

    <!--#include virtual="/includes/footer.html" -->
</body>
</html>
```

#### 例2: ナビゲーションメニュー

**/var/www/html/includes/nav.html**:

```html
<nav>
    <ul>
        <li><a href="/">Home</a></li>
        <li><a href="/about.shtml">About</a></li>
        <li><a href="/services.shtml">Services</a></li>
        <li><a href="/contact.shtml">Contact</a></li>
    </ul>
</nav>
```

**/var/www/html/index.shtml**:

```html
<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <title>Home</title>
</head>
<body>
    <!--#include virtual="/includes/nav.html" -->
    <h1>Welcome</h1>
    <p>This is the home page.</p>
</body>
</html>
```

#### 例3: 動的な著作権表示

**/var/www/html/includes/footer.html**:

```html
<!--#config timefmt="%Y" -->
<footer>
    <p>&copy; <!--#echo var="DATE_LOCAL" --> My Company. All rights reserved.</p>
    <p>Last updated: <!--#config timefmt="%Y-%m-%d" --><!--#echo var="LAST_MODIFIED" --></p>
</footer>
```

## セキュリティ注意事項

### CGIのセキュリティ

#### 1. 実行権限の最小化

CGIスクリプトは必要最小限の権限で実行してください。

```bash
# スクリプトの所有者を専用ユーザーに設定
chown www-data:www-data /var/www/cgi-bin/*.cgi
chmod 750 /var/www/cgi-bin/*.cgi
```

#### 2. 入力の検証

ユーザー入力を必ず検証してください。

```python
import cgi
import re

form = cgi.FieldStorage()
email = form.getvalue('email', '')

# メールアドレスの検証
if not re.match(r'^[\w\.-]+@[\w\.-]+\.\w+$', email):
    print("Content-Type: text/html")
    print()
    print("<h1>Invalid email address</h1>")
    exit(1)
```

#### 3. コマンドインジェクション対策

シェルコマンドを実行する際は、ユーザー入力を直接使用しないでください。

```python
# 危険な例（脆弱性あり）
import os
filename = form.getvalue('file')
os.system(f"cat {filename}")  # コマンドインジェクションの危険性

# 安全な例
import subprocess
import os.path

filename = form.getvalue('file')
# ファイル名の検証
if not os.path.basename(filename) == filename:
    # パストラバーサル攻撃を防ぐ
    exit(1)

# サブプロセスで安全に実行
result = subprocess.run(['cat', filename], capture_output=True, text=True)
print(result.stdout)
```

#### 4. エラーメッセージの制御

本番環境では詳細なエラーメッセージを表示しないでください。

```python
# 開発環境
cgitb.enable()  # 詳細なエラーメッセージ

# 本番環境
cgitb.enable(display=0, logdir="/var/log/cgi-errors")  # ログに記録のみ
```

#### 5. タイムアウトの設定

無限ループやハングアップを防ぐため、タイムアウトを設定してください。

```json
{
  "Cgi": {
    "Timeout": 30
  }
}
```

### SSIのセキュリティ

#### 1. exec ディレクティブの無効化

任意のコマンド実行を防ぐため、`exec` ディレクティブを無効化することを推奨します。

```json
{
  "Ssi": {
    "Enabled": true,
    "AllowExec": false
  }
}
```

#### 2. ファイルアクセスの制限

SSIでインクルードできるファイルをドキュメントルート内に制限してください。

```json
{
  "Ssi": {
    "Enabled": true,
    "AllowAbsolutePath": false
  }
}
```

#### 3. 拡張子の限定

SSI処理を特定の拡張子（`.shtml`）のみに限定してください。

```json
{
  "Ssi": {
    "Extensions": [".shtml"]  // .html は含めない
  }
}
```

## トラブルシューティング

### CGIスクリプトが動作しない

#### 問題1: "500 Internal Server Error"

**原因1**: スクリプトに実行権限がない

**解決策**:

```bash
chmod +x /var/www/cgi-bin/script.cgi
```

**原因2**: シバン（shebang）が間違っている

**解決策**:

スクリプトの1行目を確認：

```bash
#!/bin/bash  # 正しいインタープリタのパスを指定
```

インタープリタのパスを確認：

```bash
which python3  # /usr/bin/python3
which perl     # /usr/bin/perl
```

**原因3**: Content-Typeヘッダーが出力されていない

**解決策**:

スクリプトの最初に必ずContent-Typeヘッダーを出力：

```bash
echo "Content-Type: text/html"
echo ""
```

#### 問題2: スクリプトがタイムアウトする

**原因**: 処理に時間がかかりすぎている

**解決策**:

1. **タイムアウトを延長**

```json
{
  "Cgi": {
    "Timeout": 60
  }
}
```

2. **スクリプトを最適化**

処理時間を短縮するようにスクリプトを改善します。

#### 問題3: フォームデータが取得できない

**原因**: POST データの読み込みが正しくない

**解決策**:

Pythonの場合、`cgi.FieldStorage()`を使用：

```python
import cgi

form = cgi.FieldStorage()
name = form.getvalue('name', '')
```

Bashの場合、標準入力から読み込み：

```bash
read -n ${CONTENT_LENGTH} POST_DATA
```

### SSIが動作しない

#### 問題1: SSIディレクティブがそのまま表示される

**原因1**: SSIが有効化されていない

**解決策**:

```json
{
  "Ssi": {
    "Enabled": true
  }
}
```

**原因2**: ファイル拡張子が対象外

**解決策**:

ファイル拡張子を `.shtml` に変更するか、設定に追加：

```json
{
  "Ssi": {
    "Extensions": [".shtml", ".html"]
  }
}
```

#### 問題2: includeしたファイルが表示されない

**原因**: ファイルパスが間違っている

**解決策**:

1. **相対パスを確認** (`file`)

```html
<!--#include file="header.html" -->
```

現在のディレクトリからの相対パス。

2. **絶対パスを確認** (`virtual`)

```html
<!--#include virtual="/includes/header.html" -->
```

ドキュメントルートからの絶対パス。

3. **ファイルの存在を確認**

```bash
ls -la /var/www/html/includes/header.html
```

#### 問題3: 変数が表示されない

**原因**: 変数名が間違っている

**解決策**:

正しい変数名を使用：

```html
<!-- 正しい -->
<!--#echo var="DATE_LOCAL" -->

<!-- 間違い -->
<!--#echo var="date_local" -->
```

### ログの確認

問題が発生した場合は、ログを確認してください：

```bash
# CGIエラーログ
cat logs/cgi-error.log

# HTTPサーバーログ
cat logs/http-server.log | grep -i cgi
cat logs/http-server.log | grep -i ssi

# システムログ
tail -f /var/log/syslog  # Linux
tail -f /var/log/system.log  # macOS
```

## 関連ドキュメント

- [クイックスタート](getting-started.md) - 基本的な設定手順
- [詳細設定](configuration.md) - HTTPサーバーの詳細設定
- [トラブルシューティング](troubleshooting.md) - 問題解決

## まとめ

CGI/SSIを使用することで：

- **CGI**: フォーム処理、データベースアクセス、動的コンテンツの生成が可能
- **SSI**: ヘッダー・フッターの共通化、最終更新日時の表示など、軽量な動的処理が可能

セキュリティに注意しながら、適切に設定することで、動的なWebサイトを構築できます。
