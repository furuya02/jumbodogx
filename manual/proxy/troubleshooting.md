# Proxyサーバー - トラブルシューティング

JumboDogXのProxyサーバーで発生する可能性のある問題と解決方法を説明します。

## 一般的な問題

### Proxyサーバーが起動しない

#### 症状
- JumboDogXを起動してもProxyサーバーが動作しない
- ダッシュボードでProxyサーバーのステータスが"Stopped"または"Error"

#### 原因と解決方法

**原因1: ポート8080が既に使用されている**

**解決方法**:

**macOS**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :8080
```

**Windows**:
```cmd
# 使用中のプロセスを確認
netstat -ano | findstr :8080
```

**Linux**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :8080
```

**代替ポートの使用**:
```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 3128,  # ← 代替ポートを使用
    "TimeOut": 30
  }
}
```

クライアント側でポート3128を指定してください。

---

**原因2: `Enabled`が`false`になっている**

**解決方法**:
```json
{
  "ProxyServer": {
    "Enabled": true,  # ← trueに設定
    "BindAddress": "0.0.0.0",
    "Port": 8080
  }
}
```

---

### プロキシ経由で接続できない

#### 症状
- ブラウザでプロキシを設定してもWebサイトにアクセスできない
- タイムアウトエラーが発生する

#### 原因と解決方法

**原因1: ファイアウォールでポートがブロックされている**

**解決方法**:

**Windows**:
```powershell
# ファイアウォールルールを追加
New-NetFirewallRule -DisplayName "JumboDogX Proxy" -Direction Inbound -Protocol TCP -LocalPort 8080 -Action Allow
```

**macOS**:
```bash
# ファイアウォール設定を確認
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --getglobalstate

# JumboDogXを許可リストに追加
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --add /path/to/jumbodogx
```

**Linux (ufw)**:
```bash
# ポート8080を許可
sudo ufw allow 8080/tcp
```

---

**原因2: プロキシ設定が間違っている**

**解決方法**: プロキシ設定を確認してください

**正しい設定**:
- プロキシサーバー: `localhost`（または`127.0.0.1`）
- ポート: `8080`（設定ファイルで指定したポート）

**ブラウザの設定を確認**:
- Chrome: 設定 → システム → プロキシ設定
- Firefox: 設定 → ネットワーク設定
- Edge: 設定 → システムとパフォーマンス → プロキシ設定

---

**原因3: BindAddressが間違っている**

**解決方法**: BindAddressを確認してください

すべてのネットワークインターフェースで待ち受ける場合:
```json
{
  "ProxyServer": {
    "BindAddress": "0.0.0.0"
  }
}
```

ローカルホストのみで待ち受ける場合:
```json
{
  "ProxyServer": {
    "BindAddress": "127.0.0.1"
  }
}
```

---

## URLフィルタリング関連の問題

### 特定のサイトにアクセスできない

#### 症状
- 特定のWebサイトにアクセスすると"403 Forbidden"エラーが表示される

#### 原因と解決方法

**原因1: URLフィルタリングでブロックされている（Allow Mode）**

**解決方法**: サイトのURLをAllowリストに追加してください

```json
{
  "ProxyServer": {
    "UrlFilter": {
      "Mode": "Allow",
      "Entries": [
        "*.example.com"  # ← アクセスしたいサイトを追加
      ]
    }
  }
}
```

---

**原因2: URLフィルタリングでブロックされている（Deny Mode）**

**解決方法**: サイトのURLをDenyリストから削除してください

---

### すべてのサイトにアクセスできない

#### 症状
- どのWebサイトにもアクセスできない
- すべてのサイトで"403 Forbidden"エラーが表示される

#### 原因と解決方法

**原因: Allow Modeでエントリが空**

**解決方法**: 必要なサイトをAllowリストに追加するか、フィルタリングを無効化してください

```json
{
  "ProxyServer": {
    "UrlFilter": null  # フィルタリングを無効化
  }
}
```

または、`UrlFilter`キーを削除してください。

---

## キャッシュ関連の問題

### キャッシュが動作しない

#### 症状
- 同じページに何度アクセスしても速度が向上しない
- キャッシュヒット率が0%

#### 原因と解決方法

**原因1: キャッシュが無効**

**解決方法**: キャッシュを有効化してください

```json
{
  "ProxyServer": {
    "Cache": {
      "Enabled": true,
      "CacheDirectory": "C:\\ProxyCache",
      "MaxCacheSize": 1073741824,
      "CacheExpiration": 3600
    }
  }
}
```

---

**原因2: キャッシュディレクトリに書き込み権限がない**

**解決方法**: ディレクトリの権限を確認してください

**Windows**:
```cmd
icacls C:\ProxyCache
```

**macOS/Linux**:
```bash
ls -ld /var/cache/jumbodogx/proxy
chmod 755 /var/cache/jumbodogx/proxy
```

---

**原因3: HTTPSサイトはキャッシュされない**

HTTPSサイトの内容は暗号化されているため、プロキシサーバーではキャッシュできません（HTTPS MITMプロキシ機能がない場合）。

**解決方法**: HTTPサイトでキャッシュが動作するか確認してください。

---

### キャッシュディレクトリがすぐに一杯になる

#### 症状
- キャッシュディレクトリがすぐに最大サイズに達する

#### 原因と解決方法

**原因: 最大キャッシュサイズが小さすぎる**

**解決方法**: 最大キャッシュサイズを増やしてください

```json
{
  "ProxyServer": {
    "Cache": {
      "MaxCacheSize": 5368709120  # 5GBに増加
    }
  }
}
```

---

### 古いコンテンツが表示される

#### 症状
- 更新されたはずのWebページが古い内容のまま

#### 原因と解決方法

**原因: キャッシュの有効期限が長すぎる**

**解決方法1**: 有効期限を短くしてください

```json
{
  "ProxyServer": {
    "Cache": {
      "CacheExpiration": 600  # 10分に短縮
    }
  }
}
```

**解決方法2**: キャッシュをクリアしてください

Web UIから:
1. Settings → Proxy → Cache
2. Clear Cacheボタンをクリック

---

## HTTPS関連の問題

### HTTPSサイトにアクセスできない

#### 症状
- HTTPSサイト（https://）にアクセスするとエラーが発生する
- HTTPサイト（http://）は正常にアクセスできる

#### 原因と解決方法

**原因1: HTTPS CONNECTメソッドがブロックされている**

**解決方法**: HTTPS CONNECTメソッドが有効か確認してください（デフォルトで有効）

---

**原因2: URLフィルタリングでブロックされている**

**解決方法**: サイトのドメインをAllowリストに追加してください

```json
{
  "ProxyServer": {
    "UrlFilter": {
      "Mode": "Allow",
      "Entries": [
        "*.google.com"  # HTTPSでもドメイン名でフィルタリング可能
      ]
    }
  }
}
```

---

### 証明書エラーが発生する

#### 症状
- ブラウザで証明書エラーが表示される

#### 原因と解決方法

**原因: 透過プロキシまたはSSLインターセプトが有効**

**注意**: 現在のJumboDogXバージョンでは、SSLインターセプト（HTTPS MITMプロキシ）機能は実装されていません。通常のHTTPSプロキシとして動作します。

証明書エラーが発生する場合は、ネットワーク設定を確認してください。

---

## パフォーマンスの問題

### 接続が遅い

#### 症状
- プロキシ経由でWebサイトにアクセスすると遅い

#### 原因と解決方法

**原因1: タイムアウト値が長すぎる**

**解決方法**: タイムアウト値を調整してください

```json
{
  "ProxyServer": {
    "TimeOut": 10  # 10秒に短縮（デフォルトは30秒）
  }
}
```

---

**原因2: キャッシュディレクトリのディスクが遅い**

**解決方法**: SSDなど高速なディスクにキャッシュディレクトリを移動してください

```json
{
  "ProxyServer": {
    "Cache": {
      "CacheDirectory": "D:\\ProxyCache"  # 高速なドライブ
    }
  }
}
```

---

**原因3: ネットワーク遅延**

**解決方法**: ネットワーク遅延を測定してください

```bash
# 外部サイトへのping
ping google.com
```

---

### 大量のリクエストでタイムアウトが発生する

#### 症状
- 多くのユーザーが同時にアクセスするとタイムアウトが発生する

#### 原因と解決方法

**原因: サーバーリソース不足**

**解決方法**: サーバーのCPU/メモリ使用率を確認してください

**Windows**:
```cmd
taskmgr
```

**macOS**:
```bash
open -a "Activity Monitor"
```

**Linux**:
```bash
top
```

---

## ブラウザ設定の問題

### Chromeでプロキシが動作しない

#### 症状
- Chromeでプロキシ設定をしても動作しない

#### 原因と解決方法

**原因: OSレベルのプロキシ設定が必要**

Chromeは基本的にOSのプロキシ設定を使用します。

**解決方法**: OSレベルでプロキシを設定してください

**Windows**:
1. 設定 → ネットワークとインターネット → プロキシ
2. 「プロキシサーバーを使う」をオン
3. アドレス: `localhost`、ポート: `8080`

**macOS**:
1. システム設定 → ネットワーク → 詳細
2. プロキシ → Webプロキシ（HTTP）
3. サーバー: `localhost`、ポート: `8080`

---

### Firefoxでプロキシが動作しない

#### 症状
- Firefoxでプロキシ設定をしても動作しない

#### 原因と解決方法

**原因: プロキシ設定が間違っている**

**解決方法**: Firefoxのプロキシ設定を確認してください

1. 設定 → ネットワーク設定
2. 「手動でプロキシを設定する」を選択
3. HTTPプロキシ: `localhost`、ポート: `8080`
4. 「このプロキシをすべてのプロトコルで使用する」にチェック
5. OKをクリック

---

## Web UI関連の問題

### Web UIで設定が保存できない

#### 症状
- "Save Settings"ボタンをクリックしてもエラーが発生する

#### 原因と解決方法

**原因: ファイルのアクセス権限**

**解決方法**: 設定ファイルに書き込み権限があるか確認してください

```bash
# macOS/Linux
ls -l appsettings.json
chmod 644 appsettings.json

# Windows
icacls appsettings.json
```

---

## デバッグ方法

### ログの確認

JumboDogXのログファイルを確認して、エラーの詳細を調べてください。

**ログファイルの場所**:
```
logs/jumbodogx.log
logs/jumbodogx-YYYYMMDD.log
```

**ログの確認**:
```bash
# 最新のログを表示
tail -f logs/jumbodogx.log

# Proxyに関するログのみを表示
grep Proxy logs/jumbodogx.log

# エラーのみを表示
grep ERROR logs/jumbodogx.log

# URLフィルタリングのログを表示
grep "URL" logs/jumbodogx.log
```

詳細は[ロギング設定ガイド](../common/logging.md)を参照してください。

---

### curlでのテスト

`curl`コマンドでプロキシサーバーをテストできます。

**基本的な使用方法**:
```bash
# プロキシ経由でアクセス
curl -x http://localhost:8080 http://example.com

# ヘッダー情報を表示
curl -x http://localhost:8080 -I http://example.com

# 詳細なデバッグ情報を表示
curl -x http://localhost:8080 -v http://example.com
```

---

### ブラウザの開発者ツール

ブラウザの開発者ツールでネットワークトラフィックを確認できます。

**Chrome/Edge**:
1. F12キーを押す
2. Networkタブを選択
3. Webサイトにアクセス
4. リクエスト/レスポンスを確認

**Firefox**:
1. F12キーを押す
2. ネットワークタブを選択
3. Webサイトにアクセス
4. リクエスト/レスポンスを確認

---

## よくある質問（FAQ）

### Q1: ポート8080以外を使用できますか？

**A**: はい、設定ファイルで任意のポートを指定できます。

```json
{
  "ProxyServer": {
    "Port": 3128  # 代替ポート
  }
}
```

クライアント側でも同じポートを指定してください。

---

### Q2: HTTPS MITMプロキシ機能はありますか？

**A**: 現在のバージョンでは、HTTPS MITMプロキシ機能は実装されていません。HTTPSサイトはCONNECTメソッドで透過的にプロキシされますが、内容の検査やキャッシュはできません。

---

### Q3: 認証機能はありますか？

**A**: 現在のバージョンでは、プロキシ認証機能は実装されていません。将来のバージョンで追加予定です。

---

### Q4: 複数のプロキシをチェーンできますか？

**A**: 現在のバージョンでは、プロキシチェーン機能は実装されていません。

---

### Q5: 透過プロキシとして動作できますか？

**A**: 現在のバージョンでは、透過プロキシ機能は実装されていません。クライアント側で明示的にプロキシ設定が必要です。

---

## サポート

問題が解決しない場合は、以下を参照してください：

- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
- [ディスカッション](https://github.com/furuya02/jumbodogx/discussions)

Issue報告時には、以下の情報を含めてください：
- JumboDogXのバージョン
- OS（Windows/macOS/Linux）とバージョン
- 設定ファイル（`appsettings.json`の関連部分）
- ログファイル（`logs/jumbodogx.log`の関連部分）
- エラーメッセージ
- 使用しているブラウザ

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [キャッシュ設定](cache-configuration.md)
- [URLフィルタリング](url-filtering.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
