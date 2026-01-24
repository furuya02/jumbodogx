# DHCPサーバー - MAC ACL設定

JumboDogXのDHCPサーバーにおけるMACアドレスベースのアクセス制御（ACL）について説明します。

## MAC ACLとは

MAC ACL（Media Access Control Access Control List）は、デバイスのMACアドレスに基づいてDHCPサーバーへのアクセスを制御する仕組みです。特定のデバイスのみにIPアドレスを割り当てたり、特定のデバイスをブロックしたりできます。

## ACLの動作モード

### Allow Mode（許可リストモード）

リストに登録されているMACアドレスのデバイス**のみ**にIPアドレスを割り当てます。

**用途**:
- セキュリティが重要な環境
- 許可されたデバイスのみが接続できるネットワーク
- ゲストネットワークの完全な制御

**設定例**:
```json
{
  "DhcpServer": {
    "Enabled": true,
    "BindAddress": "192.168.1.1",
    "SubnetMask": "255.255.255.0",
    "StartIp": "192.168.1.100",
    "EndIp": "192.168.1.200",
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "00:11:22:33:44:55",
        "AA:BB:CC:DD:EE:FF",
        "11:22:33:44:55:66"
      ]
    }
  }
}
```

この設定では、3つのMACアドレスのデバイスのみがIPアドレスを取得できます。

---

### Deny Mode（拒否リストモード）

リストに登録されているMACアドレスのデバイス**以外**にIPアドレスを割り当てます。

**用途**:
- 特定のデバイスをブロックしたい場合
- ほとんどのデバイスを許可し、一部のみを拒否
- 問題のあるデバイスの隔離

**設定例**:
```json
{
  "DhcpServer": {
    "Enabled": true,
    "BindAddress": "192.168.1.1",
    "SubnetMask": "255.255.255.0",
    "StartIp": "192.168.1.100",
    "EndIp": "192.168.1.200",
    "Acl": {
      "Mode": "Deny",
      "Entries": [
        "00:11:22:33:44:55",
        "AA:BB:CC:DD:EE:FF"
      ]
    }
  }
}
```

この設定では、2つのMACアドレスのデバイス以外のすべてのデバイスがIPアドレスを取得できます。

---

## MACアドレスの形式

### 一般的な形式

MACアドレスは以下の形式で指定します：

- **コロン区切り**: `00:11:22:33:44:55`（推奨）
- **ハイフン区切り**: `00-11-22-33-44-55`
- **ドット区切り**: `0011.2233.4455`（Cisco形式）

**JumboDogXではコロン区切りを推奨します。**

### 大文字・小文字

大文字・小文字は区別されません。以下はすべて同じMACアドレスです：

- `00:11:22:33:44:55`
- `00:11:22:33:44:55`
- `00:11:22:33:44:55`

---

## MACアドレスの確認方法

### Windows

**コマンドプロンプト**:
```cmd
ipconfig /all
```

出力例:
```
イーサネット アダプター イーサネット:
   物理アドレス: 00-11-22-33-44-55
```

---

### macOS

**ターミナル**:
```bash
ifconfig | grep ether
```

出力例:
```
ether 00:11:22:33:44:55
```

または、システム設定から:
1. システム設定 → ネットワーク
2. 接続を選択 → 詳細 → ハードウェア
3. MACアドレスが表示される

---

### Linux

**ターミナル**:
```bash
ip link show

# または
ifconfig
```

出力例:
```
2: eth0: <BROADCAST,MULTICAST,UP,LOWER_UP> mtu 1500 qdisc pfifo_fast state UP mode DEFAULT group default qlen 1000
    link/ether 00:11:22:33:44:55 brd ff:ff:ff:ff:ff:ff
```

---

### Android

1. 設定 → 端末情報 → ステータス
2. Wi-Fi MACアドレスを確認

---

### iOS

1. 設定 → 一般 → 情報
2. Wi-Fiアドレスを確認

**注意**: iOS 14以降では、プライベートWi-Fiアドレス機能によりMACアドレスがランダム化される場合があります。設定でオフにすることができます。

---

## ACLエントリの管理

### 基本的なACL設定

```json
{
  "DhcpServer": {
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "00:11:22:33:44:55",
        "AA:BB:CC:DD:EE:FF",
        "11:22:33:44:55:66"
      ]
    }
  }
}
```

### 説明付きACLエントリ

管理しやすくするため、コメントを追加できます（将来のバージョンで対応予定）：

```json
{
  "DhcpServer": {
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        {
          "MacAddress": "00:11:22:33:44:55",
          "Description": "John's Laptop"
        },
        {
          "MacAddress": "AA:BB:CC:DD:EE:FF",
          "Description": "Conference Room Projector"
        }
      ]
    }
  }
}
```

**注意**: 現在のバージョンでは、単純な文字列配列のみをサポートしています。

---

## Web UIでのMAC ACL設定

### ACLモードの選択

1. JumboDogXを管理者権限で起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **DHCP** → **ACL** を選択
4. ACL Modeを選択:
   - **Allow Mode**: 許可リストモード
   - **Deny Mode**: 拒否リストモード
5. **Save Settings** ボタンをクリック

![ACLモード選択画面](images/dhcp-acl-mode.png)

### MACアドレスの追加

1. ACL設定画面で **Add Entry** ボタンをクリック
2. MACアドレスを入力（例: `00:11:22:33:44:55`）
3. **Add** ボタンをクリック
4. **Save Settings** ボタンをクリック

![MACアドレス追加画面](images/dhcp-mac-add.png)

### MACアドレスの削除

1. ACL設定画面でエントリ一覧を表示
2. 削除したいエントリの **Delete** ボタンをクリック
3. 確認ダイアログで **OK** をクリック
4. **Save Settings** ボタンをクリック

---

## 設定例集

### 企業オフィス（Allow Mode）

従業員のデバイスのみを許可:

```json
{
  "DhcpServer": {
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "00:11:22:33:44:55",  // 従業員1 Laptop
        "AA:BB:CC:DD:EE:FF",  // 従業員2 Laptop
        "11:22:33:44:55:66",  // 会議室プロジェクター
        "22:33:44:55:66:77",  // 共用プリンター
        "33:44:55:66:77:88"   // 従業員3 Desktop
      ]
    }
  }
}
```

### ゲストネットワーク（Deny Mode）

問題のあるデバイスのみをブロック:

```json
{
  "DhcpServer": {
    "Acl": {
      "Mode": "Deny",
      "Entries": [
        "00:11:22:33:44:55",  // 不正なデバイス
        "AA:BB:CC:DD:EE:FF"   // ウイルス感染デバイス
      ]
    }
  }
}
```

### IoTデバイス環境（Allow Mode）

IoTデバイスのみを許可:

```json
{
  "DhcpServer": {
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "00:11:22:33:44:55",  // スマートTV
        "AA:BB:CC:DD:EE:FF",  // セキュリティカメラ1
        "11:22:33:44:55:66",  // セキュリティカメラ2
        "22:33:44:55:66:77",  // スマートスピーカー
        "33:44:55:66:77:88"   // スマート照明ハブ
      ]
    }
  }
}
```

### 開発環境（ACLなし）

開発環境ではACLを無効化:

```json
{
  "DhcpServer": {
    "Acl": null  // ACLを無効化
  }
}
```

または:

```json
{
  "DhcpServer": {
    // Aclキーを削除
  }
}
```

---

## ACLと静的IP割り当ての組み合わせ

Allow ModeのACLと静的IP割り当てを組み合わせることで、特定のデバイスに固定IPを割り当てつつ、他のデバイスをブロックできます。

```json
{
  "DhcpServer": {
    "Enabled": true,
    "BindAddress": "192.168.1.1",
    "SubnetMask": "255.255.255.0",
    "StartIp": "192.168.1.100",
    "EndIp": "192.168.1.200",
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "00:11:22:33:44:55",  // サーバー（静的IP）
        "AA:BB:CC:DD:EE:FF",  // クライアント1（動的IP）
        "11:22:33:44:55:66"   // クライアント2（動的IP）
      ]
    },
    "StaticLeases": [
      {
        "MacAddress": "00:11:22:33:44:55",
        "IpAddress": "192.168.1.10"  // サーバーに固定IP
      }
    ]
  }
}
```

この設定では：
- `00:11:22:33:44:55`（サーバー）は`192.168.1.10`を取得
- `AA:BB:CC:DD:EE:FF`と`11:22:33:44:55:66`はIPプールから動的にIPを取得
- その他のデバイスはIPを取得できない

---

## セキュリティのベストプラクティス

### 1. Allow Modeを使用（推奨）

セキュリティが重要な環境では、Allow Modeを使用してください。

```json
{
  "DhcpServer": {
    "Acl": {
      "Mode": "Allow",
      "Entries": [/* 許可されたMACアドレスのみ */]
    }
  }
}
```

### 2. 定期的なレビュー

ACLエントリを定期的にレビューし、不要なエントリを削除してください。

### 3. MACアドレススプーフィングへの対策

MACアドレスは偽装（スプーフィング）可能です。より高いセキュリティが必要な場合は、以下の対策を検討してください：

- 802.1X認証の使用
- VLANによるネットワーク分離
- ファイアウォールルールの追加

### 4. ログの監視

DHCPサーバーのログを監視し、不正なアクセス試行を検出してください。

```bash
# ログの確認
grep DHCP logs/jumbodogx.log

# ACL拒否のログを確認
grep "ACL denied" logs/jumbodogx.log
```

---

## トラブルシューティング

### クライアントがIPアドレスを取得できない

**原因1: MACアドレスがACLに登録されていない（Allow Mode）**

**解決方法**: クライアントのMACアドレスをACLに追加してください

```bash
# クライアントのMACアドレスを確認
ipconfig /all  # Windows
ifconfig       # macOS/Linux
```

```json
{
  "DhcpServer": {
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "00:11:22:33:44:55"  # ← クライアントのMACを追加
      ]
    }
  }
}
```

---

**原因2: MACアドレスがACLに登録されている（Deny Mode）**

**解決方法**: クライアントのMACアドレスをACLから削除してください

---

**原因3: MACアドレスの形式が間違っている**

**解決方法**: MACアドレスがコロン区切りで正しく入力されているか確認してください

- 正しい: `00:11:22:33:44:55`
- 間違い: `00-11-22-33-44-55`（ハイフンは使用不可）
- 間違い: `0011.2233.4455`（ドットは使用不可）

---

### ACLが動作しない

**原因: ACLが無効になっている**

**解決方法**: ACL設定が正しく設定されているか確認してください

```json
{
  "DhcpServer": {
    "Acl": {
      "Mode": "Allow",  # ← Modeが設定されているか確認
      "Entries": [...]
    }
  }
}
```

---

## ログの確認

ACLの動作状況はログで確認できます。

```bash
# ACLに関するログを確認
grep "ACL" logs/jumbodogx.log

# 拒否されたリクエストを確認
grep "denied" logs/jumbodogx.log

# 許可されたリクエストを確認
grep "allowed" logs/jumbodogx.log
```

詳細は[ロギング設定ガイド](../common/logging.md)を参照してください。

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [IPプール設定](ip-pool-configuration.md)
- [トラブルシューティング](troubleshooting.md)
- [ACL設定](../common/acl-configuration.md)
