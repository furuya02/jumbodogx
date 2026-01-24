# DHCPサーバー

JumboDogXのDHCPサーバーは、ネットワーク上のデバイスに自動的にIPアドレスを割り当てる機能を提供します。
RFC 2131に準拠し、ローカル開発環境やテストネットワークでのIP管理に最適です。

## 主な機能

### 基本機能
- **RFC 2131準拠** - 標準的なDHCPプロトコルに対応
- **動的IP割り当て** - IPアドレスプールからの自動割り当て
- **リース管理** - IPアドレスリースの時間管理

### セキュリティ機能
- **MACアドレスACL** - 特定のデバイスのみにIPを割り当て
- **IPプール制限** - 割り当て可能なIPアドレス範囲を制限
- **Allow/Denyモード** - 柔軟なアクセス制御

### 高度な機能
- **静的IP割り当て** - 特定のMACアドレスに固定IPを割り当て
- **DNSサーバー設定** - クライアントにDNSサーバー情報を配布
- **デフォルトゲートウェイ** - ルーター情報の配布
- **サブネットマスク設定** - ネットワーク構成の柔軟な設定

## クイックスタート

DHCPサーバーをすぐに使い始めたい方は、[クイックスタートガイド](getting-started.md)をご覧ください。

**重要**: DHCPサーバーの起動には管理者権限が必要です。

## ドキュメント一覧

### 基本設定
- [クイックスタート](getting-started.md) - 最初の設定と起動方法
- [IPプール設定](ip-pool-configuration.md) - IPアドレスプールの詳細設定
- [MAC ACL設定](mac-acl.md) - MACアドレスベースのアクセス制御

### トラブルシューティング
- [トラブルシューティング](troubleshooting.md) - よくある問題と解決方法

## 設定例

### シンプルなDHCPサーバー

```json
{
  "DhcpServer": {
    "Enabled": true,
    "BindAddress": "192.168.1.1",
    "SubnetMask": "255.255.255.0",
    "StartIp": "192.168.1.100",
    "EndIp": "192.168.1.200",
    "LeaseTime": 86400,
    "DnsServers": ["8.8.8.8", "8.8.4.4"],
    "Gateway": "192.168.1.1"
  }
}
```

### MAC ACL付きDHCPサーバー

```json
{
  "DhcpServer": {
    "Enabled": true,
    "BindAddress": "192.168.1.1",
    "SubnetMask": "255.255.255.0",
    "StartIp": "192.168.1.100",
    "EndIp": "192.168.1.200",
    "LeaseTime": 86400,
    "DnsServers": ["192.168.1.1"],
    "Gateway": "192.168.1.1",
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "00:11:22:33:44:55",
        "AA:BB:CC:DD:EE:FF"
      ]
    }
  }
}
```

### 静的IP割り当て設定

```json
{
  "DhcpServer": {
    "Enabled": true,
    "BindAddress": "192.168.1.1",
    "SubnetMask": "255.255.255.0",
    "StartIp": "192.168.1.100",
    "EndIp": "192.168.1.200",
    "LeaseTime": 86400,
    "DnsServers": ["192.168.1.1"],
    "Gateway": "192.168.1.1",
    "StaticLeases": [
      {
        "MacAddress": "00:11:22:33:44:55",
        "IpAddress": "192.168.1.50"
      },
      {
        "MacAddress": "AA:BB:CC:DD:EE:FF",
        "IpAddress": "192.168.1.51"
      }
    ]
  }
}
```

**ACLモード**:
- `Allow`: 許可リストモード（リストにあるMACのみ許可）
- `Deny`: 拒否リストモード（リストにあるMAC以外を許可）

## Web UI

JumboDogXは、Webブラウザから設定できる管理画面を提供しています。

### アクセス方法
1. JumboDogXを管理者権限で起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **DHCP** を選択

### 設定画面
- **General** - 基本設定（IPプール、リース時間）
- **Network** - DNS、ゲートウェイ設定
- **ACL** - MACアドレスアクセス制御
- **Static Leases** - 静的IP割り当て

## ユースケース

### ローカル開発環境
開発用のVMや物理マシンに自動的にIPアドレスを割り当て。

### テストネットワーク
隔離されたテスト環境でのネットワーク構成テスト。

### IoTデバイステスト
複数のIoTデバイスのネットワーク接続テスト。

### ネットワーク学習
DHCPプロトコルの仕組みを理解するための実験環境。

## クライアントからの利用

### Windowsでの設定

**IPアドレスを自動取得に設定**:
1. ネットワーク設定 → アダプターのオプション
2. イーサネット/Wi-Fi → プロパティ
3. IPv4 → プロパティ
4. 「IPアドレスを自動的に取得する」を選択

**IPアドレスの更新**:
```cmd
ipconfig /release
ipconfig /renew
```

### macOSでの設定

**IPアドレスを自動取得に設定**:
1. システム設定 → ネットワーク
2. 使用中の接続を選択 → 詳細
3. TCP/IP → IPv4の構成: DHCPサーバーを使用

**IPアドレスの更新**:
```bash
sudo ipconfig set en0 DHCP
```

### Linuxでの設定

**NetworkManager（Ubuntu等）**:
```bash
sudo nmcli con mod "接続名" ipv4.method auto
sudo nmcli con up "接続名"
```

**dhclientコマンド**:
```bash
sudo dhclient -r  # リリース
sudo dhclient     # 更新
```

## リース情報の確認

### Windowsでの確認
```cmd
ipconfig /all
```

### macOS/Linuxでの確認
```bash
# インターフェース情報確認
ip addr show

# リース情報確認（dhclientを使用している場合）
cat /var/lib/dhcp/dhclient.leases
```

## 関連リンク

- [インストール手順](../common/installation.md)
- [ACL設定](../common/acl-configuration.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)

## 技術仕様

- **プロトコル**: DHCP (RFC 2131)
- **標準ポート**: 67（サーバー）、68（クライアント）
- **デフォルトリース時間**: 86400秒（24時間）
- **IPアドレス形式**: IPv4
- **ACL**: MACアドレスベース
- **管理者権限**: 必須

## 注意事項

### ネットワーク隔離
既存のDHCPサーバー（ルーター等）と競合しないよう、隔離されたネットワークで使用することを推奨します。

### 管理者権限
DHCPサーバーはポート67/68を使用するため、管理者権限での実行が必要です。

### IPアドレス競合
他のデバイスと重複しないIPアドレスプールを設定してください。

## サポート

問題が発生した場合は、以下を参照してください：
- [トラブルシューティング](troubleshooting.md)
- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
