using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace YMM4.GeminiTTS.Plugin.UI;

/// <summary>
/// 音声合成中に画面右下に表示するフローティングオーバーレイ。
/// YMM4 本体に依存しない独立ウィンドウ。
/// </summary>
internal static class SynthesisOverlay
{
    static Window? _window;
    static TextBlock? _label;
    static DispatcherTimer? _closeTimer;
    static int _count;

    public static void Increment(string voiceName)
    {
        var n = Interlocked.Increment(ref _count);
        Application.Current?.Dispatcher.Invoke(() => Show(n, voiceName));
    }

    public static void Decrement()
    {
        var n = Interlocked.Decrement(ref _count);
        Application.Current?.Dispatcher.Invoke(() =>
        {
            if (n <= 0)
                BeginFadeOut();
            else
                UpdateLabel(n, null);
        });
    }

    static void Show(int count, string voiceName)
    {
        _closeTimer?.Stop();

        if (_window == null)
            _window = CreateWindow();

        UpdateLabel(count, voiceName);
        _window.Opacity = 1;
        _window.Visibility = Visibility.Visible;
        PositionBottomRight();
    }

    static void BeginFadeOut()
    {
        if (_window == null) return;

        if (_label != null)
            _label.Text = "✓ 合成完了";

        _closeTimer ??= new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
        _closeTimer.Tick -= OnCloseTimer;
        _closeTimer.Tick += OnCloseTimer;
        _closeTimer.Start();
    }

    static void OnCloseTimer(object? sender, EventArgs e)
    {
        _closeTimer?.Stop();
        if (_window == null) return;

        var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(400));
        anim.Completed += (_, _) =>
        {
            _window.Visibility = Visibility.Collapsed;
            _window.Opacity = 1;
        };
        _window.BeginAnimation(UIElement.OpacityProperty, anim);
    }

    static void UpdateLabel(int count, string? voiceName)
    {
        if (_label == null) return;
        _label.Text = count <= 1 && voiceName != null
            ? $"🎙 合成中…  {voiceName}"
            : $"🎙 合成中…  {count} 件";
    }

    static Window CreateWindow()
    {
        _label = new TextBlock
        {
            Foreground = Brushes.White,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(14, 10, 14, 10),
            TextWrapping = TextWrapping.NoWrap,
        };

        var spinner = new ProgressBar
        {
            IsIndeterminate = true,
            Height = 3,
            Margin = new Thickness(0),
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.FromRgb(100, 181, 246)),
        };

        var panel = new StackPanel();
        panel.Children.Add(_label);
        panel.Children.Add(spinner);

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(220, 30, 30, 30)),
            CornerRadius = new CornerRadius(8),
            Child = panel,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 12,
                Opacity = 0.6,
                ShadowDepth = 2,
            },
        };

        var win = new Window
        {
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            Topmost = true,
            ShowInTaskbar = false,
            ResizeMode = ResizeMode.NoResize,
            SizeToContent = SizeToContent.WidthAndHeight,
            Content = border,
            Visibility = Visibility.Collapsed,
            IsHitTestVisible = false,
        };

        win.Show();
        return win;
    }

    static void PositionBottomRight()
    {
        if (_window == null) return;
        _window.UpdateLayout();
        var area = SystemParameters.WorkArea;
        _window.Left = area.Right - _window.ActualWidth - 16;
        _window.Top  = area.Bottom - _window.ActualHeight - 16;
    }
}
