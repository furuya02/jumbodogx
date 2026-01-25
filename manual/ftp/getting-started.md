# FTPサーバー クイックスタートガイド

このガイドでは、JumboDogXのFTPサーバーを最短で起動し、ファイル転送サービスを提供する方法を説明します。

## 前提条件

- JumboDogXがインストールされていること
- .NET 9 Runtimeがインストールされていること

詳細は[インストール手順](../common/installation.md)を参照してください。

## ステップ1: JumboDogXの起動

### コマンドラインから起動

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

## ステップ3: FTPサーバーの基本設定

### 3-1. General設定画面を開く

1. サイドメニューから **Settings** をクリック
2. **FTP** セクションを展開
3. **General** をクリック

![FTP General設定メニュー](images/ftp-general-menu.png)
*サイドメニューからFTP > Generalを選択*

### 3-2. 基本設定を入力

以下の項目を設定します：

| 項目 | 設定値 | 説明 |
|------|--------|------|
| Enable FTP Server | ✓ ON | FTPサーバーを有効化 |
| Bind Address | `0.0.0.0` または `127.0.0.1` | すべてのネットワークインターフェースでリッスン（`0.0.0.0`）またはローカルのみ（`127.0.0.1`） |
| Port | `2121` | ポート番号（開発環境では2121を推奨。本番環境では21） |
| Timeout (seconds) | `300` | 接続タイムアウト時間（秒） |
| Max Connections | `10` | 最大同時接続数 |
| Banner Message | `JumboDogX FTP Server Ready` | 接続時に表示されるウェルカムメッセージ |
| Enable SYST Command | OFF | システム情報を返すSYSTコマンドの有効/無効 |
| PASV Reservation Time | `30` | パッシブモードのポート予約時間（秒） |

![FTP General設定画面](images/ftp-general-settings.png)
*FTP Generalの基本設定画面*

**ポート番号について**:
- **2121**: 開発環境やテスト環境で推奨（管理者権限不要）
- **21**: FTPの標準ポート（本番環境で使用。macOS/Linuxでは管理者権限が必要）

### 3-3. FTPS設定（オプション）

セキュアな通信を行う場合は、FTPS（FTP over SSL/TLS）を有効にします：

1. **Enable FTPS** をONにする
2. **Certificate File** に証明書ファイル（PFX形式）のパスを入力
   - 例: `/path/to/certificate.pfx`
3. **Certificate Password** に証明書のパスワードを入力

![FTPS設定](images/ftp-ftps-settings.png)
*FTPS（FTP over SSL/TLS）設定セクション*

**セキュリティ警告**:
証明書のパスワードは設定ファイルに平文で保存されます。本番環境では環境変数やASP.NET Core User Secretsの使用を推奨します。

### 3-4. 設定を保存

1. **Save Settings** ボタンをクリック
2. 成功メッセージが表示されることを確認
3. **サーバーを再起動して設定を反映**

## ステップ4: ユーザー設定

FTPサーバーにアクセスするユーザーを設定します。

### 4-1. User設定画面を開く

1. サイドメニューから **Settings** > **FTP** を展開
2. **User** をクリック

![FTPユーザー設定メニュー](images/ftp-user-menu.png)
*サイドメニューからFTP > Userを選択*

### 4-2. ユーザーを追加

**通常ユーザーの追加**:

1. **+ Add User** ボタンをクリック
2. 以下の情報を入力：
   - **Username**: ユーザー名（例：`testuser`）
   - **Password**: パスワード（強固なパスワードを推奨）
   - **Home Directory**: ホームディレクトリのパス（例：`/tmp/jumbodogx-ftp/testuser`）
   - **Access Control**: アクセス権限を選択
     - **Full (Upload & Download)**: アップロード・ダウンロード両方可能
     - **Download Only**: ダウンロードのみ可能
     - **Upload Only**: アップロードのみ可能

![FTPユーザー追加](images/ftp-user-add.png)
*ユーザーを追加した状態*

**匿名FTPユーザーの追加**（オプション）:

1. **Add Anonymous User** ボタンをクリック
2. 自動的に以下の設定でユーザーが追加されます：
   - **Username**: `anonymous`
   - **Password**: （空）
   - **Home Directory**: `/tmp/jumbodogx-ftp`
   - **Access Control**: `Download Only`（デフォルト）

![FTP匿名ユーザー](images/ftp-anonymous-user.png)
*匿名ユーザーを追加した状態*

**匿名FTPについて**:
- ユーザー名が `anonymous`（大文字小文字を区別しない）
- パスワードは空
- 通常は「Download Only」に設定して、ダウンロードのみを許可します

### 4-3. ホームディレクトリの準備

指定したホームディレクトリが存在しない場合は、事前に作成しておきます：

```bash
# macOS/Linux
mkdir -p /tmp/jumbodogx-ftp/testuser
chmod 755 /tmp/jumbodogx-ftp/testuser

# Windows
mkdir C:\tmp\jumbodogx-ftp\testuser
```

### 4-4. 設定を保存

1. **Save Settings** ボタンをクリック
2. 成功メッセージが表示されることを確認
3. **サーバーを再起動して設定を反映**

## ステップ5: ACL設定（アクセス制御）

FTPサーバーへのアクセスをIPアドレスで制限します。

### 5-1. ACL設定画面を開く

1. サイドメニューから **Settings** > **FTP** を展開
2. **ACL** をクリック

![FTP ACL設定メニュー](images/ftp-acl-menu.png)
*サイドメニューからFTP > ACLを選択*

### 5-2. ACL Modeを選択

ACLには2つのモードがあります：

| モード | 動作 | 使用例 |
|--------|------|--------|
| **Allow Mode** | リストに登録されたIPアドレスのみ許可。それ以外は全て拒否 | 特定のIPアドレスのみアクセスを許可したい場合 |
| **Deny Mode** | リストに登録されたIPアドレスのみ拒否。それ以外は全て許可 | 特定のIPアドレスのみブロックしたい場合 |

デフォルトは **Allow Mode** です。

![FTP ACL Mode](images/ftp-acl-mode.png)
*ACL Modeの選択画面*

### 5-3. ACLエントリを追加

**Allow Modeの場合の例**（ローカルホストとローカルネットワークを許可）:

1. **+ Add ACL Entry** ボタンをクリック
2. 以下の情報を入力：
   - **Name**: `Localhost`
   - **IP Address / Range**: `127.0.0.1`
3. もう一度 **+ Add ACL Entry** をクリックして、ローカルネットワークを追加：
   - **Name**: `LocalNetwork`
   - **IP Address / Range**: `192.168.1.0/24`
4. **Save Settings** ボタンをクリック

![FTP ACL エントリ追加](images/ftp-acl-entries.png)
*ACLエントリを追加した状態*

**サポートされているアドレス形式**:
- **単一IP**: `192.168.1.100`
- **CIDR記法**: `192.168.1.0/24`

### 5-4. 重要な注意事項

**Allow Modeの場合**:
- ACLリストが空の場合、すべての接続が拒否されます
- 自分のIPアドレスを必ず追加してください

**Deny Modeの場合**:
- ACLリストが空の場合、すべての接続が許可されます

## ステップ6: 動作確認

### 6-1. FTPクライアントで接続テスト

**コマンドラインFTPクライアント（macOS/Linux）**:

```bash
# FTPサーバーに接続
ftp localhost 2121

# ユーザー名とパスワードを入力
Name: testuser
Password: ********

# 接続成功メッセージ
220 JumboDogX FTP Server Ready
331 User name okay, need password.
230 User logged in, proceed.
ftp>

# ディレクトリ一覧を表示
ftp> ls

# ファイルをアップロード
ftp> put localfile.txt

# ファイルをダウンロード
ftp> get remotefile.txt

# 切断
ftp> quit
```

**FileZillaでの接続**（GUIクライアント）:

1. FileZillaを起動
2. 以下の情報を入力：
   - **ホスト**: `127.0.0.1` または `localhost`
   - **ポート**: `2121`
   - **プロトコル**: `FTP - ファイル転送プロトコル`
   - **ユーザー名**: `testuser`
   - **パスワード**: （設定したパスワード）
3. **クイック接続** をクリック

![FileZilla接続](images/ftp-filezilla-connect.png)
*FileZillaでFTPサーバーに接続*

**curlでのテスト**:

```bash
# ファイル一覧を取得
curl ftp://localhost:2121/ -u testuser:password

# ファイルをダウンロード
curl ftp://localhost:2121/file.txt -u testuser:password -o file.txt

# ファイルをアップロード
curl -T localfile.txt ftp://localhost:2121/ -u testuser:password
```

### 6-2. Dashboardで確認

1. サイドメニューから **Dashboard** をクリック
2. **FTP Server** セクションでサーバーの状態を確認
   - サーバーのステータス（起動中/停止中）
   - ポート番号
   - Bind Address
   - 現在の接続数

![Dashboard FTP Server](images/ftp-dashboard.png)
*DashboardのFTP Serverセクション*

## よくある問題と解決方法

### 接続できない

**原因1: サーバーが起動していない**

**解決策**:
1. General設定で「Enable FTP Server」がONになっているか確認
2. JumboDogXを再起動
3. ログで "FTP Server started successfully" メッセージを確認

**原因2: ACLで拒否されている**

**解決策**:
1. Settings > FTP > ACL を開く
2. Allow Modeの場合、自分のIPアドレス（`127.0.0.1`など）が登録されているか確認
3. "Save Settings"をクリックして再起動

**原因3: ポートが使用中**

```bash
# ポートの使用状況を確認（macOS/Linux）
lsof -i :2121

# ポートの使用状況を確認（Windows）
netstat -ano | findstr :2121
```

**解決策**: 別のポート番号（例：2122、2123）を使用する

**原因4: ファイアウォールでブロックされている**

**解決策**:
- macOS: システム設定 > ネットワーク > ファイアウォール でポート2121を許可
- Windows: Windowsファイアウォールでポート2121を許可
- Linux: `sudo ufw allow 2121/tcp`

### ログインできない

**原因1: ユーザー名またはパスワードが間違っている**

**解決策**:
1. Settings > FTP > User で設定を確認
2. パスワードが暗号化されて保存されている場合、再設定が必要な可能性があります

**原因2: ホームディレクトリが存在しない**

**解決策**:
```bash
# ホームディレクトリを作成（macOS/Linux）
mkdir -p /tmp/jumbodogx-ftp/testuser
chmod 755 /tmp/jumbodogx-ftp/testuser
```

### ファイルのアップロード/ダウンロードができない

**原因1: アクセス権限が不足している**

**解決策**:
1. Settings > FTP > User でAccess Controlを確認
2. アップロードには「Full」または「Upload Only」が必要
3. ダウンロードには「Full」または「Download Only」が必要

**原因2: ディレクトリの書き込み権限がない**

```bash
# 権限を確認（macOS/Linux）
ls -la /tmp/jumbodogx-ftp/testuser

# 書き込み権限を付与
chmod 755 /tmp/jumbodogx-ftp/testuser
```

### パッシブモードで接続できない

**原因**: ファイアウォールでパッシブモードのポート範囲がブロックされている

**解決策**:
- デフォルトのパッシブモードポート範囲: 50000-50100
- ファイアウォールでこの範囲を許可する

```bash
# Linux
sudo ufw allow 50000:50100/tcp

# macOS/Windows
ファイアウォール設定で手動で許可
```

## 次のステップ

基本的なFTPサーバーが動作したら、以下のドキュメントで更に機能を拡張できます：

- [FTPS設定](ftps.md) - SSL/TLSで暗号化通信を実現
- [ユーザー管理](user-management.md) - 複数ユーザーの管理と権限設定
- [トラブルシューティング](troubleshooting.md) - 詳細なトラブルシューティングガイド

## 設定ファイルでの設定（上級者向け）

Web UIの代わりに、直接設定ファイルを編集することもできます。

### appsettings.json の場所

```
src/Jdx.WebUI/appsettings.json
```

### FTPサーバー設定の例

```json
{
  "Jdx": {
    "FtpServer": {
      "Enabled": true,
      "BindAddress": "0.0.0.0",
      "Port": 2121,
      "TimeOut": 300,
      "MaxConnections": 10,
      "BannerMessage": "JumboDogX FTP Server Ready",
      "UseSyst": false,
      "ReservationTime": 30,
      "UseFtps": false,
      "CertificateFile": "",
      "CertificatePassword": "",
      "UserList": [
        {
          "UserName": "testuser",
          "Password": "password123",
          "HomeDirectory": "/tmp/jumbodogx-ftp/testuser",
          "AccessControl": 0
        }
      ],
      "EnableAcl": 0,
      "AclList": [
        {
          "Name": "Localhost",
          "Address": "127.0.0.1"
        },
        {
          "Name": "LocalNetwork",
          "Address": "192.168.1.0/24"
        }
      ]
    }
  }
}
```

**AccessControlの値**:
- `0`: Full (Upload & Download)
- `1`: Download Only
- `2`: Upload Only

**EnableAclの値**:
- `0`: Allow Mode
- `1`: Deny Mode

設定ファイルを編集した後は、JumboDogXを再起動してください。

## まとめ

これで、JumboDogXのFTPサーバーが起動し、ファイル転送サービスを提供できるようになりました。

### 完了したこと
✓ JumboDogXの起動
✓ FTPサーバーの基本設定（ポート、タイムアウトなど）
✓ ユーザー設定（通常ユーザー・匿名ユーザー）
✓ ACL設定（IPアドレスベースのアクセス制御）
✓ FTPクライアントでの接続確認
✓ Dashboardでの動作確認

### 重要なポイント
- デフォルトのポートは **2121**（開発環境向け）
- **Allow Mode** では、ACLリストに登録されたIPアドレスのみ接続可能
- ユーザーのホームディレクトリは事前に作成しておく必要があります
- 設定変更後は **サーバーの再起動が必要**
- パスワードは暗号化されて保存されます

### 次にできること
- FTPS化してセキュアな通信を実現（証明書の設定）
- 複数ユーザーの管理と権限設定
- 仮想フォルダのマウント（異なるディレクトリをマウント）
- アクセス制御で特定のIPアドレスのみ許可

さらに詳しい設定は、各マニュアルを参照してください。
