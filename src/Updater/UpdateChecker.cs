using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace YMM4.GeminiTTS.Plugin.Updater;

internal static class UpdateChecker
{
    const string ReleasesUrl =
        "https://api.github.com/repos/mhit/YMM4-GeminiTTS-Plugin/releases/latest";

    static readonly HttpClient Http = new();

    static UpdateChecker()
    {
        Http.DefaultRequestHeaders.UserAgent.ParseAdd("YMM4-GeminiTTS-Plugin");
    }

    /// <summary>
    /// Fires at most once when a newer version is detected on GitHub.
    /// Raised on a thread-pool thread — marshal to UI thread before touching controls.
    /// If a new version was already found before this event is subscribed to,
    /// check <see cref="LatestTag"/> directly.
    /// </summary>
    public static event Action<string>? UpdateAvailable;

    /// <summary>
    /// The latest GitHub tag found, or null if not yet checked / already up-to-date.
    /// </summary>
    public static string? LatestTag { get; private set; }

    /// <summary>
    /// Current assembly version (set by /p:Version= at build time).
    /// </summary>
    public static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 1, 0);

    static bool _checked;

    /// <summary>
    /// Fire-and-forget check. Safe to call multiple times — only runs once per process.
    /// </summary>
    public static void CheckOnce() =>
        Task.Run(CheckAsync);

    static async Task CheckAsync()
    {
        if (_checked) return;
        _checked = true;

        try
        {
            var rel = await Http.GetFromJsonAsync(ReleasesUrl, UpdateCheckerContext.Default.GithubRelease)
                .ConfigureAwait(false);

            if (rel?.TagName is not string tag) return;

            var tagVer = tag.TrimStart('v');
            if (!Version.TryParse(tagVer, out var latest)) return;
            if (latest <= CurrentVersion) return;

            LatestTag = tag;
            UpdateAvailable?.Invoke(tag);
        }
        catch
        {
            // ネットワーク不可・レート制限など — 無視してプラグインの動作を妨げない
        }
    }
}

internal sealed class GithubRelease
{
    [JsonPropertyName("tag_name")]
    public string? TagName { get; set; }
}

[JsonSerializable(typeof(GithubRelease))]
internal sealed partial class UpdateCheckerContext : JsonSerializerContext { }
