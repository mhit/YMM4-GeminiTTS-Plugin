namespace YMM4.GeminiTTS.Plugin.Synthesis;

/// <summary>Input to <see cref="GeminiTtsClient"/>.</summary>
public sealed class SynthesisRequest
{
    public required string Text { get; init; }
    public required string VoiceName { get; init; }
    public string LanguageCode { get; init; } = "ja-JP";
    public int SampleRateHertz { get; init; } = 24000;
    /// <summary>Optional natural-language style directive prepended to <see cref="Text"/>.</summary>
    public string? StylePrompt { get; init; }
}
