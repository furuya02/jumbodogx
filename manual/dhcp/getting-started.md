# DHCPサーバー クイックスタートガイド

このガイドでは、JumboDogXのDHCPサーバーを最短で起動し、IPアドレスの自動割り当てを行う方法を説明します。

## 前提条件

- JumboDogXがインストールされていること
- .NET 9 Runtimeがインストールされていること
- **管理者権限**: DHCPサーバーは通常、管理者権限が必要です

詳細は[インストール手順](../common/installation.md)を参照してください。

## DHCPサーバーとは？

DHCP（Dynamic Host Configuration Protocol）サーバーは、ネットワーク上のデバイスにIPアドレスを自動的に割り当てるサービスです。

### 用途

- **ローカルネットワーク**: テスト用ネットワークでIPアドレスを自動割り当て
- **開発環境**: 仮想マシンやコンテナへのIPアドレス割り当て
- **学習・教育**: DHCPの仕組みを理解する

**注意**: JumboDogXのDHCPサーバーは、ローカルテスト環境専用です。本番環境での使用は推奨されません。

**重要**: 既存のネットワークにDHCPサーバーを追加すると、IPアドレスの競合が発生する可能性があります。必ず隔離されたネットワークで使用してください。

## ステップ1: JumboDogXの起動

### 起動方法（管理者権限が必要）

#### Windows の場合

PowerShellを**管理者として実行**：

```powershell
cd C:\JumboDogX
.\Jdx.WebUI.exe
```

#### macOS / Linux の場合

ターミナルでsudoを使用：

```bash
cd /path/to/jumbodogx
sudo dotnet run --project src/Jdx.WebUI
```

### 起動確認

ターミナルに以下のようなメッセージが表示されれば成功です：

```
Now listening on: http://localhost:5001
Application started. Press Ctrl+C to shut down.
```

## ステップ2: Web管理画面にアクセス

ブラウザで以下のURLを開きます：

```
http://localhost:5001
```

![ダッシュボード画面](images/dashboard-top.png)
*JumboDogXのダッシュボード画面*

## ステップ3: DHCP基本設定

### 3-1. DHCP設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **DHCP** セクションを展開
3. **General** をクリック

![DHCP設定画面](images/dhcp-general-menu.png)
*サイドメニューからDHCP > Generalを選択*

### 3-2. 基本設定を入力

以下の項目を設定します：

| 項目 | 設定値 | 説明 |
|------|--------|------|
| Enable Server | ✓ ON | DHCPサーバーを有効化 |
| Bind Address | ネットワークインターフェースのIPアドレス | DHCPサーバーがリッスンするアドレス（例：`192.168.1.1`） |
| Server Identifier | Bind Addressと同じ | DHCPサーバーの識別子 |
| Lease Time | `3600` | IPアドレスのリース時間（秒、デフォルト1時間） |
| Renewal Time | `1800` | リース更新時間（秒、リース時間の50%） |
| Rebinding Time | `3150` | リバインディング時間（秒、リース時間の87.5%） |

**Bind Addressについて**:
- DHCPサーバーは特定のネットワークインターフェースでリッスンする必要があります
- `0.0.0.0`は使用できません
- ネットワークインターフェースの実際のIPアドレスを指定してください（例：`192.168.1.1`）

![DHCP基本設定](images/dhcp-general-settings.png)
*DHCP Generalの設定画面*

### 3-3. 設定を保存

1. **Save Settings** ボタンをクリック
2. 成功メッセージが表示されることを確認
3. **設定は即座に反映されます**（再起動不要）

## ステップ4: IPアドレスプールの設定

DHCPサーバーが割り当てるIPアドレスの範囲を設定します。

### 4-1. IP Pool設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **DHCP** セクションを展開
3. **IP Pool Configuration** をクリック

![DHCP IP Pool設定画面](images/dhcp-ippool-menu.png)
*サイドメニューからDHCP > IP Pool Configurationを選択*

### 4-2. IPプールの追加

1. **Add IP Pool** ボタンをクリック
2. 新しいプールが追加されます
3. 以下の項目を入力：
   - **Start IP**: `192.168.1.100` （割り当て開始IPアドレス）
   - **End IP**: `192.168.1.200` （割り当て終了IPアドレス）
   - **Subnet Mask**: `255.255.255.0` （サブネットマスク）
   - **Gateway**: `192.168.1.1` （デフォルトゲートウェイ）
   - **DNS Server 1**: `8.8.8.8` （プライマリDNSサーバー）
   - **DNS Server 2**: `8.8.4.4` （セカンダリDNSサーバー）
   - **Domain Name**: `local` （ドメイン名、オプション）

4. **Save Settings** ボタンをクリック

![IPプール設定](images/dhcp-ippool-config.png)
*IPプールの設定*

### 4-3. 設定値の説明

| 項目 | 説明 | 例 |
|------|------|-----|
| Start IP | 割り当て開始IPアドレス | `192.168.1.100` |
| End IP | 割り当て終了IPアドレス | `192.168.1.200` |
| Subnet Mask | ネットワークのサブネットマスク | `255.255.255.0` (/24) |
| Gateway | デフォルトゲートウェイ | `192.168.1.1` |
| DNS Server 1 | プライマリDNSサーバー | `8.8.8.8` |
| DNS Server 2 | セカンダリDNSサーバー | `8.8.4.4` |
| Domain Name | ドメイン名 | `example.local` |

**IPプールの範囲**:
- 上記の設定では、`192.168.1.100`から`192.168.1.200`までの101個のIPアドレスが割り当て可能です
- 既存のネットワーク機器と競合しない範囲を指定してください

## ステップ5: MAC ACL設定（オプション）

特定のMACアドレスのデバイスにのみIPアドレスを割り当てる場合、MAC ACLを設定します。

### 5-1. MAC ACL設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **DHCP** セクションを展開
3. **MAC ACL** をクリック

![DHCP MAC ACL設定画面](images/dhcp-mac-acl-menu.png)
*サイドメニューからDHCP > MAC ACLを選択*

### 5-2. ACL Modeの選択

| モード | 動作 |
|--------|------|
| Allow Mode (Allowlist) | リストに登録されたMACアドレスのみIPアドレスを割り当て |
| Deny Mode (Denylist) | リストに登録されたMACアドレス以外にIPアドレスを割り当て |

**推奨**: セキュリティを重視する場合は、Allow Modeを使用してください。

### 5-3. MACアドレスエントリの追加

1. **Add MAC Entry** ボタンをクリック
2. 追加されたエントリに以下を入力：
   - **Name**: `Device 1`
   - **MAC Address**: `00:11:22:33:44:55`
3. **Save Settings** ボタンをクリック

**MACアドレスの形式**:
- `00:11:22:33:44:55` （コロン区切り、推奨）
- `00-11-22-33-44-55` （ハイフン区切り）
- `001122334455` （区切りなし）

![MAC ACLエントリ追加](images/dhcp-mac-acl-entry.png)
*MACアドレスエントリの追加*

詳細は[MAC ACL設定](mac-acl.md)を参照してください。

## ステップ6: 動作確認

### 6-1. Dashboardで確認

1. サイドメニューから **Dashboard** をクリック
2. **DHCP Server** セクションでサーバーのステータスを確認
3. サーバーが起動していることを確認
4. **Active Leases** （現在割り当てられているIPアドレス数）を確認

![Dashboard DHCP](images/dashboard-dhcp.png)
*DashboardのDHCP Serverセクション*

### 6-2. クライアントからIPアドレスを取得

DHCPクライアントからIPアドレスを取得します。

#### Windows の場合

コマンドプロンプトまたはPowerShellで実行：

```powershell
# 現在のIPアドレスを解放
ipconfig /release

# DHCPサーバーから新しいIPアドレスを取得
ipconfig /renew

# IPアドレスを確認
ipconfig /all
```

**期待される出力**:
```
Ethernet adapter:
   DHCP Enabled: Yes
   IPv4 Address: 192.168.1.100
   Subnet Mask: 255.255.255.0
   Default Gateway: 192.168.1.1
   DHCP Server: 192.168.1.1
   DNS Servers: 8.8.8.8, 8.8.4.4
```

#### macOS の場合

ターミナルで実行：

```bash
# ネットワークインターフェース名を確認
networksetup -listallhardwareports

# DHCPからIPアドレスを取得（Ethernetの場合）
sudo ipconfig set en0 DHCP

# IPアドレスを確認
ifconfig en0
```

#### Linux（Ubuntu）の場合

```bash
# DHCPクライアント（dhclient）を使用
sudo dhclient -r eth0  # 解放
sudo dhclient eth0     # 取得

# IPアドレスを確認
ip addr show eth0
```

![IP取得確認](images/dhcp-client-config.png)
*クライアントでのIPアドレス取得確認*

### 6-3. リース情報の確認

JumboDogXのWeb UIでリース情報を確認します。

1. サイドメニューから **Dashboard** をクリック
2. **DHCP Server** セクションの **Active Leases** を確認
3. または、**Settings** > **DHCP** > **General** でリース一覧を確認

**リース情報に含まれる内容**:
- IPアドレス
- MACアドレス
- リース開始時刻
- リース期限

![リース情報](images/dhcp-lease-info.png)
*DHCPリース情報の表示*

## ステップ7: ログの確認

DHCPサーバーのログを確認できます。

### 7-1. Web UIでログを確認

1. サイドメニューから **Logs** をクリック
2. **Category** ドロップダウンから **Jdx.Servers.Dhcp** を選択
3. IPアドレス割り当てのログが表示されます

![DHCPログ](images/dhcp-logs.png)
*DHCPサーバーのログ*

### 7-2. ログファイルで確認

```bash
# DHCPサーバーのログを確認
cat logs/jumbodogx-*.log | jq 'select(.SourceContext == "Jdx.Servers.Dhcp.DhcpServer")'

# IP割り当てのログのみ表示
cat logs/jumbodogx-*.log | jq 'select(.["@mt"] | test("IP address assigned"))'
```

## よくある問題と解決方法

### DHCPサーバーが起動しない

**症状**: Dashboard で "DHCP Server: Stopped" と表示される

**原因1: 管理者権限がない**

DHCPサーバーは通常、管理者権限が必要です。

**解決策**:

**Windows**: 管理者としてPowerShellを起動
```powershell
# PowerShellを管理者として実行
cd C:\JumboDogX
.\Jdx.WebUI.exe
```

**macOS / Linux**: sudoで実行
```bash
sudo dotnet run --project src/Jdx.WebUI
```

**原因2: Bind Addressが正しくない**

**解決策**:
1. Settings > DHCP > General を確認
2. Bind Address がネットワークインターフェースの実際のIPアドレスになっているか確認
3. `0.0.0.0`は使用できません

**原因3: ポート67が使用中**

```bash
# ポートの使用状況を確認（macOS/Linux）
sudo lsof -i :67

# ポートの使用状況を確認（Windows）
netstat -ano | findstr :67
```

**解決策**: 既存のDHCPサーバーを停止する

### クライアントがIPアドレスを取得できない

**症状**: `ipconfig /renew` でタイムアウトする

**原因1: ネットワークが隔離されていない**

既存のDHCPサーバーが応答している可能性があります。

**解決策**: 隔離されたネットワーク（仮想ネットワーク、VLANなど）で使用する

**原因2: ファイアウォールでブロックされている**

**解決策**: ファイアウォールでUDPポート67（サーバー）と68（クライアント）を許可

**Windows**:
```powershell
# ファイアウォールルールを追加（管理者権限）
netsh advfirewall firewall add rule name="DHCP Server" dir=in action=allow protocol=UDP localport=67
netsh advfirewall firewall add rule name="DHCP Client" dir=in action=allow protocol=UDP localport=68
```

**Linux**:
```bash
# ファイアウォールルールを追加
sudo ufw allow 67/udp
sudo ufw allow 68/udp
```

**原因3: IPプールが枯渇している**

**解決策**:
1. Settings > DHCP > IP Pool Configuration を確認
2. IPプールの範囲を拡大する
3. または、不要なリースを手動で削除

### IPアドレスの競合

**症状**: "IP address conflict" エラー

**原因**: IPプールの範囲に既に使用中のIPアドレスが含まれている

**解決策**:
1. 既存のネットワークで使用されていないIPアドレス範囲を使用
2. 例：既存が`192.168.1.1-192.168.1.99`の場合、`192.168.1.100-192.168.1.200`を使用

### MAC ACLが機能しない

**症状**: 許可していないMACアドレスにもIPアドレスが割り当てられる

**原因**: ACL Modeが正しく設定されていない

**解決策**:
1. Settings > DHCP > MAC ACL を確認
2. Access Control Modeが正しく設定されているか確認（Allow Mode / Deny Mode）
3. MACアドレスの形式が正しいか確認

## 次のステップ

基本的なDHCPサーバーが動作したら、以下のドキュメントで更に機能を拡張できます：

- [IPプール設定](ip-pool-configuration.md) - IPアドレスプールの詳細設定
- [MAC ACL設定](mac-acl.md) - MACアドレスベースのアクセス制御
- [トラブルシューティング](troubleshooting.md) - よくある問題の解決方法

## 実用的な使用例

### 例1: 仮想マシンネットワーク

仮想マシン環境でのIPアドレス自動割り当て：

**設定**:
- IP Pool: `192.168.100.100` - `192.168.100.200`
- Subnet Mask: `255.255.255.0`
- Gateway: `192.168.100.1`
- DNS: `8.8.8.8`

**用途**: VirtualBox、VMwareなどの仮想ネットワーク

### 例2: 開発用コンテナネットワーク

Dockerなどのコンテナ環境でのIPアドレス管理：

**設定**:
- IP Pool: `172.18.0.100` - `172.18.0.200`
- Subnet Mask: `255.255.0.0`
- Gateway: `172.18.0.1`

**用途**: Docker、Kubernetesなどのコンテナネットワーク

### 例3: テスト用IoTデバイス

IoTデバイスのテスト環境でのIPアドレス管理：

**設定**:
- IP Pool: `10.0.1.50` - `10.0.1.100`
- MAC ACL: Allow Mode
- 許可するデバイスのMACアドレスを登録

**用途**: IoTデバイスの開発・テスト環境

## まとめ

このガイドでは、以下の手順を完了しました：

✓ JumboDogXの起動（管理者権限で）
✓ DHCP基本設定（Bind Address、リース時間）
✓ IPアドレスプールの設定
✓ MAC ACL設定（オプション）
✓ 動作確認（クライアントからのIP取得）
✓ ログの確認

これで、JumboDogXのDHCPサーバーが起動し、IPアドレスの自動割り当てができるようになりました！

### 重要なポイント

- DHCPサーバーの起動には**管理者権限が必要**
- Bind Addressには実際のネットワークインターフェースのIPアドレスを指定
- **既存のネットワークとの競合に注意** - 隔離されたネットワークで使用を推奨
- IPプールの範囲は既存のネットワーク機器と競合しないように設定
- 設定は即座に反映されるため、再起動は不要

### セキュリティの注意

- JumboDogXのDHCPサーバーは、ローカルテスト環境専用です
- 本番環境のネットワークに追加しないでください
- 不正なIPアドレス割り当てを防ぐため、MAC ACLの使用を推奨
- 隔離されたネットワーク（仮想ネットワーク、VLANなど）で使用してください

さらに詳しい設定は、各マニュアルを参照してください。
