# コンポーネント設計

## 1. レイアウトコンポーネント

### 1.1 MainLayout.razor

アプリケーション全体のレイアウト。

```razor
@inherits LayoutComponentBase

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>
    <main>
        @Body
    </main>
</div>
```

### 1.2 NavMenu.razor

サイドバーナビゲーション。

- ダッシュボードリンク
- 設定リンク
- ログリンク
- DNSレコードリンク

## 2. ページコンポーネント

### 2.1 Dashboard.razor

サーバーステータスダッシュボード。

**機能:**
- サーバー一覧表示
- ステータスインジケータ
- クイック起動/停止ボタン
- 統計情報

### 2.2 Logs.razor

ログビューア。

**機能:**
- ログエントリ一覧
- レベルフィルタボタン
- カテゴリセレクト
- テキスト検索
- 時刻フォーマット選択
- クリアボタン（確認ダイアログ付き）
- リサイズ可能なカラム
- IP Addressカラム（クライアントIPアドレスを表示）

**状態管理:**
```csharp
private List<LogEntry> logs = new();
private IEnumerable<LogEntry> filteredLogs;
private LogLevel? selectedLevel = null;
private string selectedCategory = "";
private string searchText = "";
private string selectedTimeFormat = "HH:mm:ss.fff";
private bool showClearConfirmDialog = false;
```

### 2.3 DnsRecords.razor

DNSレコード管理。

**機能:**
- レコード一覧
- レコード追加/編集/削除
- タイプ別フィルタ

### 2.4 Settings/Index.razor

設定メインページ。

## 3. 共通コンポーネント

### 3.1 確認ダイアログ

```razor
@if (showConfirmDialog)
{
    <div class="confirm-dialog-overlay" @onclick="HideDialog" @onkeydown="HandleKeyDown">
        <div class="confirm-dialog" @onclick:stopPropagation="true"
             role="dialog" aria-labelledby="dialog-title" aria-modal="true">
            <div class="confirm-dialog-header">
                <h5 id="dialog-title">@Title</h5>
            </div>
            <div class="confirm-dialog-body">
                <p>@Message</p>
            </div>
            <div class="confirm-dialog-footer">
                <button class="btn btn-dashboard btn-dashboard-outline" @onclick="OnCancel">
                    Cancel
                </button>
                <button class="btn btn-dashboard btn-dashboard-danger" @onclick="OnConfirm">
                    @ConfirmText
                </button>
            </div>
        </div>
    </div>
}
```

### 3.2 ステータスバッジ

```razor
<span class="status-badge @GetStatusClass(status)">
    @status
</span>
```

### 3.3 ログレベルバッジ

```razor
<span class="log-level-badge @GetLevelClass(level)">
    @level
</span>
```

## 4. スタイルガイド

### 4.1 ボタンクラス

| クラス | 用途 |
|--------|------|
| `btn-dashboard` | 基本ボタン |
| `btn-dashboard-outline` | アウトラインボタン |
| `btn-dashboard-secondary` | セカンダリボタン |
| `btn-dashboard-danger` | 危険アクション用 |

### 4.2 カードスタイル

```css
.card {
    background-color: white;
    border-radius: 8px;
    border: 1px solid var(--card-border);
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}
```

### 4.3 テーブルスタイル

```css
.table {
    table-layout: fixed;
    width: 100%;
}

.table thead {
    background-color: #F7F8FA;
}
```

## 5. アクセシビリティ

### 5.1 ARIA属性

- `role="dialog"` - ダイアログ
- `aria-labelledby` - タイトル参照
- `aria-modal="true"` - モーダル表示

### 5.2 キーボード操作

- Escapeキーでダイアログ閉じる
- Tabでフォーカス移動
- Enterで確定

### 5.3 フォーカス状態

```css
.btn:focus {
    outline: 2px solid var(--btn-primary);
    outline-offset: 2px;
}
```

## 6. レスポンシブ対応

### 6.1 ブレークポイント

```css
/* モバイル対応 */
.confirm-dialog {
    min-width: min(400px, 90vw);
    margin: 1rem;
}
```

### 6.2 サイドバー

モバイル時は折りたたみ可能。

## 更新履歴

- 2026-01-24: Server LogsページにIP Addressカラムを追加
