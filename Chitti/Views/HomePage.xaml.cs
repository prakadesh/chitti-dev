using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Chitti.Data;
using Chitti.Models;
using Microsoft.EntityFrameworkCore;
using Chitti.Helpers;

namespace Chitti.Views;

public partial class HomePage : UserControl
{
    private readonly ApplicationDbContext _dbContext;

    public HomePage()
    {
        InitializeComponent();
        _dbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={AppPaths.DatabasePath}")
            .Options);
        LoadRecentActivity();
    }

    private void LoadRecentActivity()
    {
        var recent = _dbContext.ClipboardHistory
            .OrderByDescending(h => h.Timestamp)
            .Take(5)
            .ToList();
        RecentActivityList.ItemsSource = recent;
        EmptyStatePanel.Visibility = recent.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        RecentActivityList.Visibility = recent.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        //OnboardingTip.Visibility = recent.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void GitHubButton_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/Pon-Dinesh-kumar/chitti",
            UseShellExecute = true
        });
    }

    private void EnhanceText_Click(object sender, RoutedEventArgs e)
    {
        // Optionally, you can focus a text input or show a dialog for enhancement
        MessageBox.Show("Copy some text with a tag (like /grammar) to enhance it!", "Enhance Text", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PasteFromClipboard_Click(object sender, RoutedEventArgs e)
    {
        if (System.Windows.Clipboard.ContainsText())
        {
            var text = System.Windows.Clipboard.GetText();
            MessageBox.Show($"Clipboard Text:\n{text}", "Clipboard Content", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("Clipboard does not contain text.", "Clipboard", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ViewHistory_Click(object sender, RoutedEventArgs e)
    {
        // Find the main window and switch to the history page
        var mainWindow = Window.GetWindow(this) as MainWindow;
        if (mainWindow != null)
        {
            mainWindow.MainContent.Content = new HistoryPage();
        }
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