using System.Windows;
using System.Windows.Controls;

namespace ClipEnhancer.Views;

public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
    }

    public void SetClipboardStatus(string status, System.Windows.Media.Brush brush)
    {
        ClipboardStatusText.Text = status;
        ClipboardStatusText.Foreground = brush;
    }

    public void SetLastProcessedText(string text)
    {
        LastProcessedText.Text = string.IsNullOrWhiteSpace(text) ? "(None yet)" : text;
    }
} 