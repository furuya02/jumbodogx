# SMTPサーバー

## 1. 概要

JumboDogX SMTPサーバーは、メール送信の受け付けを行います。

### 1.1 主要機能

- SMTP (RFC 5321)
- ESMTP拡張
- TLS/STARTTLS
- 認証 (AUTH)
- ローカルメールボックス保存
- メール転送

## 2. プロジェクト構造

```
Jdx.Servers.Smtp/
├── SmtpServer.cs              # メインサーバークラス
├── SmtpProtocol.cs            # SMTPプロトコル処理
├── SmtpSession.cs             # セッション管理
├── SmtpCommands/              # SMTPコマンド
│   ├── HeloCommand.cs
│   ├── EhloCommand.cs
│   ├── MailCommand.cs
│   ├── RcptCommand.cs
│   ├── DataCommand.cs
│   └── QuitCommand.cs
├── Authentication/            # 認証
└── Storage/                   # メール保存
```

## 3. 設定

### 3.1 基本設定

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 25 | 標準SMTPポート |
| Submission Port | 587 | メール投稿用ポート |
| Max Message Size | 10MB | 最大メッセージサイズ |

### 3.2 認証設定

```json
{
  "Smtp": {
    "RequireAuth": true,
    "AllowedMethods": ["PLAIN", "LOGIN"]
  }
}
```

## 4. SMTPコマンド

| コマンド | 説明 |
|---------|------|
| HELO | クライアント識別（旧式） |
| EHLO | クライアント識別（ESMTP） |
| MAIL FROM | 送信元指定 |
| RCPT TO | 宛先指定 |
| DATA | メール本文開始 |
| QUIT | セッション終了 |
| RSET | セッションリセット |
| NOOP | 無操作（接続維持） |
| STARTTLS | TLS開始 |
| AUTH | 認証 |

## 5. セッション例

```
S: 220 mail.example.local ESMTP JumboDogX
C: EHLO client.local
S: 250-mail.example.local Hello
S: 250-STARTTLS
S: 250 AUTH PLAIN LOGIN
C: MAIL FROM:<sender@example.com>
S: 250 OK
C: RCPT TO:<recipient@example.local>
S: 250 OK
C: DATA
S: 354 End data with <CR><LF>.<CR><LF>
C: Subject: Test
C:
C: Test message
C: .
S: 250 OK: Message queued
C: QUIT
S: 221 Bye
```

## 6. POP3連携

受信したメールはPOP3サーバーで取得可能。

## 7. テスト

テストプロジェクト: `tests/Jdx.Servers.Smtp.Tests/`

```bash
dotnet test tests/Jdx.Servers.Smtp.Tests
```

### 7.1 動作確認

```bash
# telnetでテスト
telnet localhost 25

# swaksでテスト (要インストール)
swaks --to user@example.local --server localhost
```
