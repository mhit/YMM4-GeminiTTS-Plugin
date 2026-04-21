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
    public static readonly IReadOnlyList<GeminiVoice> All = new ReadOnlyCollection<GeminiVoice>(new[]
    {
        new GeminiVoice("Zephyr",        "Zephyr (明るい)",            "Bright"),
        new GeminiVoice("Puck",          "Puck (元気)",                "Upbeat"),
        new GeminiVoice("Charon",        "Charon (情報的)",            "Informative"),
        new GeminiVoice("Kore",          "Kore (しっかり)",            "Firm"),
        new GeminiVoice("Fenrir",        "Fenrir (熱量)",              "Excitable"),
        new GeminiVoice("Leda",          "Leda (若々しい)",            "Youthful"),
        new GeminiVoice("Orus",          "Orus (重厚)",                "Firm"),
        new GeminiVoice("Aoede",         "Aoede (軽やか)",             "Breezy"),
        new GeminiVoice("Callirhoe",     "Callirhoe (穏やか)",         "Easy-going"),
        new GeminiVoice("Autonoe",       "Autonoe (明朗)",             "Bright"),
        new GeminiVoice("Enceladus",     "Enceladus (息感)",           "Breathy"),
        new GeminiVoice("Iapetus",       "Iapetus (澄んだ)",           "Clear"),
        new GeminiVoice("Umbriel",       "Umbriel (ゆったり)",         "Easy-going"),
        new GeminiVoice("Algieba",       "Algieba (滑らか)",           "Smooth"),
        new GeminiVoice("Despina",       "Despina (なめらか)",         "Smooth"),
        new GeminiVoice("Erinome",       "Erinome (クリア)",           "Clear"),
        new GeminiVoice("Algenib",       "Algenib (しゃがれ)",         "Gravelly"),
        new GeminiVoice("Rasalgethi",    "Rasalgethi (解説向け)",      "Informative"),
        new GeminiVoice("Laomedeia",     "Laomedeia (アップビート)",   "Upbeat"),
        new GeminiVoice("Achernar",      "Achernar (柔らか)",          "Soft"),
        new GeminiVoice("Alnilam",       "Alnilam (堅実)",             "Firm"),
        new GeminiVoice("Schedar",       "Schedar (均質)",             "Even"),
        new GeminiVoice("Gacrux",        "Gacrux (成熟)",              "Mature"),
        new GeminiVoice("Pulcherrima",   "Pulcherrima (前向き)",       "Forward"),
        new GeminiVoice("Achird",        "Achird (親しみ)",            "Friendly"),
        new GeminiVoice("Zubenelgenubi", "Zubenelgenubi (カジュアル)", "Casual"),
        new GeminiVoice("Vindemiatrix",  "Vindemiatrix (優しい)",      "Gentle"),
        new GeminiVoice("Sadachbia",     "Sadachbia (活発)",           "Lively"),
        new GeminiVoice("Sadaltager",    "Sadaltager (博識)",          "Knowledgeable"),
        new GeminiVoice("Sulafat",       "Sulafat (温かい)",           "Warm"),
    });

    public static GeminiVoice? FindByName(string name) =>
        All.FirstOrDefault(v => string.Equals(v.Name, name, System.StringComparison.OrdinalIgnoreCase));

    public static GeminiVoice Default => All[3]; // Kore
}
