namespace Jdx.Core.Settings;

/// <summary>
/// 設定サービスのインターフェース
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// 現在の設定を取得する
    /// </summary>
    ApplicationSettings GetSettings();

    /// <summary>
    /// 設定を保存する
    /// </summary>
    Task SaveSettingsAsync(ApplicationSettings settings);

    /// <summary>
    /// デフォルト設定を取得する
    /// </summary>
    ApplicationSettings GetDefaultSettings();

    /// <summary>
    /// 設定をJSON形式でエクスポートする
    /// </summary>
    /// <returns>JSON形式の設定文字列</returns>
    Task<string> ExportSettingsAsync();

    /// <summary>
    /// JSON形式の設定をインポートする
    /// </summary>
    /// <param name="json">インポートする設定のJSON文字列</param>
    Task ImportSettingsAsync(string json);

    /// <summary>
    /// 設定が変更されたときに発生するイベント
    /// </summary>
    event EventHandler<ApplicationSettings>? SettingsChanged;
}
