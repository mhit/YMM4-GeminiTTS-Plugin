using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Voice;
using YukkuriMovieMaker.Plugin.Tool;
using YMM4.GeminiTTS.Plugin.Settings;
using YMM4.GeminiTTS.Plugin.UI;
using YMM4.GeminiTTS.Plugin.Updater;
using YMM4.GeminiTTS.Plugin.Voices;

namespace YMM4.GeminiTTS.Plugin;

public sealed class GeminiVoicePlugin : IVoicePlugin, IToolPlugin
{
    public GeminiVoicePlugin()
    {
        UpdateChecker.CheckOnce();
    }

    // ── IVoicePlugin ──────────────────────────────────────────────────────

    public string Name => "Geminiナレーター";

    public IEnumerable<IVoiceSpeaker> Voices =>
        string.IsNullOrWhiteSpace(GeminiTtsSettings.Default.ApiKey)
            ? []
            : VoiceCatalog.All.Select(v => new GeminiVoiceSpeaker(v));

    public bool CanUpdateVoices => false;
    public bool IsVoicesCached => true;
    public Task UpdateVoicesAsync() => Task.CompletedTask;

    public PluginDetailsAttribute Details => new() { AuthorName = "mhit" };

    // ── IToolPlugin ───────────────────────────────────────────────────────

    public IEnumerable<ToolBarGroup> GetToolBarGroups()
    {
        var toggleCmd = new RelayCommand(() => AudioTagPalette.Toggle());

        return
        [
            new ToolBarGroup
            {
                Items =
                [
                    new ToolBarItem
                    {
                        Name = "🏷 Audio Tag",
                        ToolTip = "Audio Tag 挿入パレットを表示/非表示",
                        Command = toggleCmd,
                    },
                ],
            },
        ];
    }
}

/// <summary>シンプルな ICommand 実装。</summary>
file sealed class RelayCommand(Action execute) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? _) => true;
    public void Execute(object? _) => execute();
}
