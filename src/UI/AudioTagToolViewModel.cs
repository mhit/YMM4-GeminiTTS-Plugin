using System;
using System.Windows.Input;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin;

namespace YMM4.GeminiTTS.Plugin.UI;

public sealed class AudioTagToolViewModel : Bindable, IToolViewModel
{
    public event EventHandler<CreateNewToolViewRequestedEventArgs>? CreateNewToolViewRequested;

    public string Title => "Audio Tag";

    static readonly (string Label, string Tag, string ToolTip)[] Tags =
    [
        ("囁き",   "[whispers]",    "ひそひそ声"),
        ("叫び",   "[shouting]",    "叫ぶ・大声"),
        ("興奮",   "[excited]",     "興奮・テンション高め"),
        ("笑い",   "[laughs]",      "笑いながら"),
        ("泣き",   "[crying]",      "泣きながら"),
        ("真剣",   "[serious]",     "真剣・厳粛"),
        ("好奇心", "[curious]",     "不思議そう"),
        ("─ 短",   "[short pause]", "短い間"),
        ("─ 中",   "[medium pause]","中程度の間"),
        ("─ 長",   "[long pause]",  "長い間"),
    ];

    public (string Label, string Tag, string ToolTip)[] TagItems => Tags;

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
