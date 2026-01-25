# DHCPサーバー - トラブルシューティング

JumboDogXのDHCPサーバーで発生する可能性のある問題と解決方法を説明します。

## 重要な注意事項

**DHCPサーバーの使用には管理者権限が必要です。**

また、既存のDHCPサーバー（ルーター等）と競合しないよう、隔離されたネットワークで使用することを強く推奨します。

---

## 一般的な問題

### DHCPサーバーが起動しない

#### 症状
- JumboDogXを起動してもDHCPサーバーが動作しない
- ダッシュボードでDHCPサーバーのステータスが"Stopped"または"Error"

#### 原因と解決方法

**原因1: 管理者権限がない**

DHCPサーバーはポート67/68を使用するため、管理者権限が**必須**です。

**解決方法**:

**Windows**:
```cmd
# コマンドプロンプトを「管理者として実行」
# PowerShellを「管理者として実行」

# または、Visual Studioを管理者として実行
```

**macOS**:
```bash
# sudoで実行
sudo dotnet run --project src/Jdx.WebUI
```

**Linux**:
```bash
# sudoで実行
sudo dotnet run --project src/Jdx.WebUI
```

---

**原因2: ポート67/68が既に使用されている**

既存のDHCPサーバー（dnsmasq、ISC DHCP等）がポートを使用している場合があります。

**解決方法**:

**macOS**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :67
sudo lsof -i :68
```

**Windows**:
```cmd
# 使用中のプロセスを確認
netstat -ano | findstr :67
netstat -ano | findstr :68
```

**Linux**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :67
sudo lsof -i :68

# dnsmasqが動作している場合、停止
sudo systemctl stop dnsmasq

# ISC DHCPが動作している場合、停止
sudo systemctl stop isc-dhcp-server
```

---

**原因3: `Enabled`が`false`になっている**

**解決方法**:
```json
{
  "DhcpServer": {
    "Enabled": true,  # ← trueに設定
    "BindAddress": "192.168.1.1",
    "SubnetMask": "255.255.255.0"
  }
}
```

---

**原因4: BindAddressが間違っている**

BindAddressは、DHCPサーバー自身が使用しているIPアドレスである必要があります。

**解決方法**: 自身のIPアドレスを確認してください

**Windows**:
```cmd
ipconfig
```

**macOS/Linux**:
```bash
ifconfig
# または
ip addr show
```

設定ファイルに正しいIPアドレスを設定:
```json
{
  "DhcpServer": {
    "BindAddress": "192.168.1.1"  # ← 自身のIPアドレス
  }
}
```

---

### クライアントがIPアドレスを取得できない

#### 症状
- クライアントがDHCPサーバーからIPアドレスを取得できない
- "IPアドレスを取得できませんでした"というエラーが表示される

#### 原因と解決方法

**原因1: ファイアウォールでポートがブロックされている**

**解決方法**:

**Windows**:
```powershell
# ファイアウォールルールを追加
New-NetFirewallRule -DisplayName "JumboDogX DHCP In" -Direction Inbound -Protocol UDP -LocalPort 67 -Action Allow
New-NetFirewallRule -DisplayName "JumboDogX DHCP Out" -Direction Outbound -Protocol UDP -RemotePort 68 -Action Allow
```

**macOS**:
```bash
# ファイアウォール設定を確認
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --getglobalstate

# JumboDogXを許可リストに追加
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --add /path/to/jumbodogx
```

**Linux (ufw)**:
```bash
# ポート67/68を許可
sudo ufw allow 67/udp
sudo ufw allow 68/udp
```

---

**原因2: ネットワークが隔離されていない**

既存のDHCPサーバー（ルーター等）と競合している可能性があります。

**解決方法**: 隔離されたネットワークで使用してください

- 既存のルーターのDHCP機能を無効化
- または、別のネットワークセグメントを使用

---

**原因3: IPプールが枯渇している**

**解決方法**: IPプールの範囲を確認してください

```bash
# ログでIPプールの状態を確認
grep "IP pool exhausted" logs/jumbodogx.log
```

IPプールを拡大:
```json
{
  "DhcpServer": {
    "StartIp": "192.168.1.100",
    "EndIp": "192.168.1.250"  # 範囲を拡大
  }
}
```

---

**原因4: MAC ACLで拒否されている**

**解決方法**: クライアントのMACアドレスを確認し、ACLに追加してください

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

詳細は[MAC ACL設定ガイド](mac-acl.md)を参照してください。

---

**原因5: サブネットマスクが間違っている**

**解決方法**: サブネットマスクとIPアドレス範囲が一致するか確認してください

```json
{
  "DhcpServer": {
    "BindAddress": "192.168.1.1",
    "SubnetMask": "255.255.255.0",  # /24
    "StartIp": "192.168.1.100",     # 同じサブネット内
    "EndIp": "192.168.1.200"        # 同じサブネット内
  }
}
```

---

### IPアドレスが重複する

#### 症状
- "IP address conflict"（IPアドレス競合）エラーが発生する
- 同じIPアドレスが複数のデバイスに割り当てられている

#### 原因と解決方法

**原因1: 静的IPがIPプール内にある**

**解決方法**: 静的IPをIPプールの範囲外に設定してください

```json
{
  "DhcpServer": {
    "StartIp": "192.168.1.100",
    "EndIp": "192.168.1.200",
    "StaticLeases": [
      {
        "MacAddress": "00:11:22:33:44:55",
        "IpAddress": "192.168.1.50"  # プール外に設定
      }
    ]
  }
}
```

---

**原因2: 既存のデバイスがIPプール内のIPを使用している**

**解決方法**: 既存のデバイスを確認し、IPプールの範囲を調整してください

```bash
# ネットワークスキャン（nmap）
nmap -sn 192.168.1.0/24

# または、arpコマンド
arp -a
```

---

**原因3: 複数のDHCPサーバーが動作している**

**解決方法**: 既存のDHCPサーバー（ルーター等）を無効化してください

---

## ネットワーク設定の問題

### クライアントがインターネットに接続できない

#### 症状
- クライアントはIPアドレスを取得できるが、インターネットに接続できない

#### 原因と解決方法

**原因1: ゲートウェイが設定されていない**

**解決方法**: デフォルトゲートウェイを設定してください

```json
{
  "DhcpServer": {
    "Gateway": "192.168.1.1"  # ルーターのIPアドレス
  }
}
```

---

**原因2: DNSサーバーが設定されていない**

**解決方法**: DNSサーバーを設定してください

```json
{
  "DhcpServer": {
    "DnsServers": ["8.8.8.8", "8.8.4.4"]  # Google DNS
  }
}
```

---

**原因3: ゲートウェイのIPアドレスが間違っている**

**解決方法**: 正しいゲートウェイのIPアドレスを確認してください

```bash
# Windowsでゲートウェイを確認
ipconfig | findstr "ゲートウェイ"

# macOS/Linuxでゲートウェイを確認
netstat -rn | grep default
```

---

### DNSが解決できない

#### 症状
- クライアントがドメイン名を解決できない
- IPアドレスでは接続できるが、ドメイン名では接続できない

#### 原因と解決方法

**原因: DNSサーバーが正しく設定されていない**

**解決方法**: 動作するDNSサーバーを設定してください

```json
{
  "DhcpServer": {
    "DnsServers": [
      "8.8.8.8",      # Google DNS (プライマリ)
      "8.8.4.4",      # Google DNS (セカンダリ)
      "1.1.1.1"       # Cloudflare DNS (ターシャリ)
    ]
  }
}
```

DNSサーバーのテスト:
```bash
# nslookupでテスト
nslookup google.com 8.8.8.8
```

---

## リース時間の問題

### IPアドレスがすぐに解放される

#### 症状
- クライアントのIPアドレスが頻繁に変わる

#### 原因と解決方法

**原因: リース時間が短すぎる**

**解決方法**: リース時間を長くしてください

```json
{
  "DhcpServer": {
    "LeaseTime": 86400  # 24時間（デフォルト）
  }
}
```

リース時間の推奨値:
- 開発環境: 3600秒（1時間）
- 一般的なオフィス: 28800秒（8時間）
- 標準設定: 86400秒（24時間）
- 安定した環境: 604800秒（1週間）

---

## パフォーマンスの問題

### DHCPサーバーの応答が遅い

#### 症状
- クライアントがIPアドレスを取得するのに時間がかかる

#### 原因と解決方法

**原因1: ネットワーク遅延**

**解決方法**: ネットワークの遅延を確認してください

```bash
# クライアントからDHCPサーバーへのping
ping 192.168.1.1
```

---

**原因2: リソース不足**

**解決方法**: サーバーのCPU/メモリ使用率を確認してください

**Windows**:
```cmd
# タスクマネージャーで確認
taskmgr
```

**macOS**:
```bash
# アクティビティモニタで確認
open -a "Activity Monitor"
```

**Linux**:
```bash
# topコマンドで確認
top
```

---

## Web UI関連の問題

### Web UIで設定が保存できない

#### 症状
- "Save Settings"ボタンをクリックしてもエラーが発生する

#### 原因と解決方法

**原因: ファイルのアクセス権限**

**解決方法**: 設定ファイルに書き込み権限があるか確認してください

```bash
# macOS/Linux
ls -l appsettings.json
chmod 644 appsettings.json  # 必要に応じて権限を変更

# Windows
icacls appsettings.json
```

---

### Web UIで設定が反映されない

#### 症状
- Web UIで設定を変更しても、DHCPサーバーに反映されない

#### 原因と解決方法

**原因: 設定の再読み込みが必要**

**解決方法**: JumboDogXを再起動してください

```bash
# JumboDogXを停止（Ctrl+C）
# 再度起動
sudo dotnet run --project src/Jdx.WebUI
```

---

## デバッグ方法

### ログの確認

JumboDogXのログファイルを確認して、エラーの詳細を調べてください。

**ログファイルの場所**:
```
logs/jumbodogx.log
logs/jumbodogx-YYYYMMDD.log
```

**ログの確認**:
```bash
# 最新のログを表示
tail -f logs/jumbodogx.log

# DHCPに関するログのみを表示
grep DHCP logs/jumbodogx.log

# エラーのみを表示
grep ERROR logs/jumbodogx.log

# ACL拒否のログを表示
grep "ACL denied" logs/jumbodogx.log
```

詳細は[ロギング設定ガイド](../common/logging.md)を参照してください。

---

### ネットワークキャプチャ

Wiresharkなどのパケットキャプチャツールを使用して、DHCPトラフィックを確認できます。

**Wiresharkフィルター**:
```
bootp
```

**DHCPメッセージの種類**:
- DHCP Discover: クライアントがDHCPサーバーを探す
- DHCP Offer: サーバーがIPアドレスを提供
- DHCP Request: クライアントがIPアドレスを要求
- DHCP Ack: サーバーがIPアドレスを確認

---

### クライアント側のデバッグ

**Windows**:
```cmd
# DHCPリリース
ipconfig /release

# DHCP更新
ipconfig /renew

# 詳細情報
ipconfig /all
```

**macOS**:
```bash
# DHCPリリース
sudo ipconfig set en0 BOOTP

# DHCP更新
sudo ipconfig set en0 DHCP
```

**Linux**:
```bash
# dhclientリリース
sudo dhclient -r

# dhclient更新
sudo dhclient

# NetworkManager更新
sudo nmcli con up "接続名"
```

---

## よくある質問（FAQ）

### Q1: 管理者権限なしで実行できますか？

**A**: いいえ、DHCPサーバーはポート67/68を使用するため、管理者権限が必須です。

---

### Q2: 既存のルーターと同時に使用できますか？

**A**: 技術的には可能ですが、推奨しません。IPアドレスの競合や予期しない動作が発生する可能性があります。隔離されたネットワークで使用することを強く推奨します。

---

### Q3: IPv6に対応していますか？

**A**: 現在のバージョンでは、IPv4のみをサポートしています。IPv6対応は将来のバージョンで検討中です。

---

### Q4: 複数のIPプールを設定できますか？

**A**: 現在のバージョンでは、1つのIPプールのみをサポートしています。複数のサブネットを管理する場合は、複数のJumboDogXインスタンスを実行してください。

---

### Q5: リース情報を確認できますか？

**A**: 現在のバージョンでは、リース情報の確認機能は実装されていません。将来のバージョンで追加予定です。

ログファイルでリース情報を確認できます:
```bash
grep "Lease" logs/jumbodogx.log
```

---

## セキュリティ上の注意

### DHCPスプーフィング攻撃への対策

DHCPスプーフィング攻撃（不正なDHCPサーバーがIPアドレスを配布）を防ぐため、以下の対策を検討してください：

1. **ネットワーク隔離**: 信頼できるネットワークのみで使用
2. **MAC ACL**: Allow Modeで許可されたデバイスのみを許可
3. **物理的なセキュリティ**: ネットワークへの物理的なアクセスを制限
4. **DHCP Snooping**: スイッチ側でDHCP Snooping機能を有効化（エンタープライズ環境）

---

## サポート

問題が解決しない場合は、以下を参照してください：

- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
- [ディスカッション](https://github.com/furuya02/jumbodogx/discussions)

Issue報告時には、以下の情報を含めてください：
- JumboDogXのバージョン
- OS（Windows/macOS/Linux）とバージョン
- 管理者権限で実行しているか
- 設定ファイル（`appsettings.json`の関連部分）
- ログファイル（`logs/jumbodogx.log`の関連部分）
- エラーメッセージ
- ネットワーク構成（既存のDHCPサーバーの有無等）

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [IPプール設定](ip-pool-configuration.md)
- [MAC ACL設定](mac-acl.md)
- [ACL設定](../common/acl-configuration.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
