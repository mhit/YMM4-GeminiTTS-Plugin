using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Voice;

namespace YMM4.GeminiTTS.Plugin.UI;

/// <summary>
/// YMM4 ツールパネル用 ViewModel。Audio Tag 挿入ボタンを提供する。
/// </summary>
public sealed class AudioTagToolViewModel : IToolViewModel
{
    public string? Title => "Audio Tag";

    public bool CanSuspend => false;

    public event EventHandler<CreateNewToolViewRequestedEventArgs>? CreateNewToolViewRequested;
    public event PropertyChangedEventHandler? PropertyChanged;

    static readonly (string Label, string Tag, string ToolTip)[] Tags =
    [
        ("囁き",   "[whispers]",    "ひそひそ声・囁き声"),
        ("叫び",   "[shouting]",    "叫ぶ・大声"),
        ("興奮",   "[excited]",     "興奮・テンション高め"),
        ("笑い",   "[laughs]",      "笑いながら"),
        ("泣き",   "[crying]",      "泣きながら・悲しく"),
        ("真剣",   "[serious]",     "真剣・厳粛"),
        ("好奇心", "[curious]",     "不思議そう・好奇心旺盛"),
        ("─ 短",   "[short pause]", "短い間"),
        ("─ 中",   "[medium pause]","中程度の間"),
        ("─ 長",   "[long pause]",  "長い間"),
    ];

    public ICommand[] TagCommands { get; } =
        [.. System.Linq.Enumerable.Select(Tags, t =>
            new RelayCommand(t.Tag, () => InsertTag(t.Tag)))];

    public (string Label, string ToolTip)[] TagInfos { get; } =
        [.. System.Linq.Enumerable.Select(Tags, t => (t.Label, t.ToolTip))];

    static void InsertTag(string tag)
    {
        var focused = System.Windows.Input.Keyboard.FocusedElement
            as System.Windows.Controls.TextBox;
        if (focused == null) return;
        var idx = focused.SelectionStart;
        focused.Text = focused.Text.Insert(idx, tag);
        focused.SelectionStart = idx + tag.Length;
        focused.SelectionLength = 0;
    }

    public ToolState SaveState() => new();
    public void LoadState(ToolState _) { }
}

file sealed class RelayCommand(string tip, Action execute) : ICommand
{
    public string ToolTip => tip;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? _) => true;
    public void Execute(object? _) => execute();
}
