using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
            TextInserter.Insert(t);
        });

    public ToolState SaveState() => new() { Title = Title };
    public void LoadState(ToolState _) { }
}

/// <summary>
/// フォーカスされたテキスト入力欄を記憶し、カーソル位置にタグを挿入する。
/// YMM4 のセリフ欄は RichTextBox（RichTextEditor 内部）なので両方に対応。
/// </summary>
internal static class TextInserter
{
    static UIElement? _lastFocused;

    static TextInserter()
    {
        // TextBox のフォーカスを記憶（カスタム指示欄など）
        EventManager.RegisterClassHandler(
            typeof(TextBox),
            UIElement.GotKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler((s, _) => _lastFocused = s as UIElement));

        // RichTextBox のフォーカスを記憶（YMM4 のセリフ欄 = RichTextEditor の内部）
        EventManager.RegisterClassHandler(
            typeof(RichTextBox),
            UIElement.GotKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler((s, _) => _lastFocused = s as UIElement));
    }

    public static void Insert(string text)
    {
        switch (_lastFocused)
        {
            case TextBox tb:
                var idx = tb.SelectionStart;
                tb.Text = tb.Text.Insert(idx, text);
                tb.SelectionStart = idx + text.Length;
                tb.SelectionLength = 0;
                break;

            case RichTextBox rtb:
                // RichTextBox のキャレット位置にテキストを挿入
                var pos = rtb.CaretPosition;
                pos.InsertTextInRun(text);
                // キャレットを挿入後に移動
                rtb.CaretPosition = rtb.CaretPosition.GetPositionAtOffset(text.Length)
                    ?? rtb.CaretPosition;
                break;
        }
    }
}

public sealed record TagItem(string Label, string Tag, string ToolTip);
