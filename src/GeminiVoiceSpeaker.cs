using System;
using System.Threading;
using System.Threading.Tasks;
using YukkuriMovieMaker.Plugin.Voice;
using YMM4.GeminiTTS.Plugin.Settings;
using YMM4.GeminiTTS.Plugin.Synthesis;
using YMM4.GeminiTTS.Plugin.UI;
using YMM4.GeminiTTS.Plugin.Voices;

namespace YMM4.GeminiTTS.Plugin;

/// <summary>
/// One YMM4 speaker entry = one Gemini voice (Kore, Zephyr, ...).
/// Created per <see cref="GeminiVoicePlugin.Voices"/> enumeration — cheap to construct.
/// </summary>
internal sealed class GeminiVoiceSpeaker : IVoiceSpeaker
{
    public const string ApiId = "GeminiTTS";

    // Serialize concurrent synthesis across all voices so bulk script imports
    // don't blow past the Cloud TTS per-project quota.
    static readonly SemaphoreSlim semaphore = new(1);


    readonly GeminiVoice voice;

    public GeminiVoiceSpeaker(GeminiVoice voice) => this.voice = voice;

    public string EngineName => "Geminiナレーター";
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
        // YMM4 only calls this when Format == Custom; we're Text.
        throw new NotImplementedException();

    record struct StyleResolution(string Prompt, string? AudioTag);

    static StyleResolution ResolveStyle(GeminiVoiceParameter? param, string defaultStylePrompt)
    {
        if (param is null) return new(defaultStylePrompt, null);

        // カスタム指示が入力されている場合は最優先（Audio Tagなし）
        if (!string.IsNullOrWhiteSpace(param.CustomInstruction))
            return new(param.CustomInstruction, null);

        // 感情・口調: Audio Tag で瞬間的な状態を制御
        // 用途別: Director's Notes で演技スタイル全体を制御
        return param.VoiceTone switch
        {
            // ── 感情・口調 ─────────────────────────────────────
            VoiceTone.Professional =>
                new("Calm, professional narrator. Clear enunciation, natural pacing.", null),
            VoiceTone.Cheerful =>
                new("Bright and energetic delivery.", "[excited]"),
            VoiceTone.Whisper =>
                new("Hushed and intimate. Very quiet.", "[whispers]"),
            VoiceTone.Sad =>
                new("Melancholy and subdued. Slow, heavy pacing.", "[crying]"),
            VoiceTone.Angry =>
                new("Intense and forceful delivery.", "[shouting]"),
            VoiceTone.Laughing =>
                new("Cheerful and amused.", "[laughs]"),
            VoiceTone.Excited =>
                new("Highly excited and enthusiastic.", "[excited]"),
            VoiceTone.Serious =>
                new("Serious and measured delivery.", "[serious]"),
            VoiceTone.Childlike =>
                new("Innocent and curious, childlike energy.", "[curious]"),

            // ── 用途別プリセット ───────────────────────────────
            VoiceTone.AnimeCharacter =>
                new("Director's Notes: Expressive anime voice acting style. Emotive and clear, with the energy of a professional Japanese anime performance. Allow personality to shine through.", null),
            VoiceTone.GameDialogue =>
                new("Director's Notes: Video game character dialogue. Clear and impactful delivery with distinct personality. Crisp articulation suitable for in-game playback.", null),
            VoiceTone.VirtualAvatar =>
                new("Director's Notes: Friendly virtual avatar or VTuber companion voice. Warm, approachable, and conversational. Natural and relatable.", null),
            VoiceTone.AudioDrama =>
                new("Director's Notes: Audio drama performance. Theatrical and immersive, with emotional depth and clear enunciation. Think radio drama storytelling.", null),
            VoiceTone.ExpressiveNarration =>
                new("Director's Notes: Expressive storytelling narration. Rich vocal variety, dynamic pacing, and genuine emotional engagement. Paint pictures with your voice.", null),
            VoiceTone.BrandVoice =>
                new("Director's Notes: Professional brand voice. Polished, trustworthy, and confident yet approachable. Consistent and authoritative.", null),

            // ── カスタム / デフォルト ──────────────────────────
            VoiceTone.Custom => new(defaultStylePrompt, null),
            _                => new(defaultStylePrompt, null),
        };
    }

    public async Task<IVoicePronounce?> CreateVoiceAsync(
        string text, IVoicePronounce? pronounce, IVoiceParameter? parameter, string filePath)
    {
        var settings = GeminiTtsSettings.Default;
        var param = parameter as GeminiVoiceParameter;
        var (stylePrompt, audioTag) = ResolveStyle(param, settings.DefaultStylePrompt);

        var rewrittenText = YomiDictionary.Apply(text, settings.UserDictionary);

        var timeout = settings.RequestTimeoutSeconds > 0
            ? TimeSpan.FromSeconds(settings.RequestTimeoutSeconds)
            : (TimeSpan?)null;

        SynthesisOverlay.Increment(voice.DisplayName);

        await semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            var client = GeminiTtsClient.GetOrCreate(
                settings.ApiKey,
                settings.ModelName,
                timeout);

            await client.SynthesizeToFileAsync(
                new SynthesisRequest
                {
                    Text = rewrittenText,
                    VoiceName = voice.Name,
                    LanguageCode = string.IsNullOrWhiteSpace(settings.LanguageCode)
                        ? "ja-JP"
                        : settings.LanguageCode,
                    SampleRateHertz = 24000,
                    StylePrompt = stylePrompt,
                    AudioTag = audioTag,
                },
                filePath).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
            SynthesisOverlay.Decrement();
        }

        return null;
    }
}
