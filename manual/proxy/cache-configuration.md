# Proxyサーバー - キャッシュ設定

JumboDogXのProxyサーバーにおけるキャッシュ機能の詳細設定について説明します。

## キャッシュとは

キャッシュは、一度取得したWebコンテンツをローカルに保存し、同じコンテンツへの次回のアクセス時に保存されたコピーを返す仕組みです。これにより、ネットワークトラフィックを削減し、応答速度を向上させます。

**メリット**:
- Webページの読み込み速度向上
- ネットワーク帯域幅の節約
- 外部サーバーへの負荷軽減

---

## 基本的なキャッシュ設定

### キャッシュの有効化

```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "Cache": {
      "Enabled": true,
      "CacheDirectory": "C:\\ProxyCache",
      "MaxCacheSize": 1073741824,
      "CacheExpiration": 3600
    }
  }
}
```

**設定項目**:
- **Enabled**: キャッシュ機能の有効/無効
- **CacheDirectory**: キャッシュファイルの保存ディレクトリ
- **MaxCacheSize**: 最大キャッシュサイズ（バイト）
- **CacheExpiration**: キャッシュの有効期限（秒）

---

## キャッシュディレクトリ

### ディレクトリの設定

キャッシュディレクトリは、キャッシュファイルを保存する場所です。

**Windows**:
```json
{
  "Cache": {
    "CacheDirectory": "C:\\ProxyCache"
  }
}
```

**macOS/Linux**:
```json
{
  "Cache": {
    "CacheDirectory": "/var/cache/jumbodogx/proxy"
  }
}
```

### ディレクトリの作成

キャッシュディレクトリは自動的に作成されますが、手動で作成することもできます。

**Windows**:
```cmd
mkdir C:\ProxyCache
```

**macOS/Linux**:
```bash
mkdir -p /var/cache/jumbodogx/proxy
```

### ディレクトリ構造

キャッシュは以下の構造で保存されます：

```
CacheDirectory/
├── cache_index.db          # キャッシュインデックス
├── 1a2b3c4d.cache         # キャッシュファイル
├── 5e6f7g8h.cache
├── 9i0j1k2l.cache
└── ...
```

キャッシュファイルはハッシュ値を基にした名前で保存され、`cache_index.db`でインデックス管理されます。

---

## 最大キャッシュサイズ

### サイズの設定

最大キャッシュサイズは、キャッシュディレクトリが使用できる最大容量です。

**サイズ計算**:
```
1 MB = 1,048,576 バイト
1 GB = 1,073,741,824 バイト
10 GB = 10,737,418,240 バイト
```

**設定例**:
```json
{
  "Cache": {
    "MaxCacheSize": 1073741824  # 1GB
  }
}
```

### 推奨サイズ

| 環境 | 推奨サイズ | バイト数 |
|------|-----------|---------|
| 個人用 | 500MB - 1GB | 524288000 - 1073741824 |
| 小規模オフィス | 1GB - 5GB | 1073741824 - 5368709120 |
| 中規模オフィス | 5GB - 20GB | 5368709120 - 21474836480 |
| 大規模オフィス | 20GB以上 | 21474836480以上 |

**設定例（5GB）**:
```json
{
  "Cache": {
    "MaxCacheSize": 5368709120  # 5GB
  }
}
```

### キャッシュサイズの管理

キャッシュサイズが最大値に達すると、古いキャッシュエントリが自動的に削除されます（LRU: Least Recently Used）。

---

## キャッシュの有効期限

### 有効期限の設定

キャッシュの有効期限は、キャッシュされたコンテンツが有効とみなされる時間です。

**時間計算**:
```
1 時間 = 3600 秒
1 日 = 86400 秒
1 週間 = 604800 秒
```

**設定例**:
```json
{
  "Cache": {
    "CacheExpiration": 3600  # 1時間
  }
}
```

### 推奨値

| コンテンツタイプ | 推奨有効期限 | 秒数 |
|------------------|-------------|------|
| 静的リソース（画像、CSS、JS） | 1週間 | 604800 |
| 一般的なWebページ | 1時間 | 3600 |
| 頻繁に更新されるコンテンツ | 10分 | 600 |
| APIレスポンス | 5分 | 300 |

**設定例（1時間）**:
```json
{
  "Cache": {
    "CacheExpiration": 3600
  }
}
```

### HTTPキャッシュヘッダーの尊重

キャッシュの有効期限は、HTTP応答ヘッダー（`Cache-Control`、`Expires`等）がある場合、それを優先します。

---

## キャッシュ対象の制御

### キャッシュ可能なコンテンツ

デフォルトでは、以下のコンテンツがキャッシュされます：

- **GET リクエスト**: 通常のWebページ取得
- **静的リソース**: 画像（JPG、PNG、GIF）、CSS、JavaScript
- **200 OK レスポンス**: 正常なレスポンス

### キャッシュされないコンテンツ

以下のコンテンツはキャッシュされません：

- **POST/PUT/DELETE リクエスト**: データ送信・変更
- **認証が必要なコンテンツ**: `Authorization`ヘッダー付き
- **プライベートコンテンツ**: `Cache-Control: private`ヘッダー付き
- **動的コンテンツ**: `Cache-Control: no-store`ヘッダー付き

---

## Web UIでのキャッシュ設定

### キャッシュの有効化

1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **Proxy** → **Cache** を選択
4. **Enable Cache** チェックボックスにチェックを入れる
5. 各フィールドに値を入力:
   - **Cache Directory**: キャッシュディレクトリのパス
   - **Max Cache Size**: 最大キャッシュサイズ（バイト）
   - **Cache Expiration**: 有効期限（秒）
6. **Save Settings** ボタンをクリック

![キャッシュ設定画面](images/proxy-cache-settings.png)

### キャッシュのクリア

1. Cache設定画面で **Clear Cache** ボタンをクリック
2. 確認ダイアログで **OK** をクリック
3. すべてのキャッシュが削除されます

![キャッシュクリア画面](images/proxy-cache-clear.png)

---

## 設定例集

### 個人用（軽量キャッシュ）

```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "Cache": {
      "Enabled": true,
      "CacheDirectory": "C:\\ProxyCache",
      "MaxCacheSize": 524288000,  # 500MB
      "CacheExpiration": 3600     # 1時間
    }
  }
}
```

---

### 小規模オフィス（標準キャッシュ）

```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "Cache": {
      "Enabled": true,
      "CacheDirectory": "C:\\ProxyCache",
      "MaxCacheSize": 2147483648,  # 2GB
      "CacheExpiration": 7200      # 2時間
    }
  }
}
```

---

### 大規模オフィス（大容量キャッシュ）

```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "Cache": {
      "Enabled": true,
      "CacheDirectory": "D:\\ProxyCache",  # 大容量ドライブ
      "MaxCacheSize": 21474836480,         # 20GB
      "CacheExpiration": 86400             # 24時間
    }
  }
}
```

---

### キャッシュ無効化

```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "Cache": {
      "Enabled": false  # キャッシュを無効化
    }
  }
}
```

または、`Cache`キーを削除:
```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080
    // Cache設定を削除
  }
}
```

---

## キャッシュのパフォーマンス

### キャッシュヒット率

キャッシュヒット率は、キャッシュから応答できたリクエストの割合です。

**計算式**:
```
キャッシュヒット率 = (キャッシュヒット数 / 総リクエスト数) × 100%
```

**良いキャッシュヒット率**: 50%以上

### キャッシュヒット率の向上

1. **キャッシュサイズを増やす**: より多くのコンテンツをキャッシュ
2. **有効期限を長くする**: キャッシュが長く保持される
3. **頻繁にアクセスされるサイトを特定**: 自動的にキャッシュされる

---

## キャッシュのメンテナンス

### 定期的なクリア

古いキャッシュや不要なキャッシュを定期的にクリアしてください。

**手動クリア**:
1. Web UIからClear Cacheボタンをクリック

**ファイルシステムから削除**:
```bash
# Windows
rmdir /s /q C:\ProxyCache
mkdir C:\ProxyCache

# macOS/Linux
rm -rf /var/cache/jumbodogx/proxy/*
```

### ディスク容量の監視

キャッシュディレクトリのディスク容量を定期的に確認してください。

**Windows**:
```cmd
dir C:\ProxyCache
```

**macOS/Linux**:
```bash
du -sh /var/cache/jumbodogx/proxy
```

---

## トラブルシューティング

### キャッシュが動作しない

**原因1: キャッシュが無効**

**解決方法**: キャッシュを有効化してください

```json
{
  "Cache": {
    "Enabled": true
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

### キャッシュサイズがすぐに上限に達する

**原因: 最大キャッシュサイズが小さすぎる**

**解決方法**: 最大キャッシュサイズを増やしてください

```json
{
  "Cache": {
    "MaxCacheSize": 5368709120  # 5GBに増加
  }
}
```

---

### 古いコンテンツがキャッシュされる

**原因: キャッシュの有効期限が長すぎる**

**解決方法**: 有効期限を短くしてください

```json
{
  "Cache": {
    "CacheExpiration": 1800  # 30分に短縮
  }
}
```

または、キャッシュをクリアしてください。

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [URLフィルタリング](url-filtering.md)
- [トラブルシューティング](troubleshooting.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
