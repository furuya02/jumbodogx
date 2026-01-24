# HTTPサーバー トラブルシューティング

このガイドでは、JumboDogXのHTTPサーバーでよくある問題と解決方法を説明します。

## 目次

- [サーバーが起動しない](#サーバーが起動しない)
- [ページが表示されない](#ページが表示されない)
- [403 Forbidden](#403-forbidden)
- [404 Not Found](#404-not-found)
- [500 Internal Server Error](#500-internal-server-error)
- [HTTPS接続エラー](#https接続エラー)
- [Virtual Hostが動作しない](#virtual-hostが動作しない)
- [パフォーマンス問題](#パフォーマンス問題)
- [ログの確認方法](#ログの確認方法)
- [デバッグ手順](#デバッグ手順)

## サーバーが起動しない

### 問題1: "Address already in use" エラー

**症状**: サーバー起動時に「ポートが既に使用されています」というエラーが表示される

```
Failed to bind to address 0.0.0.0:8080: Address already in use
```

**原因**: 指定したポートが既に他のプロセスで使用されている

**解決策**:

#### ポートの使用状況を確認

**macOS/Linux**:

```bash
# ポート8080を使用しているプロセスを確認
lsof -i :8080

# または
netstat -tuln | grep 8080
```

**Windows**:

```cmd
# ポート8080を使用しているプロセスを確認
netstat -ano | findstr :8080

# プロセスIDからプログラムを確認
tasklist /FI "PID eq 1234"
```

#### 解決方法

**方法1**: 他のプロセスを停止する

```bash
# プロセスを停止（macOS/Linux）
kill -9 <PID>

# プロセスを停止（Windows）
taskkill /PID <PID> /F
```

**方法2**: 別のポート番号を使用する

```json
{
  "HttpServer": {
    "Port": 8081  // 使用されていないポートに変更
  }
}
```

### 問題2: "Permission denied" エラー

**症状**: サーバー起動時に権限エラーが表示される

```
Failed to bind to address 0.0.0.0:80: Permission denied
```

**原因**: 1024以下のポート（80, 443など）は管理者権限が必要

**解決策**:

#### 方法1: 管理者権限で実行

**macOS/Linux**:

```bash
sudo dotnet run --project src/Jdx.WebUI
```

**Windows**:

PowerShellまたはコマンドプロンプトを「管理者として実行」で起動。

#### 方法2: 1024以上のポートを使用

```json
{
  "HttpServer": {
    "Port": 8080  // 1024以上のポートは管理者権限不要
  }
}
```

#### 方法3: ポートフォワーディングを設定（Linux）

```bash
# ポート80へのアクセスを8080にフォワード
sudo iptables -t nat -A PREROUTING -p tcp --dport 80 -j REDIRECT --to-port 8080
```

### 問題3: 設定ファイルエラー

**症状**: 設定ファイルの読み込みエラー

```
Failed to load configuration: Invalid JSON at line 15
```

**原因**: appsettings.jsonの構文エラー

**解決策**:

#### JSONの構文を確認

```bash
# JSONの妥当性を確認（jqが必要）
cat src/Jdx.WebUI/appsettings.json | jq .

# または、オンラインツールを使用
# https://jsonlint.com/
```

#### よくある構文エラー

```json
// 間違い: 最後の要素にカンマ
{
  "HttpServer": {
    "Port": 8080,
    "Enabled": true,  // ← この最後のカンマは不要
  }
}

// 正しい
{
  "HttpServer": {
    "Port": 8080,
    "Enabled": true
  }
}
```

### 問題4: "Document Root not found" エラー

**症状**: ドキュメントルートが見つからない

```
Document Root '/var/www/html' does not exist
```

**原因**: 指定したドキュメントルートが存在しない

**解決策**:

```bash
# ディレクトリを作成
mkdir -p /var/www/html

# 権限を設定
chmod 755 /var/www/html
```

## ページが表示されない

### 問題1: "Connection refused"

**症状**: ブラウザで接続できない

**原因**: サーバーが起動していない、または別のポートでリッスンしている

**解決策**:

#### サーバーの起動を確認

```bash
# プロセスを確認
ps aux | grep JumboDogX

# ポートのリッスン状態を確認
lsof -i :8080  # macOS/Linux
netstat -ano | findstr :8080  # Windows
```

#### サーバーを起動

```bash
cd /path/to/jumbodogx
dotnet run --project src/Jdx.WebUI
```

#### 正しいURLでアクセス

```
# サーバーがポート8080でリッスンしている場合
http://localhost:8080/
```

### 問題2: "ERR_CONNECTION_TIMED_OUT"

**症状**: 接続がタイムアウトする

**原因**: ファイアウォールでブロックされている

**解決策**:

#### ファイアウォールの設定を確認

**macOS**:

```bash
# アプリケーションファイアウォールの状態を確認
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --getglobalstate

# JumboDogXを許可
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --add /path/to/jumbodogx
```

**Linux (ufw)**:

```bash
# ファイアウォールの状態を確認
sudo ufw status

# ポート8080を許可
sudo ufw allow 8080/tcp
```

**Linux (firewalld)**:

```bash
# ファイアウォールの状態を確認
sudo firewall-cmd --state

# ポート8080を許可
sudo firewall-cmd --permanent --add-port=8080/tcp
sudo firewall-cmd --reload
```

**Windows**:

```powershell
# ファイアウォールルールを追加
New-NetFirewallRule -DisplayName "JumboDogX HTTP" -Direction Inbound -Protocol TCP -LocalPort 8080 -Action Allow
```

### 問題3: ブラウザにエラーメッセージが表示されない

**症状**: ページが真っ白、またはブラウザが「空のレスポンス」と表示

**原因**: サーバーが正常にレスポンスを返していない

**解決策**:

#### ログを確認

```bash
# エラーログを確認
tail -f logs/error.log

# HTTPサーバーログを確認
tail -f logs/http-server.log
```

#### curlでテスト

```bash
# HTTPレスポンスを確認
curl -v http://localhost:8080/

# レスポンスヘッダーのみ取得
curl -I http://localhost:8080/
```

## 403 Forbidden

### 問題1: ファイルのアクセス権限

**症状**: 特定のファイルやディレクトリにアクセスすると403エラー

**原因**: ファイルやディレクトリに読み取り権限がない

**解決策**:

```bash
# ファイルの権限を確認
ls -la /var/www/html/

# 読み取り権限を付与
chmod 644 /var/www/html/index.html  # ファイル
chmod 755 /var/www/html/  # ディレクトリ

# 再帰的に権限を設定
chmod -R 755 /var/www/html/
```

**推奨権限**:
- ディレクトリ: `755` (rwxr-xr-x)
- HTMLファイル: `644` (rw-r--r--)
- CGIスクリプト: `755` (rwxr-xr-x)

### 問題2: ACL（アクセス制御リスト）

**症状**: 特定のIPアドレスからアクセスすると403エラー

**原因**: ACLでアクセスが制限されている

**解決策**:

#### ACL設定を確認

```json
{
  "HttpServer": {
    "Acl": {
      "Rules": [
        {
          "Path": "/*",
          "Allow": ["192.168.1.0/24"],  // このネットワークのみ許可
          "Deny": ["*"]  // その他は拒否
        }
      ]
    }
  }
}
```

#### 自分のIPアドレスを確認

```bash
# 自分のIPアドレスを確認
curl ifconfig.me

# または
ip addr show  # Linux
ipconfig  # Windows
ifconfig  # macOS
```

#### ACLルールを修正

```json
{
  "Acl": {
    "Rules": [
      {
        "Path": "/*",
        "Allow": ["192.168.1.0/24", "203.0.113.50"],  // 特定のIPを追加
        "Deny": ["*"]
      }
    ]
  }
}
```

### 問題3: ディレクトリリスティング

**症状**: ディレクトリにアクセスすると403エラー

**原因**: `index.html` が存在せず、ディレクトリリスティングが無効

**解決策**:

#### 方法1: index.htmlを作成

```bash
echo "<h1>Welcome</h1>" > /var/www/html/index.html
```

#### 方法2: ディレクトリリスティングを有効化

```json
{
  "HttpServer": {
    "UseDirectoryEnum": true
  }
}
```

**注意**: 本番環境ではセキュリティリスクがあるため、通常は無効にします。

### 問題4: 認証エラー

**症状**: 認証が設定されているパスで403エラー

**原因**: 認証情報が間違っている、または設定が不適切

**解決策**:

```json
{
  "Authentication": {
    "Type": "Basic",
    "Users": [
      {
        "Username": "user",
        "Password": "correct-password",
        "Paths": ["/secure/*"]  // パスを確認
      }
    ]
  }
}
```

## 404 Not Found

### 問題1: ファイルが存在しない

**症状**: アクセスしようとしたファイルが見つからない

**原因**: ファイルが存在しない、またはパスが間違っている

**解決策**:

#### ファイルの存在を確認

```bash
# ファイルが存在するか確認
ls -la /var/www/html/index.html

# ディレクトリ内のファイル一覧
ls -la /var/www/html/
```

#### パスを確認

```
# アクセスURL
http://localhost:8080/page.html

# 実際のファイルパス
/var/www/html/page.html
          ↑
    DocumentRoot
```

#### ファイル名の大文字小文字を確認（Linux/macOS）

Linux/macOSはファイル名が大文字小文字を区別します：

```bash
# 間違い
http://localhost:8080/Index.html

# 正しい
http://localhost:8080/index.html
```

### 問題2: Document Rootの設定ミス

**症状**: ファイルは存在するが404エラー

**原因**: Document Rootの設定が間違っている

**解決策**:

```json
{
  "HttpServer": {
    "DocumentRoot": "/var/www/html"  // 正しいパスを指定
  }
}
```

#### 絶対パスを使用

相対パスではなく、絶対パスを使用してください：

```json
// 間違い
{
  "DocumentRoot": "./www/html"
}

// 正しい
{
  "DocumentRoot": "/var/www/html"
}
```

### 問題3: Welcome File の設定

**症状**: ディレクトリにアクセスすると404エラー

**原因**: Welcome File（デフォルトファイル）が設定されていない、または存在しない

**解決策**:

```json
{
  "HttpServer": {
    "WelcomeFileName": "index.html"
  }
}
```

#### index.htmlを作成

```bash
echo "<h1>Welcome</h1>" > /var/www/html/index.html
```

## 500 Internal Server Error

### 問題1: CGIスクリプトエラー

**症状**: CGIスクリプトにアクセスすると500エラー

**原因**: CGIスクリプトの実行エラー

**解決策**:

#### 実行権限を確認

```bash
chmod +x /var/www/cgi-bin/script.cgi
```

#### シバン（shebang）を確認

```bash
#!/bin/bash  # 正しいインタープリタのパスを指定
```

#### Content-Typeヘッダーを確認

CGIスクリプトは必ずContent-Typeヘッダーを出力：

```bash
#!/bin/bash
echo "Content-Type: text/html"
echo ""
echo "<h1>Hello</h1>"
```

#### CGIエラーログを確認

```bash
tail -f logs/cgi-error.log
```

### 問題2: サーバー内部エラー

**症状**: 特定のページやリクエストで500エラー

**原因**: サーバーの内部エラー

**解決策**:

#### エラーログを確認

```bash
tail -f logs/error.log
tail -f logs/http-server.log
```

#### スタックトレースを確認

ログにスタックトレースが記録されている場合、エラーの原因を特定できます。

#### サーバーを再起動

```bash
# サーバーを停止
Ctrl+C

# サーバーを起動
dotnet run --project src/Jdx.WebUI
```

### 問題3: 設定エラー

**症状**: 特定の設定を変更後に500エラー

**原因**: 設定が不正、または矛盾している

**解決策**:

#### 設定ファイルを確認

```bash
# JSONの構文を確認
cat src/Jdx.WebUI/appsettings.json | jq .
```

#### デフォルト設定に戻す

```bash
# 設定ファイルのバックアップを復元
cp src/Jdx.WebUI/appsettings.json.backup src/Jdx.WebUI/appsettings.json
```

## HTTPS接続エラー

### 問題1: "この接続ではプライバシーが保護されません"

**症状**: HTTPSページにアクセスするとブラウザで警告が表示される

**原因**: 自己署名証明書を使用している

**解決策**:

#### 開発環境の場合

証明書を信頼済みストアに追加（[SSL/TLS設定ガイド](ssl-tls.md)参照）：

```bash
# .NET開発証明書を信頼
dotnet dev-certs https --trust
```

#### 本番環境の場合

Let's Encryptなどで正規の証明書を取得（[SSL/TLS設定ガイド](ssl-tls.md)参照）。

### 問題2: "証明書のホスト名が一致しません"

**症状**: 証明書のホスト名が一致しないエラー

**原因**: 証明書のCN/SANとアクセスしているドメインが異なる

**解決策**:

#### 証明書の内容を確認

```bash
openssl pkcs12 -in certificate.pfx -nodes -passin pass:yourpassword | \
  openssl x509 -noout -text | grep -A1 "Subject:"
```

#### 正しいドメイン名でアクセス

証明書に含まれるドメイン名でアクセスしてください。

#### 証明書を再作成

正しいドメイン名を含む証明書を作成（[SSL/TLS設定ガイド](ssl-tls.md)参照）。

### 問題3: "証明書が読み込めません"

**症状**: サーバー起動時に証明書エラー

```
Failed to load certificate: The system cannot find the file specified
```

**原因**: 証明書ファイルが見つからない、またはパスワードが間違っている

**解決策**:

#### ファイルパスを確認

```bash
# 証明書ファイルの存在を確認
ls -la /path/to/certificate.pfx
```

#### 絶対パスを使用

```json
{
  "CertificateFile": "/path/to/certificate.pfx"  // 絶対パス
}
```

#### パスワードを確認

```bash
# パスワードが正しいか確認
openssl pkcs12 -in certificate.pfx -nodes -passin pass:yourpassword
```

### 問題4: "混在コンテンツ" 警告

**症状**: HTTPSページでHTTPリソースを読み込むと警告

**原因**: HTTPSページ内でHTTPのリソースを参照している

**解決策**:

#### すべてのリソースをHTTPSで読み込む

```html
<!-- 修正前 -->
<script src="http://example.com/script.js"></script>

<!-- 修正後 -->
<script src="https://example.com/script.js"></script>

<!-- または相対パス -->
<script src="/script.js"></script>
```

## Virtual Hostが動作しない

### 問題1: デフォルトサイトが表示される

**症状**: Virtual Hostを設定したのに、常にデフォルトサイトが表示される

**原因**: Hostヘッダーがマッチしていない

**解決策**:

#### Hostヘッダーを確認

ブラウザの開発者ツールでリクエストヘッダーを確認：

```
GET / HTTP/1.1
Host: example.com
```

#### Virtual Host設定を確認

```json
{
  "VirtualHosts": [
    {
      "Host": "example.com:80",  // Hostヘッダーと一致させる
      "Settings": {
        "DocumentRoot": "/var/www/example.com"
      }
    }
  ]
}
```

#### hostsファイルを確認（ローカル開発）

```bash
# /etc/hosts (macOS/Linux)
127.0.0.1 example.com

# C:\Windows\System32\drivers\etc\hosts (Windows)
127.0.0.1 example.com
```

### 問題2: ポート番号の不一致

**症状**: カスタムポートでVirtual Hostが動作しない

**原因**: Hostヘッダーのポート番号が設定と一致していない

**解決策**:

```json
{
  "VirtualHosts": [
    {
      "Host": "example.com:8080",  // ポート番号を明示的に指定
      "Settings": {
        "Port": 8080
      }
    }
  ]
}
```

アクセス時もポート番号を含める：

```
http://example.com:8080/
```

### 問題3: HTTPSのVirtual Host

**症状**: HTTPSでVirtual Hostが動作しない

**原因**: 証明書が設定されていない、またはプロトコルが間違っている

**解決策**:

```json
{
  "VirtualHosts": [
    {
      "Host": "example.com:443",
      "Settings": {
        "Protocol": "HTTPS",  // HTTPSを指定
        "CertificateFile": "/path/to/certificate.pfx",
        "CertificatePassword": "password"
      }
    }
  ]
}
```

## パフォーマンス問題

### 問題1: ページの読み込みが遅い

**症状**: ページの表示に時間がかかる

**原因**: Keep-Aliveが無効、または接続数が不足

**解決策**:

#### Keep-Aliveを有効化

```json
{
  "HttpServer": {
    "UseKeepAlive": true,
    "KeepAliveTimeout": 5
  }
}
```

#### 最大接続数を増やす

```json
{
  "HttpServer": {
    "MaxConnections": 300
  }
}
```

#### ETagを有効化

```json
{
  "HttpServer": {
    "UseEtag": true
  }
}
```

### 問題2: 大きなファイルのダウンロードが遅い

**症状**: 大きなファイルのダウンロードに時間がかかる

**原因**: Range Requestsが無効

**解決策**:

```json
{
  "HttpServer": {
    "UseRangeRequests": true,
    "MaxRangeCount": 20
  }
}
```

### 問題3: CGIスクリプトが遅い

**症状**: CGIスクリプトの実行に時間がかかる

**原因**: スクリプトの処理が重い

**解決策**:

#### タイムアウトを延長

```json
{
  "Cgi": {
    "Timeout": 60
  }
}
```

#### スクリプトを最適化

処理時間を短縮するようにスクリプトを改善。

### 問題4: メモリ使用量が多い

**症状**: サーバーのメモリ使用量が増加し続ける

**原因**: メモリリーク、またはキャッシュの肥大化

**解決策**:

#### サーバーを定期的に再起動

```bash
# Cronで定期的に再起動（毎日午前3時）
0 3 * * * systemctl restart jumbodogx
```

#### ログを確認

```bash
# メモリ使用量を監視
top  # macOS/Linux
htop  # Linux（より詳細）
```

## ログの確認方法

### ログファイルの場所

JumboDogXのログファイルは通常、以下の場所に保存されます：

```
jumbodogx/logs/
├── http-server.log    # HTTPサーバーのアクセスログ
├── error.log          # エラーログ
├── cgi-error.log      # CGIエラーログ
└── webdav.log         # WebDAVログ
```

### ログの確認コマンド

#### リアルタイムでログを監視

```bash
# エラーログを監視
tail -f logs/error.log

# HTTPサーバーログを監視
tail -f logs/http-server.log

# すべてのログを監視
tail -f logs/*.log
```

#### 最近のエラーを確認

```bash
# 最新100行を表示
tail -n 100 logs/error.log

# エラーを含む行のみ表示
grep -i error logs/http-server.log

# 特定の日時のログを表示
grep "2026-01-24 10:" logs/http-server.log
```

#### ログファイルのサイズを確認

```bash
ls -lh logs/
```

#### ログファイルのローテーション

ログファイルが肥大化する場合は、定期的に削除またはアーカイブ：

```bash
# 古いログをアーカイブ
tar czf logs-archive-$(date +%Y%m%d).tar.gz logs/*.log

# ログファイルをクリア
> logs/http-server.log
> logs/error.log
```

### ログレベルの設定

ログの詳細度を調整できます：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "JumboDogX.Http": "Debug"
    }
  }
}
```

**ログレベル**:
- `Trace`: 最も詳細（デバッグ用）
- `Debug`: デバッグ情報
- `Information`: 一般的な情報（推奨）
- `Warning`: 警告
- `Error`: エラーのみ
- `Critical`: 重大なエラーのみ

## デバッグ手順

### ステップ1: 問題を特定する

1. **症状を確認**
   - エラーメッセージは表示されるか
   - どのページで発生するか
   - いつから発生したか

2. **再現性を確認**
   - 常に発生するか
   - 特定の条件で発生するか

### ステップ2: ログを確認する

```bash
# エラーログを確認
tail -f logs/error.log

# HTTPサーバーログを確認
tail -f logs/http-server.log
```

### ステップ3: 設定を確認する

```bash
# 設定ファイルの構文を確認
cat src/Jdx.WebUI/appsettings.json | jq .

# 設定内容を確認
grep -A 10 "HttpServer" src/Jdx.WebUI/appsettings.json
```

### ステップ4: ネットワークを確認する

```bash
# ポートのリッスン状態を確認
lsof -i :8080  # macOS/Linux
netstat -ano | findstr :8080  # Windows

# ファイアウォールを確認
sudo ufw status  # Linux
```

### ステップ5: curlでテストする

```bash
# HTTPリクエストを送信
curl -v http://localhost:8080/

# HTTPSリクエストを送信（証明書検証スキップ）
curl -k -v https://localhost:443/

# Hostヘッダーを指定
curl -H "Host: example.com" http://localhost:8080/

# POSTリクエストを送信
curl -X POST -d "name=value" http://localhost:8080/cgi-bin/form.cgi
```

### ステップ6: ブラウザの開発者ツールを使用

1. **開発者ツールを開く**（F12キー）

2. **Networkタブを確認**
   - HTTPステータスコード
   - リクエストヘッダー
   - レスポンスヘッダー
   - レスポンス内容

3. **Consoleタブを確認**
   - JavaScriptエラー
   - 混在コンテンツの警告

### ステップ7: 最小構成でテスト

問題を切り分けるため、最小構成でテスト：

```json
{
  "Jdx": {
    "HttpServer": {
      "Enabled": true,
      "Protocol": "HTTP",
      "Port": 8080,
      "BindAddress": "127.0.0.1",
      "DocumentRoot": "/tmp/test",
      "WelcomeFileName": "index.html"
    }
  }
}
```

```bash
# テスト用ディレクトリとファイルを作成
mkdir -p /tmp/test
echo "<h1>Test</h1>" > /tmp/test/index.html

# サーバーを起動
dotnet run --project src/Jdx.WebUI

# ブラウザでアクセス
# http://localhost:8080/
```

### ステップ8: バージョンを確認

```bash
# JumboDogXのバージョンを確認
dotnet run --project src/Jdx.WebUI --version

# .NET Runtimeのバージョンを確認
dotnet --info
```

## よくある質問（FAQ）

### Q1: HTTPとHTTPSを同時に使用できますか？

A: はい、可能です。メインサーバーとVirtual Hostを組み合わせて設定してください。

```json
{
  "HttpServer": {
    "Protocol": "HTTP",
    "Port": 80,
    "VirtualHosts": [
      {
        "Host": "example.com:443",
        "Settings": {
          "Protocol": "HTTPS",
          "CertificateFile": "/path/to/certificate.pfx"
        }
      }
    ]
  }
}
```

### Q2: 複数のポートでリッスンできますか？

A: Virtual Hostを使用することで、複数のポートでリッスンできます。

```json
{
  "HttpServer": {
    "Port": 8080,
    "VirtualHosts": [
      {
        "Host": "example.com:8081",
        "Settings": {
          "Port": 8081
        }
      }
    ]
  }
}
```

### Q3: ログファイルの保存先を変更できますか？

A: appsettings.jsonでログの設定を変更できます。

```json
{
  "Logging": {
    "File": {
      "Path": "/var/log/jumbodogx/"
    }
  }
}
```

### Q4: パフォーマンスを改善する方法は？

A: 以下の設定を有効にしてください：

```json
{
  "HttpServer": {
    "UseKeepAlive": true,
    "UseEtag": true,
    "UseRangeRequests": true,
    "MaxConnections": 300
  }
}
```

### Q5: セキュリティを強化する方法は？

A: 以下を実施してください：

1. HTTPSを使用
2. 認証を設定
3. ACLでIPアドレスを制限
4. Serverヘッダーを無効化
5. セキュリティヘッダーを追加

```json
{
  "HttpServer": {
    "Protocol": "HTTPS",
    "ServerHeader": "",
    "CustomHeaders": {
      "X-Content-Type-Options": "nosniff",
      "X-Frame-Options": "SAMEORIGIN"
    }
  }
}
```

## サポート

問題が解決しない場合は、以下を参照してください：

### ドキュメント

- [クイックスタート](getting-started.md)
- [詳細設定](configuration.md)
- [Virtual Host設定](virtual-hosts.md)
- [SSL/TLS設定](ssl-tls.md)
- [CGI/SSI設定](cgi-ssi.md)
- [WebDAV設定](webdav.md)

### GitHubリポジトリ

- **リポジトリ**: https://github.com/furuya02/jumbodogx
- **Issue報告**: https://github.com/furuya02/jumbodogx/issues
- **ディスカッション**: https://github.com/furuya02/jumbodogx/discussions

### Issueを報告する際に含めるべき情報

1. **環境情報**
   - OS（Windows/macOS/Linux）とバージョン
   - .NET Runtimeのバージョン
   - JumboDogXのバージョン

2. **問題の詳細**
   - 症状（エラーメッセージ、スクリーンショット）
   - 再現手順
   - 期待される動作

3. **設定ファイル**
   - appsettings.json（パスワードなどは削除）

4. **ログ**
   - エラーログ
   - HTTPサーバーログ

## まとめ

このトラブルシューティングガイドでは、JumboDogX HTTPサーバーでよくある問題と解決方法を説明しました。

### 問題解決の基本手順

1. **ログを確認** - エラーメッセージから原因を特定
2. **設定を確認** - 構文エラーや設定ミスをチェック
3. **最小構成でテスト** - 問題を切り分け
4. **ドキュメントを参照** - 詳細な設定方法を確認
5. **コミュニティに相談** - GitHubで質問やIssue報告

適切なデバッグ手順を踏むことで、ほとんどの問題は解決できます。
