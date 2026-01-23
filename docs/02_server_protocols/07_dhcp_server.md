# DHCPサーバー

## 1. 概要

JumboDogX DHCPサーバーは、IPアドレスの自動割り当てを行います。

### 1.1 主要機能

- DHCP (RFC 2131)
- 動的IPアドレス割り当て
- 静的IPアドレス予約
- DHCPオプション
- リース管理
- ACL (アクセス制御 - MACアドレスベース)

## 2. プロジェクト構造

```
Jdx.Servers.Dhcp/
├── DhcpServer.cs              # メインサーバークラス
├── DhcpProtocol.cs            # DHCPプロトコル処理
├── DhcpPacket.cs              # パケット処理
├── LeaseManager.cs            # リース管理
├── AddressPool.cs             # アドレスプール
└── Options/                   # DHCPオプション
```

## 3. 設定

### 3.1 基本設定

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 67 (サーバー) / 68 (クライアント) | UDPポート |
| Lease Time | 86400 | デフォルトリース時間（秒） |

### 3.2 アドレスプール設定

```json
{
  "Dhcp": {
    "AddressPool": {
      "StartAddress": "192.168.1.100",
      "EndAddress": "192.168.1.200",
      "SubnetMask": "255.255.255.0",
      "DefaultGateway": "192.168.1.1",
      "DnsServers": ["192.168.1.1", "8.8.8.8"],
      "LeaseTime": 86400
    }
  }
}
```

### 3.3 静的予約

```json
{
  "Reservations": [
    {
      "MacAddress": "00:11:22:33:44:55",
      "IpAddress": "192.168.1.10",
      "Hostname": "server1"
    }
  ]
}
```

### 3.4 ACL設定

MACアドレスベースのアクセス制御が可能です。

```json
{
  "Dhcp": {
    "EnableAcl": true,
    "AclRules": [
      {
        "Allow": ["00:11:22:33:44:55", "AA:BB:CC:*"],
        "Deny": ["*"]
      }
    ]
  }
}
```

**ACL設定:**
- `EnableAcl`: ACL機能の有効/無効
- `Allow`: 許可するMACアドレス（ワイルドカード対応）
- `Deny`: 拒否するMACアドレス（ワイルドカード対応）
- ルールは上から順に評価
- 許可されないMACアドレスからのDHCP要求は無視される

## 4. DHCPメッセージタイプ

| タイプ | 説明 |
|--------|------|
| DISCOVER | クライアントがサーバーを探索 |
| OFFER | サーバーがIPアドレスを提案 |
| REQUEST | クライアントがIPアドレスを要求 |
| ACK | サーバーがIPアドレスを確認 |
| NAK | サーバーがIPアドレスを拒否 |
| DECLINE | クライアントがIPアドレスを拒否 |
| RELEASE | クライアントがIPアドレスを解放 |
| INFORM | クライアントが設定情報のみを要求 |

## 5. DHCPフロー

### 5.1 DORA（通常フロー）

```
Client                Server
   |--- DISCOVER --->|
   |<-- OFFER -------|
   |--- REQUEST ---->|
   |<-- ACK ---------|
```

### 5.2 リース更新

```
Client                Server
   |--- REQUEST ---->|  (リース期間の50%経過時)
   |<-- ACK ---------|
```

## 6. DHCPオプション

### 6.1 主要オプション

| コード | 名前 | 説明 |
|--------|------|------|
| 1 | Subnet Mask | サブネットマスク |
| 3 | Router | デフォルトゲートウェイ |
| 6 | DNS Server | DNSサーバー |
| 12 | Hostname | ホスト名 |
| 15 | Domain Name | ドメイン名 |
| 51 | Lease Time | リース時間 |
| 53 | Message Type | DHCPメッセージタイプ |
| 54 | Server ID | DHCPサーバーID |

## 7. リース管理

### 7.1 リース状態

- **Available**: 利用可能
- **Offered**: 提案中
- **Leased**: リース中
- **Expired**: 期限切れ

### 7.2 Web UI機能

- 現在のリース一覧表示
- リースの手動解放
- 予約の追加・編集・削除

## 8. テスト

テストプロジェクト: `tests/Jdx.Servers.Dhcp.Tests/`

```bash
dotnet test tests/Jdx.Servers.Dhcp.Tests
```

## 更新履歴

- 2026-01-24: ACL（アクセス制御リスト - MACアドレスベース）機能の追加
