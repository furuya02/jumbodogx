# DNSサーバー - ゾーン設定

JumboDogXのDNSサーバーにおけるゾーン管理とゾーン転送について説明します。

## DNSゾーンとは

DNSゾーンは、DNSネームスペースの一部を管理する単位です。
例えば、`example.local`というゾーンは、`example.local`とそのすべてのサブドメイン（`www.example.local`、`mail.example.local`など）を管理します。

## ゾーンの種類

### プライマリゾーン（Primary Zone）

ゾーンデータの**元データ**を保持し、書き込み可能なゾーンです。

**特徴**:
- ゾーンデータを直接編集可能
- すべての変更はここで行われる
- セカンダリゾーンへデータを転送する

### セカンダリゾーン（Secondary Zone）

プライマリゾーンからデータを**複製**する読み取り専用のゾーンです。

**特徴**:
- プライマリゾーンからゾーン転送でデータを取得
- 読み取り専用（データ編集不可）
- プライマリゾーンの冗長化とパフォーマンス向上に利用

## JumboDogXでのゾーン設定

JumboDogXのDNSサーバーは、デフォルトで**プライマリゾーン**として動作します。

### 基本的なゾーン設定

```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "Records": [
      {
        "Type": "SOA",
        "Name": "example.local",
        "Value": "ns1.example.local. admin.example.local. 2024010100 3600 1800 604800 86400",
        "TTL": 3600
      },
      {
        "Type": "NS",
        "Name": "example.local",
        "Value": "ns1.example.local",
        "TTL": 3600
      },
      {
        "Type": "A",
        "Name": "ns1.example.local",
        "Value": "192.168.1.10",
        "TTL": 3600
      },
      {
        "Type": "A",
        "Name": "example.local",
        "Value": "192.168.1.100",
        "TTL": 300
      }
    ]
  }
}
```

### SOAレコードの詳細

SOA（Start of Authority）レコードは、ゾーンの管理情報を定義します。

**Value形式**:
```
主ネームサーバー 管理者メール シリアル番号 リフレッシュ リトライ 有効期限 最小TTL
```

**各フィールドの説明**:

| フィールド | 説明 | 推奨値 |
|------------|------|--------|
| 主ネームサーバー | このゾーンの主サーバー | `ns1.example.local.` |
| 管理者メール | 管理者のメールアドレス（@を.に変換） | `admin.example.local.` |
| シリアル番号 | ゾーンファイルのバージョン | `2024010100` (YYYYMMDDNN形式) |
| リフレッシュ | セカンダリサーバーの更新チェック間隔（秒） | `3600` (1時間) |
| リトライ | 更新失敗時の再試行間隔（秒） | `1800` (30分) |
| 有効期限 | セカンダリサーバーでのゾーンデータ有効期限（秒） | `604800` (1週間) |
| 最小TTL | ネガティブキャッシュのTTL（秒） | `86400` (1日) |

**シリアル番号の管理**:
- 形式: `YYYYMMDDNN`
  - `YYYY`: 年（2024）
  - `MM`: 月（01）
  - `DD`: 日（01）
  - `NN`: 1日内の変更回数（00-99）
- 例: `2024010100`、`2024010101`、`2024010102`

**重要**: ゾーンデータを変更する際は、必ずシリアル番号を増やしてください。セカンダリサーバーはシリアル番号で更新を検出します。

---

## 複数ゾーンの管理

JumboDogXでは、複数のDNSゾーンを同時に管理できます。

### 複数ゾーンの設定例

```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "Records": [
      // ゾーン1: example.local
      {
        "Type": "SOA",
        "Name": "example.local",
        "Value": "ns1.example.local. admin.example.local. 2024010100 3600 1800 604800 86400",
        "TTL": 3600
      },
      {
        "Type": "NS",
        "Name": "example.local",
        "Value": "ns1.example.local",
        "TTL": 3600
      },
      {
        "Type": "A",
        "Name": "example.local",
        "Value": "192.168.1.100",
        "TTL": 300
      },

      // ゾーン2: test.local
      {
        "Type": "SOA",
        "Name": "test.local",
        "Value": "ns1.test.local. admin.test.local. 2024010100 3600 1800 604800 86400",
        "TTL": 3600
      },
      {
        "Type": "NS",
        "Name": "test.local",
        "Value": "ns1.test.local",
        "TTL": 3600
      },
      {
        "Type": "A",
        "Name": "test.local",
        "Value": "192.168.1.200",
        "TTL": 300
      }
    ]
  }
}
```

---

## サブドメインの委任

サブドメインを別のネームサーバーに委任できます。

### サブドメイン委任の設定例

```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "Records": [
      // 親ゾーン: example.local
      {
        "Type": "SOA",
        "Name": "example.local",
        "Value": "ns1.example.local. admin.example.local. 2024010100 3600 1800 604800 86400",
        "TTL": 3600
      },
      {
        "Type": "NS",
        "Name": "example.local",
        "Value": "ns1.example.local",
        "TTL": 3600
      },
      {
        "Type": "A",
        "Name": "ns1.example.local",
        "Value": "192.168.1.10",
        "TTL": 3600
      },

      // サブドメイン委任: subdomain.example.local
      {
        "Type": "NS",
        "Name": "subdomain.example.local",
        "Value": "ns2.example.local",
        "TTL": 3600
      },
      {
        "Type": "A",
        "Name": "ns2.example.local",
        "Value": "192.168.1.20",
        "TTL": 3600
      }
    ]
  }
}
```

この設定により、`subdomain.example.local`以下のすべてのクエリは`ns2.example.local`（192.168.1.20）に転送されます。

---

## 逆引きゾーン（Reverse DNS Zone）

IPアドレスからドメイン名を解決するための逆引きゾーンを設定できます。

### 逆引きゾーンの設定例

```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "Records": [
      // 正引きゾーン
      {
        "Type": "A",
        "Name": "example.local",
        "Value": "192.168.1.100",
        "TTL": 300
      },

      // 逆引きゾーン: 192.168.1.0/24
      {
        "Type": "SOA",
        "Name": "1.168.192.in-addr.arpa",
        "Value": "ns1.example.local. admin.example.local. 2024010100 3600 1800 604800 86400",
        "TTL": 3600
      },
      {
        "Type": "NS",
        "Name": "1.168.192.in-addr.arpa",
        "Value": "ns1.example.local",
        "TTL": 3600
      },
      {
        "Type": "PTR",
        "Name": "100.1.168.192.in-addr.arpa",
        "Value": "example.local",
        "TTL": 300
      }
    ]
  }
}
```

**逆引きゾーン名の形式**:
- ネットワーク: `192.168.1.0/24`
- ゾーン名: `1.168.192.in-addr.arpa`（ネットワークアドレスを逆順）

**PTRレコードの形式**:
- IPアドレス: `192.168.1.100`
- PTRレコード名: `100.1.168.192.in-addr.arpa`（IPアドレスを逆順）

---

## ゾーン転送（Zone Transfer）

ゾーン転送は、プライマリゾーンからセカンダリゾーンへDNSレコードを複製する仕組みです。

### ゾーン転送の種類

| タイプ | 説明 | 用途 |
|--------|------|------|
| AXFR | 完全ゾーン転送 | 初回同期、全データの再同期 |
| IXFR | 増分ゾーン転送 | 定期的な更新（変更差分のみ） |

**注意**: 現在のJumboDogXバージョンでは、ゾーン転送機能は実装されていません。将来のバージョンで追加予定です。

---

## Web UIでのゾーン管理

### ゾーン一覧の表示

1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **DNS** → **Zones** を選択
4. 設定されているゾーン一覧が表示されます

![ゾーン一覧画面](images/dns-zones-list.png)

### 新しいゾーンの追加

1. Zones画面で **Add Zone** ボタンをクリック
2. 各フィールドに値を入力:
   - **Zone Name**: ゾーン名（例: `newzone.local`）
   - **Primary Name Server**: 主ネームサーバー（例: `ns1.newzone.local`）
   - **Admin Email**: 管理者メール（例: `admin@newzone.local`）
3. **Add** ボタンをクリック
4. **Save Settings** ボタンをクリック

自動的にSOAレコードとNSレコードが作成されます。

![ゾーン追加画面](images/dns-zone-add.png)

---

## ゾーン設定のベストプラクティス

### SOAレコードの設定

**推奨値**:
```
ns1.example.local. admin.example.local. 2024010100 3600 1800 604800 86400
```

- **リフレッシュ**: 3600秒（1時間） - セカンダリサーバーがプライマリサーバーをチェックする間隔
- **リトライ**: 1800秒（30分） - リフレッシュ失敗時の再試行間隔
- **有効期限**: 604800秒（1週間） - プライマリサーバーとの通信が途絶えた場合の有効期限
- **最小TTL**: 86400秒（1日） - 存在しないレコードのキャッシュ時間

### NSレコードの設定

- 少なくとも1つのNSレコードを設定してください
- NSレコードで指定されたサーバーには、対応するAレコードまたはAAAAレコードを設定してください

### シリアル番号の管理

- ゾーンデータを変更するたびにシリアル番号を増やしてください
- `YYYYMMDDNN`形式を使用すると管理しやすくなります
- 1日に100回以上変更する場合は、UNIX時間（タイムスタンプ）の使用を検討してください

---

## トラブルシューティング

### ゾーンが認識されない

**原因**: SOAレコードまたはNSレコードが不足している

**解決方法**: 各ゾーンに少なくとも1つずつSOAレコードとNSレコードを設定してください

### サブドメインの委任が動作しない

**原因**: 委任先のネームサーバーのAレコードが不足している

**解決方法**: NSレコードで指定されたサーバーに対応するAレコードを追加してください

```json
{
  "Type": "NS",
  "Name": "subdomain.example.local",
  "Value": "ns2.example.local",
  "TTL": 3600
},
{
  "Type": "A",
  "Name": "ns2.example.local",
  "Value": "192.168.1.20",
  "TTL": 3600
}
```

### 逆引きが動作しない

**原因**: PTRレコードの名前形式が間違っている

**解決方法**: IPアドレスを逆順にして`.in-addr.arpa`を付加してください

- IPアドレス: `192.168.1.100`
- PTRレコード名: `100.1.168.192.in-addr.arpa`

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [リソースレコード](resource-records.md)
- [トラブルシューティング](troubleshooting.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
