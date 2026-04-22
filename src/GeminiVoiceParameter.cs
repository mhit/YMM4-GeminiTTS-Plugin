using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4.GeminiTTS.Plugin;

/// <summary>
/// Per-utterance parameters shown in YMM4's voice parameter panel.
/// </summary>
internal class GeminiVoiceParameter : VoiceParameterBase
{
    VoiceTone voiceTone = VoiceTone.Default;
    string customInstruction = string.Empty;

    [Display(
        Name = "口調・感情",
        Description = "このセリフの話し方を選択。「設定に従う」はプラグイン設定のデフォルトスタイルを使用。")]
    [EnumComboBox(EnumComboBoxItemOrderRule.DisplayOrder)]
    [DefaultValue(VoiceTone.Default)]
    public VoiceTone VoiceTone
    {
        get => voiceTone;
        set => Set(ref voiceTone, value);
    }

    [Display(
        Name = "カスタム指示",
        Description = "「カスタム」選択時、またはプリセットに追加したい指示を入力。例: ゆっくりめに、少しためらいながら")]
    [TextEditor]
    [DefaultValue("")]
    public string CustomInstruction
    {
        get => customInstruction;
        set => Set(ref customInstruction, value ?? string.Empty);
    }
}
