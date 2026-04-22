namespace YMM4.GeminiTTS.Plugin.Synthesis;

/// <summary>Input to <see cref="GeminiTtsClient"/>.</summary>
public sealed class SynthesisRequest
{
    public required string Text { get; init; }
    public required string VoiceName { get; init; }
    public string LanguageCode { get; init; } = "ja-JP";
    public int SampleRateHertz { get; init; } = 24000;

    /// <summary>
    /// Director's Notes として送るスタイル指示（例: "Speak calmly and professionally"）。
    /// null/空の場合はプロンプトなしでテキストのみ送信。
    /// </summary>
    public string? StylePrompt { get; init; }

    /// <summary>
    /// テキストの先頭に付加する Audio Tag（例: "[whispers]", "[excited]"）。
    /// Transcript 内にインラインで埋め込まれる。
    /// </summary>
    public string? AudioTag { get; init; }
}
