using System.Windows;
using System.Windows.Controls;
using ClipEnhancer.Helpers;

namespace ClipEnhancer.Views;

public partial class LogPage : UserControl
{
    public LogPage()
    {
        InitializeComponent();
        LoadLog();
    }

    private void LoadLog()
    {
        LogTextBox.Text = Logger.ReadLog();
        LogTextBox.ScrollToEnd();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadLog();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Logger.ClearLog();
        LoadLog();
    }
} 