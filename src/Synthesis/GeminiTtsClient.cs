using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace YMM4.GeminiTTS.Plugin.Synthesis;

/// <summary>
/// Gemini API (generativelanguage.googleapis.com) を REST API で叩く TTS クライアント。
/// <para>
/// エンドポイント: <c>https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}</c>
/// </para>
/// <para>
/// スタイルプロンプトは「指示：本文」形式でテキストの前に埋め込む。
/// </para>
/// </summary>
public sealed class GeminiTtsClient
{
    static readonly HttpClient HttpClient = new();
    static readonly ConcurrentDictionary<string, GeminiTtsClient> Cache = new();

    readonly string apiKey;
    readonly string model;
    readonly TimeSpan? requestTimeout;

    GeminiTtsClient(string apiKey, string model, TimeSpan? requestTimeout)
    {
        this.apiKey = apiKey;
        this.model = model;
        this.requestTimeout = requestTimeout;
    }

    public static GeminiTtsClient GetOrCreate(
        string? apiKey,
        string? model,
        TimeSpan? requestTimeout = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini API キーが設定されていません。プラグイン設定で API キーを入力してください。");

        var normalizedModel = string.IsNullOrWhiteSpace(model) ? "gemini-2.5-flash-preview-tts" : model;
        var cacheKey = $"{normalizedModel}|{apiKey}";

        return Cache.GetOrAdd(cacheKey, _ => new GeminiTtsClient(apiKey, normalizedModel, requestTimeout));
    }

    public async Task<byte[]> SynthesizeAsync(SynthesisRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var promptText = BuildPromptText(request.StylePrompt, request.AudioTag, request.Text);

        var body = new GenerateContentBody
        {
            Contents =
            [
                new Content { Parts = [new Part { Text = promptText }] }
            ],
            GenerationConfig = new GenerationConfig
            {
                ResponseModalities = ["AUDIO"],
                SpeechConfig = new SpeechConfig
                {
                    VoiceConfig = new VoiceConfig
                    {
                        PrebuiltVoiceConfig = new PrebuiltVoiceConfig
                        {
                            VoiceName = request.VoiceName,
                        }
                    }
                }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body, options: JsonOpts),
        };

        using var timeoutCts = requestTimeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(ct)
            : null;
        timeoutCts?.CancelAfter(requestTimeout!.Value);
        var effectiveCt = timeoutCts?.Token ?? ct;

        using var response = await HttpClient.SendAsync(httpRequest, effectiveCt).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(effectiveCt).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Gemini TTS API error ({(int)response.StatusCode} {response.StatusCode}): {responseBody}");

        var payload = JsonSerializer.Deserialize<GenerateContentResponse>(responseBody, JsonOpts);
        var inlineData = payload?.Candidates?[0]?.Content?.Parts?[0]?.InlineData
            ?? throw new InvalidOperationException(
                "Gemini TTS response did not contain audio data. Body: " + responseBody);

        var pcm = Convert.FromBase64String(inlineData.Data ?? string.Empty);
        var sampleRate = ParseSampleRate(inlineData.MimeType, request.SampleRateHertz);

        return WrapPcmAsWav(pcm, sampleRate);
    }

    public async Task SynthesizeToFileAsync(
        SynthesisRequest request, string outputWavPath, CancellationToken ct = default)
    {
        var bytes = await SynthesizeAsync(request, ct).ConfigureAwait(false);
        var dir = Path.GetDirectoryName(outputWavPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        await File.WriteAllBytesAsync(outputWavPath, bytes, ct).ConfigureAwait(false);
    }

    public static IReadOnlyList<string> ListKnownVoiceNames() =>
        [.. Voices.VoiceCatalog.All.Select(v => v.Name)];

    // 公式ガイドの推奨形式: Director's Notes → Transcript: [audio_tag] text
    static string BuildPromptText(string? stylePrompt, string? audioTag, string text)
    {
        var transcript = string.IsNullOrWhiteSpace(audioTag)
            ? text
            : $"{audioTag.Trim()} {text}";

        if (string.IsNullOrWhiteSpace(stylePrompt))
            return transcript;

        return $"Director's Notes: {stylePrompt.Trim()}\n\nTranscript: {transcript}";
    }

    static int ParseSampleRate(string? mimeType, int fallback)
    {
        if (!string.IsNullOrEmpty(mimeType))
        {
            var idx = mimeType.IndexOf("rate=", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0 && int.TryParse(mimeType[(idx + 5)..], out var rate))
                return rate;
        }
        // Gemini TTS は常に 24000 Hz PCM を返す。ユーザー設定値をフォールバックにすると
        // MIMEタイプ未取得時にWAVヘッダーが実際のPCMと乖離してピッチズレが起きるため固定。
        return 24000;
    }

    static byte[] WrapPcmAsWav(byte[] pcm, int sampleRate)
    {
        const int channels = 1;
        const int bitsPerSample = 16;
        var byteRate = sampleRate * channels * bitsPerSample / 8;
        var blockAlign = channels * bitsPerSample / 8;
        var dataSize = pcm.Length;
        var riffSize = 36 + dataSize;

        var wav = new byte[44 + dataSize];
        using var ms = new MemoryStream(wav);
        using var w = new BinaryWriter(ms);
        w.Write("RIFF"u8);
        w.Write(riffSize);
        w.Write("WAVE"u8);
        w.Write("fmt "u8);
        w.Write(16);
        w.Write((short)1);
        w.Write((short)channels);
        w.Write(sampleRate);
        w.Write(byteRate);
        w.Write((short)blockAlign);
        w.Write((short)bitsPerSample);
        w.Write("data"u8);
        w.Write(dataSize);
        w.Write(pcm);
        return wav;
    }

    static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // ---- DTO ----

    sealed class GenerateContentBody
    {
        public required Content[] Contents { get; init; }
        public GenerationConfig? GenerationConfig { get; init; }
    }

    sealed class Content
    {
        public required Part[] Parts { get; init; }
    }

    sealed class Part
    {
        public string? Text { get; init; }
        public InlineData? InlineData { get; init; }
    }

    sealed class InlineData
    {
        public string? MimeType { get; init; }
        public string? Data { get; init; }
    }

    sealed class GenerationConfig
    {
        public string[]? ResponseModalities { get; init; }
        public SpeechConfig? SpeechConfig { get; init; }
    }

    sealed class SpeechConfig
    {
        public VoiceConfig? VoiceConfig { get; init; }
    }

    sealed class VoiceConfig
    {
        public PrebuiltVoiceConfig? PrebuiltVoiceConfig { get; init; }
    }

    sealed class PrebuiltVoiceConfig
    {
        public string? VoiceName { get; init; }
    }

    sealed class GenerateContentResponse
    {
        public Candidate[]? Candidates { get; init; }
    }

    sealed class Candidate
    {
        public Content? Content { get; init; }
    }
}
