# DNSサーバー - トラブルシューティング

JumboDogXのDNSサーバーで発生する可能性のある問題と解決方法を説明します。

## 一般的な問題

### DNSサーバーが起動しない

#### 症状
- JumboDogXを起動してもDNSサーバーが動作しない
- ダッシュボードでDNSサーバーのステータスが"Stopped"または"Error"

#### 原因と解決方法

**原因1: ポート53が既に使用されている**

ポート53は、多くのシステムで既にDNSサービスが使用しています。

**解決方法**:

**macOS**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :53

# macOSの組み込みDNSサービスを停止（推奨しません）
sudo launchctl unload -w /System/Library/LaunchDaemons/com.apple.mDNSResponder.plist
```

**Windows**:
```cmd
# 使用中のプロセスを確認
netstat -ano | findstr :53

# DNSクライアントサービスを停止（推奨しません）
net stop dnscache
```

**Linux**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :53

# systemd-resolvedを停止（Ubuntu等）
sudo systemctl stop systemd-resolved
```

**代替ポートの使用**（推奨）:
```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 5353,
    "Records": [...]
  }
}
```

クライアント側でポート5353を指定してクエリ:
```bash
nslookup -port=5353 example.local localhost
dig @localhost -p 5353 example.local
```

---

**原因2: 管理者権限が不足している**

ポート53（1024未満の特権ポート）を使用する場合、管理者権限が必要です。

**解決方法**:

**macOS/Linux**:
```bash
sudo dotnet run --project src/Jdx.WebUI
```

**Windows**:
- コマンドプロンプトまたはPowerShellを「管理者として実行」
- Visual Studioを管理者として実行

---

**原因3: `Enabled`が`false`になっている**

設定ファイルで`Enabled`が`false`の場合、サーバーは起動しません。

**解決方法**:
```json
{
  "DnsServer": {
    "Enabled": true,  // ← trueに設定
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "Records": [...]
  }
}
```

---

### DNSクエリが応答しない

#### 症状
- `nslookup`や`dig`でクエリしても応答がない
- タイムアウトエラーが発生する

#### 原因と解決方法

**原因1: ファイアウォールでポート53がブロックされている**

**解決方法**:

**macOS**:
```bash
# ファイアウォール設定を確認
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --getglobalstate

# JumboDogXを許可リストに追加
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --add /path/to/jumbodogx
```

**Windows**:
```powershell
# ファイアウォールルールを追加
New-NetFirewallRule -DisplayName "JumboDogX DNS" -Direction Inbound -Protocol UDP -LocalPort 53 -Action Allow
```

**Linux (ufw)**:
```bash
# ポート53を許可
sudo ufw allow 53/udp
sudo ufw allow 53/tcp
```

---

**原因2: BindAddressの設定が間違っている**

**解決方法**:

ローカルホストのみで動作させる場合:
```json
{
  "DnsServer": {
    "BindAddress": "127.0.0.1",
    "Port": 53
  }
}
```

すべてのネットワークインターフェースで動作させる場合:
```json
{
  "DnsServer": {
    "BindAddress": "0.0.0.0",
    "Port": 53
  }
}
```

---

**原因3: レコードが設定されていない**

**解決方法**: 少なくとも1つのレコードを設定してください
```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "Records": [
      {
        "Type": "A",
        "Name": "test.local",
        "Value": "192.168.1.100",
        "TTL": 300
      }
    ]
  }
}
```

---

### 特定のレコードが解決できない

#### 症状
- 一部のレコードは解決できるが、特定のレコードが解決できない
- `NXDOMAIN`（ドメインが存在しない）エラーが返される

#### 原因と解決方法

**原因1: レコードの名前が間違っている**

**症状例**:
```bash
# クエリ
nslookup www.example.local localhost

# 設定
{
  "Type": "A",
  "Name": "example.local",  # ← www.example.localではない
  "Value": "192.168.1.100"
}
```

**解決方法**: レコードの名前を正しく設定してください
```json
{
  "Type": "A",
  "Name": "www.example.local",  # ← 修正
  "Value": "192.168.1.100",
  "TTL": 300
}
```

---

**原因2: CNAMEレコードとAレコードが競合している**

CNAMEレコードと他のレコードタイプ（A、AAAA等）を同じ名前で設定することはできません。

**誤った設定例**:
```json
{
  "Records": [
    {
      "Type": "A",
      "Name": "www.local",
      "Value": "192.168.1.100",
      "TTL": 300
    },
    {
      "Type": "CNAME",
      "Name": "www.local",  # ← Aレコードと名前が重複
      "Value": "example.local",
      "TTL": 300
    }
  ]
}
```

**解決方法**: どちらか一方を削除してください
```json
{
  "Records": [
    {
      "Type": "CNAME",
      "Name": "www.local",
      "Value": "example.local",
      "TTL": 300
    }
  ]
}
```

---

**原因3: キャッシュが古い**

DNSクライアントやリゾルバーがキャッシュを保持している場合、変更が反映されません。

**解決方法**:

**macOS**:
```bash
sudo dscacheutil -flushcache
sudo killall -HUP mDNSResponder
```

**Windows**:
```cmd
ipconfig /flushdns
```

**Linux**:
```bash
# systemd-resolved（Ubuntu等）
sudo systemd-resolve --flush-caches

# nscd
sudo /etc/init.d/nscd restart
```

---

### MXレコードが動作しない

#### 症状
- メールサーバーが正しく解決されない
- `dig MX`でレコードが返されない

#### 原因と解決方法

**原因: Valueの形式が間違っている**

MXレコードのValueは`優先度 メールサーバー名`の形式である必要があります。

**誤った設定例**:
```json
{
  "Type": "MX",
  "Name": "example.local",
  "Value": "mail.example.local",  # ← 優先度がない
  "TTL": 300
}
```

**正しい設定例**:
```json
{
  "Type": "MX",
  "Name": "example.local",
  "Value": "10 mail.example.local",  # ← 優先度を追加
  "TTL": 300
}
```

---

### 逆引き（PTRレコード）が動作しない

#### 症状
- IPアドレスからドメイン名が解決できない
- `nslookup <IPアドレス>`でドメイン名が返されない

#### 原因と解決方法

**原因: PTRレコードの名前形式が間違っている**

PTRレコードの名前は、IPアドレスを逆順にして`.in-addr.arpa`を付加する必要があります。

**誤った設定例**:
```json
{
  "Type": "PTR",
  "Name": "192.168.1.100",  # ← 間違い
  "Value": "example.local",
  "TTL": 300
}
```

**正しい設定例**:
```json
{
  "Type": "PTR",
  "Name": "100.1.168.192.in-addr.arpa",  # ← 逆順にして.in-addr.arpaを付加
  "Value": "example.local",
  "TTL": 300
}
```

---

## パフォーマンスの問題

### クエリのレスポンスが遅い

#### 症状
- DNSクエリの応答に時間がかかる
- タイムアウトが頻繁に発生する

#### 原因と解決方法

**原因1: TTLが短すぎる**

TTLが短い場合、クライアントはキャッシュを使用できず、毎回サーバーにクエリを送信します。

**解決方法**: TTLを長くしてください（300-3600秒を推奨）
```json
{
  "Type": "A",
  "Name": "example.local",
  "Value": "192.168.1.100",
  "TTL": 3600  # ← 1時間
}
```

---

**原因2: ネットワーク遅延**

**解決方法**:
- サーバーとクライアント間のネットワークを確認してください
- `ping`や`traceroute`でネットワーク遅延を測定してください

```bash
# ネットワーク遅延の確認
ping localhost
```

---

**原因3: 大量のクエリ**

**解決方法**: ログを確認して、不要なクエリを特定してください

```bash
# ログファイルの確認
tail -f logs/jumbodogx.log | grep DNS
```

---

## セキュリティの問題

### 外部からの不正なクエリ

#### 症状
- ログに未知のIPアドレスからのクエリが記録されている
- DNSアンプ攻撃の可能性

#### 原因と解決方法

**解決方法1: BindAddressを制限**

外部からのアクセスを防ぐため、ローカルホストのみで動作させてください。

```json
{
  "DnsServer": {
    "BindAddress": "127.0.0.1",  # ← ローカルホストのみ
    "Port": 53
  }
}
```

---

**解決方法2: ACLを設定**

特定のIPアドレスからのクエリのみを許可してください。

```json
{
  "DnsServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 53,
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "127.0.0.1",
        "192.168.1.0/24"
      ]
    }
  }
}
```

詳細は[ACL設定ガイド](../common/acl-configuration.md)を参照してください。

---

## Web UI関連の問題

### Web UIでレコードが表示されない

#### 症状
- 設定ファイルにレコードが存在するが、Web UIに表示されない

#### 原因と解決方法

**原因: ブラウザのキャッシュ**

**解決方法**: ブラウザのキャッシュをクリアしてページを再読み込みしてください
- Chrome/Edge: `Ctrl+Shift+R`（Windows）、`Cmd+Shift+R`（macOS）
- Firefox: `Ctrl+F5`（Windows）、`Cmd+Shift+R`（macOS）

---

**原因: 設定ファイルの構文エラー**

**解決方法**: JSONの構文が正しいか確認してください
```bash
# 設定ファイルの検証（jqコマンドを使用）
cat appsettings.json | jq .

# エラーがある場合、エラーメッセージが表示されます
```

---

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

# DNSに関するログのみを表示
grep DNS logs/jumbodogx.log

# エラーのみを表示
grep ERROR logs/jumbodogx.log
```

詳細は[ロギング設定ガイド](../common/logging.md)を参照してください。

---

### nslookupでのテスト

`nslookup`コマンドでDNSサーバーをテストできます。

**基本的な使用方法**:
```bash
# Aレコードのクエリ
nslookup example.local localhost

# MXレコードのクエリ
nslookup -type=MX example.local localhost

# NSレコードのクエリ
nslookup -type=NS example.local localhost

# すべてのレコードを表示
nslookup -type=ANY example.local localhost
```

---

### digでのテスト

`dig`コマンド（macOS/Linux）でより詳細なテストができます。

**基本的な使用方法**:
```bash
# Aレコードのクエリ
dig @localhost example.local

# MXレコードのクエリ
dig @localhost MX example.local

# 詳細な出力
dig @localhost example.local +trace

# 応答時間の測定
dig @localhost example.local +stats
```

---

## よくある質問（FAQ）

### Q1: ポート53以外を使用できますか？

**A**: はい、設定ファイルで任意のポートを指定できます。

```json
{
  "DnsServer": {
    "Port": 5353  # ← 5353番ポートを使用
  }
}
```

クライアント側でポートを指定してクエリしてください。
```bash
nslookup -port=5353 example.local localhost
dig @localhost -p 5353 example.local
```

---

### Q2: IPv6に対応していますか？

**A**: はい、AAAAレコードを使用してIPv6アドレスを設定できます。

```json
{
  "Type": "AAAA",
  "Name": "example.local",
  "Value": "2001:0db8:85a3::8a2e:0370:7334",
  "TTL": 300
}
```

IPv6でバインドする場合:
```json
{
  "DnsServer": {
    "BindAddress": "::",  # ← IPv6のすべてのインターフェース
    "Port": 53
  }
}
```

---

### Q3: ワイルドカードレコードは使用できますか？

**A**: はい、ワイルドカード（*）を使用できます。

```json
{
  "Type": "A",
  "Name": "*.example.local",
  "Value": "192.168.1.100",
  "TTL": 300
}
```

これにより、`anything.example.local`が`192.168.1.100`に解決されます。

---

### Q4: 設定変更後、再起動が必要ですか？

**A**: いいえ、Web UIから設定を保存すると自動的に反映されます。ただし、JSON設定ファイルを直接編集した場合は、JumboDogXの再起動が必要です。

---

### Q5: DNSサーバーを外部に公開できますか？

**A**: 技術的には可能ですが、セキュリティ上のリスクがあるため推奨しません。外部に公開する場合は、必ずACLを設定し、信頼できるIPアドレスからのアクセスのみを許可してください。

詳細は[セキュリティベストプラクティス](../common/security-best-practices.md)を参照してください。

---

## サポート

問題が解決しない場合は、以下を参照してください：

- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
- [ディスカッション](https://github.com/furuya02/jumbodogx/discussions)

Issue報告時には、以下の情報を含めてください：
- JumboDogXのバージョン
- OS（Windows/macOS/Linux）とバージョン
- 設定ファイル（`appsettings.json`の関連部分）
- ログファイル（`logs/jumbodogx.log`の関連部分）
- エラーメッセージ

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [リソースレコード](resource-records.md)
- [ゾーン設定](zone-configuration.md)
- [ACL設定](../common/acl-configuration.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
