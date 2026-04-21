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

    // ---- Persisted state ----

    string? serviceAccountJsonPath;
    string defaultVoiceName = VoiceCatalog.Default.Name;
    string languageCode = "ja-JP";
    int sampleRateHertz = 24000;
    string defaultStylePrompt =
        "プロのナレーターとして、落ち着いた口調で、聞き取りやすく話してください";

    public string? ServiceAccountJsonPath
    {
        get => serviceAccountJsonPath;
        set => Set(ref serviceAccountJsonPath, value);
    }

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

    public string DefaultStylePrompt
    {
        get => defaultStylePrompt;
        set => Set(ref defaultStylePrompt, value);
    }

    public override void Initialize() { }
}
