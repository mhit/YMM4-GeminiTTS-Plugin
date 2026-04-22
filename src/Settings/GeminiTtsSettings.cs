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
    public override string Name => "Geminiナレーター";
    public override bool HasSettingView => true;
    public override object? SettingView => new GeminiTtsSettingView { DataContext = this };

    // ---- Credentials ----

    string apiKey = string.Empty;
    string modelName = "gemini-2.5-flash-preview-tts";
    int requestTimeoutSeconds = 60;

    /// <summary>
    /// Google AI Studio の API キー。aistudio.google.com → Get API key で取得。
    /// </summary>
    public string ApiKey
    {
        get => apiKey;
        set => Set(ref apiKey, value ?? string.Empty);
    }

    /// <summary>
    /// Gemini TTS モデル名。例: gemini-2.5-flash-preview-tts, gemini-2.5-pro-preview-tts
    /// </summary>
    public string ModelName
    {
        get => modelName;
        set => Set(ref modelName, string.IsNullOrWhiteSpace(value) ? "gemini-2.5-flash-preview-tts" : value);
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

    // ---- Prompt / 辞書 ----

    string defaultStylePrompt =
        "プロのナレーターとして、落ち着いた口調で、聞き取りやすく話してください";

    string userDictionary = string.Empty;

    public string DefaultStylePrompt
    {
        get => defaultStylePrompt;
        set => Set(ref defaultStylePrompt, value);
    }

    public string UserDictionary
    {
        get => userDictionary;
        set => Set(ref userDictionary, value ?? string.Empty);
    }

    public override void Initialize() { }
}
