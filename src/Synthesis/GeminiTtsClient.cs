using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.TextToSpeech.V1;

namespace YMM4.GeminiTTS.Plugin.Synthesis;

/// <summary>
/// Thin wrapper around <see cref="TextToSpeechClient"/> that targets the
/// Gemini 3.1 Flash TTS preview model.
/// </summary>
/// <remarks>
/// Gemini TTS ignores <c>pitch</c>/<c>speakingRate</c> and does not accept SSML;
/// style is delivered via <see cref="SynthesisInput.Prompt"/>, a separate
/// system-instruction field supported by controllable voice models.
/// </remarks>
public sealed class GeminiTtsClient
{
    public const string ModelName = "gemini-3.1-flash-tts-preview";

    readonly TextToSpeechClient client;

    GeminiTtsClient(TextToSpeechClient client) => this.client = client;

    public static GeminiTtsClient Create(string? serviceAccountJsonPath)
    {
        var builder = new TextToSpeechClientBuilder();
        if (!string.IsNullOrWhiteSpace(serviceAccountJsonPath))
        {
            if (!File.Exists(serviceAccountJsonPath))
                throw new FileNotFoundException(
                    $"Service account JSON not found: {serviceAccountJsonPath}",
                    serviceAccountJsonPath);
            builder.CredentialsPath = serviceAccountJsonPath;
        }
        return new GeminiTtsClient(builder.Build());
    }

    /// <summary>Returns raw LINEAR16 WAV bytes (with RIFF header) from Cloud TTS.</summary>
    public async Task<byte[]> SynthesizeAsync(SynthesisRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new SynthesisInput { Text = request.Text };
        if (!string.IsNullOrWhiteSpace(request.StylePrompt))
            input.Prompt = request.StylePrompt;

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
                AudioConfig = new AudioConfig
                {
                    AudioEncoding = AudioEncoding.Linear16,
                    SampleRateHertz = request.SampleRateHertz,
                },
            },
            cancellationToken: ct).ConfigureAwait(false);

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
}
