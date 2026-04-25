using System;
using System.Windows.Input;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin;

namespace YMM4.GeminiTTS.Plugin.UI;

public sealed class AudioTagToolViewModel : Bindable, IToolViewModel
{
    public event EventHandler<CreateNewToolViewRequestedEventArgs>? CreateNewToolViewRequested;

    public string Title => "Audio Tag";

    static readonly TagItem[] Tags =
    [
        new("囁き",   "[whispers]",    "ひそひそ声"),
        new("叫び",   "[shouting]",    "叫ぶ・大声"),
        new("興奮",   "[excited]",     "興奮・テンション高め"),
        new("笑い",   "[laughs]",      "笑いながら"),
        new("泣き",   "[crying]",      "泣きながら"),
        new("真剣",   "[serious]",     "真剣・厳粛"),
        new("好奇心", "[curious]",     "不思議そう"),
        new("─ 短",   "[short pause]", "短い間"),
        new("─ 中",   "[medium pause]","中程度の間"),
        new("─ 長",   "[long pause]",  "長い間"),
    ];

    public TagItem[] TagItems => Tags;

    public ICommand InsertTagCommand { get; } = new ActionCommand(
        _ => true,
        tag =>
        {
            if (tag is not string t) return;
            var focused = System.Windows.Input.Keyboard.FocusedElement
                as System.Windows.Controls.TextBox;
            if (focused == null) return;
            var idx = focused.SelectionStart;
            focused.Text = focused.Text.Insert(idx, t);
            focused.SelectionStart = idx + t.Length;
            focused.SelectionLength = 0;
        });

    public ToolState SaveState() => new() { Title = Title };
    public void LoadState(ToolState _) { }
}

// ValueTuple は WPF バインディングで Item1/Item2 になるため record を使う
public sealed record TagItem(string Label, string Tag, string ToolTip);
