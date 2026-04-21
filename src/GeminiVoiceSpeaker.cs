using System.Threading;
using System.Threading.Tasks;
using YukkuriMovieMaker.Plugin.Voice;
using YMM4.GeminiTTS.Plugin.Settings;
using YMM4.GeminiTTS.Plugin.Synthesis;
using YMM4.GeminiTTS.Plugin.Voices;

namespace YMM4.GeminiTTS.Plugin;

/// <summary>
/// One YMM4 speaker entry = one Gemini voice (Kore, Zephyr, ...).
/// Created per <see cref="GeminiVoicePlugin.Voices"/> enumeration — cheap to construct.
/// </summary>
internal sealed class GeminiVoiceSpeaker : IVoiceSpeaker
{
    public const string ApiId = "GeminiTTS";

    // Serialize concurrent synthesis across all voices to avoid hammering
    // Cloud TTS quotas during bulk script imports.
    static readonly SemaphoreSlim semaphore = new(1);

    readonly GeminiVoice voice;

    public GeminiVoiceSpeaker(GeminiVoice voice) => this.voice = voice;

    public string EngineName => "Gemini TTS";
    public string SpeakerName => voice.DisplayName;
    public string API => ApiId;
    public string ID => voice.Name;

    /// <summary>Paid cloud API → YMM4 should cache generated audio on disk.</summary>
    public bool IsVoiceDataCachingRequired => true;

    public SupportedTextFormat Format => SupportedTextFormat.Text;
    public IVoiceLicense? License => null;
    public IVoiceResource? Resource => null;

    public bool IsMatch(string api, string id) => api == API && id == ID;

    public IVoiceParameter CreateVoiceParameter() => new GeminiVoiceParameter();

    public IVoiceParameter MigrateParameter(IVoiceParameter currentParameter) =>
        currentParameter is GeminiVoiceParameter ? currentParameter : CreateVoiceParameter();

    public Task<string> ConvertKanjiToYomiAsync(string text, IVoiceParameter voiceParameter) =>
        // Not used when Format == Text.
        Task.FromResult(text);

    public async Task<IVoicePronounce?> CreateVoiceAsync(
        string text, IVoicePronounce? pronounce, IVoiceParameter? parameter, string filePath)
    {
        var settings = GeminiTtsSettings.Default;

        var perUtterance = (parameter as GeminiVoiceParameter)?.StylePrompt;
        var stylePrompt = string.IsNullOrWhiteSpace(perUtterance)
            ? settings.DefaultStylePrompt
            : perUtterance;

        await semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var client = GeminiTtsClient.Create(settings.ServiceAccountJsonPath);
            await client.SynthesizeToFileAsync(
                new SynthesisRequest
                {
                    Text = text,
                    VoiceName = voice.Name,
                    LanguageCode = string.IsNullOrWhiteSpace(settings.LanguageCode)
                        ? "ja-JP"
                        : settings.LanguageCode,
                    SampleRateHertz = settings.SampleRateHertz > 0
                        ? settings.SampleRateHertz
                        : 24000,
                    StylePrompt = stylePrompt,
                },
                filePath).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }

        // No pronunciation-edit object to return.
        return null;
    }
}
