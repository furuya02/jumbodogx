# WebDAV設定ガイド

このガイドでは、JumboDogXのWebDAV（Web Distributed Authoring and Versioning）機能を設定し、Webベースでファイル管理を行う方法を説明します。

## 目次

- [WebDAVとは](#webdavとは)
- [WebDAV設定方法](#webdav設定方法)
- [クライアントからの接続](#クライアントからの接続)
- [認証設定](#認証設定)
- [トラブルシューティング](#トラブルシューティング)

## WebDAVとは

### 概要

WebDAV（Web Distributed Authoring and Versioning）は、HTTPプロトコルを拡張して、Webサーバー上のファイルやディレクトリを遠隔操作できるようにする仕組みです。

### 主な機能

1. **ファイル操作**
   - ファイルのアップロード
   - ファイルのダウンロード
   - ファイルの削除
   - ファイルの移動/コピー

2. **ディレクトリ操作**
   - ディレクトリの作成
   - ディレクトリの削除
   - ディレクトリ一覧の取得

3. **プロパティ管理**
   - ファイルのメタデータ取得
   - プロパティの設定/取得

4. **ロック機能**
   - ファイルのロック/アンロック
   - 排他制御

### 用途

- **ファイル共有**: チーム内でのファイル共有
- **リモートストレージ**: ネットワークドライブとして使用
- **バックアップ**: 遠隔地へのファイルバックアップ
- **Webサイト編集**: WebDAV対応エディタでWebサイトを直接編集
- **ドキュメント管理**: 共同作業でのドキュメント管理

### 対応プロトコル

JumboDogXがサポートするWebDAVメソッド：

| メソッド | 説明 |
|----------|------|
| PROPFIND | プロパティの取得、ディレクトリ一覧 |
| PROPPATCH | プロパティの設定 |
| MKCOL | ディレクトリの作成 |
| GET | ファイルのダウンロード |
| PUT | ファイルのアップロード |
| DELETE | ファイル/ディレクトリの削除 |
| COPY | ファイル/ディレクトリのコピー |
| MOVE | ファイル/ディレクトリの移動 |
| LOCK | ファイルのロック |
| UNLOCK | ファイルのアンロック |

## WebDAV設定方法

### Web UIからの設定

#### ステップ1: WebDAV設定画面を開く

1. ブラウザで `http://localhost:5001` にアクセス
2. **Settings** → **HTTP/HTTPS** → **WebDAV** を選択

#### ステップ2: WebDAV設定を入力

以下の項目を設定します：

| 項目 | 説明 | 例 |
|------|------|-----|
| Enable WebDAV | WebDAVを有効化 | ✓ ON |
| WebDAV Path | WebDAVのパス | `/webdav/` |
| Root Directory | WebDAVのルートディレクトリ | `/var/webdav/` |
| Enable Locking | ファイルロック機能 | ✓ ON |
| Require Authentication | 認証を必須にする | ✓ ON |

#### ステップ3: サーバーを再起動

設定を反映するため、JumboDogXを再起動します。

### appsettings.jsonでの設定

`src/Jdx.WebUI/appsettings.json` を編集します。

#### 基本的なWebDAV設定

```json
{
  "Jdx": {
    "HttpServer": {
      "WebDav": {
        "Enabled": true,
        "Path": "/webdav/",
        "RootDirectory": "/var/webdav",
        "EnableLocking": true,
        "RequireAuthentication": true
      }
    }
  }
}
```

#### 詳細なWebDAV設定

```json
{
  "Jdx": {
    "HttpServer": {
      "WebDav": {
        "Enabled": true,
        "Path": "/webdav/",
        "RootDirectory": "/var/webdav",
        "EnableLocking": true,
        "LockTimeout": 600,
        "MaxUploadSize": 104857600,
        "RequireAuthentication": true,
        "AllowedMethods": [
          "PROPFIND",
          "PROPPATCH",
          "MKCOL",
          "GET",
          "PUT",
          "DELETE",
          "COPY",
          "MOVE",
          "LOCK",
          "UNLOCK"
        ]
      }
    }
  }
}
```

### ディレクトリ構成

WebDAV用のディレクトリを作成します。

```bash
# WebDAVルートディレクトリを作成
mkdir -p /var/webdav

# 適切な権限を設定
chmod 755 /var/webdav
chown www-data:www-data /var/webdav  # または JumboDogXの実行ユーザー
```

推奨ディレクトリ構成：

```
/var/webdav/
├── shared/           # 共有ディレクトリ
│   ├── documents/
│   └── images/
├── users/            # ユーザーごとのディレクトリ
│   ├── alice/
│   └── bob/
└── projects/         # プロジェクトごとのディレクトリ
    ├── project-a/
    └── project-b/
```

## クライアントからの接続

### Windowsからの接続

#### 方法1: ネットワークドライブとしてマッピング

1. **エクスプローラーを開く**

2. **「PC」を右クリック** → **「ネットワークドライブの割り当て」**

3. **設定を入力**:
   - ドライブ文字: `Z:` （任意）
   - フォルダー: `http://localhost:8080/webdav/`
   - ログオン時に再接続する: ✓ ON
   - 別の資格情報を使用して接続する: ✓ ON（認証が必要な場合）

4. **完了をクリック**

5. **認証情報を入力**（必要な場合）:
   - ユーザー名: `your-username`
   - パスワード: `your-password`

#### 方法2: コマンドプロンプトから

```cmd
net use Z: http://localhost:8080/webdav/ /user:username password
```

#### HTTPSを使用する場合

HTTPS（ポート443以外）でWebDAVを使用する場合は、レジストリ設定が必要です。

1. **レジストリエディタを開く** (`regedit`)

2. **以下のキーを開く**:
   ```
   HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient\Parameters
   ```

3. **`BasicAuthLevel` を `2` に設定** （HTTPで認証を許可）

4. **サービスを再起動**:
   ```cmd
   net stop WebClient
   net start WebClient
   ```

#### トラブルシューティング（Windows）

**問題**: "The network folder specified is currently mapped using a different user name and password"

**解決策**:

```cmd
# 既存の接続を削除
net use Z: /delete

# 再度接続
net use Z: http://localhost:8080/webdav/ /user:username password
```

### macOSからの接続

#### 方法1: Finderから接続

1. **Finderを開く**

2. **「移動」メニュー** → **「サーバへ接続...」** （⌘K）

3. **サーバアドレスを入力**:
   ```
   http://localhost:8080/webdav/
   ```

4. **「接続」をクリック**

5. **認証情報を入力**（必要な場合）:
   - 名前: `your-username`
   - パスワード: `your-password`
   - キーチェーンに保存: ✓ ON（パスワードを保存する場合）

6. **「接続」をクリック**

#### 方法2: コマンドラインから

```bash
# WebDAVをマウント
mkdir ~/webdav
mount_webdav http://localhost:8080/webdav/ ~/webdav

# アンマウント
umount ~/webdav
```

#### HTTPSを使用する場合

```bash
# HTTPSでWebDAVをマウント
mount_webdav https://example.com/webdav/ ~/webdav
```

自己署名証明書の場合は、事前にキーチェーンに証明書を追加してください。

### Linuxからの接続

#### davfs2を使用

1. **davfs2をインストール**

```bash
# Ubuntu/Debian
sudo apt install davfs2

# CentOS/RHEL
sudo yum install davfs2

# Arch Linux
sudo pacman -S davfs2
```

2. **マウントポイントを作成**

```bash
mkdir ~/webdav
```

3. **WebDAVをマウント**

```bash
# 手動マウント
sudo mount -t davfs http://localhost:8080/webdav/ ~/webdav

# 認証情報を入力
Username: your-username
Password: your-password
```

4. **アンマウント**

```bash
sudo umount ~/webdav
```

#### 自動マウント設定

**/etc/fstab** に追加：

```
http://localhost:8080/webdav/ /home/username/webdav davfs user,noauto 0 0
```

**~/.davfs2/secrets** に認証情報を追加：

```
http://localhost:8080/webdav/ username password
```

権限を設定：

```bash
chmod 600 ~/.davfs2/secrets
```

マウント：

```bash
mount ~/webdav
```

### WebDAV対応アプリケーション

#### ファイルマネージャー

- **Cyberduck** (Windows/macOS): https://cyberduck.io/
- **WinSCP** (Windows): https://winscp.net/
- **FileZilla** (Windows/macOS/Linux): https://filezilla-project.org/

#### オフィスアプリケーション

- **Microsoft Office**: WebDAVで開く/保存をサポート
- **LibreOffice**: WebDAVサーバーへ直接保存可能

#### テキストエディタ

- **Visual Studio Code**: WebDAV拡張機能を使用
- **Sublime Text**: SFTP/WebDAV拡張機能を使用

### Cyberduckからの接続例

1. **Cyberduckを起動**

2. **「新しい接続」をクリック**

3. **設定を入力**:
   - プロトコル: **WebDAV (HTTP)**
   - サーバー: `localhost`
   - ポート: `8080`
   - ユーザー名: `your-username`
   - パスワード: `your-password`
   - パス: `/webdav/`

4. **「接続」をクリック**

## 認証設定

WebDAVは、不正アクセスを防ぐため認証を設定することを強く推奨します。

### Basic認証

#### appsettings.jsonでの設定

```json
{
  "Jdx": {
    "HttpServer": {
      "WebDav": {
        "Enabled": true,
        "RequireAuthentication": true
      },
      "Authentication": {
        "Type": "Basic",
        "Realm": "WebDAV Area",
        "Users": [
          {
            "Username": "alice",
            "Password": "password123",
            "Paths": ["/webdav/"]
          },
          {
            "Username": "bob",
            "Password": "password456",
            "Paths": ["/webdav/"]
          }
        ]
      }
    }
  }
}
```

### Digest認証（より安全）

```json
{
  "Jdx": {
    "HttpServer": {
      "WebDav": {
        "Enabled": true,
        "RequireAuthentication": true
      },
      "Authentication": {
        "Type": "Digest",
        "Realm": "WebDAV Area",
        "Users": [
          {
            "Username": "alice",
            "Password": "password123",
            "Paths": ["/webdav/"]
          }
        ]
      }
    }
  }
}
```

### パスごとのアクセス制御

特定のパスへのアクセスをユーザーごとに制限できます。

```json
{
  "Authentication": {
    "Users": [
      {
        "Username": "alice",
        "Password": "password123",
        "Paths": ["/webdav/shared/", "/webdav/users/alice/"]
      },
      {
        "Username": "bob",
        "Password": "password456",
        "Paths": ["/webdav/shared/", "/webdav/users/bob/"]
      },
      {
        "Username": "admin",
        "Password": "adminpass",
        "Paths": ["/webdav/"]
      }
    ]
  }
}
```

### IPアドレスベースのアクセス制御（ACL）

認証に加えて、IPアドレスでアクセスを制限できます。

```json
{
  "Jdx": {
    "HttpServer": {
      "Acl": {
        "Rules": [
          {
            "Path": "/webdav/*",
            "Allow": ["192.168.1.0/24", "10.0.0.0/8"],
            "Deny": ["*"]
          }
        ]
      }
    }
  }
}
```

### HTTPS + 認証（推奨）

本番環境では、必ずHTTPSと認証を組み合わせてください。

```json
{
  "Jdx": {
    "HttpServer": {
      "Protocol": "HTTPS",
      "Port": 443,
      "CertificateFile": "/path/to/certificate.pfx",
      "CertificatePassword": "${CERT_PASSWORD}",
      "WebDav": {
        "Enabled": true,
        "Path": "/webdav/",
        "RootDirectory": "/var/webdav",
        "RequireAuthentication": true
      },
      "Authentication": {
        "Type": "Digest",
        "Users": [
          {
            "Username": "alice",
            "Password": "secure-password"
          }
        ]
      }
    }
  }
}
```

## トラブルシューティング

### 接続できない

#### 問題1: "Unable to connect to server"

**原因1**: サーバーが起動していない

**解決策**:

```bash
# サーバーの起動確認
ps aux | grep JumboDogX

# ポートの確認
lsof -i :8080  # macOS/Linux
netstat -ano | findstr :8080  # Windows
```

**原因2**: ファイアウォールで遮断されている

**解決策**:

```bash
# ファイアウォールでポートを開放
# macOS
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --add /path/to/jumbodogx

# Linux (ufw)
sudo ufw allow 8080/tcp

# Linux (firewalld)
sudo firewall-cmd --permanent --add-port=8080/tcp
sudo firewall-cmd --reload
```

**原因3**: WebDAVが有効化されていない

**解決策**:

```json
{
  "WebDav": {
    "Enabled": true
  }
}
```

#### 問題2: "401 Unauthorized"

**原因**: 認証情報が間違っている、または認証が設定されていない

**解決策**:

1. **ユーザー名とパスワードを確認**

```json
{
  "Authentication": {
    "Users": [
      {
        "Username": "alice",
        "Password": "correct-password"
      }
    ]
  }
}
```

2. **認証タイプを確認**

BasicまたはDigestが正しく設定されているか確認。

3. **パスのアクセス権限を確認**

```json
{
  "Users": [
    {
      "Username": "alice",
      "Paths": ["/webdav/"]
    }
  ]
}
```

#### 問題3: "403 Forbidden"

**原因**: ディレクトリのアクセス権限が不足している

**解決策**:

```bash
# ディレクトリの権限を確認
ls -la /var/webdav/

# 適切な権限を設定
chmod 755 /var/webdav/
chown -R www-data:www-data /var/webdav/
```

### ファイル操作ができない

#### 問題1: ファイルをアップロードできない

**原因1**: ディレクトリに書き込み権限がない

**解決策**:

```bash
# 書き込み権限を付与
chmod 775 /var/webdav/
```

**原因2**: ファイルサイズ制限を超えている

**解決策**:

```json
{
  "WebDav": {
    "MaxUploadSize": 104857600  // 100MB
  }
}
```

#### 問題2: ファイルを削除できない

**原因**: ファイルがロックされている

**解決策**:

1. **ロックを解除**

WebDAVクライアントでファイルのロックを解除。

2. **ロックタイムアウトを待つ**

```json
{
  "WebDav": {
    "LockTimeout": 600  // 10分
  }
}
```

3. **サーバーを再起動**（最終手段）

### Windows特有の問題

#### 問題1: ネットワークドライブとして接続できない

**原因**: WebClientサービスが起動していない

**解決策**:

```cmd
# WebClientサービスを起動
net start WebClient

# 自動起動に設定
sc config WebClient start= auto
```

#### 問題2: "The folder you entered does not appear to be valid"

**原因**: HTTPSではないポート（80, 443以外）で接続しようとしている

**解決策**:

レジストリを編集（管理者権限が必要）：

```reg
Windows Registry Editor Version 5.00

[HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\WebClient\Parameters]
"BasicAuthLevel"=dword:00000002
```

サービスを再起動：

```cmd
net stop WebClient
net start WebClient
```

### macOS特有の問題

#### 問題1: "Mount failed"

**原因**: 証明書エラー（HTTPSの場合）

**解決策**:

自己署名証明書をキーチェーンに追加：

```bash
# 証明書をエクスポート
openssl s_client -connect localhost:443 -showcerts < /dev/null 2>/dev/null | \
  openssl x509 -outform PEM > server.crt

# キーチェーンに追加
sudo security add-trusted-cert -d -r trustRoot \
  -k /Library/Keychains/System.keychain server.crt
```

### Linux特有の問題

#### 問題1: "mount error(2): No such file or directory"

**原因**: davfs2がインストールされていない

**解決策**:

```bash
sudo apt install davfs2
```

#### 問題2: "mount error: Only root can mount WebDAV"

**原因**: 一般ユーザーにマウント権限がない

**解決策**:

ユーザーを `davfs2` グループに追加：

```bash
sudo usermod -aG davfs2 $USER
```

ログアウトして再ログイン。

### パフォーマンス問題

#### 問題: ファイル一覧の取得が遅い

**原因**: 大量のファイルがある

**解決策**:

1. **ディレクトリを整理**

ファイル数を減らすか、サブディレクトリに分割。

2. **キャッシュを有効化**（今後の実装予定）

### ログの確認

問題が発生した場合は、ログを確認してください：

```bash
# WebDAVログ
cat logs/webdav.log

# HTTPサーバーログ
cat logs/http-server.log | grep -i webdav
cat logs/http-server.log | grep -i propfind
cat logs/http-server.log | grep -i mkcol

# エラーログ
cat logs/error.log
```

## ベストプラクティス

### セキュリティ

1. **必ず認証を設定**

```json
{
  "WebDav": {
    "RequireAuthentication": true
  }
}
```

2. **HTTPSを使用**

```json
{
  "Protocol": "HTTPS",
  "CertificateFile": "/path/to/certificate.pfx"
}
```

3. **強力なパスワードを使用**

```json
{
  "Users": [
    {
      "Username": "alice",
      "Password": "Str0ng!P@ssw0rd#2026"
    }
  ]
}
```

4. **IPアドレス制限を追加**

```json
{
  "Acl": {
    "Rules": [
      {
        "Path": "/webdav/*",
        "Allow": ["192.168.1.0/24"],
        "Deny": ["*"]
      }
    ]
  }
}
```

### パフォーマンス

1. **ファイルサイズ制限を適切に設定**

```json
{
  "WebDav": {
    "MaxUploadSize": 52428800  // 50MB
  }
}
```

2. **ロックタイムアウトを適切に設定**

```json
{
  "WebDav": {
    "LockTimeout": 300  // 5分
  }
}
```

### 管理

1. **ディレクトリ構造を整理**

```
/var/webdav/
├── shared/        # 共有ファイル
├── users/         # ユーザーごとのディレクトリ
└── archive/       # アーカイブ
```

2. **定期的なバックアップ**

```bash
# バックアップスクリプト
#!/bin/bash
BACKUP_DIR="/backup/webdav"
DATE=$(date +%Y%m%d)

tar czf "${BACKUP_DIR}/webdav-${DATE}.tar.gz" /var/webdav/
```

3. **ログの監視**

```bash
# アクセスログの監視
tail -f logs/webdav.log
```

## 設定例

### シンプルなWebDAV設定

```json
{
  "Jdx": {
    "HttpServer": {
      "Enabled": true,
      "Protocol": "HTTP",
      "Port": 8080,
      "DocumentRoot": "/var/www/html",
      "WebDav": {
        "Enabled": true,
        "Path": "/webdav/",
        "RootDirectory": "/var/webdav",
        "EnableLocking": true,
        "RequireAuthentication": true
      },
      "Authentication": {
        "Type": "Basic",
        "Users": [
          {
            "Username": "user",
            "Password": "password"
          }
        ]
      }
    }
  }
}
```

### セキュアなWebDAV設定（HTTPS + Digest認証）

```json
{
  "Jdx": {
    "HttpServer": {
      "Enabled": true,
      "Protocol": "HTTPS",
      "Port": 443,
      "CertificateFile": "/etc/ssl/certs/example.com.pfx",
      "CertificatePassword": "${CERT_PASSWORD}",
      "WebDav": {
        "Enabled": true,
        "Path": "/webdav/",
        "RootDirectory": "/var/webdav",
        "EnableLocking": true,
        "LockTimeout": 600,
        "MaxUploadSize": 104857600,
        "RequireAuthentication": true
      },
      "Authentication": {
        "Type": "Digest",
        "Realm": "Secure WebDAV",
        "Users": [
          {
            "Username": "alice",
            "Password": "secure-password-123",
            "Paths": ["/webdav/"]
          }
        ]
      },
      "Acl": {
        "Rules": [
          {
            "Path": "/webdav/*",
            "Allow": ["192.168.1.0/24"],
            "Deny": ["*"]
          }
        ]
      }
    }
  }
}
```

## 関連ドキュメント

- [クイックスタート](getting-started.md) - 基本的な設定手順
- [詳細設定](configuration.md) - HTTPサーバーの詳細設定
- [SSL/TLS設定](ssl-tls.md) - HTTPS化の設定
- [ACL設定](../common/acl-configuration.md) - アクセス制御の詳細
- [トラブルシューティング](troubleshooting.md) - 問題解決

## まとめ

WebDAVを使用することで：

- Webブラウザを使わずにファイル管理が可能
- ネットワークドライブとして使用可能
- 複数のクライアントから安全にアクセス可能
- ファイルの共同編集やバージョン管理が容易

適切に設定とセキュリティ対策を行うことで、効率的なファイル共有システムを構築できます。
