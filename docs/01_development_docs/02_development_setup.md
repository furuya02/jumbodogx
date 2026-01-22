# 開発環境セットアップ

## 1. 必要条件

### 1.1 必須

- .NET 9.0 SDK
- Git

### 1.2 推奨

- Visual Studio 2022 または Visual Studio Code
- Docker（テスト環境用）

## 2. セットアップ手順

### 2.1 リポジトリのクローン

```bash
git clone https://github.com/furuya02/jumbodogx.git
cd jumbodogx
```

### 2.2 依存関係の復元

```bash
dotnet restore
```

### 2.3 ビルド

```bash
dotnet build
```

### 2.4 実行

```bash
# Web UIを含むホストアプリケーションを起動
dotnet run --project src/Jdx.WebUI
```

ブラウザで `http://localhost:5000` にアクセス。

## 3. VS Code 設定

### 3.1 推奨拡張機能

`.vscode/extensions.json` で定義されている拡張機能をインストール。

### 3.2 デバッグ

`.vscode/launch.json` に定義されたデバッグ構成を使用。

- F5: デバッグ開始
- Ctrl+Shift+B: ビルド

## 4. プロジェクト構造

```
jumbodogx/
├── src/                    # ソースコード
├── tests/                  # テストコード
├── benchmarks/             # ベンチマーク
├── docs/                   # ドキュメント
├── samples/                # サンプル
│   └── www/               # Webコンテンツサンプル
├── Jdx.sln                # ソリューションファイル
├── Directory.Build.props  # 共通ビルド設定
├── global.json            # SDK バージョン指定
└── .editorconfig          # エディタ設定
```

## 5. テスト実行

### 5.1 全テスト実行

```bash
dotnet test
```

### 5.2 特定プロジェクトのテスト

```bash
dotnet test tests/Jdx.Core.Tests
```

### 5.3 カバレッジ付きテスト

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 6. ビルド設定

### 6.1 Directory.Build.props

共通のビルド設定は `Directory.Build.props` で管理。

```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <LangVersion>13.0</LangVersion>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

### 6.2 global.json

SDKバージョンを固定。

```json
{
  "sdk": {
    "version": "9.0.100",
    "rollForward": "latestFeature"
  }
}
```

## 7. トラブルシューティング

### 7.1 ポート競合

サーバー起動時にポート競合エラーが発生した場合：

1. 使用中のポートを確認
2. Web UIで別のポートを設定
3. 競合するアプリケーションを停止

### 7.2 ビルドエラー

```bash
# クリーンビルド
dotnet clean
dotnet build
```
