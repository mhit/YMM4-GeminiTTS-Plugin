using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4.GeminiTTS.Plugin;

/// <summary>
/// Per-utterance parameters shown in YMM4's voice parameter panel.
/// Gemini TTS ignores numeric <c>pitch</c>/<c>speakingRate</c>, so this plugin
/// exposes only a free-form style override.
/// </summary>
internal class GeminiVoiceParameter : VoiceParameterBase
{
    string stylePrompt = string.Empty;

    [Display(
        Name = "Style Prompt",
        Description = "このセリフに前置する自然言語のスタイル指示 (例: 囁き声で、少しためらいながら)。空の場合はプラグイン設定のデフォルトが使われます。")]
    [DefaultValue("")]
    public string StylePrompt
    {
        get => stylePrompt;
        set => Set(ref stylePrompt, value ?? string.Empty);
    }
}
