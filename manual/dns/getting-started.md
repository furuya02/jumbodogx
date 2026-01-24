# DNSサーバー クイックスタートガイド

このガイドでは、JumboDogXのDNSサーバーを最短で起動し、ローカルDNSサーバーとして動作させる方法を説明します。

## 前提条件

- JumboDogXがインストールされていること
- .NET 9 Runtimeがインストールされていること

詳細は[インストール手順](../common/installation.md)を参照してください。

## DNSサーバーとは？

DNS（Domain Name System）サーバーは、ドメイン名をIPアドレスに変換するサービスです。

### 用途

- **ローカル開発環境**: テスト用のドメイン名を設定
- **社内ネットワーク**: プライベートなホスト名を管理
- **学習・教育**: DNSの仕組みを理解する

**注意**: JumboDogXのDNSサーバーは、ローカルテスト環境専用です。本番環境での使用は推奨されません。

## ステップ1: JumboDogXの起動

### 起動方法

```bash
cd /path/to/jumbodogx
dotnet run --project src/Jdx.WebUI
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

## ステップ3: DNS基本設定

### 3-1. DNS設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **DNS** セクションを展開
3. **General** をクリック

![DNS設定画面](images/dns-general-menu.png)
*サイドメニューからDNS > Generalを選択*

### 3-2. 基本設定を入力

以下の項目を設定します：

| 項目 | 設定値 | 説明 |
|------|--------|------|
| Enable Server | ✓ ON | DNSサーバーを有効化 |
| Bind Address | `0.0.0.0` または `127.0.0.1` | すべてのネットワークインターフェースでリッスン（`0.0.0.0`）またはローカルのみ（`127.0.0.1`） |
| Port | `53` | DNS標準ポート（変更も可能） |
| Timeout | `3` | クエリのタイムアウト時間（秒） |

**ポートについて**:
- 標準のDNSポートは53番です
- ポート53を使用するには、管理者権限が必要な場合があります
- テスト環境では、5053などの高いポート番号も使用できます

![DNS基本設定](images/dns-general-settings.png)
*DNS Generalの設定画面*

### 3-3. 設定を保存

1. **Save Settings** ボタンをクリック
2. 成功メッセージが表示されることを確認
3. **設定は即座に反映されます**（再起動不要）

## ステップ4: DNSレコードの追加

### 4-1. Records設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **DNS** セクションを展開
3. **Records** をクリック

![DNS Records設定画面](images/dns-records-menu.png)
*サイドメニューからDNS > Recordsを選択*

### 4-2. Aレコードの追加（ホスト名 → IPアドレス）

最も基本的なDNSレコードであるAレコードを追加します。

1. **Add Record** ボタンをクリック
2. 新しいレコードが追加されます
3. 以下の項目を入力：
   - **Type**: `A` を選択
   - **Name**: `test.local` （ホスト名）
   - **Value**: `192.168.1.100` （IPアドレス）
   - **TTL**: `300` （キャッシュ時間、秒単位）

4. **Save Settings** ボタンをクリック

![Aレコード追加](images/dns-record-a.png)
*Aレコードの追加*

### 4-3. 複数のレコードを追加（例）

実用的な設定例として、複数のレコードを追加します：

| Type | Name | Value | TTL | 説明 |
|------|------|-------|-----|------|
| A | `localhost.local` | `127.0.0.1` | 300 | ローカルホスト |
| A | `server.local` | `192.168.1.100` | 300 | サーバー |
| A | `web.local` | `192.168.1.101` | 300 | Webサーバー |
| CNAME | `www.local` | `web.local` | 300 | Webサーバーのエイリアス |

**レコードタイプの説明**:
- **A**: ホスト名をIPv4アドレスに変換
- **AAAA**: ホスト名をIPv6アドレスに変換
- **CNAME**: ホスト名を別のホスト名に変換（エイリアス）
- **MX**: メールサーバーを指定
- **NS**: ネームサーバーを指定
- **PTR**: IPアドレスをホスト名に変換（逆引き）
- **SOA**: ゾーンの権威情報

![複数レコード追加](images/dns-records-multiple.png)
*複数のDNSレコードを追加*

## ステップ5: ACL設定（アクセス制御）

**重要**: デフォルトでは、DNSサーバーは **ACLがAllow Mode（全てDeny）** になっています。
このままではlocalhostからもアクセスできないため、ACL設定が必要です。

### 5-1. ACL設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **DNS** セクションを展開
3. **ACL** をクリック

![DNS ACL設定画面](images/dns-acl-menu.png)
*サイドメニューからDNS > ACLを選択*

### 5-2. ACL Modeの確認

デフォルト設定を確認します：

| 項目 | デフォルト値 | 説明 |
|------|------------|------|
| Access Control Mode | Allow Mode (Allowlist) | リストに登録されたIPアドレスのみアクセス許可（デフォルトは全拒否） |

**Allow ModeとDeny Modeの違い**:
- **Allow Mode (Allowlist)**: リストに登録されたIPアドレスのみ許可。それ以外は全て拒否（デフォルト）
- **Deny Mode (Denylist)**: リストに登録されたIPアドレスのみ拒否。それ以外は全て許可

### 5-3. localhostからのアクセスを許可

ローカルマシンからアクセスできるように、127.0.0.1を追加します：

1. **Add ACL Entry** ボタンをクリック
2. 追加されたエントリに以下を入力：
   - **Name**: `Localhost`
   - **IP Address / Range**: `127.0.0.1`
3. **Save Settings** ボタンをクリック

![ACLエントリ追加](images/dns-acl-localhost.png)
*127.0.0.1を追加した状態*

### 5-4. ローカルネットワークからのアクセスを許可（オプション）

同じネットワーク内の他のマシンからもアクセスを許可する場合：

1. **Add ACL Entry** ボタンをクリック
2. 追加されたエントリに以下を入力：
   - **Name**: `Local Network`
   - **IP Address / Range**: `192.168.1.0/24`
3. **Save Settings** ボタンをクリック

**サポートされているアドレス形式**:
- **単一IP**: `192.168.1.100`
- **IPレンジ（CIDR）**: `192.168.1.0/24`
- **IPレンジ（開始-終了）**: `192.168.1.1-192.168.1.254`
- **ワイルドカード**: `192.168.1.*`

詳細は[ACL設定ガイド](../common/acl-configuration.md)を参照してください。

## ステップ6: 動作確認

### 6-1. Dashboardで確認

1. サイドメニューから **Dashboard** をクリック
2. **DNS Server** セクションでサーバーのステータスを確認
3. サーバーが起動していることを確認

![Dashboard DNS](images/dashboard-dns.png)
*DashboardのDNS Serverセクション*

### 6-2. nslookupコマンドで確認

DNSサーバーが正しく動作しているか確認します。

#### Windows の場合

コマンドプロンプトまたはPowerShellで実行：

```powershell
# JumboDogX DNSサーバーに問い合わせ
nslookup test.local localhost

# または、ポート番号を指定（非標準ポートの場合）
nslookup -port=5053 test.local localhost
```

**期待される出力**:
```
Server:  localhost
Address:  127.0.0.1

Name:    test.local
Address:  192.168.1.100
```

#### macOS / Linux の場合

ターミナルで実行：

```bash
# JumboDogX DNSサーバーに問い合わせ
nslookup test.local localhost

# または、digコマンドを使用
dig @localhost test.local

# ポート番号を指定（非標準ポートの場合）
dig @localhost -p 5053 test.local
```

**期待される出力**:
```
;; ANSWER SECTION:
test.local.             300     IN      A       192.168.1.100
```

![nslookup実行結果](images/dns-nslookup.png)
*nslookupコマンドの実行結果*

### 6-3. ブラウザで確認（オプション）

DNSサーバーを使用してWebサイトにアクセスする場合：

1. OS のDNS設定を変更（後述）
2. ブラウザで `http://test.local` にアクセス
3. 対応するIPアドレスのWebサーバーが表示されることを確認

## ステップ7: OSのDNS設定変更（オプション）

JumboDogXのDNSサーバーをシステム全体で使用する場合、OSのDNS設定を変更します。

**注意**: この設定を変更すると、インターネット接続に影響する可能性があります。テスト後は元の設定に戻すことを推奨します。

### Windows の場合

1. **コントロールパネル** > **ネットワークとインターネット** > **ネットワーク接続** を開く
2. 使用中のネットワークアダプタを右クリック > **プロパティ**
3. **インターネット プロトコル バージョン 4 (TCP/IPv4)** を選択 > **プロパティ**
4. **次のDNSサーバーのアドレスを使う** を選択
5. **優先DNSサーバー**: `127.0.0.1` （JumboDogXのDNSサーバー）
6. **代替DNSサーバー**: `8.8.8.8` （Googleの公開DNSなど）
7. **OK** をクリック

![Windows DNS設定](images/dns-windows-config.png)
*WindowsのDNS設定画面*

### macOS の場合

1. **システム設定** > **ネットワーク** を開く
2. 使用中のネットワークを選択 > **詳細**
3. **DNS** タブをクリック
4. **+** ボタンをクリックして `127.0.0.1` を追加
5. **OK** をクリック

![macOS DNS設定](images/dns-macos-config.png)
*macOSのDNS設定画面*

### Linux（Ubuntu）の場合

#### 方法1: NetworkManager を使用

```bash
# DNSサーバーを設定
nmcli connection modify <接続名> ipv4.dns "127.0.0.1 8.8.8.8"
nmcli connection up <接続名>
```

#### 方法2: /etc/resolv.conf を編集（一時的）

```bash
# 現在の設定をバックアップ
sudo cp /etc/resolv.conf /etc/resolv.conf.backup

# DNSサーバーを設定
echo "nameserver 127.0.0.1" | sudo tee /etc/resolv.conf
echo "nameserver 8.8.8.8" | sudo tee -a /etc/resolv.conf
```

**元に戻す場合**:
```bash
sudo mv /etc/resolv.conf.backup /etc/resolv.conf
```

## よくある問題と解決方法

### DNSクエリが応答しない

**症状**: `nslookup` や `dig` コマンドでタイムアウトする

**原因1: ACLで拒否されている**

DNSサーバーはデフォルトでACLがAllow Mode（全てDeny）になっているため、ACL設定が必要です。

**解決策**:
1. Settings > DNS > ACL を開く
2. "Add ACL Entry"で`127.0.0.1`を追加
3. "Save Settings"をクリック

**原因2: サーバーが起動していない**

**解決策**:
1. Dashboard > DNS Serverでステータスを確認
2. Settings > DNS > Generalで "Enable Server" がONか確認

**原因3: ポートが使用中**

```bash
# ポートの使用状況を確認（macOS/Linux）
sudo lsof -i :53

# ポートの使用状況を確認（Windows）
netstat -ano | findstr :53
```

**解決策**: 別のポート番号（例：5053）を使用する

### レコードが見つからない

**症状**: `nslookup` で "server can't find" エラー

**原因**: レコードが正しく設定されていない

**解決策**:
1. Settings > DNS > Records を開く
2. レコードが正しく追加されているか確認
3. Type, Name, Value が正確か確認
4. "Save Settings" をクリックしたか確認

### "Permission denied" エラー（ポート53使用時）

**症状**: ポート53でサーバーが起動できない

**原因**: ポート1024未満は管理者権限が必要

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

または、高いポート番号（5053など）を使用する。

### CNAMEレコードが動作しない

**症状**: CNAMEレコードで名前解決できない

**原因**: 参照先のAレコードが存在しない

**解決策**:
1. CNAMEレコードの参照先（Value）のホスト名が正しいか確認
2. 参照先のAレコードが登録されているか確認

**例**:
```
A    web.local    192.168.1.101     # これが必要
CNAME www.local   web.local         # これはweb.localを参照
```

## 次のステップ

基本的なDNSサーバーが動作したら、以下のドキュメントで更に機能を拡張できます：

- [レコード設定](resource-records.md) - 各種DNSレコードの詳細設定
- [ゾーン設定](zone-configuration.md) - DNSゾーンの管理
- [トラブルシューティング](troubleshooting.md) - よくある問題の解決方法
- [ACL設定ガイド](../common/acl-configuration.md) - アクセス制御の詳細設定

## 実用的な使用例

### 例1: ローカル開発環境

開発中のWebアプリケーションに簡単にアクセスできるようにする：

```
A    dev.local        127.0.0.1
A    api.local        127.0.0.1
A    db.local         127.0.0.1
```

ブラウザで `http://dev.local:3000` のようにアクセス可能になります。

### 例2: 複数サーバーの管理

社内ネットワークの各サーバーに名前を付ける：

```
A    file-server.local    192.168.1.100
A    web-server.local     192.168.1.101
A    mail-server.local    192.168.1.102
CNAME www.local          web-server.local
```

### 例3: テスト環境

テスト用のドメイン名を設定：

```
A    test.example.com     192.168.1.200
A    staging.example.com  192.168.1.201
```

**注意**: 実際のドメイン名（example.com）を使用する場合は、外部への影響に注意してください。

## まとめ

このガイドでは、以下の手順を完了しました：

✓ JumboDogXの起動とWeb管理画面へのアクセス
✓ DNS基本設定（ポート、Bind Address）
✓ DNSレコードの追加（Aレコード、CNAMEレコード）
✓ ACL設定（localhostからのアクセス許可）
✓ 動作確認（nslookup、digコマンド）
✓ OSのDNS設定変更（オプション）

これで、JumboDogXのDNSサーバーが起動し、ローカルDNSサーバーとして使用できるようになりました！

### 重要なポイント

- **DNSサーバーはデフォルトでACLがAllow Mode（全てDeny）** - 127.0.0.1の追加が必須
- ポート53を使用するには管理者権限が必要（または高いポート番号を使用）
- 設定は即座に反映されるため、再起動は不要
- Dashboardでサーバーの状態を確認できる

### セキュリティの注意

- JumboDogXのDNSサーバーは、ローカルテスト環境専用です
- インターネットに直接公開しないでください
- ACLで適切にアクセス制御してください
- テスト後はOSのDNS設定を元に戻すことを推奨します

さらに詳しい設定は、各マニュアルを参照してください。
