# DHCPサーバー スクリーンショット撮影ガイド

このディレクトリには、DHCPサーバーのクイックスタートガイドで使用するスクリーンショットを配置します。

## 必要なスクリーンショット一覧

### 1. dashboard-top.png
**撮影対象**: JumboDogXのダッシュボード画面（トップページ）
**撮影タイミング**: アプリケーション起動直後
**URL**: `http://localhost:5001/`
**説明**:
- ダッシュボードのトップ画面全体
- サイドメニュー、ヘッダーを含む
- DHCP Serverセクションが表示されている状態

---

### 2. dhcp-general-menu.png
**撮影対象**: サイドメニューのDHCP > General
**撮影タイミング**: Settings > DHCPを展開した状態
**説明**:
- サイドメニューが展開された状態
- DHCPセクションが展開され、Generalが表示されている状態
- ハイライト表示でGeneralが選択されている

---

### 3. dhcp-ip-pool-settings.png
**撮影対象**: DHCP IPプール設定画面
**撮影タイミング**: DHCP IP Pool設定画面を開いた状態
**URL**: `http://localhost:5001/settings/dhcp/ip-pool`
**説明**:
- IP Pool Settingsセクション:
  - Bind Address: "192.168.1.1"
  - Subnet Mask: "255.255.255.0"
  - Start IP: "192.168.1.100"
  - End IP: "192.168.1.200"
  - Lease Time: "86400"（秒）
- "Save Settings"ボタンが表示されている

---

### 4. dhcp-network-settings.png
**撮影対象**: DHCP Network設定画面
**撮影タイミング**: DHCP Network設定画面を開いた状態
**URL**: `http://localhost:5001/settings/dhcp/network`
**説明**:
- Network Settingsセクション:
  - DNS Servers: "8.8.8.8, 8.8.4.4"（カンマ区切り）
  - Gateway: "192.168.1.1"
- "Save Settings"ボタンが表示されている

---

### 5. dhcp-acl-menu.png
**撮影対象**: サイドメニューからDHCP > ACLを選択
**撮影タイミング**: Settings > DHCPを展開した状態
**説明**:
- サイドメニューが展開された状態
- DHCPセクションの下にACLメニューが表示されている
- ハイライト表示でACLが選択されている

---

### 6. dhcp-acl-mode.png
**撮影対象**: ACL Mode選択画面
**撮影タイミング**: ACL設定画面を開いた状態
**URL**: `http://localhost:5001/settings/dhcp/acl`
**説明**:
- ACL Modeセクション
- Allow Mode（ラジオボタン選択）
- Deny Mode（ラジオボタン未選択）
- 現在のモードの説明（青色の情報メッセージ）

---

### 7. dhcp-mac-add.png
**撮影対象**: MACアドレスを追加する画面
**撮影タイミング**: Add MAC Addressボタンをクリックした後
**URL**: `http://localhost:5001/settings/dhcp/acl`
**説明**:
- Add MAC Address フォームが表示されている:
  - MAC Address: "00:11:22:33:44:55"
- "Add"ボタンが表示されている

---

### 8. dhcp-acl-entries.png
**撮影対象**: ACLエントリを追加した状態
**撮影タイミング**: 複数のMACアドレスを追加した後
**URL**: `http://localhost:5001/settings/dhcp/acl`
**説明**:
- MAC Access Control Listテーブルに以下のエントリが表示されている:
  - Entry 1: MAC Address: "00:11:22:33:44:55"
  - Entry 2: MAC Address: "AA:BB:CC:DD:EE:FF"
  - Entry 3: MAC Address: "11:22:33:44:55:66"
- 各エントリにDeleteボタンが表示されている
- "Save Settings"ボタンが表示されている

---

### 9. dhcp-static-lease-menu.png
**撮影対象**: サイドメニューからDHCP > Static Leasesを選択
**撮影タイミング**: Settings > DHCPを展開した状態
**説明**:
- サイドメニューが展開された状態
- DHCPセクションの下にStatic Leasesメニューが表示されている
- ハイライト表示でStatic Leasesが選択されている

---

### 10. dhcp-static-lease-add.png
**撮影対象**: 静的IP割り当てを追加する画面
**撮影タイミング**: Add Static Leaseボタンをクリックした後
**URL**: `http://localhost:5001/settings/dhcp/static-leases`
**説明**:
- Add Static Lease フォームが表示されている:
  - MAC Address: "00:11:22:33:44:55"
  - IP Address: "192.168.1.50"
- "Add"ボタンが表示されている

---

### 11. dhcp-static-leases-list.png
**撮影対象**: 静的IP割り当て一覧画面
**撮影タイミング**: 複数の静的IPを追加した後
**URL**: `http://localhost:5001/settings/dhcp/static-leases`
**説明**:
- Static Leasesテーブルに以下のエントリが表示されている:
  - Entry 1:
    - MAC Address: "00:11:22:33:44:55"
    - IP Address: "192.168.1.50"
  - Entry 2:
    - MAC Address: "AA:BB:CC:DD:EE:FF"
    - IP Address: "192.168.1.51"
- 各エントリにEdit, Deleteボタンが表示されている
- "Save Settings"ボタンが表示されている

---

### 12. dhcp-client-test-windows.png
**撮影対象**: Windowsクライアントでipconfig /renewを実行した結果
**撮影タイミング**: コマンドプロンプトでipconfig /renewを実行した後
**説明**:
- コマンドプロンプトのスクリーンショット
- 以下のコマンドと結果が表示されている:
```
ipconfig /release
ipconfig /renew
ipconfig /all
```
- IPv4アドレス: 192.168.1.100（DHCPサーバーから取得）
- サブネットマスク: 255.255.255.0
- デフォルトゲートウェイ: 192.168.1.1
- DNSサーバー: 8.8.8.8, 8.8.4.4

---

### 13. dhcp-client-mac-address.png
**撮影対象**: クライアントのMACアドレス確認
**撮影タイミング**: ipconfig /all（Windows）またはifconfig（macOS/Linux）を実行した後
**説明**:
- ターミナルまたはコマンドプロンプトのスクリーンショット
- 物理アドレス（MACアドレス）が強調表示されている
- 例: 物理アドレス: 00-11-22-33-44-55（Windows）
- 例: ether 00:11:22:33:44:55（macOS/Linux）

---

### 14. dhcp-dashboard.png
**撮影対象**: DashboardのDHCP Serverセクション
**撮影タイミング**: DHCPサーバーが起動している状態
**URL**: `http://localhost:5001/`
**説明**:
- DHCP Serverセクション
- サーバーのステータス（Running）
- Bind Address（192.168.1.1）
- IP Pool（192.168.1.100 - 192.168.1.200）
- 現在のリース数などの統計情報

---

## スクリーンショット撮影時の注意事項

### 推奨設定
- **ブラウザ**: Chrome、Firefox、Safari等の最新版
- **画面サイズ**: 1920x1080以上推奨
- **ブラウザウィンドウ**: できるだけ広く（フルスクリーンまたは最大化）
- **ズーム**: 100%（デフォルト）

### 撮影方法
- **macOS**: `Command + Shift + 4` → スペースキー → ウィンドウをクリック
- **Windows**: `Alt + PrintScreen` または Snipping Tool
- **Linux**: GNOME Screenshot、Spectacle等

### ファイル形式
- **推奨**: PNG形式
- **品質**: 圧縮なし、または可逆圧縮
- **ファイル名**: 上記の一覧に記載された名前を使用（小文字、ハイフン区切り）

### 画像の編集
- 個人情報や機密情報が含まれていないか確認
- 必要に応じて、特定の部分をぼかしまたは黒塗り
- 画像サイズが大きすぎる場合は、適切にリサイズ（幅1200px程度推奨）

---

## 撮影の進め方

以下の順序で撮影すると効率的です：

### 第1段階: JumboDogXの設定
1. JumboDogXを**管理者権限**で起動 → **dashboard-top.png**
2. Settings > DHCP > IP Poolを開く → **dhcp-general-menu.png**, **dhcp-ip-pool-settings.png**
3. Settings > DHCP > Networkを開く → **dhcp-network-settings.png**
4. Settings > DHCP > ACLを開く → **dhcp-acl-menu.png**, **dhcp-acl-mode.png**
5. Add MAC Addressでアドレスを追加（撮影用にフォームを開いた状態） → **dhcp-mac-add.png**
6. 複数のMACアドレスを追加 → **dhcp-acl-entries.png**
7. Settings > DHCP > Static Leasesを開く → **dhcp-static-lease-menu.png**
8. Add Static Leaseで静的IPを追加（撮影用にフォームを開いた状態） → **dhcp-static-lease-add.png**
9. 複数の静的IPを追加 → **dhcp-static-leases-list.png**
10. Dashboardに戻る → **dhcp-dashboard.png**

### 第2段階: クライアント側のテスト
11. Windowsクライアントでipconfig /renewを実行 → **dhcp-client-test-windows.png**
12. クライアントのMACアドレスを確認 → **dhcp-client-mac-address.png**

---

## テストデータの準備

スクリーンショット撮影用に、以下のテストデータを準備してください：

### DHCPサーバー設定

**IPプール**:
```
Bind Address: 192.168.1.1
Subnet Mask: 255.255.255.0
Start IP: 192.168.1.100
End IP: 192.168.1.200
Lease Time: 86400（秒）
```

**ネットワーク設定**:
```
DNS Servers: 8.8.8.8, 8.8.4.4
Gateway: 192.168.1.1
```

### MAC ACLエントリ

```
Entry 1: 00:11:22:33:44:55
Entry 2: AA:BB:CC:DD:EE:FF
Entry 3: 11:22:33:44:55:66
```

### 静的IPリース

```
Entry 1:
  MAC Address: 00:11:22:33:44:55
  IP Address: 192.168.1.50

Entry 2:
  MAC Address: AA:BB:CC:DD:EE:FF
  IP Address: 192.168.1.51
```

### 重要な注意事項

**管理者権限で起動**:
DHCPサーバーは管理者権限で実行する必要があります。

**Windows**:
```cmd
# コマンドプロンプトを「管理者として実行」
cd C:\path\to\jumbodogx
dotnet run --project src\Jdx.WebUI
```

**macOS/Linux**:
```bash
# sudoで実行
sudo dotnet run --project src/Jdx.WebUI
```

**ネットワーク隔離**:
既存のDHCPサーバー（ルーター等）と競合しないよう、隔離されたテストネットワークで撮影することを推奨します。

---

## スクリーンショット配置後

すべてのスクリーンショットを配置した後、以下を確認してください：

- [ ] ファイル名が正しいか
- [ ] PNG形式であるか
- [ ] 画像が鮮明で読みやすいか
- [ ] 個人情報や機密情報が含まれていないか
- [ ] マニュアルで正しく表示されるか

マニュアルを確認する場合は、Markdownビューアーまたはブラウザで`getting-started.md`を開いてください。

---

## スクリーンショット一覧（チェックリスト）

- [ ] dashboard-top.png
- [ ] dhcp-general-menu.png
- [ ] dhcp-ip-pool-settings.png
- [ ] dhcp-network-settings.png
- [ ] dhcp-acl-menu.png
- [ ] dhcp-acl-mode.png
- [ ] dhcp-mac-add.png
- [ ] dhcp-acl-entries.png
- [ ] dhcp-static-lease-menu.png
- [ ] dhcp-static-lease-add.png
- [ ] dhcp-static-leases-list.png
- [ ] dhcp-client-test-windows.png
- [ ] dhcp-client-mac-address.png
- [ ] dhcp-dashboard.png

全て揃ったらこのチェックリストを完了させてください。
