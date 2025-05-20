using Chitti.Helpers;
using System.Windows;

namespace Chitti.Views;

public partial class LogWindow : Window
{
    public LogWindow()
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

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
} 