# 技術スタック

## 1. コア技術

### 1.1 フレームワーク・言語

| 項目 | バージョン | 用途 |
|------|----------|------|
| .NET | 9.0 | ランタイム・SDK |
| C# | 13.0 | 開発言語 |
| ASP.NET Core | 9.0 | Web フレームワーク |
| Blazor Server | 9.0 | Web UI フレームワーク |

### 1.2 主要ライブラリ

#### ロギング

| ライブラリ | バージョン | 用途 |
|-----------|----------|------|
| Serilog | 4.2.0+ | 構造化ロギング |
| Serilog.Extensions.Hosting | 8.0.0+ | ホスト統合 |
| Serilog.Sinks.Console | 6.0.0+ | コンソール出力 |
| Serilog.Sinks.File | 6.0.0+ | ファイル出力 |
| Serilog.Sinks.Async | 2.1.0 | 非同期出力 |

#### テスト

| ライブラリ | バージョン | 用途 |
|-----------|----------|------|
| xunit | 2.9.2 | テストフレームワーク |
| FluentAssertions | 6.12.2 | アサーション |
| Moq | 4.20.72 | モック |
| coverlet.collector | 6.0.2 | カバレッジ収集 |

## 2. プロジェクト設定

### 2.1 コンパイル設定

```xml
<PropertyGroup>
  <LangVersion>13.0</LangVersion>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

### 2.2 ターゲットフレームワーク

- すべてのプロジェクト: `net9.0`

## 3. 開発ツール

### 3.1 IDE

- Visual Studio 2022
- Visual Studio Code（推奨拡張機能付き）

### 3.2 CI/CD

- GitHub Actions（予定）

### 3.3 コード品質

- .editorconfig によるコーディング規約統一
- Directory.Build.props による一元管理
