# POP3サーバー

## 1. 概要

JumboDogX POP3サーバーは、メール受信サービスを提供します。

### 1.1 主要機能

- POP3 (RFC 1939)
- TLS/SSL
- 認証
- メールボックス管理
- ACL (アクセス制御)

## 2. プロジェクト構造

```
Jdx.Servers.Pop3/
├── Pop3Server.cs              # メインサーバークラス
├── Pop3Protocol.cs            # POP3プロトコル処理
├── Pop3Session.cs             # セッション管理
├── Pop3Commands/              # POP3コマンド
│   ├── UserCommand.cs
│   ├── PassCommand.cs
│   ├── StatCommand.cs
│   ├── ListCommand.cs
│   ├── RetrCommand.cs
│   ├── DeleCommand.cs
│   └── QuitCommand.cs
├── Authentication/            # 認証
└── Mailbox/                   # メールボックス
```

## 3. 設定

### 3.1 基本設定

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 110 | 標準POP3ポート |
| SSL Port | 995 | POP3S用ポート |

### 3.2 メールボックス設定

```json
{
  "Pop3": {
    "MailboxPath": "./mailboxes",
    "Users": [
      {
        "Username": "user1",
        "Password": "password1"
      }
    ]
  }
}
```

## 4. POP3コマンド

### 4.1 認証状態

| コマンド | 説明 |
|---------|------|
| USER | ユーザー名指定 |
| PASS | パスワード指定 |
| APOP | MD5認証（オプション） |
| QUIT | 接続終了 |

### 4.2 トランザクション状態

| コマンド | 説明 |
|---------|------|
| STAT | メールボックス状態 |
| LIST | メッセージ一覧 |
| RETR | メッセージ取得 |
| DELE | メッセージ削除マーク |
| NOOP | 無操作 |
| RSET | 削除マーク解除 |
| TOP | メッセージヘッダ取得 |
| UIDL | 一意ID取得 |
| QUIT | セッション終了（削除実行） |

## 5. セッション例

```
S: +OK POP3 server ready
C: USER user1
S: +OK
C: PASS password1
S: +OK Logged in
C: STAT
S: +OK 2 320
C: LIST
S: +OK 2 messages
S: 1 120
S: 2 200
S: .
C: RETR 1
S: +OK 120 octets
S: [メール内容]
S: .
C: DELE 1
S: +OK Marked for deletion
C: QUIT
S: +OK Bye
```

## 6. アクセス制御

### 6.1 ACL設定

IPアドレスベースのアクセス制御が可能です。

```json
{
  "Pop3": {
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

### 6.2 ACLの動作

- 許可リストに一致するIPアドレスからの接続を許可
- 拒否リストに一致する場合は接続を拒否
- ルールは上から順に評価

## 7. SMTP連携

SMTPサーバーで受信したメールを保存・提供。

## 8. テスト

テストプロジェクト: `tests/Jdx.Servers.Pop3.Tests/`

```bash
dotnet test tests/Jdx.Servers.Pop3.Tests
```

## 更新履歴

- 2026-01-24: ACL（アクセス制御リスト）機能の追加
