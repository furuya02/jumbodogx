# DNSサーバー

JumboDogXのDNSサーバーは、ドメイン名からIPアドレスへの変換を行う機能を提供します。
ローカル開発環境やテストネットワークで独自のDNSレコードを管理できます。

## 主な機能

### 基本機能
- **標準DNSレコード対応** - A, AAAA, CNAME, MX, NS, PTR, SOAレコード
- **高速な名前解決** - 効率的なレコード検索
- **柔軟な設定** - レコード単位での細かい制御

### セキュリティ機能
- **アクセス制御（ACL）** - IPアドレスベースのクエリ制限
- **タイムアウト設定** - DoS攻撃への対策

### 高度な機能
- **ゾーン管理** - 複数のDNSゾーンを管理可能
- **TTL設定** - レコードごとにキャッシュ時間を制御
- **ワイルドカードレコード** - 柔軟なレコードマッチング

## クイックスタート

DNSサーバーをすぐに使い始めたい方は、[クイックスタートガイド](getting-started.md)をご覧ください。

## ドキュメント一覧

### 基本設定
- [クイックスタート](getting-started.md) - 最初の設定と起動方法
- [リソースレコード](resource-records.md) - DNSレコードの詳細設定
- [ゾーン設定](zone-configuration.md) - DNSゾーンの管理方法

### トラブルシューティング
- [トラブルシューティング](troubleshooting.md) - よくある問題と解決方法

## 設定例

### シンプルなDNSサーバー

```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "TimeOut": 3,
    "Records": [
      {
        "Type": "A",
        "Name": "test.local",
        "Value": "192.168.1.100",
        "TTL": 300
      },
      {
        "Type": "CNAME",
        "Name": "www.local",
        "Value": "test.local",
        "TTL": 300
      }
    ]
  }
}
```

### 複数レコードを持つDNSサーバー

```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "Records": [
      {
        "Type": "A",
        "Name": "server.local",
        "Value": "192.168.1.100",
        "TTL": 300
      },
      {
        "Type": "A",
        "Name": "web.local",
        "Value": "192.168.1.101",
        "TTL": 300
      },
      {
        "Type": "MX",
        "Name": "mail.local",
        "Value": "10 mail.local",
        "TTL": 300
      }
    ]
  }
}
```

**レコードタイプ**:
- `A`: IPv4アドレス
- `AAAA`: IPv6アドレス
- `CNAME`: 別名（エイリアス）
- `MX`: メールサーバー
- `NS`: ネームサーバー
- `PTR`: 逆引き
- `SOA`: ゾーン情報

## Web UI

JumboDogXは、Webブラウザから設定できる管理画面を提供しています。

### アクセス方法
1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **DNS** を選択

### 設定画面
- **General** - 基本設定
- **Records** - DNSレコードの管理
- **ACL** - アクセス制御リスト

## ユースケース

### ローカル開発環境
開発中のWebアプリケーションに簡単にアクセスできるようにドメイン名を割り当て。

### 社内ネットワーク
プライベートなホスト名を管理し、社内サーバーへのアクセスを簡略化。

### テスト環境
テスト用のドメイン名を設定し、本番環境を模したテストを実施。

### 学習・教育
DNSの仕組みを理解するための実験環境として活用。

## クライアントからの利用

### nslookupコマンド

**Windows/macOS/Linux**:
```bash
nslookup test.local localhost
```

### digコマンド（macOS/Linux）

```bash
dig @localhost test.local
```

### OSのDNS設定

**Windows**:
1. ネットワーク設定 → プロパティ
2. 優先DNSサーバー: `127.0.0.1`

**macOS**:
1. システム設定 → ネットワーク → 詳細
2. DNS → `127.0.0.1` を追加

**Linux**:
```bash
echo "nameserver 127.0.0.1" | sudo tee /etc/resolv.conf
```

## 関連リンク

- [インストール手順](../common/installation.md)
- [ACL設定](../common/acl-configuration.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)

## 技術仕様

- **プロトコル**: DNS (RFC 1035)
- **対応レコード**: A, AAAA, CNAME, MX, NS, PTR, SOA
- **標準ポート**: 53 (UDP/TCP)
- **最大クエリサイズ**: 512バイト（UDP）、65535バイト（TCP）
- **タイムアウト**: 設定可能（デフォルト3秒）
- **デフォルトTTL**: 300秒

## サポート

問題が発生した場合は、以下を参照してください：
- [トラブルシューティング](troubleshooting.md)
- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
