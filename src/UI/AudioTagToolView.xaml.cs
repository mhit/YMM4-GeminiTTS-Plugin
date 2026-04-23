using System.Windows;
using System.Windows.Controls;

namespace YMM4.GeminiTTS.Plugin.UI;

public partial class AudioTagToolView : UserControl
{
    public AudioTagToolView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        ButtonPanel.Children.Clear();
        if (DataContext is not AudioTagToolViewModel vm) return;

        for (int i = 0; i < vm.TagCommands.Length; i++)
        {
            var (label, tooltip) = vm.TagInfos[i];
            var cmd = vm.TagCommands[i];

            var btn = new Button
            {
                Content = label,
                Command = cmd,
                Focusable = false,
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(2),
                ToolTip = tooltip,
            };
            ButtonPanel.Children.Add(btn);
        }
    }
}
