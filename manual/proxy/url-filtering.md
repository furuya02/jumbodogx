# Proxyサーバー - URLフィルタリング

JumboDogXのProxyサーバーにおけるURLフィルタリング機能について説明します。

## URLフィルタリングとは

URLフィルタリングは、特定のWebサイトやURLへのアクセスを制御する機能です。業務に不要なサイトへのアクセスをブロックしたり、特定のサイトのみを許可したりできます。

**用途**:
- 不適切なWebサイトへのアクセスをブロック
- 業務時間中のSNS・動画サイトへのアクセス制限
- セキュリティリスクのあるサイトのブロック
- 特定のサイトのみを許可するホワイトリスト運用

---

## フィルタリングモード

### Allow Mode（許可リストモード）

リストに登録されているURL**のみ**にアクセスできます。

**用途**:
- セキュリティが重要な環境
- 特定のサイトのみを使用する環境
- 子供向けの制限された環境

**設定例**:
```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "UrlFilter": {
      "Mode": "Allow",
      "Entries": [
        "*.google.com",
        "*.github.com",
        "*.stackoverflow.com"
      ]
    }
  }
}
```

この設定では、Google、GitHub、Stack Overflowのみにアクセスできます。

---

### Deny Mode（拒否リストモード）

リストに登録されているURL**以外**にアクセスできます。

**用途**:
- 特定のサイトのみをブロック
- ほとんどのサイトを許可し、一部のみを拒否
- 既知の悪質なサイトのブロック

**設定例**:
```json
{
  "ProxyServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 8080,
    "UrlFilter": {
      "Mode": "Deny",
      "Entries": [
        "*.facebook.com",
        "*.twitter.com",
        "*.youtube.com"
      ]
    }
  }
}
```

この設定では、Facebook、Twitter、YouTube以外のすべてのサイトにアクセスできます。

---

## URLパターンの指定

### ワイルドカード（*）

ワイルドカード（`*`）を使用して、複数のURLをマッチングできます。

**例**:
```json
{
  "Entries": [
    "*.facebook.com",      # すべてのFacebookサブドメイン
    "example.com/*",       # example.comの全ページ
    "*.google.*/search*"   # GoogleのすべてのTLDで検索ページ
  ]
}
```

### ドメイン名

**完全一致**:
```json
{
  "Entries": [
    "example.com"  # example.comのみ（サブドメインは含まない）
  ]
}
```

**サブドメイン含む**:
```json
{
  "Entries": [
    "*.example.com"  # すべてのサブドメイン（www.example.com、api.example.com等）
  ]
}
```

### パス

**特定のパス**:
```json
{
  "Entries": [
    "example.com/admin/*"  # example.comの/admin以下のすべてのページ
  ]
}
```

### クエリパラメータ

**特定のパラメータ**:
```json
{
  "Entries": [
    "example.com/search?q=*"  # 検索クエリを含むページ
  ]
}
```

---

## 設定例集

### オフィス環境（Deny Mode）

SNS・動画サイトをブロック:
```json
{
  "ProxyServer": {
    "UrlFilter": {
      "Mode": "Deny",
      "Entries": [
        "*.facebook.com",
        "*.twitter.com",
        "*.instagram.com",
        "*.youtube.com",
        "*.tiktok.com",
        "*.netflix.com"
      ]
    }
  }
}
```

---

### 教育機関（Allow Mode）

教育関連サイトのみを許可:
```json
{
  "ProxyServer": {
    "UrlFilter": {
      "Mode": "Allow",
      "Entries": [
        "*.google.com",
        "*.wikipedia.org",
        "*.khanacademy.org",
        "*.coursera.org",
        "*.edx.org",
        "*.github.com"
      ]
    }
  }
}
```

---

### 開発環境（Allow Mode）

開発関連サイトのみを許可:
```json
{
  "ProxyServer": {
    "UrlFilter": {
      "Mode": "Allow",
      "Entries": [
        "*.github.com",
        "*.stackoverflow.com",
        "*.npmjs.com",
        "*.pypi.org",
        "*.docker.com",
        "*.cloud.google.com",
        "*.aws.amazon.com",
        "*.azure.com"
      ]
    }
  }
}
```

---

### セキュリティ（Deny Mode）

既知の悪質なサイトをブロック:
```json
{
  "ProxyServer": {
    "UrlFilter": {
      "Mode": "Deny",
      "Entries": [
        "*.malware-site.com",
        "*.phishing-site.net",
        "*.spam-site.org"
      ]
    }
  }
}
```

---

### 子供向け（Allow Mode）

子供向けサイトのみを許可:
```json
{
  "ProxyServer": {
    "UrlFilter": {
      "Mode": "Allow",
      "Entries": [
        "*.disney.com",
        "*.pbskids.org",
        "*.nickelodeon.com",
        "*.wikipedia.org",
        "*.google.com/search*"
      ]
    }
  }
}
```

---

## Web UIでのURLフィルタリング設定

### フィルタリングモードの選択

1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **Proxy** → **URL Filter** を選択
4. **Filter Mode** を選択:
   - **Allow Mode**: 許可リストモード
   - **Deny Mode**: 拒否リストモード
5. **Save Settings** ボタンをクリック

![フィルタリングモード選択画面](images/proxy-url-filter-mode.png)

### URLパターンの追加

1. URL Filter設定画面で **Add Entry** ボタンをクリック
2. URLパターンを入力（例: `*.facebook.com`）
3. **Add** ボタンをクリック
4. **Save Settings** ボタンをクリック

![URLパターン追加画面](images/proxy-url-filter-add.png)

### URLパターンの削除

1. URL Filter設定画面でエントリ一覧を表示
2. 削除したいエントリの **Delete** ボタンをクリック
3. 確認ダイアログで **OK** をクリック
4. **Save Settings** ボタンをクリック

---

## カテゴリ別フィルタリング例

### SNS全般をブロック

```json
{
  "UrlFilter": {
    "Mode": "Deny",
    "Entries": [
      "*.facebook.com",
      "*.twitter.com",
      "*.instagram.com",
      "*.linkedin.com",
      "*.tiktok.com",
      "*.snapchat.com",
      "*.pinterest.com"
    ]
  }
}
```

---

### 動画サイトをブロック

```json
{
  "UrlFilter": {
    "Mode": "Deny",
    "Entries": [
      "*.youtube.com",
      "*.vimeo.com",
      "*.dailymotion.com",
      "*.twitch.tv",
      "*.netflix.com",
      "*.hulu.com"
    ]
  }
}
```

---

### ニュースサイトのみを許可

```json
{
  "UrlFilter": {
    "Mode": "Allow",
    "Entries": [
      "*.bbc.com",
      "*.cnn.com",
      "*.nytimes.com",
      "*.reuters.com",
      "*.apnews.com"
    ]
  }
}
```

---

### ショッピングサイトをブロック

```json
{
  "UrlFilter": {
    "Mode": "Deny",
    "Entries": [
      "*.amazon.com",
      "*.ebay.com",
      "*.aliexpress.com",
      "*.walmart.com",
      "*.target.com"
    ]
  }
}
```

---

## 高度なフィルタリング

### 正規表現の使用

**注意**: 現在のバージョンでは、正規表現はサポートされていません。ワイルドカード（*）のみ使用できます。

将来のバージョンで正規表現のサポートが追加される予定です。

---

### 時間帯によるフィルタリング

**注意**: 現在のバージョンでは、時間帯によるフィルタリングはサポートされていません。

将来のバージョンで時間帯フィルタリングのサポートが追加される予定です。

---

## ブロック時の動作

### ブロックページ

URLフィルタリングでブロックされたURLにアクセスすると、以下のメッセージが表示されます：

```
403 Forbidden

This URL is blocked by the proxy server.
URL: http://example.com
```

---

## セキュリティのベストプラクティス

### 1. Allow Modeを使用（可能な場合）

セキュリティが重要な環境では、Allow Modeを使用してください。

```json
{
  "UrlFilter": {
    "Mode": "Allow",
    "Entries": [/* 許可されたURLのみ */]
  }
}
```

---

### 2. 定期的なレビュー

フィルタリングリストを定期的にレビューし、不要なエントリを削除してください。

---

### 3. ログの監視

ブロックされたアクセスをログで確認し、不正なアクセスを検出してください。

```bash
# ブロックされたアクセスを確認
grep "URL blocked" logs/jumbodogx.log
```

---

### 4. ユーザーへの通知

URLフィルタリングが有効であることをユーザーに通知してください。

---

## トラブルシューティング

### 特定のサイトにアクセスできない

**原因1: URLフィルタリングでブロックされている（Allow Mode）**

**解決方法**: サイトのURLをAllowリストに追加してください

```json
{
  "UrlFilter": {
    "Mode": "Allow",
    "Entries": [
      "example.com"  # ← アクセスしたいサイトを追加
    ]
  }
}
```

---

**原因2: URLフィルタリングでブロックされている（Deny Mode）**

**解決方法**: サイトのURLをDenyリストから削除してください

---

### ワイルドカードが動作しない

**原因: パターンが間違っている**

**解決方法**: 正しいワイルドカードパターンを使用してください

- 正しい: `*.example.com`（すべてのサブドメイン）
- 間違い: `*example.com`（ドットがない）
- 正しい: `example.com/*`（すべてのパス）
- 間違い: `example.com*`（スラッシュがない）

---

### サブドメインもブロックしたい

**解決方法**: ワイルドカードを使用してください

```json
{
  "Entries": [
    "*.example.com"  # example.comのすべてのサブドメイン
  ]
}
```

---

## ログの確認

URLフィルタリングの動作状況はログで確認できます。

```bash
# URLフィルタリングに関するログを確認
grep "URL filter" logs/jumbodogx.log

# ブロックされたURLを確認
grep "URL blocked" logs/jumbodogx.log

# 許可されたURLを確認
grep "URL allowed" logs/jumbodogx.log
```

詳細は[ロギング設定ガイド](../common/logging.md)を参照してください。

---

## よくある質問（FAQ）

### Q1: HTTPSサイトもフィルタリングできますか？

**A**: はい、ドメイン名に基づいてフィルタリングできます。ただし、URLのパス部分（`https://example.com/path`の`/path`）はHTTPS通信の性質上、プロキシサーバーからは見えないため、フィルタリングできません。

---

### Q2: 複数のワイルドカードを使用できますか？

**A**: はい、複数のワイルドカードを使用できます。

例: `*.google.*/search*`（すべてのGoogleドメインで検索ページ）

---

### Q3: 正規表現を使用できますか？

**A**: 現在のバージョンでは、正規表現はサポートされていません。ワイルドカード（`*`）のみ使用できます。

---

### Q4: 時間帯によるフィルタリングはできますか？

**A**: 現在のバージョンでは、時間帯フィルタリングはサポートされていません。将来のバージョンで追加予定です。

---

### Q5: カテゴリベースのフィルタリング（アダルト、ギャンブル等）はできますか？

**A**: 現在のバージョンでは、カテゴリベースのフィルタリングはサポートされていません。手動でURLリストを作成する必要があります。

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [キャッシュ設定](cache-configuration.md)
- [トラブルシューティング](troubleshooting.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
- [ロギング設定](../common/logging.md)
