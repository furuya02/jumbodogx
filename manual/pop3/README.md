# POP3サーバー

JumboDogXのPOP3サーバーは、メール受信機能を提供します。
RFC 1939に準拠し、ローカル開発環境やテストネットワークでのメール受信テストに最適です。

## 主な機能

### 基本機能
- **RFC 1939準拠** - 標準的なPOP3プロトコルに対応
- **メールボックス管理** - ユーザーごとのメールボックス
- **メール取得** - メールクライアントからのメール取得

### セキュリティ機能
- **ユーザー認証** - ユーザー名とパスワードによる認証
- **パスワードハッシュ** - SHA256/SHA512によるパスワード保護
- **タイムアウト設定** - 不正な接続への対策

### 高度な機能
- **複数アカウント** - 複数のユーザーアカウントを管理
- **メールファイル管理** - ファイルシステムベースのメール保存
- **柔軟なバインド設定** - 特定のIPアドレスやポートで待ち受け

## クイックスタート

POP3サーバーをすぐに使い始めたい方は、[クイックスタートガイド](getting-started.md)をご覧ください。

## ドキュメント一覧

### 基本設定
- [クイックスタート](getting-started.md) - 最初の設定と起動方法
- [認証設定](authentication.md) - ユーザー認証の詳細設定

### トラブルシューティング
- [トラブルシューティング](troubleshooting.md) - よくある問題と解決方法

## 設定例

### シンプルなPOP3サーバー

```json
{
  "Pop3Server": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 110,
    "TimeOut": 30,
    "MailDir": "C:\\JumboDogX\\Mailboxes",
    "Users": [
      {
        "UserName": "user1@example.com",
        "Password": "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
        "HashType": "SHA256"
      }
    ]
  }
}
```

### 複数ユーザー設定

```json
{
  "Pop3Server": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 110,
    "TimeOut": 30,
    "MailDir": "C:\\JumboDogX\\Mailboxes",
    "Users": [
      {
        "UserName": "user1@example.com",
        "Password": "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8",
        "HashType": "SHA256"
      },
      {
        "UserName": "user2@example.com",
        "Password": "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        "HashType": "SHA512"
      }
    ]
  }
}
```

**ハッシュタイプ**:
- `SHA256`: SHA-256ハッシュ（推奨）
- `SHA512`: SHA-512ハッシュ（より安全）

## Web UI

JumboDogXは、Webブラウザから設定できる管理画面を提供しています。

### アクセス方法
1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **POP3** を選択

### 設定画面
- **General** - 基本設定（ポート、タイムアウト、メールディレクトリ）
- **Users** - ユーザーアカウント管理

## ユースケース

### ローカル開発環境
開発中のアプリケーションからメール受信機能をテスト。

### メール受信テスト
本番環境にデプロイする前にメール受信フローを検証。

### SMTPとの連携
JumboDogXのSMTPサーバーと連携して、完全なメールシステムを構築。

### 学習・教育
POP3プロトコルの仕組みを理解するための実験環境。

## クライアントからの利用

### telnetコマンド

**Windows/macOS/Linux**:
```bash
telnet localhost 110
```

POP3コマンド例:
```
USER user1@example.com
PASS password
STAT
LIST
RETR 1
DELE 1
QUIT
```

### メールクライアント設定

**Thunderbird**:
1. アカウント設定 → サーバー設定
2. サーバー名: `localhost`
3. ポート: `110`
4. セキュリティ: なし
5. 認証方式: 通常のパスワード認証

**Outlook**:
1. アカウント追加 → 手動設定
2. 受信サーバー: `localhost`
3. ポート: `110`
4. 暗号化: なし

### Pythonでのメール受信

```python
import poplib

# POP3サーバーに接続
pop = poplib.POP3('localhost', 110)
pop.user('user1@example.com')
pop.pass_('password')

# メール一覧を取得
num_messages = len(pop.list()[1])
print(f'Total messages: {num_messages}')

# メールを取得
for i in range(num_messages):
    msg = pop.retr(i+1)
    print(b'\n'.join(msg[1]).decode('utf-8'))

pop.quit()
```

## メールボックス構造

メールは以下のディレクトリ構造で保存されます：

```
MailDir/
└── user1@example.com/
    ├── 1.eml
    ├── 2.eml
    └── 3.eml
```

各ユーザーのメールボックスは、`MailDir`で指定したディレクトリ配下にユーザー名のサブディレクトリとして作成されます。

## 関連リンク

- [インストール手順](../common/installation.md)
- [ロギング設定](../common/logging.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)

## 技術仕様

- **プロトコル**: POP3 (RFC 1939)
- **認証**: USER/PASS コマンド
- **標準ポート**: 110
- **タイムアウト**: 設定可能（デフォルト30秒）
- **パスワードハッシュ**: SHA256, SHA512
- **メール形式**: EML形式（RFC 822）

## サポート

問題が発生した場合は、以下を参照してください：
- [トラブルシューティング](troubleshooting.md)
- [GitHubリポジトリ](https://github.com/furuya02/jumbodogx)
- [Issue報告](https://github.com/furuya02/jumbodogx/issues)
