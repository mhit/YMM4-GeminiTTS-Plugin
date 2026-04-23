using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Voice;
using YMM4.GeminiTTS.Plugin.Settings;
using YMM4.GeminiTTS.Plugin.UI;
using YMM4.GeminiTTS.Plugin.Updater;
using YMM4.GeminiTTS.Plugin.Voices;

namespace YMM4.GeminiTTS.Plugin;

public sealed class GeminiVoicePlugin : IVoicePlugin
{
    public GeminiVoicePlugin()
    {
        UpdateChecker.CheckOnce();

        // YMM4 のメインウィンドウが表示されてから Audio Tag パレットを自動表示
        Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            if (Application.Current?.MainWindow is Window w)
                w.Loaded += (_, _) => AudioTagPalette.EnsureVisible();
            else
                AudioTagPalette.EnsureVisible();
        });
    }

    public string Name => "Geminiナレーター";

    public IEnumerable<IVoiceSpeaker> Voices =>
        string.IsNullOrWhiteSpace(GeminiTtsSettings.Default.ApiKey)
            ? []
            : VoiceCatalog.All.Select(v => new GeminiVoiceSpeaker(v));

    public bool CanUpdateVoices => false;
    public bool IsVoicesCached => true;
    public Task UpdateVoicesAsync() => Task.CompletedTask;

    public PluginDetailsAttribute Details => new() { AuthorName = "mhit" };
}
