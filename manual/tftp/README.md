# TFTPサーバー

JumboDogXのTFTPサーバーは、シンプルなファイル転送機能を提供します。
RFC 1350に準拠し、ネットワーク機器のファームウェア更新やPXEブートに最適です。

## 主な機能

### 基本機能
- **RFC 1350準拠** - 標準的なTFTPプロトコルに対応
- **ファイル送受信** - GET/PUTによるファイル転送
- **高速転送** - 軽量プロトコルによる効率的な転送

### セキュリティ機能
- **アクセス制御（ACL）** - IPアドレスベースのアクセス制限
- **読み取り専用モード** - PUTを無効化してファイル保護
- **ルートディレクトリ制限** - 指定ディレクトリ外へのアクセス防止

### 高度な機能
- **柔軟なバインド設定** - 特定のIPアドレスやポートで待ち受け
- **タイムアウト設定** - 転送タイムアウトの制御
- **複数クライアント対応** - 同時接続のサポート

## クイックスタート

TFTPサーバーをすぐに使い始めたい方は、[クイックスタートガイド](getting-started.md)をご覧ください。

## ドキュメント一覧

### 基本設定
- [クイックスタート](getting-started.md) - 最初の設定と起動方法
- [アクセス制御](access-control.md) - ACLとアクセス権限の設定

### トラブルシューティング
- [トラブルシューティング](troubleshooting.md) - よくある問題と解決方法

## 設定例

### シンプルなTFTPサーバー

```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot",
    "TimeOut": 5
  }
}
```

### 読み取り専用TFTPサーバー

```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot",
    "TimeOut": 5,
    "ReadOnly": true
  }
}
```

### ACL設定付きTFTPサーバー

```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot",
    "TimeOut": 5,
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "192.168.1.0/24",
        "10.0.0.100"
      ]
    }
  }
}
```

**ACLモード**:
- `Allow`: 許可リストモード（リストにあるIPのみ許可）
- `Deny`: 拒否リストモード（リストにあるIP以外を許可）

## Web UI

JumboDogXは、Webブラウザから設定できる管理画面を提供しています。

### アクセス方法
1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **TFTP** を選択

### 設定画面
- **General** - 基本設定（ポート、ルートディレクトリ）
- **Options** - 読み取り専用モード、タイムアウト
- **ACL** - アクセス制御リスト

## ユースケース

### ネットワーク機器のファームウェア更新
ルーターやスイッチのファームウェアをTFTP経由で更新。

### PXEブート
ネットワークブートでOSインストーラーやブートイメージを配信。

### シンプルなファイル転送
軽量なプロトコルでの高速ファイル転送。

### 組み込みシステム開発
IoTデバイスやマイコンへのファームウェア転送。

### 学習・教育
TFTPプロトコルの仕組みを理解するための実験環境。

## クライアントからの利用

### Windowsでの利用

**TFTPクライアントの有効化**:
1. コントロールパネル → プログラムと機能
2. Windowsの機能の有効化または無効化
3. 「TFTPクライアント」にチェック

**ファイルダウンロード**:
```cmd
tftp -i localhost GET test.txt
```

**ファイルアップロード**:
```cmd
tftp -i localhost PUT test.txt
```

### macOS/Linuxでの利用

**ファイルダウンロード**:
```bash
tftp localhost
tftp> get test.txt
tftp> quit
```

**ファイルアップロード**:
```bash
tftp localhost
tftp> put test.txt
tftp> quit
```

**コマンドライン一発実行**:
```bash
# ダウンロード
curl -o test.txt tftp://localhost/test.txt

# アップロード
curl -T test.txt tftp://localhost/
```

### Pythonでの利用

```python
import tftpy

# ダウンロード
client = tftpy.TftpClient('localhost', 69)
client.download('test.txt', 'downloaded.txt')

# アップロード
client.upload('local.txt', 'remote.txt')
```

## ルートディレクトリ構造

TFTPサーバーは`RootDirectory`で指定したディレクトリをルートとして動作します。

```
RootDirectory/
├── firmware/
│   ├── router.bin
│   └── switch.bin
├── pxe/
│   ├── pxelinux.0
│   └── vmlinuz
└── test.txt
```

クライアントからは相対パスでアクセス:
```bash
tftp> get firmware/router.bin
tftp> get pxe/pxelinux.0
```

## 関連リンク

- [インストール手順](../common/installation.md)
- [ACL設定](../common/acl-configuration.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)

## 技術仕様

- **プロトコル**: TFTP (RFC 1350)
- **標準ポート**: 69（UDP）
- **最大ファイルサイズ**: 32MB（デフォルト）
- **ブロックサイズ**: 512バイト
- **タイムアウト**: 設定可能（デフォルト5秒）
- **転送モード**: octet（バイナリ）、netascii（テキスト）

## セキュリティ上の注意

### 認証なし
TFTPは認証機能を持たないため、機密ファイルの転送には使用しないでください。

### 暗号化なし
通信は暗号化されないため、セキュアな転送にはSFTPなどを使用してください。

### ACL設定推奨
必ずACLを設定し、信頼できるIPアドレスからのアクセスのみを許可してください。

### 読み取り専用モード
不要なファイルアップロードを防ぐため、読み取り専用モードの使用を推奨します。

## サポート

問題が発生した場合は、以下を参照してください：
- [トラブルシューティング](troubleshooting.md)
- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
