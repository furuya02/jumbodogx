# TFTPサーバー - アクセス制御

JumboDogXのTFTPサーバーにおけるアクセス制御（ACL）と読み取り専用モードについて説明します。

## アクセス制御の重要性

TFTPは**認証機能を持たない**シンプルなプロトコルです。そのため、適切なアクセス制御を設定しないと、誰でもファイルをダウンロード・アップロードできてしまいます。

**セキュリティリスク**:
- 機密ファイルの漏洩
- 不正なファイルのアップロード
- サーバーリソースの不正使用

---

## ACL（Access Control List）

### ACLの動作モード

JumboDogXのTFTPサーバーは、IPアドレスベースのACLをサポートしています。

#### Allow Mode（許可リストモード）

リストに登録されているIPアドレス**のみ**がTFTPサーバーにアクセスできます。

**用途**:
- セキュリティが重要な環境
- 特定のクライアントのみにアクセスを許可
- ファームウェア更新など限定的な用途

**設定例**:
```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot",
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "192.168.1.100",
        "192.168.1.101",
        "192.168.1.0/24"
      ]
    }
  }
}
```

この設定では、`192.168.1.100`、`192.168.1.101`、および`192.168.1.0/24`ネットワーク内のIPアドレスのみがアクセスできます。

---

#### Deny Mode（拒否リストモード）

リストに登録されているIPアドレス**以外**がTFTPサーバーにアクセスできます。

**用途**:
- ほとんどのクライアントを許可し、一部のみを拒否
- 問題のあるクライアントのブロック

**設定例**:
```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot",
    "Acl": {
      "Mode": "Deny",
      "Entries": [
        "192.168.1.50",
        "10.0.0.100"
      ]
    }
  }
}
```

この設定では、`192.168.1.50`と`10.0.0.100`以外のすべてのIPアドレスがアクセスできます。

---

### IPアドレスの形式

ACLエントリには、以下の形式でIPアドレスを指定できます：

**1. 単一IPアドレス**:
```json
{
  "Entries": [
    "192.168.1.100"
  ]
}
```

**2. CIDR表記**:
```json
{
  "Entries": [
    "192.168.1.0/24",    # 192.168.1.0-255
    "10.0.0.0/16",       # 10.0.0.0-255.255
    "172.16.0.0/12"      # 172.16.0.0-172.31.255.255
  ]
}
```

**3. IPアドレス範囲**:
```json
{
  "Entries": [
    "192.168.1.100-192.168.1.200"
  ]
}
```

**4. ワイルドカード**:
```json
{
  "Entries": [
    "192.168.1.*",       # 192.168.1.0-255
    "10.0.*.*"           # 10.0.0.0-10.0.255.255
  ]
}
```

---

## 読み取り専用モード

### ReadOnlyオプション

読み取り専用モードを有効にすると、クライアントはファイルのダウンロード（GET）のみができ、アップロード（PUT）はできなくなります。

**設定例**:
```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot",
    "ReadOnly": true
  }
}
```

**用途**:
- ファームウェアの配布専用サーバー
- PXEブートサーバー
- 設定ファイルの配布

**メリット**:
- 不正なファイルアップロードを防止
- サーバーの改ざんリスクを軽減
- ストレージの不正使用を防止

---

## ACLと読み取り専用モードの組み合わせ

最も安全な設定は、**Allow Mode + ReadOnly Mode**の組み合わせです。

```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot",
    "ReadOnly": true,
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "192.168.1.0/24"
      ]
    }
  }
}
```

この設定では：
- `192.168.1.0/24`ネットワーク内のIPアドレスのみがアクセス可能
- ファイルのダウンロードのみ許可（アップロード不可）

---

## Web UIでのアクセス制御設定

### ACLモードの選択

1. JumboDogXを起動
2. ブラウザで `http://localhost:5001` にアクセス
3. サイドメニューから **Settings** → **TFTP** → **ACL** を選択
4. ACL Modeを選択:
   - **Allow Mode**: 許可リストモード
   - **Deny Mode**: 拒否リストモード
5. **Save Settings** ボタンをクリック

![ACLモード選択画面](images/tftp-acl-mode.png)

### IPアドレスの追加

1. ACL設定画面で **Add Entry** ボタンをクリック
2. IPアドレスを入力（例: `192.168.1.100`または`192.168.1.0/24`）
3. **Add** ボタンをクリック
4. **Save Settings** ボタンをクリック

![IPアドレス追加画面](images/tftp-acl-add.png)

### 読み取り専用モードの設定

1. Settings → TFTP → Generalを選択
2. **Read Only Mode** チェックボックスにチェックを入れる
3. **Save Settings** ボタンをクリック

![読み取り専用モード設定画面](images/tftp-readonly-mode.png)

---

## 設定例集

### ファームウェア配布サーバー（最も安全）

```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot\\Firmware",
    "ReadOnly": true,
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "192.168.1.0/24"
      ]
    }
  }
}
```

---

### PXEブートサーバー

```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "192.168.1.1",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot\\PXE",
    "ReadOnly": true,
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "192.168.1.0/24"
      ]
    }
  }
}
```

---

### 開発環境（読み書き可能）

```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot\\Dev",
    "ReadOnly": false,
    "Acl": {
      "Mode": "Allow",
      "Entries": [
        "127.0.0.1",
        "192.168.1.0/24"
      ]
    }
  }
}
```

---

### 特定のIPのみブロック

```json
{
  "TftpServer": {
    "Enabled": true,
    "BindAddress": "0.0.0.0",
    "Port": 69,
    "RootDirectory": "C:\\TftpRoot",
    "Acl": {
      "Mode": "Deny",
      "Entries": [
        "192.168.1.50",
        "10.0.0.100"
      ]
    }
  }
}
```

---

## ルートディレクトリのセキュリティ

### ディレクトリ構造

TFTPサーバーは、`RootDirectory`で指定したディレクトリ配下のファイルのみにアクセスできます。

```
RootDirectory/
├── firmware/
│   ├── router.bin
│   └── switch.bin
├── pxe/
│   ├── pxelinux.0
│   └── vmlinuz
└── config/
    └── network.cfg
```

**重要**: `RootDirectory`の外側のファイルにはアクセスできません（パストラバーサル攻撃への対策）。

---

### ファイルのアクセス権限

ルートディレクトリとファイルのアクセス権限を適切に設定してください。

**Windows**:
```cmd
# ディレクトリの権限を確認
icacls C:\TftpRoot

# 読み取り専用に設定
icacls C:\TftpRoot /grant:r Users:R /inheritance:r
```

**macOS/Linux**:
```bash
# 読み取り専用に設定
chmod 755 /path/to/TftpRoot
chmod 644 /path/to/TftpRoot/*
```

---

## セキュリティのベストプラクティス

### 1. 常にACLを設定

ACLを設定せずにTFTPサーバーを実行すると、誰でもアクセスできてしまいます。

```json
{
  "TftpServer": {
    "Acl": {
      "Mode": "Allow",
      "Entries": ["192.168.1.0/24"]
    }
  }
}
```

---

### 2. 読み取り専用モードを使用（可能な場合）

ファイルのアップロードが不要な場合は、読み取り専用モードを有効にしてください。

```json
{
  "TftpServer": {
    "ReadOnly": true
  }
}
```

---

### 3. ローカルネットワークのみで使用

外部からのアクセスを防ぐため、ローカルネットワークのみで使用してください。

```json
{
  "TftpServer": {
    "BindAddress": "192.168.1.1",  # ローカルネットワークのみ
    "Acl": {
      "Mode": "Allow",
      "Entries": ["192.168.1.0/24"]
    }
  }
}
```

---

### 4. 機密ファイルを配置しない

TFTPは暗号化されないため、機密ファイルを配置しないでください。

---

### 5. ログを監視

アクセスログを定期的に確認し、不正なアクセスを検出してください。

```bash
# ログの確認
grep TFTP logs/jumbodogx.log

# ACL拒否のログを確認
grep "ACL denied" logs/jumbodogx.log
```

---

## トラブルシューティング

### クライアントがファイルをダウンロードできない

**原因1: ACLで拒否されている**

**解決方法**: クライアントのIPアドレスをACLに追加してください

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

---

**原因2: ファイルが存在しない**

**解決方法**: ルートディレクトリにファイルが存在するか確認してください

```bash
# Windows
dir C:\TftpRoot

# macOS/Linux
ls -la /path/to/TftpRoot
```

---

### クライアントがファイルをアップロードできない

**原因: 読み取り専用モードが有効**

**解決方法**: 読み取り専用モードを無効にしてください

```json
{
  "TftpServer": {
    "ReadOnly": false  # ← アップロードを許可
  }
}
```

**注意**: セキュリティリスクを考慮してください。

---

## 関連リンク

- [クイックスタート](getting-started.md)
- [トラブルシューティング](troubleshooting.md)
- [ACL設定](../common/acl-configuration.md)
- [セキュリティベストプラクティス](../common/security-best-practices.md)
