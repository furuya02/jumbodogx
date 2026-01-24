# FTPサーバー

JumboDogXのFTPサーバーは、ファイル転送機能を提供する高機能FTPサーバーです。
パッシブモード、アクティブモード、FTPS（FTP over SSL/TLS）、ユーザー認証、アクセス制御など、
企業レベルのファイル転送サービスに必要な機能を備えています。

## 主な機能

### 基本機能
- **FTP (RFC 959)準拠** - 標準的なFTPプロトコルをサポート
- **パッシブモード** - ファイアウォール環境での接続に対応
- **アクティブモード** - 従来型のFTP接続方式
- **ユーザー認証** - ユーザー名・パスワードによる認証
- **匿名FTP** - 匿名ユーザーのアクセスに対応

### セキュリティ機能
- **FTPS (FTP over SSL/TLS)** - 暗号化通信に対応
- **アクセス制御（ACL）** - IPアドレスベースのアクセス制限
- **ユーザー権限管理** - アップロード/ダウンロードの権限制御

### 高度な機能
- **仮想フォルダマウント** - 異なるディレクトリをマウント可能
- **複数ユーザー管理** - ユーザーごとにホームディレクトリと権限を設定
- **柔軟な設定** - タイムアウト、最大接続数などを細かく調整可能

## クイックスタート

FTPサーバーをすぐに使い始めたい方は、[クイックスタートガイド](getting-started.md)をご覧ください。

## ドキュメント一覧

### 基本設定
- [クイックスタート](getting-started.md) - 最初の設定と起動方法
- [ユーザー管理](user-management.md) - ユーザーアカウントの管理方法

### 機能別ガイド
- [FTPS設定](ftps.md) - SSL/TLSによる暗号化通信の設定

### トラブルシューティング
- [トラブルシューティング](troubleshooting.md) - よくある問題と解決方法

## 設定例

### シンプルなFTPサーバー

```json
{
  "FtpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 2121,
    "TimeOut": 300,
    "MaxConnections": 10,
    "BannerMessage": "JumboDogX FTP Server Ready",
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
      }
    ]
  }
}
```

### 匿名FTPサーバー（ダウンロードのみ）

```json
{
  "FtpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 2121,
    "UserList": [
      {
        "UserName": "anonymous",
        "Password": "",
        "HomeDirectory": "/tmp/jumbodogx-ftp",
        "AccessControl": 1
      }
    ],
    "EnableAcl": 1,
    "AclList": []
  }
}
```

**AccessControl の値**:
- `0`: Full (Upload & Download)
- `1`: Download Only
- `2`: Upload Only

**EnableAcl の値**:
- `0`: Allow Mode（リストに登録されたIPのみ許可）
- `1`: Deny Mode（リストに登録されたIPのみ拒否）

### FTPS対応サーバー（暗号化通信）

```json
{
  "FtpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 2121,
    "UseFtps": true,
    "CertificateFile": "/path/to/certificate.pfx",
    "CertificatePassword": "your-password",
    "UserList": [
      {
        "UserName": "secureuser",
        "Password": "strongpassword",
        "HomeDirectory": "/tmp/jumbodogx-ftp/secureuser",
        "AccessControl": 0
      }
    ]
  }
}
```

## Web UI

JumboDogXは、Webブラウザから設定できる管理画面を提供しています。

### アクセス方法
1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **FTP** を選択

### 設定画面
- **General** - 基本設定とFTPS設定
- **User** - ユーザーアカウントの管理
- **ACL** - アクセス制御リスト
- **Virtual Folder** - 仮想フォルダマウント

## ユースケース

### ファイル共有サーバー
社内やチーム内でのファイル共有に利用できます。

### バックアップサーバー
定期的なファイルバックアップの転送先として活用できます。

### 開発環境のファイル転送
開発環境とテスト環境間でのファイル転送に便利です。

### 公開ファイル配布
匿名FTPを使って、誰でもアクセス可能なファイル配布サーバーとして運用できます。

## 接続方法

### コマンドラインクライアント

**macOS/Linux**:
```bash
ftp localhost 2121
```

**Windows**:
```cmd
ftp
open localhost 2121
```

### GUIクライアント

以下のFTPクライアントで接続できます：
- **FileZilla** - クロスプラットフォーム対応の定番FTPクライアント
- **Cyberduck** - macOS/Windows対応のファイル転送クライアント
- **WinSCP** - Windows用のSCP/FTPクライアント

### curlコマンド

```bash
# ファイル一覧
curl ftp://localhost:2121/ -u username:password

# ファイルダウンロード
curl ftp://localhost:2121/file.txt -u username:password -o file.txt

# ファイルアップロード
curl -T localfile.txt ftp://localhost:2121/ -u username:password
```

## 関連リンク

- [インストール手順](../common/installation.md)
- [ACL設定](../common/acl-configuration.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)

## 技術仕様

- **プロトコル**: FTP (RFC 959), FTPS (FTP over SSL/TLS)
- **転送モード**: パッシブモード、アクティブモード
- **認証方式**: ユーザー名/パスワード、匿名認証
- **対応証明書**: PFX形式（FTPS使用時）
- **最大接続数**: 設定可能（デフォルト10）
- **タイムアウト**: 設定可能（デフォルト300秒）
- **パッシブモードポート範囲**: 50000-50100（設定可能）

## サポート対象のFTPコマンド

### 認証コマンド
- `USER` - ユーザー名の送信
- `PASS` - パスワードの送信

### ファイル転送コマンド
- `LIST` - ディレクトリ一覧（詳細）
- `NLST` - ファイル名一覧
- `RETR` - ファイルのダウンロード
- `STOR` - ファイルのアップロード
- `APPE` - ファイルへの追記

### ディレクトリ操作コマンド
- `PWD` - 現在のディレクトリを表示
- `CWD` - ディレクトリの変更
- `CDUP` - 親ディレクトリへ移動
- `MKD` - ディレクトリの作成
- `RMD` - ディレクトリの削除

### ファイル操作コマンド
- `DELE` - ファイルの削除
- `RNFR/RNTO` - ファイルのリネーム
- `SIZE` - ファイルサイズの取得

### その他のコマンド
- `TYPE` - 転送タイプの設定（A: ASCII, I: Binary）
- `PASV` - パッシブモードへ切り替え
- `PORT` - アクティブモードへ切り替え
- `QUIT` - 接続の切断
- `SYST` - システム情報の取得（オプション）

## サポート

問題が発生した場合は、以下を参照してください：
- [トラブルシューティング](troubleshooting.md)
- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
