using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Voice;
using YMM4.GeminiTTS.Plugin.Settings;
using YMM4.GeminiTTS.Plugin.Voices;

namespace YMM4.GeminiTTS.Plugin;

/// <summary>
/// Entry point discovered by YMM4. Exposes the 30 Gemini 3.1 Flash TTS voices
/// as individual speakers in the character editor once the service account is
/// configured.
/// </summary>
public sealed class GeminiVoicePlugin : IVoicePlugin
{
    public string Name => "Geminiナレーター";

    /// <summary>
    /// Hides the voices from the character editor until the user has configured
    /// credentials. Matches the pattern used by the official community plugin.
    /// </summary>
    public IEnumerable<IVoiceSpeaker> Voices =>
        string.IsNullOrWhiteSpace(GeminiTtsSettings.Default.ApiKey)
            ? []
            : VoiceCatalog.All.Select(v => new GeminiVoiceSpeaker(v));

    public bool CanUpdateVoices => false;

    public bool IsVoicesCached => true;

    public Task UpdateVoicesAsync() => Task.CompletedTask;

    public PluginDetailsAttribute Details => new()
    {
        AuthorName = "mhit",
    };
}
