namespace YMM4.GeminiTTS.Plugin.Synthesis;

/// <summary>Input to <see cref="GeminiTtsClient"/>.</summary>
public sealed class SynthesisRequest
{
    public required string Text { get; init; }
    public required string VoiceName { get; init; }
    public string LanguageCode { get; init; } = "ja-JP";
    public int SampleRateHertz { get; init; } = 24000;

    /// <summary>
    /// Optional natural-language system instruction (e.g. "speak in a calm, professional tone").
    /// Passed through <c>SynthesisInput.Prompt</c>, not concatenated into <see cref="Text"/>.
    /// </summary>
    public string? StylePrompt { get; init; }

    /// <summary>
    /// Optional Cloud TTS <c>effects_profile_id</c> values applied in order
    /// (e.g. <c>"headphone-class-device"</c>). Empty means no post-processing.
    /// </summary>
    public IReadOnlyList<string>? EffectsProfileIds { get; init; }
}
