using System;
using System.IO;
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
            var client = GeminiTtsClient.Create(settings.ServiceAccountJsonPath);
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
        }
    }
}
