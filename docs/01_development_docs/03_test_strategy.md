# テスト戦略

## 1. テスト方針

### 1.1 テストピラミッド

```
        ┌─────┐
        │ E2E │  少数の統合テスト
       ─┴─────┴─
      /  統合   \  サーバー間連携テスト
    ─────────────
   /    単体     \  ユニットテスト（大多数）
  ───────────────
```

### 1.2 カバレッジ目標

- コアライブラリ: 80%以上
- サーバー実装: 70%以上
- Web UI: 機能テスト中心

## 2. テストフレームワーク

### 2.1 使用ライブラリ

| ライブラリ | 用途 |
|-----------|------|
| xunit | テストフレームワーク |
| FluentAssertions | 流暢なアサーション |
| Moq | モックライブラリ |
| coverlet.collector | カバレッジ収集 |

### 2.2 テストプロジェクト

```
tests/
├── Jdx.Core.Tests/          # コア機能テスト
├── Jdx.Servers.Http.Tests/  # HTTPサーバーテスト
├── Jdx.Servers.Dns.Tests/   # DNSサーバーテスト
├── Jdx.Servers.Smtp.Tests/  # SMTPサーバーテスト
├── Jdx.Servers.Pop3.Tests/  # POP3サーバーテスト
├── Jdx.Servers.Tftp.Tests/  # TFTPサーバーテスト
└── Jdx.Servers.Dhcp.Tests/  # DHCPサーバーテスト
```

## 3. テスト実行

### 3.1 コマンドライン

```bash
# 全テスト実行
dotnet test

# 特定プロジェクト
dotnet test tests/Jdx.Core.Tests

# 詳細出力
dotnet test --logger "console;verbosity=detailed"

# カバレッジ付き
dotnet test --collect:"XPlat Code Coverage"
```

### 3.2 フィルタリング

```bash
# 特定カテゴリ
dotnet test --filter "Category=Unit"

# 特定テストメソッド
dotnet test --filter "FullyQualifiedName~ConnectionLimiter"
```

## 4. テストパターン

### 4.1 Arrange-Act-Assert (AAA)

```csharp
[Fact]
public void Method_Scenario_ExpectedBehavior()
{
    // Arrange
    var sut = new SystemUnderTest();

    // Act
    var result = sut.DoSomething();

    // Assert
    result.Should().BeTrue();
}
```

### 4.2 FluentAssertions の使用

```csharp
// 基本アサーション
result.Should().Be(expected);
result.Should().NotBeNull();

// コレクション
list.Should().HaveCount(3);
list.Should().Contain(item);

// 例外
action.Should().Throw<ArgumentException>();
```

### 4.3 Moq の使用

```csharp
var mock = new Mock<IService>();
mock.Setup(x => x.GetValue()).Returns("test");

var sut = new Consumer(mock.Object);
sut.Execute();

mock.Verify(x => x.GetValue(), Times.Once);
```

## 5. テストカテゴリ

### 5.1 単体テスト

- 個別クラス・メソッドのテスト
- 外部依存はモック化
- 高速実行

### 5.2 統合テスト

- 複数コンポーネントの連携テスト
- 実際のネットワーク通信を含む場合あり
- 実行に時間がかかる場合あり

## 6. ベンチマーク

### 6.1 BenchmarkDotNet

`benchmarks/Jdx.Benchmarks` でパフォーマンス計測。

```bash
dotnet run -c Release --project benchmarks/Jdx.Benchmarks
```

## 7. CI/CD統合

### 7.1 GitHub Actions（予定）

- PRごとにテスト実行
- mainマージ時にカバレッジレポート生成
- リリースビルドの自動化
