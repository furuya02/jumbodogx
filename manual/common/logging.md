# ロギングガイド

このガイドでは、JumboDogXのロギング機能を使用して、サーバーの動作状況を監視・分析する方法を説明します。

## ロギングの概要

JumboDogXは、**Serilog**を使用した**構造化ロギング**を採用しています。

### ロギングの特徴

- **構造化ログ**: JSON形式で出力、機械可読で検索・分析が容易
- **自動ローテーション**: 日次およびサイズベースで自動ローテーション
- **複数の出力先**: コンソール、ファイル、Web UIログビューア
- **柔軟なフィルタリング**: ログレベル、カテゴリ、サーバー種別でフィルタリング
- **リアルタイム表示**: Web UIで最新ログをリアルタイムに表示

## ログの出力先

JumboDogXは、以下の3つの出力先にログを記録します。

### 1. コンソール出力

アプリケーション起動時のターミナルにリアルタイムでログが表示されます。

**形式**: JSON形式（Compact JSON）

**用途**:
- 開発時のデバッグ
- 起動時の問題確認
- リアルタイムモニタリング

![コンソールログ](images/console-log.png)
*ターミナルに表示されるJSONログ*

### 2. ファイル出力

ログファイルとして永続的に保存されます。

**ファイルパス**:
- Host アプリケーション: `logs/jumbodogx-host{Date}.log`
- WebUI アプリケーション: `logs/jumbodogx-webui{Date}.log`

**例**:
```
logs/jumbodogx-webui20260124.log
```

**形式**: JSON形式（Compact JSON）

**用途**:
- 長期的なログ保存
- 過去のログ分析
- トラブルシューティング

### 3. Web UIログビューア

Web管理画面でログを表示・検索できます。

**アクセス方法**:
1. ブラウザで `http://localhost:5001` を開く
2. サイドメニューから **Logs** をクリック

![Web UIログビューア](images/logs-viewer.png)
*Web UIのログビューア画面*

**用途**:
- 直感的なログ確認
- リアルタイムログ監視
- フィルタリング・検索

## Web UIログビューアの使い方

### 基本的な使い方

Web UIログビューアは、JumboDogXの全サーバーのログを統合表示します。

#### ログビューアを開く

1. ブラウザで `http://localhost:5001` を開く
2. サイドメニューから **Logs** をクリック
3. 最新のログエントリが表示されます

![ログビューア基本画面](images/logs-basic.png)
*ログビューア基本画面*

### ログのフィルタリング

#### ログレベルでフィルタ

画面上部の **Log Level** ドロップダウンから選択：

- **All** - すべてのログを表示
- **Information** - 通常の動作ログ
- **Warning** - 警告メッセージ
- **Error** - エラーメッセージのみ

![ログレベルフィルタ](images/logs-level-filter.png)
*ログレベルによるフィルタリング*

#### カテゴリでフィルタ

**Category** ドロップダウンから特定のサーバーを選択：

- **All** - すべてのカテゴリ
- **Jdx.Servers.Http** - HTTPサーバーのログのみ
- **Jdx.Servers.Dns** - DNSサーバーのログのみ
- **Jdx.Servers.Smtp** - SMTPサーバーのログのみ
- その他のサーバー

#### テキスト検索

**Search** 入力欄にキーワードを入力して検索：

**例**:
- IPアドレスで検索: `192.168.1.100`
- エラーメッセージで検索: `Connection failed`
- ファイル名で検索: `index.html`

![テキスト検索](images/logs-search.png)
*テキスト検索機能*

### ログのクリア

不要になったログをクリアする場合：

1. **Clear Logs** ボタンをクリック
2. 確認ダイアログで **OK** をクリック
3. 現在のセッションのログがクリアされます

**注意**: ファイルに保存されたログはクリアされません。

![ログクリア](images/logs-clear.png)
*ログクリア機能*

## ログレベル

JumboDogXは、以下のログレベルを使用します。

| レベル | 説明 | 使用例 |
|--------|------|--------|
| **Verbose** | 最も詳細なデバッグ情報 | 変数の値、内部状態 |
| **Debug** | デバッグ情報 | 関数の呼び出し、処理フロー |
| **Information** | 通常の操作情報 | サーバー起動、リクエスト受信 |
| **Warning** | 警告（動作は継続） | タイムアウト、リトライ |
| **Error** | エラー（操作失敗） | 接続エラー、ファイル読み込み失敗 |
| **Fatal** | 致命的エラー | サーバー停止、回復不能なエラー |

### ログレベルの例

#### Information（通常ログ）

```json
{
  "@t": "2026-01-24T10:30:45.123Z",
  "@mt": "HTTP request received",
  "@l": "Information",
  "Method": "GET",
  "Path": "/index.html",
  "ClientIP": "192.168.1.100"
}
```

#### Warning（警告）

```json
{
  "@t": "2026-01-24T10:31:15.456Z",
  "@mt": "Request timeout after {Timeout}ms",
  "@l": "Warning",
  "Timeout": 3000,
  "ClientIP": "192.168.1.101"
}
```

#### Error（エラー）

```json
{
  "@t": "2026-01-24T10:32:00.789Z",
  "@mt": "Failed to read file {FilePath}",
  "@l": "Error",
  "FilePath": "/var/www/index.html",
  "Exception": "FileNotFoundException: The file does not exist"
}
```

## ログファイルの管理

### ログファイルの場所

ログファイルは、JumboDogXのインストールディレクトリ内の `logs/` フォルダに保存されます。

```
jumbodogx/
└── logs/
    ├── jumbodogx-webui20260124.log     # 今日のログ
    ├── jumbodogx-webui20260123.log     # 昨日のログ
    └── jumbodogx-webui20260122.log     # 一昨日のログ
```

### ログローテーション

JumboDogXは、以下の条件でログファイルを自動的にローテーションします：

#### 1. 日次ローテーション

- **タイミング**: 毎日0時（深夜）
- **動作**: 新しいログファイルが作成され、日付がファイル名に追加されます
- **例**: `jumbodogx-webui20260124.log` → `jumbodogx-webui20260125.log`

#### 2. サイズベースローテーション

- **しきい値**: 10MB
- **動作**: ログファイルが10MBを超えると、自動的に新しいファイルに切り替え

### ログ保持期間

- **デフォルト**: 過去30日間のログを保持
- **動作**: 30日より古いログファイルは自動的に削除されます

**カスタマイズ**: 保持期間を変更したい場合は、`appsettings.json` を編集してください（後述）。

![ログファイル一覧](images/logs-files.png)
*ログファイルの一覧*

## ログ形式（JSON構造）

JumboDogXのログは、**Compact JSON形式**で出力されます。

### ログエントリの構造

```json
{
  "@t": "2026-01-24T10:30:45.123Z",           // タイムスタンプ（UTC）
  "@mt": "HTTP request received",             // メッセージテンプレート
  "@l": "Information",                        // ログレベル
  "Application": "JumboDogX.WebUI",           // アプリケーション名
  "SourceContext": "Jdx.Servers.Http.HttpServer",  // ログ生成元
  "Method": "GET",                            // プロパティ
  "Path": "/index.html",                      // プロパティ
  "ClientIP": "192.168.1.100"                 // プロパティ
}
```

### 主要フィールド

| フィールド | 説明 |
|-----------|------|
| `@t` | タイムスタンプ（UTC、ISO 8601形式） |
| `@mt` | メッセージテンプレート |
| `@l` | ログレベル |
| `Application` | アプリケーション名（Host / WebUI） |
| `SourceContext` | ログを生成したクラス名 |
| その他 | 構造化データ（Method, Path, ClientIPなど） |

## ログの分析

### コマンドラインでの分析（jqを使用）

ログファイルはJSON形式なので、`jq`コマンドを使用して簡単に分析できます。

#### すべてのErrorログを表示

```bash
cat logs/jumbodogx-webui*.log | jq 'select(.["@l"] == "Error")'
```

#### 特定のIPアドレスのログを抽出

```bash
cat logs/jumbodogx-webui*.log | jq 'select(.ClientIP == "192.168.1.100")'
```

#### HTTPリクエストのメソッド別集計

```bash
cat logs/jumbodogx-webui*.log | jq -r '.Method' | sort | uniq -c
```

**出力例**:
```
  1500 GET
   200 POST
    50 PUT
    10 DELETE
```

#### タイムスタンプとメッセージのみ表示

```bash
cat logs/jumbodogx-webui*.log | jq -r '"\(.["@t"]) - \(.["@mt"])"'
```

**出力例**:
```
2026-01-24T10:30:45.123Z - HTTP request received
2026-01-24T10:30:46.456Z - File sent successfully
2026-01-24T10:30:47.789Z - Connection closed
```

![jqコマンドでの分析](images/logs-jq-analysis.png)
*jqコマンドを使用したログ分析*

### ログ分析ツール

JSON形式のログは、以下のツールでも分析できます：

- **Elasticsearch + Kibana** - 大規模ログ分析
- **Grafana Loki** - ログ集約と可視化
- **Seq** - .NET向けログ分析プラットフォーム
- **jq** - コマンドラインでのJSON処理

## ログ設定のカスタマイズ

### appsettings.json でのログ設定

ログの動作は、`appsettings.json` ファイルで設定できます。

#### ログレベルの変更

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",      // デフォルトレベル
      "Override": {
        "Microsoft": "Warning",      // Microsoft系のログは Warning以上
        "System": "Warning",         // System系のログは Warning以上
        "Jdx.Servers.Http": "Debug"  // HTTPサーバーは Debug以上
      }
    }
  }
}
```

**ログレベルの値**:
- `Verbose` - すべてのログ
- `Debug` - デバッグ以上
- `Information` - 通常動作以上（推奨）
- `Warning` - 警告以上
- `Error` - エラーのみ
- `Fatal` - 致命的エラーのみ

#### 出力先の設定

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console"             // コンソール出力
      },
      {
        "Name": "File",               // ファイル出力
        "Args": {
          "path": "logs/jumbodogx-.log",
          "rollingInterval": "Day",   // 日次ローテーション
          "retainedFileCountLimit": 30,  // 保持期間（30日）
          "fileSizeLimitBytes": 10485760  // サイズ上限（10MB）
        }
      }
    ]
  }
}
```

#### ログフォーマットの変更

より詳細なログフォーマットに変更する場合：

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

### 設定変更後の反映

設定ファイルを変更した後は、JumboDogXを再起動してください。

```bash
# Ctrl+C で停止
# 再起動
dotnet run --project src/Jdx.WebUI
```

## 実用的なログ活用例

### 例1: HTTPリクエストの監視

**目的**: どのページがアクセスされているか確認

**ログの確認**:
```bash
cat logs/jumbodogx-webui*.log | jq 'select(.SourceContext == "Jdx.Servers.Http.HttpServer") | {time: .["@t"], method: .Method, path: .Path, client: .ClientIP}'
```

**出力例**:
```json
{
  "time": "2026-01-24T10:30:45.123Z",
  "method": "GET",
  "path": "/index.html",
  "client": "192.168.1.100"
}
```

### 例2: エラーの調査

**目的**: エラーが発生した原因を調査

**ログの確認**:
```bash
cat logs/jumbodogx-webui*.log | jq 'select(.["@l"] == "Error")'
```

### 例3: アクセス元IPアドレスの統計

**目的**: どのIPアドレスから最もアクセスがあるか確認

**ログの確認**:
```bash
cat logs/jumbodogx-webui*.log | jq -r '.ClientIP' | sort | uniq -c | sort -rn | head -10
```

**出力例**:
```
   500 192.168.1.100
   200 192.168.1.101
    50 192.168.1.102
```

### 例4: サーバー起動/停止の履歴

**目的**: サーバーがいつ起動・停止したか確認

**ログの確認**:
```bash
cat logs/jumbodogx-webui*.log | jq 'select(.["@mt"] | test("started|stopped"))'
```

![ログ活用例](images/logs-use-cases.png)
*ログの実用的な活用例*

## よくある問題と解決方法

### ログファイルが作成されない

**原因**: ログディレクトリへの書き込み権限がない

**解決策**:

```bash
# ログディレクトリの権限を確認
ls -la logs/

# 権限を付与（Linux/macOS）
chmod 755 logs/
```

### ログが大量に出力される

**原因**: ログレベルが `Debug` または `Verbose` に設定されている

**解決策**:

1. `appsettings.json` を編集
2. `MinimumLevel.Default` を `Information` に変更
3. JumboDogXを再起動

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"  // Debug → Information に変更
    }
  }
}
```

### Web UIでログが表示されない

**原因**: LogServiceが正しく動作していない可能性

**確認事項**:
1. JumboDogXが起動しているか確認
2. ブラウザのキャッシュをクリアして再読み込み
3. コンソールログでエラーがないか確認

### ログファイルが削除されない

**原因**: ログ保持期間の設定が長すぎる

**解決策**:

`appsettings.json` の `retainedFileCountLimit` を調整：

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "retainedFileCountLimit": 7  // 7日間に変更
        }
      }
    ]
  }
}
```

### JSON形式が読みにくい

**症状**: コンソールログのJSON形式が見にくい

**解決策**: `jq`コマンドでフォーマット

```bash
# リアルタイムでフォーマット表示
dotnet run --project src/Jdx.WebUI | jq
```

または、出力フォーマットを変更（前述の「ログフォーマットの変更」を参照）。

## セキュリティとプライバシー

### ログに含まれる情報

JumboDogXのログには、以下の情報が含まれる可能性があります：

- IPアドレス
- リクエストURL
- ファイルパス
- ユーザー名（認証時）

### 推奨事項

1. **ログファイルのアクセス権限を制限**
   ```bash
   chmod 600 logs/*.log  # 所有者のみ読み書き可能
   ```

2. **ログの定期的なクリーンアップ**
   - 不要になったログは削除
   - 保持期間を適切に設定

3. **機密情報のマスキング**
   - パスワードなどの機密情報はログに記録しない設計

## まとめ

このガイドでは、以下の内容を学びました：

✓ JumboDogXのロギングの概要と特徴
✓ ログの出力先（コンソール、ファイル、Web UI）
✓ Web UIログビューアの使い方
✓ ログレベルとその意味
✓ ログファイルの管理とローテーション
✓ JSON形式のログ構造
✓ ログの分析方法（jqコマンド）
✓ ログ設定のカスタマイズ
✓ 実用的なログ活用例
✓ トラブルシューティング

適切なロギング設定により、JumboDogXの動作状況を効果的に監視・分析できます。

## 関連ドキュメント

- [インストールガイド](installation.md) - JumboDogXのセットアップ
- [セキュリティベストプラクティス](security-best-practices.md) - セキュアな運用方法
- [ACL設定ガイド](acl-configuration.md) - アクセス制御とログの関連
