# TFTPサーバー クイックスタートガイド

このガイドでは、JumboDogXのTFTPサーバーを最短で起動し、シンプルなファイル転送を行う方法を説明します。

## 前提条件

- JumboDogXがインストールされていること
- .NET 9 Runtimeがインストールされていること

詳細は[インストール手順](../common/installation.md)を参照してください。

## TFTPサーバーとは？

TFTP（Trivial File Transfer Protocol）は、シンプルなファイル転送プロトコルです。FTPよりも機能は少ないですが、軽量で実装が簡単です。

### 用途

- **ネットワーク機器の設定**: ルーター、スイッチのファームウェア更新
- **PXEブート**: ネットワークブート用のファイル配信
- **シンプルなファイル転送**: 認証不要の軽量ファイル転送
- **学習・教育**: TFTPの仕組みを理解する

**注意**: JumboDogXのTFTPサーバーは、ローカルテスト環境専用です。本番環境での使用は推奨されません。

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

## ステップ3: TFTP基本設定

### 3-1. TFTP設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **TFTP** セクションを展開
3. **General** をクリック

![TFTP設定画面](images/tftp-general-menu.png)
*サイドメニューからTFTP > Generalを選択*

### 3-2. 基本設定を入力

以下の項目を設定します：

| 項目 | 設定値 | 説明 |
|------|--------|------|
| Enable Server | ✓ ON | TFTPサーバーを有効化 |
| Bind Address | `0.0.0.0` または `127.0.0.1` | すべてのネットワークインターフェースでリッスン（`0.0.0.0`）またはローカルのみ（`127.0.0.1`） |
| Port | `69` | TFTP標準ポート（テスト環境では6969なども可） |
| Root Directory | `/path/to/tftp/files` | ファイルの保存先ディレクトリ |
| Timeout | `5` | パケットのタイムアウト時間（秒） |
| Max Packet Size | `512` | 最大パケットサイズ（バイト、標準は512） |

**ポートについて**:
- **69番**: TFTP標準ポート（管理者権限が必要な場合あり）
- **6969番**: テスト用代替ポート

**Root Directoryについて**:
- TFTPサーバーがファイルを読み書きするディレクトリ
- 絶対パスを指定してください
- ディレクトリが存在しない場合は作成されます

![TFTP基本設定](images/tftp-general-settings.png)
*TFTP Generalの設定画面*

### 3-3. Root Directoryの作成

TFTPサーバー用のディレクトリを作成します：

**Linux / macOS**:
```bash
mkdir -p /var/tftp
chmod 755 /var/tftp
```

**Windows**:
```powershell
New-Item -ItemType Directory -Path C:\tftp -Force
```

### 3-4. 設定を保存

1. **Save Settings** ボタンをクリック
2. 成功メッセージが表示されることを確認
3. **設定は即座に反映されます**（再起動不要）

## ステップ4: ACL設定（アクセス制御）

**重要**: デフォルトでは、TFTPサーバーは **ACLがAllow Mode（全てDeny）** になっています。
このままではlocalhostからもアクセスできないため、ACL設定が必要です。

### 4-1. ACL設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **TFTP** セクションを展開
3. **Access Control** をクリック

![TFTP ACL設定画面](images/tftp-acl-menu.png)
*サイドメニューからTFTP > Access Controlを選択*

### 4-2. localhostからのアクセスを許可

1. **Add ACL Entry** ボタンをクリック
2. 追加されたエントリに以下を入力：
   - **Name**: `Localhost`
   - **IP Address / Range**: `127.0.0.1`
3. **Save Settings** ボタンをクリック

![ACLエントリ追加](images/tftp-acl-localhost.png)
*127.0.0.1を追加した状態*

### 4-3. ローカルネットワークからのアクセスを許可（オプション）

同じネットワーク内の他のマシンからもアクセスを許可する場合：

1. **Add ACL Entry** ボタンをクリック
2. 追加されたエントリに以下を入力：
   - **Name**: `Local Network`
   - **IP Address / Range**: `192.168.1.0/24`
3. **Save Settings** ボタンをクリック

詳細は[アクセス制御設定](access-control.md)を参照してください。

## ステップ5: テストファイルの配置

TFTPサーバーからダウンロードできるテストファイルを作成します。

### 5-1. テストファイルの作成

**Linux / macOS**:
```bash
echo "Hello from TFTP Server!" > /var/tftp/test.txt
chmod 644 /var/tftp/test.txt
```

**Windows**:
```powershell
"Hello from TFTP Server!" | Out-File -FilePath C:\tftp\test.txt -Encoding ASCII
```

### 5-2. ファイル構成の例

```
/var/tftp/  (または C:\tftp\)
├── test.txt                # テストファイル
├── firmware/               # ファームウェア用ディレクトリ
│   └── router.bin
├── configs/                # 設定ファイル用ディレクトリ
│   └── switch.cfg
└── images/                 # イメージファイル用ディレクトリ
    └── bootloader.img
```

![テストファイル作成](images/tftp-test-files.png)
*テストファイルの配置*

## ステップ6: ファイル転送のテスト

### 6-1. tftpコマンドでテスト（ダウンロード）

TFTPサーバーが正しく動作しているか、tftpコマンドで確認します。

#### Windows の場合

コマンドプロンプトまたはPowerShellで実行：

```cmd
# TFTPクライアントを使用
tftp -i localhost GET test.txt downloaded_test.txt

# ダウンロードしたファイルを確認
type downloaded_test.txt
```

**オプション説明**:
- `-i`: バイナリモード（デフォルトはテキストモード）

#### macOS / Linux の場合

ターミナルで実行：

```bash
# TFTPクライアントをインストール（必要に応じて）
# Ubuntu/Debian
sudo apt install tftp-hpa

# macOS
brew install tftp-hpa

# ファイルをダウンロード
tftp localhost <<EOF
get test.txt downloaded_test.txt
quit
EOF

# ダウンロードしたファイルを確認
cat downloaded_test.txt
```

**期待される出力**:
```
Hello from TFTP Server!
```

![tftpダウンロードテスト](images/tftp-download-test.png)
*tftpコマンドでのダウンロードテスト*

### 6-2. ファイルのアップロード

TFTPサーバーにファイルをアップロードするテストです。

**注意**: アップロード機能を有効にする場合は、セキュリティリスクに注意してください。

#### Windows の場合

```cmd
# テストファイルを作成
echo "Upload test" > upload_test.txt

# TFTPサーバーにアップロード
tftp -i localhost PUT upload_test.txt
```

#### macOS / Linux の場合

```bash
# テストファイルを作成
echo "Upload test" > upload_test.txt

# TFTPサーバーにアップロード
tftp localhost <<EOF
put upload_test.txt
quit
EOF
```

#### アップロードしたファイルの確認

**Linux / macOS**:
```bash
cat /var/tftp/upload_test.txt
```

**Windows**:
```powershell
type C:\tftp\upload_test.txt
```

### 6-3. Pythonスクリプトでテスト

Pythonの`tftpy`ライブラリを使用したテストスクリプトです。

まず、tftpyをインストール：

```bash
pip install tftpy
```

**tftp_test.py**:
```python
import tftpy

# TFTP設定
tftp_host = 'localhost'
tftp_port = 69

# TFTPクライアントを作成
client = tftpy.TftpClient(tftp_host, tftp_port)

try:
    # ファイルをダウンロード
    print('ファイルをダウンロード中...')
    client.download('test.txt', 'downloaded_test.txt')
    print('ダウンロード成功！')

    # ダウンロードしたファイルを表示
    with open('downloaded_test.txt', 'r') as f:
        print(f'内容: {f.read()}')

    # ファイルをアップロード（オプション）
    # print('ファイルをアップロード中...')
    # client.upload('upload.txt', 'uploaded_test.txt')
    # print('アップロード成功！')

except Exception as e:
    print(f'エラー: {e}')
```

実行：

```bash
python tftp_test.py
```

## ステップ7: 動作確認

### 7-1. Dashboardで確認

1. サイドメニューから **Dashboard** をクリック
2. **TFTP Server** セクションでサーバーのステータスを確認
3. サーバーが起動していることを確認
4. **Total Transfers** （総転送数）を確認

![Dashboard TFTP](images/dashboard-tftp.png)
*DashboardのTFTP Serverセクション*

### 7-2. ログの確認

#### Web UIでログを確認

1. サイドメニューから **Logs** をクリック
2. **Category** ドロップダウンから **Jdx.Servers.Tftp** を選択
3. ファイル転送のログが表示されます

![TFTPログ](images/tftp-logs.png)
*TFTPサーバーのログ*

#### ログファイルで確認

```bash
# TFTPサーバーのログを確認
cat logs/jumbodogx-*.log | jq 'select(.SourceContext == "Jdx.Servers.Tftp.TftpServer")'

# ファイル転送のログのみ表示
cat logs/jumbodogx-*.log | jq 'select(.["@mt"] | test("File transfer"))'
```

## よくある問題と解決方法

### TFTPサーバーに接続できない

**症状**: tftpコマンドでタイムアウトする

**原因1: ACLで拒否されている**

TFTPサーバーはデフォルトでACLがAllow Mode（全てDeny）になっているため、ACL設定が必要です。

**解決策**:
1. Settings > TFTP > Access Control を開く
2. "Add ACL Entry"で`127.0.0.1`を追加
3. "Save Settings"をクリック

**原因2: サーバーが起動していない**

**解決策**:
1. Dashboard > TFTP Serverでステータスを確認
2. Settings > TFTP > Generalで "Enable Server" がONか確認

**原因3: ポートが使用中**

```bash
# ポートの使用状況を確認（macOS/Linux）
sudo lsof -i :69

# ポートの使用状況を確認（Windows）
netstat -ano | findstr :69
```

**解決策**: 別のポート番号（例：6969）を使用する

### ファイルが見つからない

**症状**: "File not found" エラー

**原因**: Root Directory内にファイルが存在しない

**解決策**:
```bash
# Root Directoryを確認
ls -la /var/tftp/

# ファイルを配置
cp /path/to/file.txt /var/tftp/
chmod 644 /var/tftp/file.txt
```

### アップロードできない

**症状**: "Permission denied" または "Access violation" エラー

**原因1: Root Directoryの書き込み権限がない**

**解決策**:
```bash
# ディレクトリの権限を確認（Linux/macOS）
ls -la /var/tftp/
chmod 755 /var/tftp/

# Windowsの場合は、フォルダのプロパティ > セキュリティで書き込み権限を付与
```

**原因2: アップロード先ファイルが既に存在する**

TFTPは既存ファイルの上書きを許可しない場合があります。

**解決策**: 既存ファイルを削除または別のファイル名を使用

### "Permission denied" エラー（ポート69使用時）

**症状**: ポート69でサーバーが起動できない

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

または、高いポート番号（6969など）を使用する。

## 次のステップ

基本的なTFTPサーバーが動作したら、以下のドキュメントで更に機能を拡張できます：

- [アクセス制御設定](access-control.md) - ACLの詳細設定
- [トラブルシューティング](troubleshooting.md) - よくある問題の解決方法
- [ACL設定ガイド](../common/acl-configuration.md) - アクセス制御の詳細設定

## 実用的な使用例

### 例1: ネットワーク機器のファームウェア更新

ルーターやスイッチのファームウェアをTFTP経由で更新：

**設定**:
- Root Directory: `/var/tftp/firmware`
- ファームウェアファイルを配置: `router_v2.0.bin`

**ルーターからのアクセス**:
```
Router# copy tftp://192.168.1.100/router_v2.0.bin flash:
```

### 例2: PXEブート環境

ネットワークブート用のファイルを配信：

**ディレクトリ構成**:
```
/var/tftp/
├── pxelinux.0           # PXE ブートローダー
├── pxelinux.cfg/        # PXE 設定
│   └── default
├── vmlinuz              # Linux カーネル
└── initrd.img           # initramfs
```

**DHCPサーバー設定**（別途）:
```
next-server 192.168.1.100;
filename "pxelinux.0";
```

### 例3: 設定ファイルのバックアップ

ネットワーク機器の設定ファイルをバックアップ：

**スイッチからのバックアップ**:
```
Switch# copy running-config tftp://192.168.1.100/switch-backup.cfg
```

**定期的なバックアップスクリプト**（Pythonの例）:
```python
import tftpy
import datetime

# TFTP設定
tftp_host = '192.168.1.100'

# バックアップファイル名（日付付き）
backup_file = f'switch-backup-{datetime.date.today()}.cfg'

# TFTPでアップロード
client = tftpy.TftpClient(tftp_host, 69)
# (ここではスイッチからアップロードされることを想定)
```

## まとめ

このガイドでは、以下の手順を完了しました：

✓ JumboDogXの起動とWeb管理画面へのアクセス
✓ TFTP基本設定（ポート、Root Directory）
✓ ACL設定（localhostからのアクセス許可）
✓ テストファイルの配置
✓ ファイル転送のテスト（ダウンロード、アップロード）
✓ ログの確認

これで、JumboDogXのTFTPサーバーが起動し、シンプルなファイル転送ができるようになりました！

### 重要なポイント

- **TFTPサーバーはデフォルトでACLがAllow Mode（全てDeny）** - 127.0.0.1の追加が必須
- ポート69を使用するには管理者権限が必要（または6969を使用）
- Root Directoryの読み書き権限を適切に設定する必要がある
- TFTPは認証機能がないため、ACLで適切にアクセス制御してください
- 設定は即座に反映されるため、再起動は不要

### セキュリティの注意

- JumboDogXのTFTPサーバーは、ローカルテスト環境専用です
- **TFTPには認証機能がありません** - ACLで必ずアクセス制御してください
- インターネットに直接公開しないでください
- アップロード機能を有効にする場合は、特に注意が必要です
- 機密ファイルの転送には使用しないでください

さらに詳しい設定は、各マニュアルを参照してください。
