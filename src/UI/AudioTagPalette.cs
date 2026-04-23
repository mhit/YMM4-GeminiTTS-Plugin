using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace YMM4.GeminiTTS.Plugin.UI;

/// <summary>
/// Audio Tag 挿入パレット。
/// Focusable=False のボタンで構成するため、TextBox のカーソル位置を保持したまま挿入できる。
/// </summary>
internal static class AudioTagPalette
{
    static Window? _window;

    static readonly (string Label, string Tag)[] Tags =
    [
        ("囁き",       "[whispers]"),
        ("叫び",       "[shouting]"),
        ("興奮",       "[excited]"),
        ("笑い",       "[laughs]"),
        ("泣き",       "[crying]"),
        ("真剣",       "[serious]"),
        ("好奇心",     "[curious]"),
        ("─ 短",       "[short pause]"),
        ("─ 中",       "[medium pause]"),
        ("─ 長",       "[long pause]"),
    ];

    public static void EnsureVisible()
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            if (_window == null)
                _window = CreateWindow();

            _window.Visibility = Visibility.Visible;
        });
    }

    public static void Toggle() =>
        Application.Current?.Dispatcher.Invoke(() =>
        {
            if (_window == null)
                _window = CreateWindow();

            _window.Visibility = _window.IsVisible
                ? Visibility.Collapsed
                : Visibility.Visible;
        });

    static Window CreateWindow()
    {
        var wrap = new WrapPanel { Orientation = Orientation.Horizontal, MaxWidth = 220 };

        foreach (var (label, tag) in Tags)
        {
            var btn = new Button
            {
                Content = label,
                Tag = tag,
                Focusable = false,          // TextBox フォーカスを奪わない
                Padding = new Thickness(6, 3, 6, 3),
                Margin = new Thickness(2),
                FontSize = 11,
                Background = new SolidColorBrush(Color.FromArgb(255, 55, 55, 60)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 120, 120, 130)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                ToolTip = tag,
            };
            btn.Click += OnTagButtonClick;
            wrap.Children.Add(btn);
        }

        var titleBar = new DockPanel { Background = new SolidColorBrush(Color.FromArgb(255, 35, 35, 40)) };

        var title = new TextBlock
        {
            Text = "🏷 Audio Tag",
            Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200)),
            FontSize = 11,
            Margin = new Thickness(8, 4, 0, 4),
            VerticalAlignment = VerticalAlignment.Center,
        };
        DockPanel.SetDock(title, Dock.Left);

        var closeBtn = new Button
        {
            Content = "✕",
            Focusable = false,
            Width = 24, Height = 22,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 2, 4, 2),
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200)),
            BorderThickness = new Thickness(0),
            FontSize = 10,
            Cursor = Cursors.Hand,
            VerticalAlignment = VerticalAlignment.Center,
        };
        closeBtn.Click += (_, _) => _window!.Visibility = Visibility.Collapsed;
        DockPanel.SetDock(closeBtn, Dock.Right);

        titleBar.Children.Add(closeBtn);
        titleBar.Children.Add(title);

        var root = new DockPanel { Background = new SolidColorBrush(Color.FromArgb(235, 28, 28, 32)) };
        DockPanel.SetDock(titleBar, Dock.Top);
        root.Children.Add(titleBar);
        root.Children.Add(new Border
        {
            Child = wrap,
            Padding = new Thickness(4),
        });

        var win = new Window
        {
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            Topmost = true,
            ShowInTaskbar = false,
            ResizeMode = ResizeMode.NoResize,
            SizeToContent = SizeToContent.WidthAndHeight,
            Content = new Border
            {
                Child = root,
                CornerRadius = new CornerRadius(6),
                BorderBrush = new SolidColorBrush(Color.FromArgb(120, 80, 80, 90)),
                BorderThickness = new Thickness(1),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 10,
                    Opacity = 0.5,
                    ShadowDepth = 2,
                },
            },
            Left = SystemParameters.WorkArea.Right - 250,
            Top = SystemParameters.WorkArea.Bottom - 220,
        };

        // タイトルバードラッグで移動
        titleBar.MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                win.DragMove();
        };

        win.Show();
        return win;
    }

    static void OnTagButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string tag) return;

        var focused = Keyboard.FocusedElement as TextBox;
        if (focused == null) return;

        var idx = focused.SelectionStart;
        focused.Text = focused.Text.Insert(idx, tag);
        focused.SelectionStart = idx + tag.Length;
        focused.SelectionLength = 0;
    }
}
