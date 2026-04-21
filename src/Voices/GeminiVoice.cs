namespace YMM4.GeminiTTS.Plugin.Voices;

/// <summary>Single Gemini TTS voice entry. Name is what the API expects; DisplayName is Japanese UI label.</summary>
public sealed record GeminiVoice(string Name, string DisplayName, string Character);
