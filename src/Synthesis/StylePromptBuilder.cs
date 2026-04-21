using System;

namespace YMM4.GeminiTTS.Plugin.Synthesis;

/// <summary>
/// Gemini TTS has no SSML; tone and delivery are controlled by prepending a
/// natural-language directive to the text. This helper assembles that combined
/// prompt in the format <c>{style}: {text}</c>.
/// </summary>
public static class StylePromptBuilder
{
    public static string Build(string text, string? stylePrompt)
    {
        if (string.IsNullOrWhiteSpace(stylePrompt))
            return text;

        var style = stylePrompt.Trim();
        if (style.EndsWith(":", StringComparison.Ordinal) || style.EndsWith("：", StringComparison.Ordinal))
            return $"{style} {text}";
        return $"{style}: {text}";
    }
}
