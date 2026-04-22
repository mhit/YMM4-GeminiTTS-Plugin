using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace YMM4.GeminiTTS.Plugin.Voices;

/// <summary>
/// Hard-coded catalog of Gemini 3.1 Flash TTS voices.
/// Character descriptions follow the hints Google publishes in AI Studio.
/// </summary>
public static class VoiceCatalog
{
    // Gemini TTS は短縮形のボイス名 (prebuiltVoiceConfig.voiceName) を受け付ける
    public static readonly IReadOnlyList<GeminiVoice> All = new ReadOnlyCollection<GeminiVoice>(new[]
    {
        new GeminiVoice("Zephyr",        "Zephyr (明るい・女性)",            "Bright"),
        new GeminiVoice("Puck",          "Puck (元気・男性)",                "Upbeat"),
        new GeminiVoice("Charon",        "Charon (情報的・男性)",            "Informative"),
        new GeminiVoice("Kore",          "Kore (しっかり・女性)",            "Firm"),
        new GeminiVoice("Fenrir",        "Fenrir (熱量・男性)",              "Excitable"),
        new GeminiVoice("Leda",          "Leda (若々しい・女性)",            "Youthful"),
        new GeminiVoice("Orus",          "Orus (重厚・男性)",                "Firm"),
        new GeminiVoice("Aoede",         "Aoede (軽やか・女性)",             "Breezy"),
        new GeminiVoice("Callirrhoe",    "Callirrhoe (穏やか・女性)",        "Easy-going"),
        new GeminiVoice("Autonoe",       "Autonoe (明朗・女性)",             "Bright"),
        new GeminiVoice("Enceladus",     "Enceladus (息感・男性)",           "Breathy"),
        new GeminiVoice("Iapetus",       "Iapetus (澄んだ・男性)",           "Clear"),
        new GeminiVoice("Umbriel",       "Umbriel (ゆったり・男性)",         "Easy-going"),
        new GeminiVoice("Algieba",       "Algieba (滑らか・男性)",           "Smooth"),
        new GeminiVoice("Despina",       "Despina (なめらか・女性)",         "Smooth"),
        new GeminiVoice("Erinome",       "Erinome (クリア・女性)",           "Clear"),
        new GeminiVoice("Algenib",       "Algenib (しゃがれ・男性)",         "Gravelly"),
        new GeminiVoice("Rasalgethi",    "Rasalgethi (解説向け・男性)",      "Informative"),
        new GeminiVoice("Laomedeia",     "Laomedeia (アップビート・女性)",   "Upbeat"),
        new GeminiVoice("Achernar",      "Achernar (柔らか・女性)",          "Soft"),
        new GeminiVoice("Alnilam",       "Alnilam (堅実・男性)",             "Firm"),
        new GeminiVoice("Schedar",       "Schedar (均質・男性)",             "Even"),
        new GeminiVoice("Gacrux",        "Gacrux (成熟・女性)",              "Mature"),
        new GeminiVoice("Pulcherrima",   "Pulcherrima (前向き・女性)",       "Forward"),
        new GeminiVoice("Achird",        "Achird (親しみ・男性)",            "Friendly"),
        new GeminiVoice("Zubenelgenubi", "Zubenelgenubi (カジュアル・男性)", "Casual"),
        new GeminiVoice("Vindemiatrix",  "Vindemiatrix (優しい・女性)",      "Gentle"),
        new GeminiVoice("Sadachbia",     "Sadachbia (活発・男性)",           "Lively"),
        new GeminiVoice("Sadaltager",    "Sadaltager (博識・男性)",          "Knowledgeable"),
        new GeminiVoice("Sulafat",       "Sulafat (温かい・女性)",           "Warm"),
    });

    public static GeminiVoice? FindByName(string name) =>
        All.FirstOrDefault(v => string.Equals(v.Name, name, System.StringComparison.OrdinalIgnoreCase));

    public static GeminiVoice Default => All[3]; // Kore
}
