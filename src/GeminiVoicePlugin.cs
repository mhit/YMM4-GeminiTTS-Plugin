using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Voice;
using YMM4.GeminiTTS.Plugin.Voices;

namespace YMM4.GeminiTTS.Plugin;

/// <summary>
/// Entry point discovered by YMM4. Exposes the 30 Gemini 3.1 Flash TTS voices
/// as individual speakers in the character editor.
/// </summary>
public sealed class GeminiVoicePlugin : IVoicePlugin
{
    public string Name => "Gemini TTS";

    /// <summary>
    /// The voice catalog is hard-coded and preview-stable, so enumeration is
    /// instant and doesn't need to be cached.
    /// </summary>
    public IEnumerable<IVoiceSpeaker> Voices =>
        VoiceCatalog.All.Select(v => new GeminiVoiceSpeaker(v));

    public bool CanUpdateVoices => false;

    public bool IsVoicesCached => true;

    public Task UpdateVoicesAsync() => Task.CompletedTask;

    public PluginDetailsAttribute Details => new()
    {
        AuthorName = "mhit",
    };
}
