using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using YMM4.GeminiTTS.Plugin.Synthesis;
using YMM4.GeminiTTS.Plugin.Voices;

namespace YMM4.GeminiTTS.Plugin.Settings;

public partial class GeminiTtsSettingView : UserControl
{
    static readonly string[] LanguageCodes =
    [
        "ja-JP", "en-US", "en-GB", "zh-CN", "zh-TW",
        "ko-KR", "fr-FR", "de-DE", "es-ES", "pt-BR", "it-IT",
    ];

    static readonly string[] Models =
    [
        "gemini-2.5-flash-preview-tts",
        "gemini-2.5-pro-preview-tts",
        "gemini-3.1-flash-tts-preview",
    ];

    public GeminiTtsSettingView()
    {
        InitializeComponent();

        LanguageComboBox.ItemsSource = LanguageCodes;
        VoiceComboBox.ItemsSource = VoiceCatalog.All;
        ModelComboBox.ItemsSource = Models;
    }

    void BulkRegisterButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Gemini の全ボイス（30件）をキャラクター設定に一括追加します。\n" +
            "設定を反映するため YMM4 を自動で再起動します。\n\n" +
            "未保存の作業がある場合は先にプロジェクトを保存してください。\n\n続けますか？",
            "キャラクター一括登録",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.OK) return;

        try
        {
            var (added, skipped, _) = CharacterBulkRegistrar.Register();

            // YMM4 を再起動して設定を反映
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (exePath != null)
                Process.Start(exePath);

            // 強制終了でシャットダウン保存をスキップ
            Process.GetCurrentProcess().Kill();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"失敗: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    async void TestSynthesisButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not GeminiTtsSettings settings) return;

        var button = (Button)sender;
        button.IsEnabled = false;
        TestResultText.Text = "";
        TestProgressBar.Visibility = System.Windows.Visibility.Visible;
        TestProgressLabel.Visibility = System.Windows.Visibility.Visible;
        try
        {
            var timeout = settings.RequestTimeoutSeconds > 0
                ? TimeSpan.FromSeconds(settings.RequestTimeoutSeconds)
                : (TimeSpan?)null;
            var client = GeminiTtsClient.GetOrCreate(
                settings.ApiKey,
                settings.ModelName,
                timeout);
            var outputPath = Path.Combine(Path.GetTempPath(), "ymm4-gemini-tts-test.wav");

            await client.SynthesizeToFileAsync(
                new SynthesisRequest
                {
                    Text = "テスト音声です。",
                    VoiceName = settings.DefaultVoiceName,
                    LanguageCode = settings.LanguageCode,
                    SampleRateHertz = settings.SampleRateHertz,
                    StylePrompt = settings.DefaultStylePrompt,
                },
                outputPath);
            TestResultText.Text = $"成功: {outputPath}";
        }
        catch (Exception ex)
        {
            TestResultText.Text = $"失敗: {ex.Message}";
        }
        finally
        {
            button.IsEnabled = true;
            TestProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            TestProgressLabel.Visibility = System.Windows.Visibility.Collapsed;
        }
    }

}
