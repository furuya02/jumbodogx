# DNSサーバー - リソースレコード設定

JumboDogXのDNSサーバーでサポートされているリソースレコードの詳細設定について説明します。

## リソースレコードとは

DNSリソースレコードは、ドメイン名とIPアドレスなどの情報を関連付けるデータベースのエントリです。
各レコードには、タイプ、名前、値、TTL（Time To Live）などの属性があります。

## サポートされているレコードタイプ

### Aレコード（Address Record）

ドメイン名をIPv4アドレスにマッピングします。

**設定例**:
```json
{
  "Type": "A",
  "Name": "example.local",
  "Value": "192.168.1.100",
  "TTL": 300
}
```

**ユースケース**:
- Webサーバーのアドレス解決
- メールサーバーのアドレス解決
- 一般的なホスト名の解決

**注意点**:
- Valueには有効なIPv4アドレス（xxx.xxx.xxx.xxx形式）を指定してください
- 同じNameに対して複数のAレコードを設定すると、ラウンドロビン動作します

---

### AAAAレコード（IPv6 Address Record）

ドメイン名をIPv6アドレスにマッピングします。

**設定例**:
```json
{
  "Type": "AAAA",
  "Name": "example.local",
  "Value": "2001:0db8:85a3::8a2e:0370:7334",
  "TTL": 300
}
```

**ユースケース**:
- IPv6環境でのアドレス解決
- デュアルスタック環境（IPv4/IPv6両対応）

**注意点**:
- Valueには有効なIPv6アドレスを指定してください
- IPv6圧縮表記（::）がサポートされています

---

### CNAMEレコード（Canonical Name Record）

ドメイン名の別名（エイリアス）を定義します。

**設定例**:
```json
{
  "Type": "CNAME",
  "Name": "www.local",
  "Value": "example.local",
  "TTL": 300
}
```

**ユースケース**:
- サブドメインのエイリアス
- 複数のドメイン名を1つのホストに向ける
- サービスの移行時の段階的な切り替え

**注意点**:
- CNAMEレコードと他のレコードタイプ（A、AAAA等）を同じ名前で共存させることはできません
- Valueには完全修飾ドメイン名（FQDN）を指定してください

---

### MXレコード（Mail Exchange Record）

メールサーバーを指定します。

**設定例**:
```json
{
  "Type": "MX",
  "Name": "example.local",
  "Value": "10 mail.example.local",
  "TTL": 300
}
```

**Valueの形式**: `優先度 メールサーバー名`
- 優先度: 0-65535の整数（小さい値ほど高優先）
- メールサーバー名: メールサーバーのFQDN

**複数MXレコードの設定**:
```json
{
  "Records": [
    {
      "Type": "MX",
      "Name": "example.local",
      "Value": "10 mail1.example.local",
      "TTL": 300
    },
    {
      "Type": "MX",
      "Name": "example.local",
      "Value": "20 mail2.example.local",
      "TTL": 300
    }
  ]
}
```

**ユースケース**:
- メールシステムの冗長化
- メール配送のロードバランシング
- フェイルオーバー設定

---

### NSレコード（Name Server Record）

ドメインの権威ネームサーバーを指定します。

**設定例**:
```json
{
  "Type": "NS",
  "Name": "example.local",
  "Value": "ns1.example.local",
  "TTL": 3600
}
```

**ユースケース**:
- サブドメインの委任
- ゾーンの権威サーバー指定

**注意点**:
- NSレコードで指定されたサーバーには、対応するAまたはAAAAレコードが必要です

---

### PTRレコード（Pointer Record）

IPアドレスからドメイン名への逆引き（reverse DNS）を提供します。

**設定例**:
```json
{
  "Type": "PTR",
  "Name": "100.1.168.192.in-addr.arpa",
  "Value": "example.local",
  "TTL": 300
}
```

**Nameの形式**:
- IPv4: `xxx.xxx.xxx.xxx.in-addr.arpa`（IPアドレスを逆順）
- IPv6: `x.x.x.x...x.x.x.x.ip6.arpa`（各16進数を逆順、ドット区切り）

**ユースケース**:
- メールサーバーの認証（スパムフィルター回避）
- ログファイルでのホスト名表示
- トラブルシューティング

---

### SOAレコード（Start of Authority Record）

DNSゾーンの管理情報を定義します。

**設定例**:
```json
{
  "Type": "SOA",
  "Name": "example.local",
  "Value": "ns1.example.local. admin.example.local. 2024010100 3600 1800 604800 86400",
  "TTL": 3600
}
```

**Valueの形式**:
```
主ネームサーバー 管理者メール シリアル番号 リフレッシュ リトライ 有効期限 最小TTL
```

- **主ネームサーバー**: このゾーンの主サーバー
- **管理者メール**: 管理者のメールアドレス（@を.に変換）
- **シリアル番号**: ゾーンファイルのバージョン（YYYYMMDDNN形式推奨）
- **リフレッシュ**: セカンダリサーバーの更新チェック間隔（秒）
- **リトライ**: 更新失敗時の再試行間隔（秒）
- **有効期限**: セカンダリサーバーでのゾーンデータ有効期限（秒）
- **最小TTL**: ネガティブキャッシュのTTL（秒）

**ユースケース**:
- ゾーンの権威情報の定義
- セカンダリDNSサーバーとの連携

---

## TTL（Time To Live）

TTLは、DNSレコードがキャッシュされる時間を秒単位で指定します。

### TTLの推奨値

| 用途 | TTL | 説明 |
|------|-----|------|
| 頻繁に変更 | 60-300秒 | 開発環境、テスト環境 |
| 通常運用 | 300-3600秒 | 本番環境の一般的なレコード |
| 安定したレコード | 3600-86400秒 | めったに変更しないレコード |
| インフラレコード | 86400秒以上 | NSレコード、SOAレコード |

### TTL設定のベストプラクティス

**短いTTLのメリット**:
- 迅速な変更反映
- 柔軟な運用

**短いTTLのデメリット**:
- DNSサーバーへのクエリ増加
- ネットワークトラフィック増加

**長いTTLのメリット**:
- DNSサーバーの負荷軽減
- クライアント側のレスポンス向上

**長いTTLのデメリット**:
- 変更の反映に時間がかかる
- 緊急時の対応が困難

---

## ワイルドカードレコード

ワイルドカード（*）を使用して、複数のサブドメインをまとめて処理できます。

**設定例**:
```json
{
  "Type": "A",
  "Name": "*.example.local",
  "Value": "192.168.1.100",
  "TTL": 300
}
```

**動作**:
- `anything.example.local` → `192.168.1.100`
- `test.example.local` → `192.168.1.100`
- `abc.def.example.local` → `192.168.1.100`

**注意点**:
- 明示的なレコードが優先されます
- `www.example.local`に明示的なレコードがあれば、ワイルドカードより優先されます

---

## 複数値の設定

同じNameに対して複数のレコードを設定できます。

### ラウンドロビン（負荷分散）

```json
{
  "Records": [
    {
      "Type": "A",
      "Name": "www.local",
      "Value": "192.168.1.100",
      "TTL": 300
    },
    {
      "Type": "A",
      "Name": "www.local",
      "Value": "192.168.1.101",
      "TTL": 300
    },
    {
      "Type": "A",
      "Name": "www.local",
      "Value": "192.168.1.102",
      "TTL": 300
    }
  ]
}
```

クエリのたびに、異なるIPアドレスが返されます（ラウンドロビン）。

---

## Web UIでの設定

### レコードの追加

1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **DNS** → **Records** を選択
4. **Add Record** ボタンをクリック
5. 各フィールドに値を入力:
   - **Type**: レコードタイプ（A, AAAA, CNAME, MX, NS, PTR, SOA）
   - **Name**: ドメイン名
   - **Value**: レコードの値
   - **TTL**: キャッシュ時間（秒）
6. **Add** ボタンをクリック
7. **Save Settings** ボタンをクリック

![レコード追加画面](images/dns-record-add.png)

### レコードの編集

1. Records画面でレコード一覧を表示
2. 編集したいレコードの **Edit** ボタンをクリック
3. 値を修正
4. **Update** ボタンをクリック
5. **Save Settings** ボタンをクリック

![レコード編集画面](images/dns-record-edit.png)

### レコードの削除

1. Records画面でレコード一覧を表示
2. 削除したいレコードの **Delete** ボタンをクリック
3. 確認ダイアログで **OK** をクリック
4. **Save Settings** ボタンをクリック

---

## 設定例集

### Webサーバーとメールサーバー

```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "Records": [
      {
        "Type": "A",
        "Name": "example.local",
        "Value": "192.168.1.100",
        "TTL": 300
      },
      {
        "Type": "A",
        "Name": "www.example.local",
        "Value": "192.168.1.100",
        "TTL": 300
      },
      {
        "Type": "A",
        "Name": "mail.example.local",
        "Value": "192.168.1.101",
        "TTL": 300
      },
      {
        "Type": "MX",
        "Name": "example.local",
        "Value": "10 mail.example.local",
        "TTL": 300
      }
    ]
  }
}
```

### 負荷分散Webサーバー

```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "Records": [
      {
        "Type": "A",
        "Name": "web.local",
        "Value": "192.168.1.100",
        "TTL": 60
      },
      {
        "Type": "A",
        "Name": "web.local",
        "Value": "192.168.1.101",
        "TTL": 60
      },
      {
        "Type": "A",
        "Name": "web.local",
        "Value": "192.168.1.102",
        "TTL": 60
      }
    ]
  }
}
```

### IPv6対応サーバー

```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "::",
    "Port": 53,
    "Records": [
      {
        "Type": "A",
        "Name": "example.local",
        "Value": "192.168.1.100",
        "TTL": 300
      },
      {
        "Type": "AAAA",
        "Name": "example.local",
        "Value": "2001:0db8:85a3::8a2e:0370:7334",
        "TTL": 300
      }
    ]
  }
}
```

---

## トラブルシューティング

### レコードが反映されない

**原因**: キャッシュが古いTTL値を保持している

**解決方法**:
```bash
# macOS/Linux
sudo dscacheutil -flushcache
sudo killall -HUP mDNSResponder

# Windows
ipconfig /flushdns
```

### CNAMEレコードが動作しない

**原因**: 同じNameに他のレコードタイプ（A、AAAA等）が存在する

**解決方法**: CNAMEレコードと他のレコードタイプは共存できません。どちらか一方を削除してください。

### MXレコードの優先度が効かない

**原因**: Valueの形式が間違っている

**解決方法**: `優先度 メールサーバー名`の形式で指定してください。例: `10 mail.local`

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [ゾーン設定](zone-configuration.md)
- [トラブルシューティング](troubleshooting.md)
- [ACL設定](../common/acl-configuration.md)
