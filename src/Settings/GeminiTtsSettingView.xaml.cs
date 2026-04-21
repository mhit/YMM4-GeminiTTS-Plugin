using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using YMM4.GeminiTTS.Plugin.Synthesis;

namespace YMM4.GeminiTTS.Plugin.Settings;

public partial class GeminiTtsSettingView : UserControl
{
    public GeminiTtsSettingView()
    {
        InitializeComponent();
    }

    void BrowseJsonButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not GeminiTtsSettings settings) return;

        var dlg = new OpenFileDialog
        {
            Title = "Service Account JSON を選択",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            CheckFileExists = true,
        };
        if (dlg.ShowDialog() == true)
            settings.ServiceAccountJsonPath = dlg.FileName;
    }

    async void TestSynthesisButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not GeminiTtsSettings settings) return;

        var button = (Button)sender;
        button.IsEnabled = false;
        TestResultText.Text = "合成中...";
        try
        {
            var timeout = settings.RequestTimeoutSeconds > 0
                ? TimeSpan.FromSeconds(settings.RequestTimeoutSeconds)
                : (TimeSpan?)null;
            var client = GeminiTtsClient.GetOrCreate(
                settings.ServiceAccountJsonPath, settings.Endpoint, timeout);
            var outputPath = Path.Combine(Path.GetTempPath(), "ymm4-gemini-tts-test.wav");

            var effects = string.IsNullOrWhiteSpace(settings.EffectsProfileId)
                ? null
                : settings.EffectsProfileId.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            await client.SynthesizeToFileAsync(
                new SynthesisRequest
                {
                    Text = "テスト音声です。",
                    VoiceName = settings.DefaultVoiceName,
                    LanguageCode = settings.LanguageCode,
                    SampleRateHertz = settings.SampleRateHertz,
                    StylePrompt = settings.DefaultStylePrompt,
                    EffectsProfileIds = effects,
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
        }
    }

    async void ListVoicesButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not GeminiTtsSettings settings) return;

        var button = (Button)sender;
        button.IsEnabled = false;
        TestResultText.Text = "取得中...";
        try
        {
            var timeout = settings.RequestTimeoutSeconds > 0
                ? TimeSpan.FromSeconds(settings.RequestTimeoutSeconds)
                : (TimeSpan?)null;
            var client = GeminiTtsClient.GetOrCreate(
                settings.ServiceAccountJsonPath, settings.Endpoint, timeout);
            var response = await client.ListGeminiVoicesAsync(settings.LanguageCode ?? string.Empty);
            var geminiVoices = response.Voices
                .Where(v => v.Name != null)
                .OrderBy(v => v.Name, StringComparer.Ordinal)
                .Select(v => $"{v.Name}  [{string.Join(",", v.LanguageCodes)}]  {v.SsmlGender}")
                .ToArray();
            TestResultText.Text = geminiVoices.Length == 0
                ? "Voice が見つかりませんでした。"
                : $"{geminiVoices.Length} 件取得:\n" + string.Join("\n", geminiVoices);
        }
        catch (Exception ex)
        {
            TestResultText.Text = $"失敗: {ex.Message}";
        }
        finally
        {
            button.IsEnabled = true;
        }
    }
}
