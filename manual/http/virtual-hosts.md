# Virtual Host設定ガイド

このガイドでは、JumboDogXのVirtual Host機能を使用して、1台のサーバーで複数のWebサイトを運用する方法を説明します。

## 目次

- [Virtual Hostとは](#virtual-hostとは)
- [基本設定](#基本設定)
- [設定方法](#設定方法)
- [運用例](#運用例)
- [HTTPSとの組み合わせ](#httpsとの組み合わせ)
- [トラブルシューティング](#トラブルシューティング)

## Virtual Hostとは

### 概要

Virtual Host（仮想ホスト）は、1台のサーバーで複数のドメインやサブドメインを運用できる機能です。

### 動作原理

HTTPリクエストの `Host` ヘッダーを使用して、どのサイトにアクセスしているかを判断します。

```
GET / HTTP/1.1
Host: example.com
```

上記のリクエストが来た場合、`example.com` 用のVirtual Host設定が適用されます。

### 用途

- **複数サイトの運用**: 個人サイト、ブログ、ポートフォリオを1台のサーバーで管理
- **サブドメインの運用**: `www.example.com`、`blog.example.com`、`api.example.com`
- **開発環境の分離**: プロジェクトごとに異なるドキュメントルートを設定
- **テスト環境**: 本番環境とテスト環境を同じサーバーで運用

## 基本設定

### ディレクトリ構成

Virtual Hostを使用する際の推奨ディレクトリ構成：

```
/var/www/
├── example.com/
│   ├── index.html
│   └── assets/
├── blog.example.com/
│   ├── index.html
│   └── posts/
└── api.example.com/
    ├── index.html
    └── v1/
```

### ホスト名とポート番号

Virtual Hostの識別には、ホスト名とポート番号の組み合わせを使用します：

| 設定値 | 説明 |
|--------|------|
| `example.com:80` | HTTP標準ポート |
| `example.com:443` | HTTPS標準ポート |
| `example.com:8080` | カスタムポート |
| `*.example.com:80` | ワイルドカード（サブドメイン全体） |

**重要**: クライアントから送信される `Host` ヘッダーには、デフォルトポート（HTTP:80、HTTPS:443）以外の場合にポート番号が含まれます。

## 設定方法

### Web UIからの設定

#### ステップ1: Virtual Host画面を開く

1. ブラウザで `http://localhost:5001` にアクセス
2. **Settings** → **HTTP/HTTPS** → **Virtual Hosts** を選択

#### ステップ2: Virtual Hostを追加

1. **Add Virtual Host** ボタンをクリック
2. 以下の項目を設定：

| 項目 | 説明 | 例 |
|------|------|-----|
| Host | ホスト名とポート | `example.com:80` |
| Enable | 有効化 | ✓ ON |
| Bind Address | バインドアドレス | `0.0.0.0` |
| Protocol | プロトコル | `HTTP` または `HTTPS` |
| Document Root | ドキュメントルート | `/var/www/example.com` |

3. **Save** ボタンをクリック

#### ステップ3: サーバーを再起動

設定を反映するため、JumboDogXを再起動します。

### appsettings.jsonでの設定

`src/Jdx.WebUI/appsettings.json` を編集して直接設定できます。

#### 基本的なVirtual Host設定

```json
{
  "Jdx": {
    "HttpServer": {
      "VirtualHosts": [
        {
          "Host": "example.com:80",
          "Enabled": true,
          "BindAddress": "0.0.0.0",
          "Settings": {
            "Protocol": "HTTP",
            "DocumentRoot": "/var/www/example.com",
            "WelcomeFileName": "index.html"
          }
        },
        {
          "Host": "blog.example.com:80",
          "Enabled": true,
          "BindAddress": "0.0.0.0",
          "Settings": {
            "Protocol": "HTTP",
            "DocumentRoot": "/var/www/blog",
            "WelcomeFileName": "index.html"
          }
        }
      ]
    }
  }
}
```

## 運用例

### 例1: 複数ドメインの運用

異なるドメインを1台のサーバーで運用する例：

```json
{
  "HttpServer": {
    "VirtualHosts": [
      {
        "Host": "example.com:80",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTP",
          "DocumentRoot": "/var/www/example.com",
          "WelcomeFileName": "index.html"
        }
      },
      {
        "Host": "mycompany.local:80",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTP",
          "DocumentRoot": "/var/www/company",
          "WelcomeFileName": "index.html"
        }
      }
    ]
  }
}
```

### 例2: サブドメインの運用

メインサイトとブログを分ける例：

```json
{
  "HttpServer": {
    "VirtualHosts": [
      {
        "Host": "www.example.com:80",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTP",
          "DocumentRoot": "/var/www/example.com",
          "WelcomeFileName": "index.html"
        }
      },
      {
        "Host": "blog.example.com:80",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTP",
          "DocumentRoot": "/var/www/blog",
          "WelcomeFileName": "index.html"
        }
      },
      {
        "Host": "api.example.com:80",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTP",
          "DocumentRoot": "/var/www/api",
          "WelcomeFileName": "index.json"
        }
      }
    ]
  }
}
```

### 例3: カスタムポートの運用

開発環境で異なるポートを使用する例：

```json
{
  "HttpServer": {
    "VirtualHosts": [
      {
        "Host": "dev.example.com:8080",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTP",
          "DocumentRoot": "/var/www/dev",
          "WelcomeFileName": "index.html"
        }
      },
      {
        "Host": "staging.example.com:8081",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTP",
          "DocumentRoot": "/var/www/staging",
          "WelcomeFileName": "index.html"
        }
      }
    ]
  }
}
```

**アクセス方法**:
- `http://dev.example.com:8080/`
- `http://staging.example.com:8081/`

### 例4: ローカル開発環境

hostsファイルを使用したローカル開発：

**hostsファイルの編集**:

```bash
# /etc/hosts (macOS/Linux) または C:\Windows\System32\drivers\etc\hosts (Windows)
127.0.0.1 project1.local
127.0.0.1 project2.local
127.0.0.1 blog.local
```

**Virtual Host設定**:

```json
{
  "HttpServer": {
    "VirtualHosts": [
      {
        "Host": "project1.local:80",
        "Enabled": true,
        "BindAddress": "127.0.0.1",
        "Settings": {
          "Protocol": "HTTP",
          "DocumentRoot": "/Users/username/projects/project1/public",
          "WelcomeFileName": "index.html"
        }
      },
      {
        "Host": "project2.local:80",
        "Enabled": true,
        "BindAddress": "127.0.0.1",
        "Settings": {
          "Protocol": "HTTP",
          "DocumentRoot": "/Users/username/projects/project2/dist",
          "WelcomeFileName": "index.html"
        }
      }
    ]
  }
}
```

## HTTPSとの組み合わせ

### HTTPS Virtual Hostの設定

各Virtual Hostに個別の証明書を設定できます。

```json
{
  "HttpServer": {
    "VirtualHosts": [
      {
        "Host": "example.com:443",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTPS",
          "DocumentRoot": "/var/www/example.com",
          "CertificateFile": "/path/to/example.com.pfx",
          "CertificatePassword": "password123"
        }
      },
      {
        "Host": "blog.example.com:443",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTPS",
          "DocumentRoot": "/var/www/blog",
          "CertificateFile": "/path/to/blog.example.com.pfx",
          "CertificatePassword": "password456"
        }
      }
    ]
  }
}
```

### HTTPとHTTPSの同時運用

同じドメインでHTTPとHTTPSを両方設定する例：

```json
{
  "HttpServer": {
    "VirtualHosts": [
      {
        "Host": "example.com:80",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTP",
          "DocumentRoot": "/var/www/example.com"
        }
      },
      {
        "Host": "example.com:443",
        "Enabled": true,
        "BindAddress": "0.0.0.0",
        "Settings": {
          "Protocol": "HTTPS",
          "DocumentRoot": "/var/www/example.com",
          "CertificateFile": "/path/to/example.com.pfx",
          "CertificatePassword": "password"
        }
      }
    ]
  }
}
```

### 証明書のセキュリティ

パスワードを環境変数で管理することを推奨します：

```bash
export EXAMPLE_CERT_PASSWORD="password123"
export BLOG_CERT_PASSWORD="password456"
```

```json
{
  "Settings": {
    "CertificatePassword": "${EXAMPLE_CERT_PASSWORD}"
  }
}
```

## ポート番号の扱い

### Hostヘッダーとポート番号

クライアント（ブラウザ）から送信される `Host` ヘッダーのポート番号の扱い：

| アクセスURL | Hostヘッダー | 説明 |
|-------------|--------------|------|
| `http://example.com/` | `example.com` | デフォルトポート（80）は省略 |
| `http://example.com:80/` | `example.com` | 明示的に80を指定しても省略される |
| `http://example.com:8080/` | `example.com:8080` | カスタムポートは含まれる |
| `https://example.com/` | `example.com` | デフォルトポート（443）は省略 |
| `https://example.com:443/` | `example.com` | 明示的に443を指定しても省略される |
| `https://example.com:8443/` | `example.com:8443` | カスタムポートは含まれる |

### Virtual Host設定のポート指定

**推奨設定**:

```json
{
  "VirtualHosts": [
    {
      "Host": "example.com:80",
      "Settings": {
        "Protocol": "HTTP",
        "Port": 80
      }
    },
    {
      "Host": "example.com:443",
      "Settings": {
        "Protocol": "HTTPS",
        "Port": 443
      }
    },
    {
      "Host": "example.com:8080",
      "Settings": {
        "Protocol": "HTTP",
        "Port": 8080
      }
    }
  ]
}
```

### マッチングの優先順位

1. **完全一致**: `example.com:8080` → `example.com:8080`
2. **ホスト名のみ**: `example.com` → `example.com:80` または `example.com:443`
3. **ワイルドカード**: `*.example.com:80` → `blog.example.com:80`
4. **デフォルト**: どのVirtual Hostにもマッチしない場合のデフォルト動作

## トラブルシューティング

### Virtual Hostが動作しない

#### 問題1: ページが表示されない

**症状**: Virtual Hostでアクセスしても404エラーまたはデフォルトページが表示される

**原因と解決策**:

1. **ホスト名の設定を確認**

```bash
# hostsファイルの確認（macOS/Linux）
cat /etc/hosts

# hostsファイルの確認（Windows）
type C:\Windows\System32\drivers\etc\hosts
```

正しく設定されているか確認：
```
127.0.0.1 example.local
127.0.0.1 blog.local
```

2. **Virtual Host設定のホスト名を確認**

`Host` 項目がhostsファイルと一致しているか確認：

```json
{
  "Host": "example.local:80"  // hostsファイルの設定と一致
}
```

3. **Hostヘッダーの確認**

ブラウザの開発者ツールでリクエストヘッダーを確認：

```
GET / HTTP/1.1
Host: example.local
```

#### 問題2: ポート番号でアクセスできない

**症状**: `http://example.com:8080/` でアクセスできない

**原因**: Virtual Host設定のポート番号が間違っている

**解決策**:

```json
{
  "Host": "example.com:8080",  // ポート番号を明示的に指定
  "Settings": {
    "Port": 8080
  }
}
```

#### 問題3: 間違ったサイトが表示される

**症状**: `blog.example.com` にアクセスしても `www.example.com` のページが表示される

**原因**: Virtual Hostの優先順位やマッチングの問題

**解決策**:

1. **Host設定の確認**

```json
{
  "VirtualHosts": [
    {
      "Host": "www.example.com:80",  // 完全一致
      "Enabled": true,
      "Settings": {
        "DocumentRoot": "/var/www/example.com"
      }
    },
    {
      "Host": "blog.example.com:80",  // 完全一致
      "Enabled": true,
      "Settings": {
        "DocumentRoot": "/var/www/blog"
      }
    }
  ]
}
```

2. **サーバーを再起動**

```bash
# JumboDogXを再起動
Ctrl+C
dotnet run --project src/Jdx.WebUI
```

### HTTPSが動作しない

#### 問題1: 証明書エラー

**症状**: HTTPS Virtual Hostで証明書エラーが発生する

**原因**: 証明書のホスト名が一致していない

**解決策**:

証明書のCN（Common Name）またはSAN（Subject Alternative Name）にVirtual Hostのホスト名が含まれているか確認：

```bash
# 証明書の内容を確認
openssl pkcs12 -in certificate.pfx -nodes -passin pass:yourpassword | openssl x509 -noout -text
```

証明書に複数のホスト名を含める：

```bash
# SANを含む証明書を作成
cat > san.cnf << EOF
[req]
distinguished_name = req_distinguished_name
x509_extensions = v3_req
prompt = no

[req_distinguished_name]
CN = example.com

[v3_req]
subjectAltName = @alt_names

[alt_names]
DNS.1 = example.com
DNS.2 = www.example.com
DNS.3 = blog.example.com
EOF

openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes -config san.cnf
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem -password pass:yourpassword
```

#### 問題2: 混在コンテンツの警告

**症状**: HTTPSページでHTTPリソースを読み込むと警告が表示される

**解決策**:

すべてのリソースをHTTPSで読み込むように修正：

```html
<!-- 修正前 -->
<script src="http://example.com/script.js"></script>
<img src="http://example.com/image.jpg">

<!-- 修正後 -->
<script src="https://example.com/script.js"></script>
<img src="https://example.com/image.jpg">

<!-- または相対パスを使用 -->
<script src="/script.js"></script>
<img src="/image.jpg">
```

### パフォーマンス問題

#### 問題: Virtual Hostが多いと応答が遅い

**症状**: Virtual Hostを多数設定すると応答時間が長くなる

**解決策**:

1. **使用していないVirtual Hostを無効化**

```json
{
  "Host": "old-site.com:80",
  "Enabled": false  // 無効化
}
```

2. **ワイルドカードを活用**

個別にVirtual Hostを作成するのではなく、ワイルドカードを使用：

```json
{
  "Host": "*.example.com:80",
  "Settings": {
    "DocumentRoot": "/var/www/${hostname}"
  }
}
```

### ログの確認

問題が発生した場合は、ログを確認してください：

```bash
# ログディレクトリの確認
ls -la logs/

# HTTPサーバーログの確認
cat logs/http-server.log

# エラーログの確認
cat logs/error.log
```

## ベストプラクティス

### セキュリティ

1. **HTTPSの使用**: 本番環境では必ずHTTPSを使用
2. **証明書の管理**: パスワードを環境変数で管理
3. **アクセス制御**: 必要に応じてACLを設定

```json
{
  "Settings": {
    "Protocol": "HTTPS",
    "CertificatePassword": "${CERT_PASSWORD}",
    "Acl": {
      "Rules": [
        {
          "Path": "/admin/*",
          "Allow": ["192.168.1.0/24"],
          "Deny": ["*"]
        }
      ]
    }
  }
}
```

### パフォーマンス

1. **Keep-Aliveの有効化**: 接続の再利用で高速化

```json
{
  "Settings": {
    "UseKeepAlive": true,
    "KeepAliveTimeout": 5
  }
}
```

2. **ETagの有効化**: キャッシュの効率化

```json
{
  "Settings": {
    "UseEtag": true
  }
}
```

### 管理

1. **ドキュメントルートの整理**: プロジェクトごとにディレクトリを分ける
2. **ログの監視**: 定期的にログを確認
3. **バックアップ**: 設定ファイルと証明書のバックアップ

## 関連ドキュメント

- [クイックスタート](getting-started.md) - 基本的な設定手順
- [詳細設定](configuration.md) - HTTPサーバーの詳細設定
- [SSL/TLS設定](ssl-tls.md) - HTTPS化の設定
- [トラブルシューティング](troubleshooting.md) - 問題解決

## まとめ

Virtual Host機能を使用することで：

- 1台のサーバーで複数のWebサイトを運用可能
- ドメインごとに異なる設定を適用可能
- 開発環境とテスト環境を同じサーバーで管理可能
- HTTPSとHTTPを柔軟に組み合わせ可能

適切に設定することで、効率的なWebサーバー運用が実現できます。
