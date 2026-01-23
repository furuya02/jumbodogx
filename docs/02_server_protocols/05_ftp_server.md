# FTPサーバー

## 1. 概要

JumboDogX FTPサーバーは、ファイル転送サービスを提供します。

### 1.1 主要機能

- FTP (RFC 959)
- パッシブモード
- アクティブモード
- TLS/FTPS
- 仮想ディレクトリ
- ユーザー認証
- ACL (アクセス制御)

## 2. プロジェクト構造

```
Jdx.Servers.Ftp/
├── FtpServer.cs               # メインサーバークラス
├── FtpProtocol.cs             # FTPプロトコル処理
├── FtpSession.cs              # セッション管理
├── FtpCommands/               # FTPコマンド
│   ├── UserCommand.cs
│   ├── PassCommand.cs
│   ├── ListCommand.cs
│   ├── RetrCommand.cs
│   ├── StorCommand.cs
│   └── ...
├── DataConnection/            # データ接続
│   ├── PassiveMode.cs
│   └── ActiveMode.cs
├── VirtualFileSystem/         # 仮想ファイルシステム
└── Authentication/            # 認証
```

## 3. 設定

### 3.1 基本設定

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Command Port | 21 | コマンドポート |
| Passive Port Range | 50000-50100 | パッシブモード用ポート範囲 |
| Root Directory | ./ftp | ルートディレクトリ |

### 3.2 ユーザー設定

```json
{
  "Ftp": {
    "Users": [
      {
        "Username": "user1",
        "Password": "password1",
        "HomeDirectory": "./ftp/user1",
        "Permissions": ["read", "write", "delete"]
      }
    ],
    "AnonymousAccess": false
  }
}
```

## 4. FTPコマンド

### 4.1 認証コマンド

| コマンド | 説明 |
|---------|------|
| USER | ユーザー名 |
| PASS | パスワード |

### 4.2 転送コマンド

| コマンド | 説明 |
|---------|------|
| LIST | ディレクトリ一覧 |
| NLST | ファイル名一覧 |
| RETR | ファイル取得 |
| STOR | ファイル保存 |
| APPE | ファイル追記 |

### 4.3 ディレクトリコマンド

| コマンド | 説明 |
|---------|------|
| PWD | 現在のディレクトリ |
| CWD | ディレクトリ変更 |
| CDUP | 親ディレクトリへ |
| MKD | ディレクトリ作成 |
| RMD | ディレクトリ削除 |

### 4.4 その他

| コマンド | 説明 |
|---------|------|
| TYPE | 転送タイプ (A/I) |
| PASV | パッシブモード |
| PORT | アクティブモード |
| DELE | ファイル削除 |
| RNFR/RNTO | リネーム |
| SIZE | ファイルサイズ |
| QUIT | 切断 |

## 5. 転送モード

### 5.1 パッシブモード

クライアントがサーバーに接続。ファイアウォール環境で推奨。

```
C: PASV
S: 227 Entering Passive Mode (127,0,0,1,195,89)
```

### 5.2 アクティブモード

サーバーがクライアントに接続。

```
C: PORT 127,0,0,1,4,1
S: 200 PORT command successful
```

## 6. セキュリティ

### 6.1 FTPS

TLS/SSLによる暗号化通信。

```
C: AUTH TLS
S: 234 Using authentication type TLS
```

### 6.2 ACL設定

IPアドレスベースのアクセス制御が可能です。

```json
{
  "Ftp": {
    "EnableAcl": true,
    "AclRules": [
      {
        "Allow": ["192.168.1.0/24", "10.0.0.0/8"],
        "Deny": ["*"]
      }
    ]
  }
}
```

**ACLの動作:**
- 許可リストに一致するIPアドレスからの接続を許可
- 拒否リストに一致する場合は接続を拒否
- ルールは上から順に評価

## 7. テスト

```bash
# FTPクライアントでテスト
ftp localhost

# curlでテスト
curl ftp://localhost/file.txt -u user:pass
```

## 更新履歴

- 2026-01-24: ACL（アクセス制御リスト）機能の追加
