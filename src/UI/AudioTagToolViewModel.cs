using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin;

namespace YMM4.GeminiTTS.Plugin.UI;

public sealed class AudioTagToolViewModel : Bindable, IToolViewModel
{
    public event EventHandler<CreateNewToolViewRequestedEventArgs>? CreateNewToolViewRequested;

    public string Title => "Audio Tag";

    public TagGroup[] Groups { get; } =
    [
        new("😮 感情・口調", [
            new("囁き",       "[whispers]",       "静かに囁く・ひそひそ声"),
            new("叫び",       "[shouting]",       "大声で叫ぶ"),
            new("興奮",       "[excited]",        "テンション高め・興奮"),
            new("笑い",       "[laughs]",         "笑いながら話す"),
            new("くすくす",   "[giggles]",        "くすくす笑う"),
            new("泣き",       "[crying]",         "泣きながら話す"),
            new("真剣",       "[serious]",        "真剣・威厳ある口調"),
            new("好奇心",     "[curious]",        "不思議そう・疑問"),
            new("ためらい",   "[hesitates]",      "言葉に詰まる"),
            new("息をのむ",   "[gasps]",          "驚いて息をのむ"),
        ]),
        new("💨 動作・反応", [
            new("ため息",     "[sighs]",          "ため息をつく"),
            new("咳払い",     "[clears throat]",  "咳払い"),
            new("鼻すする",   "[sniffs]",         "鼻をすする"),
            new("あくび",     "[yawns]",          "あくびをする"),
            new("鼻を鳴らす", "[snorts]",         "鼻を鳴らす・冷笑"),
        ]),
        new("⏸ 間・ポーズ", [
            new("短い間",     "[short pause]",    "短い間（0.5秒程度）"),
            new("中程度の間", "[medium pause]",   "中程度の間（1秒程度）"),
            new("長い間",     "[long pause]",     "長い間（2秒程度）"),
        ]),
    ];

    public ICommand InsertTagCommand { get; }

    public AudioTagToolViewModel()
    {
        InsertTagCommand = new ActionCommand(
            _ => true,
            tag =>
            {
                if (tag is not string t) return;
                ClipboardInsert.Send(t);
            });
    }

    public ToolState SaveState() => new() { Title = Title };
    public void LoadState(ToolState _) { }
}

public sealed record TagItem(string Label, string Tag, string ToolTip);
public sealed record TagGroup(string Header, TagItem[] Items);

/// <summary>
/// クリップボード経由でテキストを挿入する。
/// WPF の FocusScope をまたぐ場合でも OS レベルの Ctrl+V で確実に動作する。
/// </summary>
internal static class ClipboardInsert
{
    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, nuint dwExtraInfo);

    const byte VK_CONTROL = 0x11;
    const byte VK_V = 0x56;
    const uint KEYEVENTF_KEYUP = 0x0002;

    public static void Send(string text)
    {
        // クリップボードを保存
        var prev = GetClipboard();
        Clipboard.SetText(text);

        // Ctrl+V を送信（OS レベル）
        keybd_event(VK_CONTROL, 0, 0, 0);
        keybd_event(VK_V, 0, 0, 0);
        keybd_event(VK_V, 0, KEYEVENTF_KEYUP, 0);
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);

        // 少し待ってからクリップボードを復元
        System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
            Application.Current?.Dispatcher.Invoke(() => RestoreClipboard(prev)));
    }

    static (string? text, bool hasText) GetClipboard()
    {
        try { return (Clipboard.GetText(), Clipboard.ContainsText()); }
        catch { return (null, false); }
    }

    static void RestoreClipboard((string? text, bool hasText) prev)
    {
        try
        {
            if (prev.hasText && prev.text != null)
                Clipboard.SetText(prev.text);
            else
                Clipboard.Clear();
        }
        catch { }
    }
}
