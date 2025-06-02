using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Chitti.Services;

namespace Chitti.Views;

public partial class MainWindow : Window
{
    private readonly ClipboardMonitorService _clipboardMonitor;
    private readonly NotifyIcon _notifyIcon;
    private HomePage _homePage;
    private HistoryPage _historyPage;
    private SettingsPage _settingsPage;
    private LogPage _logPage;

    public MainWindow(
        ClipboardMonitorService clipboardMonitor,
        NotifyIcon notifyIcon)
    {
        InitializeComponent();
        this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Chitti;component/Assets/logo.png"));
        
        _clipboardMonitor = clipboardMonitor;
        _notifyIcon = notifyIcon;

        _clipboardMonitor.StatusChanged += OnStatusChanged;

        _homePage = new HomePage();
        _historyPage = new HistoryPage();
        _settingsPage = new SettingsPage();
        _logPage = new LogPage();
        MainContent.Content = _homePage;

        Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void OnStatusChanged(object? sender, string message)
    {
        Dispatcher.Invoke(() =>
        {
            if (message.StartsWith("Error"))
            {
                _homePage.SetClipboardStatus(message, System.Windows.Media.Brushes.Red);
            }
            else if (message.Contains("Processing"))
            {
                _homePage.SetClipboardStatus(message, System.Windows.Media.Brushes.Orange);
            }
            else
            {
                _homePage.SetClipboardStatus(message, System.Windows.Media.Brushes.Green);
            }
        });
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.ShowDialog();
    }

    private void HistoryButton_Click(object sender, RoutedEventArgs e)
    {
        var historyWindow = new HistoryWindow();
        historyWindow.ShowDialog();
    }

    private void LogButton_Click(object sender, RoutedEventArgs e)
    {
        var logWindow = new LogWindow();
        logWindow.ShowDialog();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.ShowBalloonTip(
                3000,
                "Chitti Minimized",
                "Chitti is still running in the system tray. Double-click the icon to restore.",
                ToolTipIcon.Info);
        }
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void HomeNav_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _homePage;
    }

    private void HistoryNav_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _historyPage;
        _historyPage.RefreshHistory();
    }

    private void SettingsNav_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _settingsPage;
    }

    private void LogNav_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = _logPage;
    }

    private void TagsNav_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new TagsPage();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            MaximizeButton_Click(sender, e);
        }
        else
        {
            DragMove();
        }
    }
} 