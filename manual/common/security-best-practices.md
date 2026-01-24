# セキュリティベストプラクティス

このガイドでは、JumboDogXを安全に運用するためのセキュリティベストプラクティスを説明します。

## 重要な注意事項

**⚠️ JumboDogXは、ローカルテスト環境専用です**

JumboDogXは、以下の用途を想定して設計されています：

- **ローカル開発環境**
- **テスト環境**
- **学習・教育目的**

以下の用途には**推奨されません**：

- ❌ 本番環境での使用
- ❌ インターネットへの公開
- ❌ 重要なデータの取り扱い
- ❌ セキュリティ監査が必要な環境

## セットアップ時の必須設定

### 1. デフォルトパスワードの変更（最優先）

**危険度**: 🔴 高

JumboDogXをインストールしたら、**必ず最初にデフォルトパスワードを変更**してください。

#### パスワードの変更手順

1. JumboDogXを停止
2. `appsettings.json` を開く
3. `Users` セクションのパスワードハッシュを変更

**現在の設定**（デフォルト）:
```json
{
  "Jdx": {
    "Users": [
      {
        "Username": "admin",
        "PasswordHash": "REPLACE_WITH_SECURE_SHA256_HASH"  // ⚠️ 変更必須
      }
    ]
  }
}
```

#### 強力なパスワードハッシュの生成

**macOS / Linux**:
```bash
echo -n 'YourStrongPassword' | shasum -a 256
```

**Windows (PowerShell)**:
```powershell
$password = 'YourStrongPassword'
$hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash([System.Text.Encoding]::UTF8.GetBytes($password))
[BitConverter]::ToString($hash).Replace('-', '').ToLower()
```

**変更後**:
```json
{
  "Jdx": {
    "Users": [
      {
        "Username": "admin",
        "PasswordHash": "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8"  // ✓ 変更済み
      }
    ]
  }
}
```

4. JumboDogXを再起動

![パスワード変更](images/security-password-change.png)
*パスワードハッシュの変更*

#### 強力なパスワードの条件

- **最低12文字以上**
- 大文字、小文字、数字、記号を含む
- 辞書にある単語を避ける
- 推測されやすい情報（誕生日、名前など）を避ける

**良い例**:
- `MyS3cur3P@ssw0rd!2026`
- `Tr0ub4dor&3!xK7pQ`

**悪い例**:
- `password` - 短すぎる、一般的すぎる
- `admin123` - 推測されやすい
- `19900101` - 数字のみ

### 2. ネットワークバインディングの設定

**危険度**: 🟠 中

デフォルトでは、JumboDogXはlocalhostのみでリッスンします。

#### localhost専用（推奨）

**設定**:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://127.0.0.1:5001"  // ✓ ローカルのみ
      }
    }
  }
}
```

**用途**: 同じマシン内からのみアクセス

#### ネットワークアクセスを許可（慎重に）

**設定**:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5001"  // ⚠️ すべてのインターフェースでリッスン
      }
    }
  }
}
```

**注意事項**:
- ファイアウォールで保護されたネットワーク内でのみ使用
- インターネットに直接公開しない
- ACLで適切にアクセス制御

![ネットワークバインディング](images/security-network-binding.png)
*ネットワークバインディングの設定*

### 3. CGI実行の無効化

**危険度**: 🔴 高（有効化時）

CGI機能は、**コマンドインジェクションのリスク**があります。

#### デフォルト設定（推奨）

```json
{
  "Jdx": {
    "HttpServer": {
      "VirtualHosts": [
        {
          "Settings": {
            "UseCgi": false  // ✓ CGI無効（推奨）
          }
        }
      ]
    }
  }
}
```

#### CGIを有効化する場合（慎重に）

CGIを有効化する場合は、以下の対策を実施してください：

1. **信頼できるCGIスクリプトのみ配置**
2. **入力値の厳格な検証**
3. **実行権限の最小化**
4. **ログの監視**

```json
{
  "Jdx": {
    "HttpServer": {
      "VirtualHosts": [
        {
          "Settings": {
            "UseCgi": true,  // ⚠️ CGI有効化（リスクあり）
            "CgiPath": "/usr/bin/"  // CGIインタープリタのパス
          }
        }
      ]
    }
  }
}
```

### 4. WebDAV書き込みアクセスの制限

**危険度**: 🟠 中（有効化時）

WebDAV書き込みアクセスは、**不正なファイルアップロードのリスク**があります。

#### デフォルト設定（推奨）

```json
{
  "Jdx": {
    "HttpServer": {
      "VirtualHosts": [
        {
          "Settings": {
            "WebDav": {
              "Enabled": false,      // または読み取り専用
              "AllowWrite": false    // ✓ 書き込み無効（推奨）
            }
          }
        }
      ]
    }
  }
}
```

#### 書き込みを許可する場合（認証必須）

```json
{
  "Jdx": {
    "HttpServer": {
      "VirtualHosts": [
        {
          "Settings": {
            "WebDav": {
              "Enabled": true,
              "AllowWrite": true,    // ⚠️ 書き込み許可
              "Authentication": {
                "Enabled": true,     // ✓ 認証を有効化（必須）
                "Users": [
                  {
                    "Username": "webdav-user",
                    "PasswordHash": "..."
                  }
                ]
              }
            }
          }
        }
      ]
    }
  }
}
```

**必須対策**:
- 認証を必ず有効化
- 強力なパスワードを使用
- ACLで許可するIPアドレスを制限
- アップロードファイルのサイズ制限

## アクセス制御（ACL）

### ACLの基本設定

**推奨**: すべてのサーバーでACLを設定してください。

#### 開発環境（localhost専用）

```
ACL Mode: Allow Mode
エントリ:
- Name: Localhost
  IP: 127.0.0.1
```

#### 社内ネットワーク

```
ACL Mode: Allow Mode
エントリ:
- Name: Office Network
  IP: 192.168.1.0/24
```

#### 攻撃元のブロック

```
ACL Mode: Deny Mode
エントリ:
- Name: Blocked Attacker
  IP: 203.0.113.50
```

詳細は、[ACL設定ガイド](acl-configuration.md)を参照してください。

### AttackDb自動ブロック機能の活用

JumboDogXは、攻撃を自動検出してブロックする機能があります。

**対応サーバー**:
- HTTP/HTTPS サーバー（Apache Killer攻撃など）

**動作**:
1. 不審なアクセスパターンを検出
2. AttackDbに記録
3. 一定の条件を満たすと自動的にACLに追加

**確認方法**:
- Settings > HTTP/HTTPS > Virtual Host > ACL で自動追加エントリを確認
- Logsで "Attack detected" メッセージを確認

![AttackDb自動ブロック](images/security-attackdb.png)
*AttackDbによる自動ブロック機能*

## HTTPS/SSL/TLS設定

### 証明書の使用

本番環境やネットワーク公開する場合は、**必ずHTTPSを使用**してください。

#### 自己署名証明書の作成（テスト用）

```bash
# OpenSSLで自己署名証明書を作成
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes

# PFX形式に変換
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem
```

#### HTTPS設定

```json
{
  "Jdx": {
    "HttpServer": {
      "VirtualHosts": [
        {
          "Settings": {
            "Protocol": "HTTPS",
            "CertificateFile": "/path/to/certificate.pfx",
            "CertificatePassword": "your-password"
          }
        }
      ]
    }
  }
}
```

**注意**: 自己署名証明書は、ブラウザで警告が表示されます。テスト環境でのみ使用してください。

詳細は、[証明書セットアップガイド](../../docs/01_development_docs/05_certificate_setup.md)を参照してください。

![HTTPS設定](images/security-https.png)
*HTTPS設定画面*

## 認証とアクセス管理

### HTTP Basic認証の設定

重要なコンテンツには、認証を設定してください。

```json
{
  "Jdx": {
    "HttpServer": {
      "VirtualHosts": [
        {
          "Settings": {
            "Authentication": {
              "Enabled": true,
              "Realm": "Restricted Area",
              "Users": [
                {
                  "Username": "user1",
                  "PasswordHash": "..."  // SHA256ハッシュ
                }
              ]
            }
          }
        }
      ]
    }
  }
}
```

### FTP/POP3/SMTPのユーザー管理

各サーバーのユーザー管理機能を活用してください。

**ベストプラクティス**:
- ユーザーごとに異なるパスワードを設定
- 不要なユーザーは削除
- 定期的にパスワードを変更

## ファイアウォール設定

### ポートの制限

必要なポートのみ開放してください。

#### 最小限の設定例

| サービス | ポート | 開放する？ |
|---------|--------|-----------|
| Web UI | 5001 | 必要に応じて |
| HTTP | 80, 8080 | 必要に応じて |
| HTTPS | 443, 8443 | 必要に応じて |
| FTP | 21 | 必要に応じて |
| SMTP | 25 | ❌ 閉じる（推奨） |
| POP3 | 110 | ❌ 閉じる（推奨） |
| DNS | 53 | 必要に応じて |

#### ファイアウォールルールの例

**Linux (ufw)**:
```bash
# デフォルトで全て拒否
sudo ufw default deny incoming
sudo ufw default allow outgoing

# 必要なポートのみ許可
sudo ufw allow from 192.168.1.0/24 to any port 5001
sudo ufw allow from 192.168.1.0/24 to any port 8080

# 有効化
sudo ufw enable
```

**Windows ファイアウォール**:
1. **Windows セキュリティ** を開く
2. **ファイアウォールとネットワーク保護** をクリック
3. **詳細設定** をクリック
4. **受信の規則** で新しいルールを作成

![ファイアウォール設定](images/security-firewall.png)
*ファイアウォールでのポート制限*

## ログ監視

### セキュリティイベントの監視

定期的にログを確認して、不審なアクセスを検出してください。

#### 監視すべきログ

1. **ログイン失敗**
   ```bash
   cat logs/*.log | jq 'select(.["@mt"] | test("Authentication failed"))'
   ```

2. **ACLによるブロック**
   ```bash
   cat logs/*.log | jq 'select(.["@mt"] | test("Access denied"))'
   ```

3. **エラーログ**
   ```bash
   cat logs/*.log | jq 'select(.["@l"] == "Error")'
   ```

4. **攻撃検出**
   ```bash
   cat logs/*.log | jq 'select(.["@mt"] | test("Attack detected"))'
   ```

詳細は、[ロギングガイド](logging.md)を参照してください。

### ログのバックアップ

重要なログは、定期的にバックアップしてください。

```bash
# ログをバックアップディレクトリにコピー
cp -r logs/ /backup/jumbodogx-logs-$(date +%Y%m%d)/
```

## ファイルとディレクトリのアクセス権限

### 推奨設定

#### Linux / macOS

```bash
# アプリケーションファイル（読み取り専用）
chmod 755 Jdx.WebUI

# 設定ファイル（所有者のみ読み書き）
chmod 600 appsettings.json

# ログディレクトリ（所有者のみ読み書き）
chmod 700 logs/

# Document Root（読み取り専用）
chmod 755 /path/to/document-root
```

#### Windows

1. ファイルを右クリック > **プロパティ**
2. **セキュリティ** タブをクリック
3. 不要なユーザーの権限を削除
4. 必要最小限の権限のみ付与

![ファイル権限設定](images/security-file-permissions.png)
*ファイルとディレクトリのアクセス権限*

## データバックアップ

### 定期的なバックアップ

以下のデータを定期的にバックアップしてください：

1. **設定ファイル**: `appsettings.json`
2. **ログファイル**: `logs/`
3. **Webコンテンツ**: Document Rootのファイル
4. **メールデータ**: POP3/SMTPのメールボックス
5. **FTPデータ**: FTPサーバーのファイル

#### バックアップスクリプトの例

```bash
#!/bin/bash
# バックアップディレクトリ
BACKUP_DIR="/backup/jumbodogx/$(date +%Y%m%d)"
mkdir -p "$BACKUP_DIR"

# 設定ファイルをバックアップ
cp appsettings.json "$BACKUP_DIR/"

# ログをバックアップ
cp -r logs/ "$BACKUP_DIR/logs/"

# Webコンテンツをバックアップ
cp -r /path/to/document-root "$BACKUP_DIR/www/"

echo "Backup completed: $BACKUP_DIR"
```

### 設定のインポート/エクスポート

JumboDogXは、設定のインポート/エクスポート機能を提供しています。

#### エクスポート

1. Web UI > **Settings** > **Backup & Restore** を開く
2. **Export Settings** ボタンをクリック
3. JSONファイルをダウンロード

#### インポート

1. Web UI > **Settings** > **Backup & Restore** を開く
2. エクスポートしたJSONファイルを選択
3. **Import Settings** ボタンをクリック
4. アプリケーションを再起動

![設定のエクスポート](images/security-export-settings.png)
*設定のインポート/エクスポート機能*

## 定期的なメンテナンス

### セキュリティチェックリスト

以下のタスクを定期的に実施してください：

#### 毎日

- [ ] ログで異常なアクセスがないか確認
- [ ] ダッシュボードでサーバーステータスを確認

#### 毎週

- [ ] ログファイルのサイズを確認
- [ ] ディスク容量を確認
- [ ] 不要なファイルを削除

#### 毎月

- [ ] ACLエントリを見直し
- [ ] 不要なユーザーアカウントを削除
- [ ] パスワードの変更を検討
- [ ] バックアップの確認

#### 四半期ごと

- [ ] 設定の見直し
- [ ] .NET Runtimeのアップデート確認
- [ ] セキュリティパッチの適用

![メンテナンスチェックリスト](images/security-maintenance.png)
*定期的なメンテナンスチェックリスト*

## よくあるセキュリティリスク

### 1. デフォルトパスワードの放置

**リスク**: 🔴 高

**症状**: デフォルトパスワードが変更されていない

**対策**: 本ガイドの「デフォルトパスワードの変更」を参照

### 2. インターネットへの直接公開

**リスク**: 🔴 高

**症状**: JumboDogXがインターネットに直接公開されている

**対策**:
- ファイアウォールで保護
- プライベートネットワーク内でのみ使用
- リバースプロキシ（nginx, Apache）を使用

### 3. CGI/WebDavの無制限な有効化

**リスク**: 🔴 高

**症状**: CGIやWebDAVが認証なしで有効化されている

**対策**:
- 必要がない場合は無効化
- 認証を必須化
- ACLで適切にアクセス制御

### 4. ACLの未設定

**リスク**: 🟠 中

**症状**: ACLが設定されていない、または全許可

**対策**: [ACL設定ガイド](acl-configuration.md)を参照して適切に設定

### 5. ログの未監視

**リスク**: 🟡 低〜中

**症状**: ログを一度も確認していない

**対策**: [ロギングガイド](logging.md)を参照して定期的に監視

## セキュリティインシデント対応

### 不正アクセスが疑われる場合

1. **即座にサーバーを停止**
   ```bash
   # Ctrl+C でJumboDogXを停止
   ```

2. **ログを確認**
   ```bash
   cat logs/*.log | jq 'select(.["@l"] == "Error" or .["@l"] == "Warning")'
   ```

3. **攻撃元IPアドレスを特定**
   ```bash
   cat logs/*.log | jq -r '.ClientIP' | sort | uniq -c | sort -rn
   ```

4. **ACLでブロック**
   - 攻撃元IPアドレスをACLのDeny Modeに追加

5. **設定を見直し**
   - パスワードを変更
   - 不要な機能を無効化
   - ACLを厳格化

6. **バックアップから復元（必要に応じて）**

### 脆弱性の報告

セキュリティ脆弱性を発見した場合は、[SECURITY.md](../../SECURITY.md)を参照して報告してください。

## まとめ

このガイドでは、以下のセキュリティベストプラクティスを学びました：

✓ デフォルトパスワードの変更（最優先）
✓ ネットワークバインディングの適切な設定
✓ CGI/WebDAV機能の慎重な有効化
✓ ACLによるアクセス制御
✓ HTTPS/SSL/TLSの使用
✓ 認証とアクセス管理
✓ ファイアウォール設定
✓ ログ監視とバックアップ
✓ 定期的なメンテナンス
✓ セキュリティインシデント対応

**重要**: JumboDogXは、ローカルテスト環境専用です。本番環境やインターネット公開での使用は推奨されません。

## 関連ドキュメント

- [インストールガイド](installation.md) - セキュアなインストール方法
- [ACL設定ガイド](acl-configuration.md) - アクセス制御の詳細
- [ロギングガイド](logging.md) - セキュリティログの監視
- [証明書セットアップ](../../docs/01_development_docs/05_certificate_setup.md) - SSL/TLS証明書の設定
