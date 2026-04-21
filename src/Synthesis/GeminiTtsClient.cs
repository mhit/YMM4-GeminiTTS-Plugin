using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;
using Google.Cloud.TextToSpeech.V1;

namespace YMM4.GeminiTTS.Plugin.Synthesis;

/// <summary>
/// Cached, thread-safe wrapper around <see cref="TextToSpeechClient"/> targeting
/// the Gemini 3.1 Flash TTS preview model.
/// </summary>
/// <remarks>
/// <para>
/// Gemini TTS does not accept SSML and largely ignores <c>pitch</c>/<c>speakingRate</c>
/// — style is delivered via <see cref="SynthesisInput.Prompt"/> plus inline
/// audio tags inside <see cref="SynthesisInput.Text"/>.
/// </para>
/// <para>
/// Clients are keyed by (endpoint, credentials path). YMM4 can fire many
/// synthesize calls in quick succession when importing a script, so we avoid
/// rebuilding the gRPC channel / parsing the service-account JSON per call.
/// </para>
/// </remarks>
public sealed class GeminiTtsClient
{
    public const string ModelName = "gemini-3.1-flash-tts-preview";

    static readonly ConcurrentDictionary<string, TextToSpeechClient> clients = new();

    readonly TextToSpeechClient client;
    readonly TimeSpan? requestTimeout;

    GeminiTtsClient(TextToSpeechClient client, TimeSpan? requestTimeout)
    {
        this.client = client;
        this.requestTimeout = requestTimeout;
    }

    /// <summary>
    /// Returns a cached client for the given configuration.
    /// </summary>
    /// <param name="serviceAccountJsonPath">
    /// Path to a Google service-account JSON key. Pass null/empty to fall back
    /// to Application Default Credentials (ADC).
    /// </param>
    /// <param name="endpoint">
    /// Override host, e.g. <c>"eu-texttospeech.googleapis.com:443"</c>. Null or
    /// empty uses the global default.
    /// </param>
    /// <param name="requestTimeout">
    /// Per-request deadline. Null means the SDK default.
    /// </param>
    public static GeminiTtsClient GetOrCreate(
        string? serviceAccountJsonPath,
        string? endpoint = null,
        TimeSpan? requestTimeout = null)
    {
        var normalizedPath = string.IsNullOrWhiteSpace(serviceAccountJsonPath) ? "" : serviceAccountJsonPath;
        var normalizedEndpoint = string.IsNullOrWhiteSpace(endpoint) ? "" : endpoint;
        var cacheKey = $"{normalizedEndpoint}|{normalizedPath}";

        var tts = clients.GetOrAdd(cacheKey, _ => BuildClient(normalizedPath, normalizedEndpoint));
        return new GeminiTtsClient(tts, requestTimeout);
    }

    static TextToSpeechClient BuildClient(string credentialsPath, string endpoint)
    {
        var builder = new TextToSpeechClientBuilder();

        if (!string.IsNullOrEmpty(credentialsPath))
        {
            if (!File.Exists(credentialsPath))
                throw new FileNotFoundException(
                    $"Service account JSON not found: {credentialsPath}",
                    credentialsPath);
            builder.CredentialsPath = credentialsPath;
        }

        if (!string.IsNullOrEmpty(endpoint))
            builder.Endpoint = endpoint;

        return builder.Build();
    }

    /// <summary>Returns raw LINEAR16 WAV bytes (with RIFF header) from Cloud TTS.</summary>
    public async Task<byte[]> SynthesizeAsync(SynthesisRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new SynthesisInput { Text = request.Text };
        if (!string.IsNullOrWhiteSpace(request.StylePrompt))
            input.Prompt = request.StylePrompt;

        var audio = new AudioConfig
        {
            AudioEncoding = AudioEncoding.Linear16,
            SampleRateHertz = request.SampleRateHertz,
        };
        if (request.EffectsProfileIds is { Count: > 0 } effects)
        {
            foreach (var id in effects)
                if (!string.IsNullOrWhiteSpace(id))
                    audio.EffectsProfileId.Add(id);
        }

        // WithTimeout / WithCancellationToken are extension methods on CallSettings
        // (Google.Api.Gax.Grpc.CallSettingsExtensions) that tolerate a null receiver.
        CallSettings? callSettings = null;
        if (requestTimeout is { } t)
            callSettings = callSettings.WithTimeout(t);
        callSettings = callSettings.WithCancellationToken(ct);

        var response = await client.SynthesizeSpeechAsync(
            new SynthesizeSpeechRequest
            {
                Input = input,
                Voice = new VoiceSelectionParams
                {
                    LanguageCode = request.LanguageCode,
                    Name = request.VoiceName,
                    ModelName = ModelName,
                },
                AudioConfig = audio,
            },
            callSettings).ConfigureAwait(false);

        return response.AudioContent.ToByteArray();
    }

    /// <summary>
    /// Convenience overload that writes the synthesized audio to <paramref name="outputWavPath"/>.
    /// </summary>
    public async Task SynthesizeToFileAsync(
        SynthesisRequest request, string outputWavPath, CancellationToken ct = default)
    {
        var bytes = await SynthesizeAsync(request, ct).ConfigureAwait(false);
        var dir = Path.GetDirectoryName(outputWavPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        await File.WriteAllBytesAsync(outputWavPath, bytes, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists every Gemini-TTS voice the API reports for <paramref name="languageCode"/>.
    /// Used by the settings "validate" UI — lets the user sanity-check that the
    /// service-account key actually reaches Cloud TTS and that the voices we
    /// bake into the catalog still exist.
    /// </summary>
    public async Task<ListVoicesResponse> ListGeminiVoicesAsync(
        string languageCode = "", CancellationToken ct = default)
    {
        return await client.ListVoicesAsync(
            new ListVoicesRequest { LanguageCode = languageCode ?? string.Empty },
            CallSettings.FromCancellationToken(ct)).ConfigureAwait(false);
    }
}
