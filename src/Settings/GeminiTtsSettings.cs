using YukkuriMovieMaker.Plugin;
using YMM4.GeminiTTS.Plugin.Voices;

namespace YMM4.GeminiTTS.Plugin.Settings;

/// <summary>
/// Plugin-wide settings persisted by YMM4's <see cref="SettingsBase{T}"/>.
/// Accessed as a singleton via <c>GeminiTtsSettings.Default</c>.
/// </summary>
public class GeminiTtsSettings : SettingsBase<GeminiTtsSettings>
{
    public override SettingsCategory Category => SettingsCategory.Voice;
    public override string Name => "Gemini TTS";
    public override bool HasSettingView => true;
    public override object? SettingView => new GeminiTtsSettingView { DataContext = this };

    // ---- Credentials / transport ----

    string? serviceAccountJsonPath;
    string endpoint = string.Empty;
    int requestTimeoutSeconds = 60;

    public string? ServiceAccountJsonPath
    {
        get => serviceAccountJsonPath;
        set => Set(ref serviceAccountJsonPath, value);
    }

    /// <summary>
    /// Optional Cloud TTS host override (e.g. <c>us-texttospeech.googleapis.com:443</c>).
    /// Empty = global default host.
    /// </summary>
    public string Endpoint
    {
        get => endpoint;
        set => Set(ref endpoint, value ?? string.Empty);
    }

    public int RequestTimeoutSeconds
    {
        get => requestTimeoutSeconds;
        set => Set(ref requestTimeoutSeconds, value);
    }

    // ---- Voice selection defaults ----

    string defaultVoiceName = VoiceCatalog.Default.Name;
    string languageCode = "ja-JP";
    int sampleRateHertz = 24000;

    public string DefaultVoiceName
    {
        get => defaultVoiceName;
        set => Set(ref defaultVoiceName, value);
    }

    public string LanguageCode
    {
        get => languageCode;
        set => Set(ref languageCode, value);
    }

    public int SampleRateHertz
    {
        get => sampleRateHertz;
        set => Set(ref sampleRateHertz, value);
    }

    // ---- Prompt & audio post-processing ----

    string defaultStylePrompt =
        "プロのナレーターとして、落ち着いた口調で、聞き取りやすく話してください";

    /// <summary>
    /// Value for Cloud TTS <c>AudioConfig.effects_profile_id</c>. Empty = no
    /// post-processing. Multiple profiles (semicolon-separated) are applied
    /// left-to-right.
    /// </summary>
    string effectsProfileId = "headphone-class-device";

    /// <summary>
    /// Multi-line kanji→yomigana substitution table, one <c>phrase=yomi</c>
    /// entry per line. Gemini TTS ignores Cloud TTS' <c>custom_pronunciations</c>
    /// field, so the only reliable override is rewriting characters directly.
    /// </summary>
    string userDictionary = string.Empty;

    public string DefaultStylePrompt
    {
        get => defaultStylePrompt;
        set => Set(ref defaultStylePrompt, value);
    }

    public string EffectsProfileId
    {
        get => effectsProfileId;
        set => Set(ref effectsProfileId, value ?? string.Empty);
    }

    public string UserDictionary
    {
        get => userDictionary;
        set => Set(ref userDictionary, value ?? string.Empty);
    }

    public override void Initialize() { }
}
