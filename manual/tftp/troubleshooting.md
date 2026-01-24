# TFTPサーバー - トラブルシューティング

JumboDogXのTFTPサーバーで発生する可能性のある問題と解決方法を説明します。

## 一般的な問題

### TFTPサーバーが起動しない

#### 症状
- JumboDogXを起動してもTFTPサーバーが動作しない
- ダッシュボードでTFTPサーバーのステータスが"Stopped"または"Error"

#### 原因と解決方法

**原因1: ポート69が既に使用されている**

**解決方法**:

**macOS**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :69
```

**Windows**:
```cmd
# 使用中のプロセスを確認
netstat -ano | findstr :69
```

**Linux**:
```bash
# 使用中のプロセスを確認
sudo lsof -i :69

# tftpd-hpaが動作している場合、停止
sudo systemctl stop tftpd-hpa
```

**代替ポートの使用**（推奨）:
```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 6969,  # ← 代替ポートを使用
    "RootDirectory": "C:\\TftpRoot"
  }
}
```

**注意**: 標準のTFTPクライアントはポート69を使用するため、代替ポートでは接続できない場合があります。

---

**原因2: 管理者権限が不足している**

ポート69（1024未満の特権ポート）を使用する場合、管理者権限が必要です。

**解決方法**:

**macOS/Linux**:
```bash
sudo dotnet run --project src/Jdx.WebUI
```

**Windows**:
- コマンドプロンプトまたはPowerShellを「管理者として実行」

---

**原因3: `Enabled`が`false`になっている**

**解決方法**:
```json
{
  "TftpServer": {
    "Enabled": true,  # ← trueに設定
    "BindAddress": "0.0.0.0",
    "Port": 69
  }
}
```

---

**原因4: RootDirectoryが存在しない**

**解決方法**: ルートディレクトリを作成してください

```bash
# Windows
mkdir C:\TftpRoot

# macOS/Linux
mkdir -p /path/to/TftpRoot
```

---

### ファイルをダウンロードできない

#### 症状
- TFTPクライアントでGETコマンドを実行してもファイルがダウンロードできない
- タイムアウトエラーが発生する

#### 原因と解決方法

**原因1: ファイアウォールでポートがブロックされている**

**解決方法**:

**Windows**:
```powershell
# ファイアウォールルールを追加
New-NetFirewallRule -DisplayName "JumboDogX TFTP" -Direction Inbound -Protocol UDP -LocalPort 69 -Action Allow
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
# ポート69を許可
sudo ufw allow 69/udp
```

---

**原因2: ACLで拒否されている**

**解決方法**: クライアントのIPアドレスを確認し、ACLに追加してください

```bash
# クライアントのIPアドレスを確認
ipconfig  # Windows
ifconfig  # macOS/Linux
```

```json
{
  "TftpServer": {
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "192.168.1.100"  # ← クライアントのIPを追加
      ]
    }
  }
}
```

詳細は[アクセス制御ガイド](access-control.md)を参照してください。

---

**原因3: ファイルが存在しない**

**解決方法**: ルートディレクトリにファイルが存在するか確認してください

```bash
# Windows
dir C:\TftpRoot

# macOS/Linux
ls -la /path/to/TftpRoot
```

ファイルが存在しない場合、ルートディレクトリに配置:
```bash
# Windows
copy test.txt C:\TftpRoot\

# macOS/Linux
cp test.txt /path/to/TftpRoot/
```

---

**原因4: ファイルのアクセス権限が不適切**

**解決方法**: ファイルに読み取り権限があるか確認してください

**Windows**:
```cmd
icacls C:\TftpRoot\test.txt
```

**macOS/Linux**:
```bash
ls -l /path/to/TftpRoot/test.txt

# 読み取り権限を追加
chmod 644 /path/to/TftpRoot/test.txt
```

---

### ファイルをアップロードできない

#### 症状
- TFTPクライアントでPUTコマンドを実行してもファイルがアップロードできない

#### 原因と解決方法

**原因1: 読み取り専用モードが有効**

**解決方法**: 読み取り専用モードを無効にしてください

```json
{
  "TftpServer": {
    "ReadOnly": false  # ← アップロードを許可
  }
}
```

**注意**: セキュリティリスクを考慮してください。読み取り専用モードの使用を推奨します。

---

**原因2: ディレクトリに書き込み権限がない**

**解決方法**: ルートディレクトリに書き込み権限があるか確認してください

**Windows**:
```cmd
icacls C:\TftpRoot
```

**macOS/Linux**:
```bash
ls -ld /path/to/TftpRoot

# 書き込み権限を追加
chmod 755 /path/to/TftpRoot
```

---

**原因3: ACLで拒否されている**

**解決方法**: クライアントのIPアドレスをACLに追加してください（ダウンロードと同様）

---

## 接続の問題

### タイムアウトエラーが発生する

#### 症状
- TFTPクライアントで接続するとタイムアウトエラーが発生する

#### 原因と解決方法

**原因1: サーバーが起動していない**

**解決方法**: TFTPサーバーが起動しているか確認してください

```bash
# ログを確認
grep "TFTP server started" logs/jumbodogx.log
```

---

**原因2: BindAddressが間違っている**

**解決方法**: BindAddressを確認してください

すべてのネットワークインターフェースで待ち受ける場合:
```json
{
  "TftpServer": {
    "BindAddress": "0.0.0.0"
  }
}
```

特定のIPアドレスで待ち受ける場合:
```json
{
  "TftpServer": {
    "BindAddress": "192.168.1.1"  # サーバーのIPアドレス
  }
}
```

---

**原因3: ネットワーク遅延**

**解決方法**: タイムアウト値を増やしてください

```json
{
  "TftpServer": {
    "TimeOut": 10  # 10秒に増加（デフォルトは5秒）
  }
}
```

---

### 転送が途中で止まる

#### 症状
- ファイル転送が途中で止まる
- 大きなファイルの転送に失敗する

#### 原因と解決方法

**原因1: タイムアウト値が短い**

**解決方法**: タイムアウト値を増やしてください

```json
{
  "TftpServer": {
    "TimeOut": 30  # 30秒に増加
  }
}
```

---

**原因2: ネットワーク接続が不安定**

**解決方法**: ネットワーク接続を確認してください

```bash
# サーバーへのping
ping 192.168.1.1

# パケットロスを確認
ping -c 100 192.168.1.1  # macOS/Linux
ping -n 100 192.168.1.1  # Windows
```

---

**原因3: ファイルサイズが大きすぎる**

TFTPは大きなファイルの転送には向いていません。

**解決方法**:
- ファイルを分割する
- または、SCP/SFTP/FTPなど別のプロトコルを使用する

---

## パフォーマンスの問題

### 転送速度が遅い

#### 症状
- ファイル転送に時間がかかる

#### 原因と解決方法

**原因1: TFTPプロトコルの制限**

TFTPはシンプルなプロトコルであり、転送速度は他のプロトコル（FTP、SCP等）より遅くなります。

**解決方法**: 大量のファイルや大きなファイルの転送には、FTPやSCPを使用してください。

---

**原因2: ネットワーク遅延**

**解決方法**: ネットワーク遅延を測定してください

```bash
# ネットワーク遅延の測定
ping 192.168.1.1
```

---

## クライアント関連の問題

### Windowsクライアントで接続できない

#### 症状
- WindowsのTFTPクライアントで接続できない

#### 原因と解決方法

**原因: TFTPクライアントが有効になっていない**

**解決方法**: WindowsのTFTPクライアントを有効化してください

1. コントロールパネル → プログラムと機能
2. Windowsの機能の有効化または無効化
3. 「TFTPクライアント」にチェック
4. OKをクリックして再起動

---

### curlで接続できない

#### 症状
- `curl -T file.txt tftp://localhost/`でアップロードできない

#### 原因と解決方法

**原因1: 読み取り専用モードが有効**

**解決方法**: 読み取り専用モードを無効にしてください（または、GETを使用）

```bash
# ダウンロード（読み取り専用モードでも可能）
curl -o test.txt tftp://localhost/test.txt

# アップロード（読み取り専用モードでは不可）
curl -T test.txt tftp://localhost/
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
chmod 644 appsettings.json

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

# TFTPに関するログのみを表示
grep TFTP logs/jumbodogx.log

# エラーのみを表示
grep ERROR logs/jumbodogx.log

# ACL拒否のログを表示
grep "ACL denied" logs/jumbodogx.log
```

詳細は[ロギング設定ガイド](../common/logging.md)を参照してください。

---

### TFTPクライアントでのテスト

**Windows**:
```cmd
# ファイルダウンロード
tftp -i localhost GET test.txt

# ファイルアップロード
tftp -i localhost PUT test.txt
```

**macOS/Linux**:
```bash
# インタラクティブモード
tftp localhost
tftp> get test.txt
tftp> put test.txt
tftp> quit

# コマンドライン一発実行
curl -o test.txt tftp://localhost/test.txt
curl -T test.txt tftp://localhost/
```

---

### ネットワークキャプチャ

Wiresharkなどのパケットキャプチャツールを使用して、TFTPトラフィックを確認できます。

**Wiresharkフィルター**:
```
tftp
```

---

## よくある質問（FAQ）

### Q1: ポート69以外を使用できますか？

**A**: はい、設定ファイルで任意のポートを指定できます。ただし、標準のTFTPクライアントはポート69を使用するため、接続できない場合があります。

```json
{
  "TftpServer": {
    "Port": 6969  # 代替ポート
  }
}
```

---

### Q2: 認証機能はありますか？

**A**: いいえ、TFTPプロトコルは認証機能を持ちません。アクセス制御にはACLを使用してください。

---

### Q3: 暗号化に対応していますか？

**A**: いいえ、TFTPは暗号化されません。機密ファイルの転送には、SCP/SFTPを使用してください。

---

### Q4: 大きなファイルを転送できますか？

**A**: TFTPはシンプルなプロトコルであり、大きなファイルの転送には向いていません。最大32MB（デフォルト）までです。

---

### Q5: サブディレクトリにアクセスできますか？

**A**: はい、ルートディレクトリ配下のサブディレクトリにアクセスできます。

```bash
# サブディレクトリ内のファイルをダウンロード
tftp localhost
tftp> get firmware/router.bin
```

---

## セキュリティ上の注意

### TFTPの制限

TFTPは以下の制限があります：

- **認証なし**: 誰でもアクセス可能（ACLで制限可能）
- **暗号化なし**: 通信内容が平文で送信される
- **シンプル**: 転送速度が遅く、大きなファイルには不向き

### 推奨事項

1. **ACLを必ず設定**: 信頼できるIPアドレスのみを許可
2. **読み取り専用モードを使用**: ファイルアップロードを無効化
3. **機密ファイルを配置しない**: 暗号化されないため
4. **ローカルネットワークのみで使用**: 外部からのアクセスを防ぐ
5. **ログを監視**: 不正なアクセスを検出

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
- 使用しているTFTPクライアント

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [アクセス制御](access-control.md)
- [ACL設定](../common/acl-configuration.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
