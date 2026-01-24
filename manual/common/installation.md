# JumboDogX インストールガイド

このガイドでは、JumboDogXをインストールし、最初の起動まで行う方法を説明します。

## 前提条件

JumboDogXは.NET 9で構築されたクロスプラットフォームアプリケーションです。
以下の環境で動作します：

- **対応OS**: Windows 10/11, macOS 12+, Linux (Ubuntu 20.04+, Debian 11+, など)
- **必須**: .NET 9 Runtime または SDK
- **推奨**: Visual Studio Code (設定ファイルの編集に便利)

## ステップ1: .NET 9のインストール

JumboDogXを実行するには、.NET 9 RuntimeまたはSDKが必要です。

### 1-1. どちらをインストールすべきか？

| 用途 | 必要なもの |
|------|-----------|
| JumboDogXを使用するだけ | .NET 9 Runtime |
| ソースコードから自分でビルドする | .NET 9 SDK |
| 開発・カスタマイズする | .NET 9 SDK |

**初めての方へ**: JumboDogXを使うだけなら**.NET 9 Runtime**をインストールしてください。

### 1-2. Windows へのインストール

#### 方法1: インストーラーを使う（推奨）

1. [.NET 9 ダウンロードページ](https://dotnet.microsoft.com/download/dotnet/9.0)にアクセス
2. **.NET Runtime 9.x.x** セクションの **Windows x64 Installer** をクリック
3. ダウンロードした `.exe` ファイルを実行
4. インストーラーの指示に従ってインストール

![Windows インストーラー](images/windows-installer.png)
*Windows用.NET Runtimeインストーラー*

#### 方法2: winget を使う

PowerShellまたはコマンドプロンプトで実行：

```powershell
# Runtime のみ
winget install Microsoft.DotNet.Runtime.9

# SDK（開発者向け）
winget install Microsoft.DotNet.SDK.9
```

#### インストール確認

コマンドプロンプトまたはPowerShellで確認：

```powershell
dotnet --version
```

`9.x.x` のようなバージョン番号が表示されればインストール成功です。

### 1-3. macOS へのインストール

#### 方法1: インストーラーを使う（推奨）

1. [.NET 9 ダウンロードページ](https://dotnet.microsoft.com/download/dotnet/9.0)にアクセス
2. **.NET Runtime 9.x.x** セクションの **macOS Installer** をクリック
   - Intel Mac: x64
   - Apple Silicon Mac (M1/M2/M3): ARM64
3. ダウンロードした `.pkg` ファイルを実行
4. インストーラーの指示に従ってインストール

![macOS インストーラー](images/macos-installer.png)
*macOS用.NET Runtimeインストーラー*

#### 方法2: Homebrew を使う

ターミナルで実行：

```bash
# Runtime のみ
brew install dotnet

# SDK（開発者向け）
brew install --cask dotnet-sdk
```

#### インストール確認

ターミナルで確認：

```bash
dotnet --version
```

`9.x.x` のようなバージョン番号が表示されればインストール成功です。

### 1-4. Linux へのインストール

#### Ubuntu / Debian の場合

```bash
# パッケージリストを更新
sudo apt update

# 必要なパッケージをインストール
sudo apt install -y wget

# Microsoft パッケージリポジトリを追加
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# .NET Runtime をインストール
sudo apt update
sudo apt install -y dotnet-runtime-9.0

# SDK をインストールする場合（開発者向け）
# sudo apt install -y dotnet-sdk-9.0
```

#### Fedora / CentOS / RHEL の場合

```bash
# .NET Runtime をインストール
sudo dnf install dotnet-runtime-9.0

# SDK をインストールする場合（開発者向け）
# sudo dnf install dotnet-sdk-9.0
```

#### インストール確認

```bash
dotnet --version
```

`9.x.x` のようなバージョン番号が表示されればインストール成功です。

## ステップ2: JumboDogXのダウンロード

JumboDogXは、GitHubからダウンロードできます。

### 2-1. リリース版をダウンロード（推奨）

1. [JumboDogX Releases](https://github.com/furuya02/jumbodogx/releases)にアクセス
2. 最新バージョンの **Assets** セクションを展開
3. お使いのOSに応じたファイルをダウンロード：
   - Windows: `jumbodogx-win-x64.zip`
   - macOS (Intel): `jumbodogx-osx-x64.zip`
   - macOS (Apple Silicon): `jumbodogx-osx-arm64.zip`
   - Linux: `jumbodogx-linux-x64.zip`

![GitHub Releases](images/github-releases.png)
*GitHub ReleasesページからJumboDogXをダウンロード*

### 2-2. ソースコードからビルド（開発者向け）

Gitがインストールされている場合、ソースコードをクローンしてビルドできます。

```bash
# リポジトリをクローン
git clone https://github.com/furuya02/jumbodogx.git
cd jumbodogx

# 依存関係を復元
dotnet restore

# ビルド
dotnet build

# 実行
dotnet run --project src/Jdx.WebUI
```

## ステップ3: インストール

### 3-1. ダウンロードしたファイルを展開

ダウンロードしたZIPファイルを展開します。

#### Windows の場合

1. ZIPファイルを右クリック
2. **すべて展開** をクリック
3. 展開先を選択（例：`C:\JumboDogX`）
4. **展開** をクリック

#### macOS の場合

1. ZIPファイルをダブルクリックして展開
2. 展開されたフォルダを任意の場所に移動（例：`/Applications/JumboDogX`）

**macOS セキュリティ警告について**:

初回起動時に「開発元を確認できないため開けません」と表示される場合：

1. **システム設定** > **プライバシーとセキュリティ** を開く
2. 下部の **このまま開く** をクリック
3. または、ターミナルで以下を実行：
   ```bash
   sudo xattr -r -d com.apple.quarantine /path/to/jumbodogx
   ```

![macOS セキュリティ警告](images/macos-security.png)
*macOSのセキュリティ警告と対処方法*

#### Linux の場合

```bash
# ZIPファイルを展開
unzip jumbodogx-linux-x64.zip -d ~/jumbodogx

# 実行権限を付与
chmod +x ~/jumbodogx/Jdx.WebUI
```

### 3-2. ファイル構成の確認

展開後、以下のようなファイル構成になります：

```
jumbodogx/
├── Jdx.WebUI                    # 実行ファイル（Linux/macOS）
├── Jdx.WebUI.exe                # 実行ファイル（Windows）
├── appsettings.json             # 設定ファイル
├── wwwroot/                     # Web UI 静的ファイル
├── samples/                     # サンプルファイル
│   └── www/                     # サンプルWebコンテンツ
└── logs/                        # ログファイル（初回起動時に作成）
```

## ステップ4: 初回起動

### 4-1. アプリケーションを起動

#### Windows の場合

1. エクスプローラーで展開したフォルダを開く
2. `Jdx.WebUI.exe` をダブルクリック
3. または、コマンドプロンプト/PowerShellで実行：
   ```powershell
   cd C:\JumboDogX
   .\Jdx.WebUI.exe
   ```

#### macOS / Linux の場合

ターミナルで実行：

```bash
cd /path/to/jumbodogx
./Jdx.WebUI
```

### 4-2. 起動確認

ターミナルまたはコンソールに以下のようなメッセージが表示されれば成功です：

```
Now listening on: http://localhost:5001
Application started. Press Ctrl+C to shut down.
```

![起動メッセージ](images/startup-console.png)
*JumboDogX起動時のコンソール出力*

### 4-3. Web管理画面にアクセス

1. ブラウザで以下のURLを開きます：
   ```
   http://localhost:5001
   ```

2. JumboDogXのダッシュボード画面が表示されます

![ダッシュボード](images/dashboard-initial.png)
*初回起動時のJumboDogXダッシュボード*

**成功！** これでJumboDogXが起動し、Web管理画面にアクセスできるようになりました。

## ステップ5: 基本設定（オプション）

### 5-1. ポート番号の変更

デフォルトのポート番号（5001）を変更したい場合：

1. JumboDogXを終了（ターミナルで `Ctrl+C`）
2. `appsettings.json` を編集：

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5001"  // ポート番号を変更
      }
    }
  }
}
```

3. JumboDogXを再起動

### 5-2. ネットワーク経由でアクセス

他のコンピューターからWeb管理画面にアクセスしたい場合：

1. JumboDogXを終了
2. `appsettings.json` を編集：

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5001"  // すべてのIPアドレスでリッスン
      }
    }
  }
}
```

3. JumboDogXを再起動
4. ブラウザで `http://<サーバーのIPアドレス>:5001` にアクセス

**セキュリティ警告**: ネットワーク経由でアクセスする場合は、必ずファイアウォールで保護されたネットワーク内で使用してください。

![ネットワークアクセス設定](images/network-access.png)
*ネットワーク経由でのアクセス設定*

## よくある問題と解決方法

### .NET が認識されない

**エラーメッセージ**:
```
'dotnet' は、内部コマンドまたは外部コマンド、操作可能なプログラムまたはバッチ ファイルとして認識されていません。
```

**原因**: .NET がインストールされていないか、パスが通っていない

**解決策**:
1. ステップ1の手順に従って.NET 9をインストール
2. ターミナル/コマンドプロンプトを再起動
3. `dotnet --version` で確認

### ポートが使用中

**エラーメッセージ**:
```
Failed to bind to address http://localhost:5001: address already in use.
```

**原因**: ポート5001が別のアプリケーションで使用されている

**解決策**:
1. 使用中のアプリケーションを終了
2. または、ステップ5-1の手順で別のポート番号に変更

#### ポート使用状況の確認

**Windows**:
```powershell
netstat -ano | findstr :5001
```

**macOS / Linux**:
```bash
lsof -i :5001
```

### "Permission denied" エラー（Linux/macOS）

**エラーメッセージ**:
```
Permission denied
```

**原因**: 実行権限が付与されていない

**解決策**:
```bash
chmod +x /path/to/jumbodogx/Jdx.WebUI
```

### ブラウザでアクセスできない

**症状**: ブラウザで `http://localhost:5001` にアクセスしても表示されない

**確認事項**:
1. JumboDogXが起動しているか確認
2. ターミナルに "Now listening on" メッセージが表示されているか確認
3. ファイアウォールでポートがブロックされていないか確認
4. 別のブラウザで試す

**Windows ファイアウォールの確認**:
1. **Windows セキュリティ** を開く
2. **ファイアウォールとネットワーク保護** をクリック
3. **アプリケーションのファイアウォール通過を許可** をクリック
4. JumboDogXが許可されているか確認

### macOS でアプリケーションが起動しない

**症状**: "開発元を確認できないため開けません" と表示される

**解決策**:

**方法1**: システム設定から許可
1. **システム設定** > **プライバシーとセキュリティ** を開く
2. 下部の **このまま開く** をクリック

**方法2**: ターミナルから実行
```bash
sudo xattr -r -d com.apple.quarantine /path/to/jumbodogx
```

## 次のステップ

JumboDogXのインストールが完了しました。次は各サーバーの設定を行いましょう：

### サーバー別クイックスタートガイド

- [HTTPサーバー クイックスタート](../http/getting-started.md) - Webサイトを公開
- [DNSサーバー クイックスタート](../dns/getting-started.md) - ローカルDNSサーバーを構築
- [FTPサーバー クイックスタート](../ftp/getting-started.md) - ファイル転送サーバーを構築
- [SMTPサーバー クイックスタート](../smtp/getting-started.md) - メール送信サーバーを構築
- [POP3サーバー クイックスタート](../pop3/getting-started.md) - メール受信サーバーを構築
- [DHCPサーバー クイックスタート](../dhcp/getting-started.md) - IPアドレス割り当てサーバーを構築
- [TFTPサーバー クイックスタート](../tftp/getting-started.md) - シンプルなファイル転送サーバーを構築
- [Proxyサーバー クイックスタート](../proxy/getting-started.md) - HTTPプロキシサーバーを構築

### 共通設定ガイド

- [ACL設定ガイド](acl-configuration.md) - アクセス制御の設定方法
- [ロギングガイド](logging.md) - ログの確認と設定
- [セキュリティベストプラクティス](security-best-practices.md) - セキュアな運用方法

## アンインストール

JumboDogXをアンインストールする場合：

### Windows の場合

1. JumboDogXを終了
2. インストールフォルダを削除
3. 必要に応じて、.NET Runtimeをアンインストール：
   - **設定** > **アプリ** > **インストールされているアプリ** から .NET を検索してアンインストール

### macOS / Linux の場合

```bash
# インストールフォルダを削除
rm -rf /path/to/jumbodogx

# 必要に応じて、ログファイルを削除
rm -rf ~/Library/Logs/JumboDogX  # macOS
rm -rf ~/.local/share/JumboDogX  # Linux
```

## まとめ

このガイドでは、以下の手順を完了しました：

✓ .NET 9 Runtimeのインストール
✓ JumboDogXのダウンロードと展開
✓ JumboDogXの起動とWeb管理画面へのアクセス
✓ 基本設定（オプション）

これで、JumboDogXを使用する準備が整いました！

各サーバーの詳細な設定方法は、それぞれのクイックスタートガイドを参照してください。
