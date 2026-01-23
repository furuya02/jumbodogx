# TFTPサーバー

## 1. 概要

JumboDogX TFTPサーバーは、シンプルなファイル転送サービスを提供します。

### 1.1 主要機能

- TFTP (RFC 1350)
- ブロックサイズオプション (RFC 2348)
- タイムアウトオプション (RFC 2349)
- 転送サイズオプション (RFC 2349)
- ACL (アクセス制御)

### 1.2 用途

- ネットワークブート (PXE)
- ルーター/スイッチの設定バックアップ
- ファームウェア更新

## 2. プロジェクト構造

```
Jdx.Servers.Tftp/
├── TftpServer.cs              # メインサーバークラス
├── TftpProtocol.cs            # TFTPプロトコル処理
├── TftpSession.cs             # セッション管理
├── Packets/                   # パケット処理
│   ├── ReadRequestPacket.cs
│   ├── WriteRequestPacket.cs
│   ├── DataPacket.cs
│   ├── AckPacket.cs
│   └── ErrorPacket.cs
└── Options/                   # オプション処理
```

## 3. 設定

### 3.1 基本設定

| 項目 | デフォルト | 説明 |
|------|----------|------|
| Port | 69 | 待ち受けポート (UDP) |
| Root Directory | ./tftp | ルートディレクトリ |
| Block Size | 512 | デフォルトブロックサイズ |
| Timeout | 5 | タイムアウト（秒） |

### 3.2 アクセス制御

```json
{
  "Tftp": {
    "RootDirectory": "./tftp",
    "AllowRead": true,
    "AllowWrite": true,
    "MaxBlockSize": 65464,
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

**ACL設定:**
- `EnableAcl`: ACL機能の有効/無効
- `Allow`: 許可するIPアドレス/ネットワーク
- `Deny`: 拒否するIPアドレス/ネットワーク
- ルールは上から順に評価

## 4. TFTPパケット

### 4.1 オペコード

| 値 | タイプ | 説明 |
|----|--------|------|
| 1 | RRQ | 読み取り要求 |
| 2 | WRQ | 書き込み要求 |
| 3 | DATA | データ |
| 4 | ACK | 確認応答 |
| 5 | ERROR | エラー |
| 6 | OACK | オプション確認 |

### 4.2 エラーコード

| コード | 説明 |
|--------|------|
| 0 | 未定義 |
| 1 | ファイルが見つからない |
| 2 | アクセス違反 |
| 3 | ディスク満杯 |
| 4 | 不正な操作 |
| 5 | 不明な転送ID |
| 6 | ファイルが既に存在 |
| 7 | ユーザーが見つからない |

## 5. 転送フロー

### 5.1 読み取り (RRQ)

```
Client                Server
   |--- RRQ(file) --->|
   |<-- DATA(1) ------|
   |--- ACK(1) ------>|
   |<-- DATA(2) ------|
   |--- ACK(2) ------>|
   ...
   |<-- DATA(n)<512 --|  (最終ブロック)
   |--- ACK(n) ------>|
```

### 5.2 書き込み (WRQ)

```
Client                Server
   |--- WRQ(file) --->|
   |<-- ACK(0) -------|
   |--- DATA(1) ----->|
   |<-- ACK(1) -------|
   |--- DATA(2) ----->|
   |<-- ACK(2) -------|
   ...
```

## 6. オプション

### 6.1 blksize

ブロックサイズの変更（8-65464バイト）。

### 6.2 timeout

再送タイムアウトの設定（1-255秒）。

### 6.3 tsize

転送サイズの通知。

## 7. テスト

テストプロジェクト: `tests/Jdx.Servers.Tftp.Tests/`

```bash
dotnet test tests/Jdx.Servers.Tftp.Tests
```

### 7.1 動作確認

```bash
# tftpコマンドでテスト
tftp localhost
tftp> get testfile.txt
tftp> put upload.txt
```

## 更新履歴

- 2026-01-24: ACL（アクセス制御リスト）機能の追加
