# Proxyサーバー クイックスタートガイド

このガイドでは、JumboDogXのProxyサーバーを最短で起動し、HTTPプロキシとして使用する方法を説明します。

## 前提条件

- JumboDogXがインストールされていること
- .NET 9 Runtimeがインストールされていること

詳細は[インストール手順](../common/installation.md)を参照してください。

## Proxyサーバーとは？

Proxyサーバーは、クライアントとWebサーバーの間に立って、HTTPリクエストを中継するサービスです。

### 主な機能

- **キャッシュ**: Webページをキャッシュして高速化
- **URLフィルタリング**: 特定のURLへのアクセスを制限
- **アクセス制御**: IPアドレスベースのアクセス制御
- **ログ記録**: Webアクセスのログを記録

### 用途

- **ローカル開発環境**: Webアクセスのデバッグ
- **テスト環境**: キャッシュ動作のテスト
- **学習・教育**: Proxyの仕組みを理解する

**注意**: JumboDogXのProxyサーバーは、ローカルテスト環境専用です。本番環境での使用は推奨されません。

## ステップ1: JumboDogXの起動

### 起動方法

```bash
cd /path/to/jumbodogx
dotnet run --project src/Jdx.WebUI
```

### 起動確認

ターミナルに以下のようなメッセージが表示されれば成功です：

```
Now listening on: http://localhost:5001
Application started. Press Ctrl+C to shut down.
```

## ステップ2: Web管理画面にアクセス

ブラウザで以下のURLを開きます：

```
http://localhost:5001
```

![ダッシュボード画面](images/dashboard-top.png)
*JumboDogXのダッシュボード画面*

## ステップ3: Proxy基本設定

### 3-1. Proxy設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **Proxy** セクションを展開
3. **General** をクリック

![Proxy設定画面](images/proxy-general-menu.png)
*サイドメニューからProxy > Generalを選択*

### 3-2. 基本設定を入力

以下の項目を設定します：

| 項目 | 設定値 | 説明 |
|------|--------|------|
| Enable Server | ✓ ON | Proxyサーバーを有効化 |
| Bind Address | `0.0.0.0` または `127.0.0.1` | すべてのネットワークインターフェースでリッスン（`0.0.0.0`）またはローカルのみ（`127.0.0.1`） |
| Port | `8080` | Proxy標準ポート（3128、8888なども一般的） |
| Timeout | `60` | リクエストのタイムアウト時間（秒） |
| Max Connections | `100` | 最大同時接続数 |

**ポートについて**:
- **8080番**: 一般的なProxyポート
- **3128番**: Squidなどのプロキシで使用される標準ポート
- **8888番**: テスト用代替ポート

![Proxy基本設定](images/proxy-general-settings.png)
*Proxy Generalの設定画面*

### 3-3. 設定を保存

1. **Save Settings** ボタンをクリック
2. 成功メッセージが表示されることを確認
3. **設定は即座に反映されます**（再起動不要）

## ステップ4: キャッシュ設定

Webページをキャッシュして、アクセスを高速化します。

### 4-1. Cache設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **Proxy** セクションを展開
3. **Cache Configuration** をクリック

![Proxy Cache設定画面](images/proxy-cache-menu.png)
*サイドメニューからProxy > Cache Configurationを選択*

### 4-2. キャッシュ設定を入力

| 項目 | 設定値 | 説明 |
|------|--------|------|
| Enable Cache | ✓ ON | キャッシュを有効化 |
| Cache Directory | `data/proxy/cache` | キャッシュの保存先ディレクトリ |
| Max Cache Size | `100` | 最大キャッシュサイズ（MB） |
| Max Object Size | `10` | キャッシュする最大オブジェクトサイズ（MB） |
| Cache Expiration | `3600` | キャッシュの有効期限（秒、デフォルト1時間） |

![Proxyキャッシュ設定](images/proxy-cache-config.png)
*Proxyキャッシュ設定画面*

### 4-3. 設定を保存

1. **Save Settings** ボタンをクリック
2. キャッシュディレクトリが自動的に作成されます

## ステップ5: URLフィルタリング設定（オプション）

特定のURLへのアクセスを許可または拒否できます。

### 5-1. URL Filtering設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **Proxy** セクションを展開
3. **URL Filtering** をクリック

![Proxy URL Filtering設定画面](images/proxy-urlfilter-menu.png)
*サイドメニューからProxy > URL Filteringを選択*

### 5-2. URLフィルタの追加

1. **Filter Mode** を選択：
   - **Allow Mode (Allowlist)**: リストに登録されたURLのみ許可
   - **Deny Mode (Denylist)**: リストに登録されたURLのみ拒否

2. **Add URL Filter** ボタンをクリック
3. 追加されたエントリに以下を入力：
   - **Name**: `Block Social Media`
   - **URL Pattern**: `*.facebook.com` （ワイルドカードが使用可能）

4. **Save Settings** ボタンをクリック

**URLパターンの例**:
- `*.example.com` - example.comとそのサブドメイン全て
- `example.com/admin/*` - example.comの/admin/配下全て
- `*.google.*` - Googleの全ドメイン

![URLフィルタ設定](images/proxy-urlfilter-config.png)
*URLフィルタリング設定*

## ステップ6: ブラウザのProxy設定

JumboDogXのProxyサーバーを使用するように、ブラウザを設定します。

### 6-1. Google Chromeの設定

1. **設定** > **詳細設定** > **システム** を開く
2. **パソコンのプロキシ設定を開く** をクリック
3. OSのプロキシ設定画面が開きます

**Windows**:
1. **プロキシサーバーを使う** をONにする
2. **アドレス**: `localhost` または `127.0.0.1`
3. **ポート**: `8080`
4. **保存**

**macOS**:
1. **Webプロキシ（HTTP）** にチェック
2. **Webプロキシサーバー**: `localhost`
3. **ポート**: `8080`
4. **OK**

![Chrome Proxy設定](images/proxy-chrome-config.png)
*Google ChromeのProxy設定*

### 6-2. Firefoxの設定

1. **設定** > **ネットワーク設定** を開く
2. **接続設定...** ボタンをクリック
3. **手動でプロキシを設定する** を選択
4. **HTTPプロキシ**: `localhost`、ポート: `8080`
5. **すべてのプロトコルでこのプロキシサーバーを使用する** にチェック
6. **OK**

![Firefox Proxy設定](images/proxy-firefox-config.png)
*FirefoxのProxy設定*

### 6-3. curlコマンドで確認

ブラウザを使わずに、curlコマンドでProxyをテストできます。

```bash
# Proxy経由でWebページを取得
curl -x http://localhost:8080 http://example.com

# HTTPSサイトにアクセス（SSL証明書検証をスキップ）
curl -x http://localhost:8080 -k https://example.com

# ヘッダー情報を表示
curl -x http://localhost:8080 -I http://example.com
```

**期待される出力**:
```
HTTP/1.1 200 OK
Content-Type: text/html
...
```

![curl Proxyテスト](images/proxy-curl-test.png)
*curlコマンドでのProxyテスト*

## ステップ7: 動作確認

### 7-1. Dashboardで確認

1. サイドメニューから **Dashboard** をクリック
2. **Proxy Server** セクションでサーバーのステータスを確認
3. サーバーが起動していることを確認
4. **Total Requests** （総リクエスト数）を確認
5. **Cache Hit Rate** （キャッシュヒット率）を確認

![Dashboard Proxy](images/dashboard-proxy.png)
*DashboardのProxy Serverセクション*

### 7-2. Webページへのアクセス

ブラウザでWebページにアクセスして、Proxy経由でアクセスできることを確認します。

1. ブラウザで `http://example.com` にアクセス
2. ページが表示されることを確認
3. 再度同じページにアクセスして、キャッシュから表示されることを確認（高速化）

### 7-3. ログの確認

#### Web UIでログを確認

1. サイドメニューから **Logs** をクリック
2. **Category** ドロップダウンから **Jdx.Servers.Proxy** を選択
3. Proxyリクエストのログが表示されます

![Proxyログ](images/proxy-logs.png)
*Proxyサーバーのログ*

#### ログファイルで確認

```bash
# Proxyサーバーのログを確認
cat logs/jumbodogx-*.log | jq 'select(.SourceContext == "Jdx.Servers.Proxy.ProxyServer")'

# リクエストのログのみ表示
cat logs/jumbodogx-*.log | jq 'select(.["@mt"] | test("Request received"))'

# アクセスされたURLを一覧表示
cat logs/jumbodogx-*.log | jq -r 'select(.Url) | .Url' | sort | uniq
```

## よくある問題と解決方法

### Proxyサーバーに接続できない

**症状**: ブラウザで "プロキシサーバーに接続できません" エラー

**原因1: サーバーが起動していない**

**解決策**:
1. Dashboard > Proxy Serverでステータスを確認
2. Settings > Proxy > Generalで "Enable Server" がONか確認

**原因2: ポートが間違っている**

**解決策**:
1. Settings > Proxy > Generalでポート番号を確認
2. ブラウザのProxy設定で同じポート番号を使用しているか確認

**原因3: ポートが使用中**

```bash
# ポートの使用状況を確認（macOS/Linux）
lsof -i :8080

# ポートの使用状況を確認（Windows）
netstat -ano | findstr :8080
```

**解決策**: 別のポート番号（例：3128、8888）を使用する

### Webページが表示されない

**症状**: Proxy設定後、Webページが表示されない

**原因1: URLフィルタリングで拒否されている**

**解決策**:
1. Settings > Proxy > URL Filtering を確認
2. Filter Modeと登録されたURLパターンを確認
3. テスト時はフィルタリングを無効化してみる

**原因2: タイムアウトが短すぎる**

**解決策**:
1. Settings > Proxy > General を確認
2. Timeoutの値を増やす（例：60秒 → 120秒）

### HTTPSサイトにアクセスできない

**症状**: HTTPサイトは表示されるが、HTTPSサイトが表示されない

**原因**: ProxyはHTTPSのトンネリング（CONNECT method）に対応している必要があります

**注意**: JumboDogXのProxyサーバーは、基本的なHTTP Proxyです。HTTPSサイトへのアクセスは制限がある場合があります。

**解決策（テスト環境）**:
- HTTP以外のサイト（HTTPSなど）は、Proxy設定を無効化して直接アクセス
- または、ブラウザのProxy設定で「次のアドレスにはプロキシを使用しない」に `*.google.com` などを追加

### キャッシュが機能しない

**症状**: 同じページに何度アクセスしてもキャッシュされない

**原因**: キャッシュが無効化されているか、キャッシュ対象外

**解決策**:
1. Settings > Proxy > Cache Configuration を確認
2. "Enable Cache" がONか確認
3. Max Cache Sizeが十分か確認
4. ログでキャッシュヒット/ミスを確認

### URLフィルタリングが機能しない

**症状**: ブロックしたいURLにアクセスできてしまう

**原因**: URLパターンが正しくない

**解決策**:
1. URLパターンを確認（ワイルドカード `*` の使用方法）
2. 正規表現ではなく、ワイルドカードパターンを使用
3. 例：`*.example.com` は `example.com` とそのサブドメイン全て

## 次のステップ

基本的なProxyサーバーが動作したら、以下のドキュメントで更に機能を拡張できます：

- [キャッシュ設定](cache-configuration.md) - キャッシュの詳細設定
- [URLフィルタリング](url-filtering.md) - URLフィルタリングの詳細
- [トラブルシューティング](troubleshooting.md) - よくある問題の解決方法

## 実用的な使用例

### 例1: ローカル開発環境でのWebアクセスログ記録

開発中のWebアプリケーションのHTTPリクエストをログ記録：

**設定**:
- Port: `8080`
- Bind Address: `127.0.0.1` （ローカルのみ）
- Enable Cache: OFF （開発時はキャッシュ無効）

**ブラウザ設定**:
- HTTP Proxy: `localhost:8080`

**ログ確認**:
```bash
# アクセスされたURLを確認
cat logs/jumbodogx-*.log | jq -r 'select(.Url) | .Url'
```

### 例2: 特定サイトのアクセスブロック

開発環境で気が散るサイト（SNSなど）をブロック：

**URLフィルタリング設定**:
- Filter Mode: Deny Mode
- URLパターン:
  - `*.facebook.com`
  - `*.twitter.com`
  - `*.instagram.com`
  - `*.youtube.com`

### 例3: キャッシュによる高速化テスト

Webアプリケーションのキャッシュ動作をテスト：

**キャッシュ設定**:
- Enable Cache: ON
- Max Cache Size: 100MB
- Cache Expiration: 3600秒（1時間）

**テスト手順**:
1. 初回アクセス時の読み込み時間を計測
2. 2回目のアクセス時の読み込み時間を計測（キャッシュヒット）
3. Dashboard でCache Hit Rateを確認

### 例4: APIリクエストのデバッグ

WebアプリケーションからのAPIリクエストを監視：

**設定**:
- Port: `8080`
- ログレベル: Debug

**アプリケーション設定**:
```javascript
// Node.js axios の例
const axios = require('axios');

axios.defaults.proxy = {
  host: 'localhost',
  port: 8080
};

// APIリクエスト
axios.get('https://api.example.com/users')
  .then(response => console.log(response.data));
```

**ログ確認**:
```bash
# APIリクエストを確認
cat logs/jumbodogx-*.log | jq 'select(.Url | test("api.example.com"))'
```

## まとめ

このガイドでは、以下の手順を完了しました：

✓ JumboDogXの起動とWeb管理画面へのアクセス
✓ Proxy基本設定（ポート、Bind Address）
✓ キャッシュ設定
✓ URLフィルタリング設定（オプション）
✓ ブラウザのProxy設定
✓ 動作確認（Webアクセス、キャッシュ、ログ）

これで、JumboDogXのProxyサーバーが起動し、HTTPプロキシとして使用できるようになりました！

### 重要なポイント

- Proxyサーバーは一般的なHTTP Proxyとして動作
- キャッシュ機能でWebアクセスを高速化
- URLフィルタリングで特定のサイトへのアクセスを制御
- ログ記録でWebアクセスを監視
- 設定は即座に反映されるため、再起動は不要

### セキュリティの注意

- JumboDogXのProxyサーバーは、ローカルテスト環境専用です
- インターネットに直接公開しないでください
- URLフィルタリングは完全なセキュリティ対策ではありません
- HTTPSサイトへのアクセスは制限がある場合があります
- 機密情報を扱うWebアクセスには使用しないでください

### Proxy設定の解除

テスト終了後は、ブラウザのProxy設定を解除してください：

**Windows / macOS**:
1. プロキシ設定画面を開く
2. **プロキシサーバーを使う** をOFFにする

**Firefox**:
1. **設定** > **ネットワーク設定** > **接続設定...**
2. **プロキシを使用しない** を選択

さらに詳しい設定は、各マニュアルを参照してください。
