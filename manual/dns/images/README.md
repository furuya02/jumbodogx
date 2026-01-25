# DNSサーバー スクリーンショット撮影ガイド

このディレクトリには、DNSサーバーのクイックスタートガイドで使用するスクリーンショットを配置します。

## 必要なスクリーンショット一覧

### 1. dashboard-top.png
**撮影対象**: JumboDogXのダッシュボード画面（トップページ）
**撮影タイミング**: アプリケーション起動直後
**URL**: `http://localhost:5001/`
**説明**:
- ダッシュボードのトップ画面全体
- サイドメニュー、ヘッダーを含む
- DNS Serverセクションが表示されている状態

---

### 2. dns-general-menu.png
**撮影対象**: サイドメニューのDNS > General
**撮影タイミング**: Settings > DNSを展開した状態
**説明**:
- サイドメニューが展開された状態
- DNSセクションが展開され、Generalが表示されている状態
- ハイライト表示でGeneralが選択されている

---

### 3. dns-general-settings.png
**撮影対象**: DNS General設定画面
**撮影タイミング**: DNS General設定画面を開いた状態
**URL**: `http://localhost:5001/settings/dns/general`
**説明**:
- Basic Settingsセクション（Enable DNS Server, Bind Address, Port, Timeout）
- 各フィールドにデフォルト値が入力されている状態
- Enable DNS Serverがオンになっている

---

### 4. dns-records-menu.png
**撮影対象**: サイドメニューからDNS > Recordsを選択
**撮影タイミング**: Settings > DNSを展開した状態
**説明**:
- サイドメニューが展開された状態
- DNSセクションの下にRecordsメニューが表示されている
- ハイライト表示でRecordsが選択されている

---

### 5. dns-record-add.png
**撮影対象**: DNSレコードを追加する画面
**撮影タイミング**: Add Recordボタンをクリックした後
**URL**: `http://localhost:5001/settings/dns/records`
**説明**:
- Add Record フォームが表示されている:
  - Type: ドロップダウンで"A"を選択
  - Name: "test.local"
  - Value: "192.168.1.100"
  - TTL: "300"
- "Add"ボタンが表示されている

---

### 6. dns-records-list.png
**撮影対象**: DNSレコード一覧画面
**撮影タイミング**: レコードを2-3個追加した後
**URL**: `http://localhost:5001/settings/dns/records`
**説明**:
- DNS Recordsテーブルに複数のレコードが表示されている:
  - Record 1:
    - Type: "A"
    - Name: "test.local"
    - Value: "192.168.1.100"
    - TTL: "300"
  - Record 2:
    - Type: "CNAME"
    - Name: "www.local"
    - Value: "test.local"
    - TTL: "300"
  - Record 3:
    - Type: "MX"
    - Name: "mail.local"
    - Value: "10 mail.local"
    - TTL: "300"
- 各レコードにEdit, Deleteボタンが表示されている
- "Save Settings"ボタンが表示されている

---

### 7. dns-record-types.png
**撮影対象**: レコードタイプのドロップダウン
**撮影タイミング**: Add Record画面でTypeドロップダウンを開いた状態
**URL**: `http://localhost:5001/settings/dns/records`
**説明**:
- Typeドロップダウンが展開されている
- 以下のレコードタイプが表示されている:
  - A
  - AAAA
  - CNAME
  - MX
  - NS
  - PTR
  - SOA

---

### 8. dns-acl-menu.png
**撮影対象**: サイドメニューからDNS > ACLを選択
**撮影タイミング**: Settings > DNSを展開した状態
**説明**:
- サイドメニューが展開された状態
- DNSセクションの下にACLメニューが表示されている
- ハイライト表示でACLが選択されている

---

### 9. dns-acl-mode.png
**撮影対象**: ACL Mode選択画面
**撮影タイミング**: ACL設定画面を開いた状態
**URL**: `http://localhost:5001/settings/dns/acl`
**説明**:
- ACL Modeセクション
- Allow Mode（ラジオボタン選択）
- Deny Mode（ラジオボタン未選択）
- 現在のモードの説明（青色の情報メッセージ）

---

### 10. dns-acl-entries.png
**撮影対象**: ACLエントリを追加した状態
**撮影タイミング**: Add ACL Entryで127.0.0.1とローカルネットワークを追加した後
**URL**: `http://localhost:5001/settings/dns/acl`
**説明**:
- Access Control Listテーブルに以下のエントリが表示されている:
  - Entry 1:
    - Name: "Localhost"
    - IP Address / Range: "127.0.0.1"
  - Entry 2:
    - Name: "LocalNetwork"
    - IP Address / Range: "192.168.1.0/24"
- "Save Settings"ボタンが表示されている

---

### 11. dns-nslookup-test.png
**撮影対象**: nslookupコマンドでDNS解決をテストした結果
**撮影タイミング**: ターミナルでnslookupコマンドを実行した後
**説明**:
- ターミナルまたはコマンドプロンプトのスクリーンショット
- 以下のコマンドと結果が表示されている:
```
nslookup test.local localhost
Server:  localhost
Address:  127.0.0.1

Name:    test.local
Address:  192.168.1.100
```

---

### 12. dns-dig-test.png
**撮影対象**: digコマンドでDNS解決をテストした結果
**撮影タイミング**: ターミナルでdigコマンドを実行した後（macOS/Linux）
**説明**:
- ターミナルのスクリーンショット
- 以下のコマンドと結果が表示されている:
```
dig @localhost test.local

; <<>> DiG 9.x.x <<>> @localhost test.local
; (1 server found)
;; global options: +cmd
;; Got answer:
;; ->>HEADER<<- opcode: QUERY, status: NOERROR, id: xxxxx
;; flags: qr aa rd; QUERY: 1, ANSWER: 1, AUTHORITY: 0, ADDITIONAL: 0

;; QUESTION SECTION:
;test.local.                    IN      A

;; ANSWER SECTION:
test.local.             300     IN      A       192.168.1.100

;; Query time: x msec
;; SERVER: 127.0.0.1#53(localhost)
;; WHEN: ...
;; MSG SIZE  rcvd: xx
```

---

### 13. dns-dashboard.png
**撮影対象**: DashboardのDNS Serverセクション
**撮影タイミング**: DNSサーバーが起動している状態
**URL**: `http://localhost:5001/`
**説明**:
- DNS Serverセクション
- サーバーのステータス（Running）
- ポート番号（53）
- Bind Address（0.0.0.0）
- 現在のクエリ数などの統計情報

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
1. JumboDogXを起動 → **dashboard-top.png**
2. Settings > DNS > Generalを開く → **dns-general-menu.png**, **dns-general-settings.png**
3. Settings > DNS > Recordsを開く → **dns-records-menu.png**
4. Add Recordでtest.localを追加（撮影用にフォームを開いた状態） → **dns-record-add.png**
5. Typeドロップダウンを開く → **dns-record-types.png**
6. test.local（A）、www.local（CNAME）、mail.local（MX）を追加 → **dns-records-list.png**
7. Settings > DNS > ACLを開く → **dns-acl-menu.png**, **dns-acl-mode.png**
8. Add ACL Entryで127.0.0.1とローカルネットワークを追加 → **dns-acl-entries.png**
9. Dashboardに戻る → **dns-dashboard.png**

### 第2段階: DNSクエリのテスト
10. ターミナルまたはコマンドプロンプトを開く
11. nslookup test.local localhostを実行 → **dns-nslookup-test.png**
12. （macOS/Linux）dig @localhost test.localを実行 → **dns-dig-test.png**

---

## テストデータの準備

スクリーンショット撮影用に、以下のテストデータを準備してください：

### DNSレコード

**レコード1（Aレコード）**:
```
Type: A
Name: test.local
Value: 192.168.1.100
TTL: 300
```

**レコード2（CNAMEレコード）**:
```
Type: CNAME
Name: www.local
Value: test.local
TTL: 300
```

**レコード3（MXレコード）**:
```
Type: MX
Name: mail.local
Value: 10 mail.local
TTL: 300
```

### ACLエントリ

**エントリ1**:
```
Name: Localhost
IP Address / Range: 127.0.0.1
```

**エントリ2**:
```
Name: LocalNetwork
IP Address / Range: 192.168.1.0/24
```

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
- [ ] dns-general-menu.png
- [ ] dns-general-settings.png
- [ ] dns-records-menu.png
- [ ] dns-record-add.png
- [ ] dns-records-list.png
- [ ] dns-record-types.png
- [ ] dns-acl-menu.png
- [ ] dns-acl-mode.png
- [ ] dns-acl-entries.png
- [ ] dns-nslookup-test.png
- [ ] dns-dig-test.png
- [ ] dns-dashboard.png

全て揃ったらこのチェックリストを完了させてください。
