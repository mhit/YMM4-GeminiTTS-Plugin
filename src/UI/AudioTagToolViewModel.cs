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
        new("😮 感情・表情", [
            new("囁き",         "[whispers]",       "ひそひそ声・静かに囁く"),
            new("叫び",         "[shouting]",       "大声で叫ぶ"),
            new("興奮",         "[excited]",        "テンション高め・興奮"),
            new("驚き",         "[amazed]",         "驚きの口調"),
            new("笑い",         "[laughs]",         "笑いながら話す"),
            new("くすくす",     "[giggles]",        "くすくす笑う（音として挿入）"),
            new("泣き",         "[crying]",         "泣きながら話す"),
            new("震え声",       "[trembling]",      "震える・怯えた口調"),
            new("疲れ",         "[tired]",          "疲れた・気だるい口調"),
            new("真剣",         "[serious]",        "真剣・落ち着いた口調"),
            new("好奇心",       "[curious]",        "不思議そう（\"curious\"と発音される）"),
            new("パニック",     "[panicked]",       "パニック状態の口調"),
            new("怒り",         "[angry]",          "怒った口調"),
            new("共感",         "[empathetic]",     "共感・寄り添う口調"),
            new("いたずら",     "[mischievously]",  "いたずらっぽい口調"),
            new("皮肉",         "[sarcastic]",      "皮肉っぽい口調"),
        ]),
        new("💨 動作・非言語音", [
            new("ため息",       "[sighs]",          "ため息音を挿入"),
            new("息をのむ",     "[gasp]",           "息をのむ音を挿入（驚き・恐怖）"),
            new("咳払い",       "[cough]",          "咳払いの音を挿入"),
            new("えーと",       "[uhm]",            "躊躇・考える「えーと」音を挿入"),
        ]),
        new("⏸ 間・ポーズ", [
            new("短い間",       "[short pause]",    "短い間（約250ms・読点相当）"),
            new("中程度の間",   "[medium pause]",   "中程度の間（約500ms・文末相当）"),
            new("長い間",       "[long pause]",     "長い間（約1秒以上・劇的効果）"),
        ]),
        new("⚡ スピード", [
            new("超速",         "[very fast]",      "非常に速く話す"),
            new("速く",         "[fast]",           "速く話す"),
            new("ゆっくり",     "[slow]",           "ゆっくり話す"),
            new("超ゆっくり",   "[very slow]",      "非常にゆっくり話す"),
        ]),
    ];

    public ICommand InsertTagCommand { get; }

    // 最後にクリップボードにコピーされたタグ（表示用）
    string _lastCopied = "";
    public string LastCopied { get => _lastCopied; private set => Set(ref _lastCopied, value); }

    bool _showCopied;
    public bool ShowCopied { get => _showCopied; private set => Set(ref _showCopied, value); }

    System.Threading.CancellationTokenSource? _hideCts;

    public AudioTagToolViewModel()
    {
        InsertTagCommand = new ActionCommand(
            _ => true,
            tag =>
            {
                if (tag is not string t) return;

                // クリップボードにコピー（メインの手段）
                Clipboard.SetText(t);

                // "コピーしました" 通知を2秒表示
                LastCopied = $"コピー: {t}";
                ShowCopied = true;
                _hideCts?.Cancel();
                _hideCts = new System.Threading.CancellationTokenSource();
                var cts = _hideCts;
                System.Threading.Tasks.Task.Delay(2000, cts.Token).ContinueWith(task =>
                {
                    if (!task.IsCanceled)
                        Application.Current?.Dispatcher.Invoke(() => ShowCopied = false);
                });

                // Ctrl+V でのペーストも試みる（フォーカスが当たっていれば機能する）
                ClipboardInsert.TrySendPaste();
            });
    }

    public ToolState SaveState() => new() { Title = Title };
    public void LoadState(ToolState _) { }
}

public sealed record TagItem(string Label, string Tag, string ToolTip);
public sealed record TagGroup(string Header, TagItem[] Items);

internal static class ClipboardInsert
{
    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, nuint dwExtraInfo);

    const byte VK_CONTROL = 0x11;
    const byte VK_V = 0x56;
    const uint KEYEVENTF_KEYUP = 0x0002;

    static IInputElement? _lastFocused;

    static ClipboardInsert()
    {
        EventManager.RegisterClassHandler(
            typeof(UIElement),
            UIElement.GotKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler((s, _) =>
            {
                if (s is UIElement { Focusable: true } elem)
                    _lastFocused = elem;
            }));
    }

    public static void TrySendPaste()
    {
        if (_lastFocused != null)
            Keyboard.Focus(_lastFocused);

        keybd_event(VK_CONTROL, 0, 0, 0);
        keybd_event(VK_V, 0, 0, 0);
        keybd_event(VK_V, 0, KEYEVENTF_KEYUP, 0);
        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
    }
}
