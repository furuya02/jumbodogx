# ACL（アクセス制御リスト）機能概要

本ドキュメントでは、JumboDogXのACL（Access Control List）機能の設計・実装・使用方法について包括的にまとめます。

## 1. ACLとは

ACL（Access Control List）は、IPアドレスまたはMACアドレスに基づいてサーバーへの接続を許可・拒否する機能です。JumboDogXでは、各プロトコルサーバーごとに独立したACL設定を持ち、きめ細かなアクセス制御を実現しています。

## 2. 対応サーバー一覧

| サーバー | ACLフィルタ | アドレス種別 | 備考 |
|---------|------------|------------|------|
| HTTP/HTTPS | `HttpAclFilter` | IPアドレス | Virtual Host単位での設定に対応 |
| DNS | `DnsAclFilter` | IPアドレス | |
| FTP | `FtpAclFilter` | IPアドレス | |
| TFTP | `TftpAclFilter` | IPアドレス | |
| POP3 | `Pop3AclFilter` | IPアドレス | |
| SMTP | 設定のみ（UIあり） | IPアドレス | サーバー側フィルタ未実装 |
| Proxy | 設定のみ（UIあり） | IPアドレス | サーバー側フィルタ未実装 |
| DHCP | `DhcpMacAclFilter` | MACアドレス | 許可リストモード専用 |

## 3. 動作モード

### 3.1 Allow Mode（許可リスト / EnableAcl = 0）

- **リストに登録されたアドレスのみ許可**し、それ以外はすべて拒否する
- **リストが空の場合**: すべて拒否（fail-secure）
- **用途**: 社内ネットワークや特定端末からのみアクセスさせたい場合

### 3.2 Deny Mode（拒否リスト / EnableAcl = 1）

- **リストに登録されたアドレスのみ拒否**し、それ以外はすべて許可する
- **リストが空の場合**: すべて許可
- **用途**: 攻撃元IPなど特定のアドレスだけをブロックしたい場合

### 3.3 判定フロー

```
接続要求
  │
  ├─ ACLリストが空？
  │   ├─ Allow Mode → 拒否（fail-secure）
  │   └─ Deny Mode  → 許可
  │
  ├─ IPアドレスがリストに一致？
  │   ├─ Allow Mode + 一致   → 許可
  │   ├─ Allow Mode + 不一致 → 拒否
  │   ├─ Deny Mode  + 一致   → 拒否
  │   └─ Deny Mode  + 不一致 → 許可
```

## 4. データモデル

### 4.1 AclEntry（IPアドレスベース）

```csharp
// src/Jdx.Core/Settings/ApplicationSettings.cs
public class AclEntry
{
    public string Name { get; set; } = "";        // 説明（例: "Office Network"）
    public string Address { get; set; } = "";     // IPアドレスまたはCIDR範囲
}
```

各サーバー設定には以下のプロパティが含まれます:

| プロパティ | 型 | 説明 |
|-----------|-----|------|
| `EnableAcl` | `int` | 0 = Allow Mode, 1 = Deny Mode |
| `AclList` | `List<AclEntry>` | ACLエントリのリスト |

### 4.2 DhcpMacEntry（MACアドレスベース、DHCP専用）

DHCPサーバーは独自のMAC ACLを使用します:

| プロパティ | 型 | 説明 |
|-----------|-----|------|
| `UseMacAcl` | `bool` | MAC ACLの有効/無効 |
| `MacAclList` | `List<DhcpMacEntry>` | MACアドレスエントリのリスト |

## 5. サポートするアドレス形式

### 5.1 IPアドレス形式

| 形式 | 例 | 説明 |
|------|---|------|
| 単一IPv4 | `192.168.1.100` | 特定のIPアドレス1つ |
| CIDR（IPv4） | `192.168.1.0/24` | ネットワーク範囲（推奨） |
| 単一IPv6 | `::1` | IPv6アドレス |
| CIDR（IPv6） | `2001:db8::/32` | IPv6ネットワーク範囲 |

IPアドレスのマッチングロジックは `IpAddressMatcher` クラス（`src/Jdx.Core/Network/IpAddressMatcher.cs`）に実装されています。

#### よく使うCIDR範囲

| CIDR | 範囲 | ホスト数 |
|------|------|---------|
| `/32` | 1アドレス | 1 |
| `/24` | 256アドレス | 256 |
| `/16` | 65,536アドレス | 65,536 |
| `/8` | 16,777,216アドレス | 16,777,216 |

### 5.2 MACアドレス形式（DHCP専用）

| 形式 | 例 |
|------|---|
| コロン区切り（推奨） | `00:11:22:33:44:55` |
| ハイフン区切り | `00-11-22-33-44-55` |
| 区切りなし | `001122334455` |

大文字・小文字は区別されません。内部的にはセパレータを除去して正規化した上で比較を行います。

## 6. 実装アーキテクチャ

### 6.1 コアコンポーネント

```
src/Jdx.Core/
├── Settings/ApplicationSettings.cs    # AclEntry モデル定義、各サーバー設定
└── Network/IpAddressMatcher.cs        # IPアドレスマッチングエンジン
```

### 6.2 サーバー別フィルタ

各サーバーの ACL フィルタは共通のパターンで実装されており、`IsAllowed()` メソッドを提供します。

```
src/Jdx.Servers.Http/HttpAclFilter.cs
src/Jdx.Servers.Dns/DnsAclFilter.cs
src/Jdx.Servers.Ftp/FtpAclFilter.cs
src/Jdx.Servers.Tftp/TftpAclFilter.cs
src/Jdx.Servers.Pop3/Pop3AclFilter.cs
src/Jdx.Servers.Dhcp/DhcpMacAclFilter.cs
```

### 6.3 共通フィルタロジック（IPベース）

```csharp
public bool IsAllowed(string remoteAddress)
{
    // 1. ACLリストが空の場合
    //    Allow Mode → deny all（fail-secure）
    //    Deny Mode  → allow all
    if (AclList.Count == 0)
        return EnableAcl != 0;

    // 2. IPアドレスをパース
    if (!IPAddress.TryParse(remoteAddress, out var ipAddress))
        return false;  // パース失敗は拒否

    // 3. リスト内のエントリと照合
    bool matches = AclList.Any(entry =>
        IpAddressMatcher.Matches(ipAddress, entry.Address));

    // 4. モードに応じて判定
    //    Allow Mode: 一致 → 許可, 不一致 → 拒否
    //    Deny Mode:  一致 → 拒否, 不一致 → 許可
    return EnableAcl == 0 ? matches : !matches;
}
```

### 6.4 DHCP MACフィルタの特徴

- `UseMacAcl` フラグで有効/無効を制御
- **許可リスト専用**（Allowモードのみ）
- MACアドレスはセパレータ除去+大文字化で正規化
- `HashSet<string>` による高速な照合

### 6.5 Web UI コンポーネント

```
src/Jdx.WebUI/Components/
├── Shared/AclManager.razor              # 共通ACL管理コンポーネント
└── Pages/Settings/
    ├── Http/Acl.razor                   # HTTP ACL（Virtual Host対応）
    ├── Dns/Acl.razor                    # DNS ACL
    ├── Ftp/Acl.razor                    # FTP ACL
    ├── Tftp/Acl.razor                   # TFTP ACL
    ├── Pop3/Acl.razor                   # POP3 ACL
    ├── Smtp/Acl.razor                   # SMTP ACL
    ├── Proxy/Acl.razor                  # Proxy ACL
    └── Dhcp/MacAcl.razor               # DHCP MAC ACL
```

共通コンポーネント `AclManager.razor` は以下の機能を提供します:
- Allow / Deny モード切替（ラジオボタン）
- ACLエントリのCRUD（テーブル形式）
- カスタムバリデーション関数のサポート
- Virtual Host対応
- 警告・ヘルプセクションのカスタマイズ

## 7. 使用方法

### 7.1 Web UIでの設定手順

1. サイドメニューから **Settings** を開く
2. 対象サーバーのセクション（例: HTTP/HTTPS）を展開
3. **ACL** メニューをクリック
4. **ACL Mode** で Allow Mode / Deny Mode を選択
5. **+ Add ACL Entry** で新しいエントリを追加
   - **Name**: エントリの説明（例: `Office Network`）
   - **IP Address / Range**: IPアドレスまたはCIDR（例: `192.168.1.0/24`）
6. **Save Settings** をクリック

### 7.2 HTTP Virtual Host別の設定

HTTPサーバーでは、Virtual Host単位でACLを設定できます:

1. **Settings** > **HTTP/HTTPS** > 対象Virtual Hostを展開
2. **ACL** をクリック
3. そのVirtual Hostに固有のACLルールを設定

### 7.3 DHCP MAC ACLの設定

1. **Settings** > **DHCP** > **ACL** を開く
2. MACアドレスACLを有効化
3. 許可するデバイスのMACアドレスを追加
4. **Save Settings** をクリック

## 8. 設定例

### 例1: localhostのみ許可（開発環境）

| 項目 | 設定 |
|------|------|
| Mode | Allow Mode |
| エントリ1 | Name: `Localhost`, Address: `127.0.0.1` |
| エントリ2 | Name: `IPv6 Localhost`, Address: `::1` |

### 例2: 社内ネットワークを許可

| 項目 | 設定 |
|------|------|
| Mode | Allow Mode |
| エントリ1 | Name: `Office Network`, Address: `192.168.1.0/24` |
| エントリ2 | Name: `VPN Users`, Address: `10.0.0.0/24` |

### 例3: 攻撃元IPをブロック

| 項目 | 設定 |
|------|------|
| Mode | Deny Mode |
| エントリ1 | Name: `Blocked Attacker`, Address: `203.0.113.50` |
| エントリ2 | Name: `Spam Network`, Address: `198.51.100.0/24` |

### 例4: DHCPで特定デバイスのみ許可

| 項目 | 設定 |
|------|------|
| UseMacAcl | `true` |
| エントリ1 | MAC: `00:11:22:33:44:55`（Office PC 1） |
| エントリ2 | MAC: `00:11:22:33:44:66`（Office PC 2） |

## 9. 攻撃検出連携（AttackDb）

HTTPサーバーには攻撃検出システム `HttpAttackDb`（`src/Jdx.Servers.Http/HttpAttackDb.cs`）が実装されています。

- 一定時間内に複数回の不正アクセスを検出するとアラートを発生
- Apache Killer攻撃などのパターンを検出
- **ACL自動追加機能は未実装**（TODOとしてコードに記載あり）

## 10. セキュリティ特性

### Fail-Secure設計

Allow Modeでリストが空の場合、すべての接続を拒否します。これにより、設定ミスで意図しないアクセスを許可するリスクを防ぎます。

### ログ出力

すべてのACL判定結果（許可/拒否）がログに記録されます:
- 許可: `LogDebug` レベル
- 拒否: `LogWarning` レベル
- ログには接続元アドレス、ACLモード、マッチしたルールが含まれます

### 推奨事項

1. **Allow Modeの使用を推奨** - より厳格なセキュリティを実現
2. **CIDR表記の使用を推奨** - 明確で標準的なネットワーク範囲指定
3. **定期的なACLの見直し** - 不要なエントリの削除、新拠点の追加
4. **IPv6対応の確認** - `127.0.0.1` だけでなく `::1` も登録
5. **ログの監視** - 拒否されたアクセスの定期的な確認

## 11. テスト

ACLのテストは `tests/Jdx.Servers.Http.Tests/HttpAclFilterTests.cs` に実装されており、以下のケースをカバーしています:

- 空ACLリストの動作（fail-secure確認）
- Allow Modeでのリスト内/外IPの判定
- Deny Modeでのリスト内/外IPの判定
- CIDR範囲マッチング
- 無効なIPアドレスの処理
- 複数ACLエントリの動作

## 12. 既知の制限事項

1. **SMTP/Proxy**: UI上にACL設定画面はあるが、サーバー側のフィルタ実装がない
2. **AttackDb自動ACL追加**: 攻撃検出は動作するが、ACLへの自動追加は未実装
3. **DHCP MAC ACL**: 許可リストモードのみ（拒否リストモードなし）
4. **正規表現/ワイルドカード**: IPアドレスのパターンマッチングでは正規表現やワイルドカードは非対応（CIDR表記のみ）

## 関連ドキュメント

- [ACL設定ガイド（マニュアル）](../../manual/common/acl-configuration.md)
- [DHCP MAC ACL設定（マニュアル）](../../manual/dhcp/mac-acl.md)
- [アーキテクチャ設計](../01_development_docs/01_architecture_design.md)
- [ロギング設計](../01_development_docs/04_logging_design.md)
