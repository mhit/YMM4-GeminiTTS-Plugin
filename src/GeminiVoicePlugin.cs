using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Voice;
using YMM4.GeminiTTS.Plugin.Settings;
using YMM4.GeminiTTS.Plugin.UI;
using YMM4.GeminiTTS.Plugin.Updater;
using YMM4.GeminiTTS.Plugin.Voices;

namespace YMM4.GeminiTTS.Plugin;

public sealed class GeminiVoicePlugin : IVoicePlugin, IToolPlugin
{
    public GeminiVoicePlugin()
    {
        UpdateChecker.CheckOnce();
    }

    // ── IPlugin ───────────────────────────────────────────────────────────
    public string Name => "Geminiナレーター";
    public PluginDetailsAttribute Details => new() { AuthorName = "mhit" };

    // ── IVoicePlugin ──────────────────────────────────────────────────────
    public IEnumerable<IVoiceSpeaker> Voices =>
        string.IsNullOrWhiteSpace(GeminiTtsSettings.Default.ApiKey)
            ? []
            : VoiceCatalog.All.Select(v => new GeminiVoiceSpeaker(v));

    public bool CanUpdateVoices => false;
    public bool IsVoicesCached => true;
    public Task UpdateVoicesAsync() => Task.CompletedTask;

    // ── IToolPlugin ───────────────────────────────────────────────────────
    public Type ViewModelType => typeof(AudioTagToolViewModel);
    public Type ViewType => typeof(AudioTagToolView);
}
