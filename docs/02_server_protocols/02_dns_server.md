# DNSサーバー

## 1. 概要

JumboDogX DNSサーバーは、ローカル環境でのドメイン名解決を提供します。

### 1.1 主要機能

- Aレコード
- AAAAレコード
- CNAMEレコード
- MXレコード
- NSレコード
- TXTレコード
- 上位DNSへの転送

## 2. プロジェクト構造

```
Jdx.Servers.Dns/
├── DnsServer.cs               # メインサーバークラス
├── DnsProtocol.cs             # DNSプロトコル処理
├── DnsQuery.cs                # クエリパーサー
├── DnsResponse.cs             # レスポンス生成
├── Records/                   # レコードタイプ
│   ├── ARecord.cs
│   ├── AaaaRecord.cs
│   ├── CnameRecord.cs
│   ├── MxRecord.cs
│   ├── NsRecord.cs
│   └── TxtRecord.cs
└── Forwarding/                # 転送機能
```

## 3. 設定

### 3.1 基本設定

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 53 | 待ち受けポート (UDP) |
| TTL | 3600 | デフォルトTTL（秒） |

### 3.2 レコード設定

```json
{
  "DnsRecords": [
    {
      "Type": "A",
      "Name": "example.local",
      "Value": "192.168.1.100",
      "TTL": 3600
    },
    {
      "Type": "MX",
      "Name": "example.local",
      "Value": "mail.example.local",
      "Priority": 10
    }
  ]
}
```

## 4. サポートレコードタイプ

| タイプ | 説明 | 例 |
|--------|------|-----|
| A | IPv4アドレス | 192.168.1.1 |
| AAAA | IPv6アドレス | ::1 |
| CNAME | 別名 | www -> example.local |
| MX | メールサーバー | mail.example.local |
| NS | ネームサーバー | ns.example.local |
| TXT | テキスト情報 | "v=spf1 ..." |

## 5. 上位DNS転送

未登録ドメインの解決を上位DNSサーバーに転送。

```json
{
  "Forwarding": {
    "Enabled": true,
    "Servers": ["8.8.8.8", "8.8.4.4"]
  }
}
```

## 6. Web UI操作

### 6.1 レコード管理

- レコードの追加・編集・削除
- レコード一覧表示
- タイプ別フィルタリング

## 7. テスト

テストプロジェクト: `tests/Jdx.Servers.Dns.Tests/`

```bash
dotnet test tests/Jdx.Servers.Dns.Tests
```

### 7.1 動作確認

```bash
# nslookupでテスト
nslookup example.local 127.0.0.1

# digでテスト
dig @127.0.0.1 example.local A
```
