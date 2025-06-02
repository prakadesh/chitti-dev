using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Chitti.Data;
using Chitti.Models;
using Microsoft.EntityFrameworkCore;
using Chitti.Helpers;

namespace Chitti.Views;

public partial class SettingsPage : UserControl
{
    private readonly ApplicationDbContext _dbContext;

    public SettingsPage()
    {
        InitializeComponent();
        _dbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={AppPaths.DatabasePath}")
            .Options);

        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _dbContext.AppSettings.FirstOrDefault();
        if (settings != null)
        {
            ApiKeyBox.Password = ApiKeyProtector.Decrypt(settings.ApiKey);
            EnableMonitoringCheckBox.IsChecked = settings.IsClipboardMonitoringEnabled;
            TimeoutBox.Text = settings.ApiTimeoutSeconds.ToString();
            DetailedNotificationsCheckBox.IsChecked = settings.ShowDetailedNotifications;
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settings = await _dbContext.AppSettings.FirstOrDefaultAsync() ?? new AppSettings();
            
            settings.ApiKey = ApiKeyProtector.Encrypt(ApiKeyBox.Password);
            settings.IsClipboardMonitoringEnabled = EnableMonitoringCheckBox.IsChecked ?? true;
            settings.ApiTimeoutSeconds = int.Parse(TimeoutBox.Text);
            settings.ShowDetailedNotifications = DetailedNotificationsCheckBox.IsChecked ?? true;

            if (settings.Id == 0)
                _dbContext.AppSettings.Add(settings);
            else
                _dbContext.AppSettings.Update(settings);

            await _dbContext.SaveChangesAsync();

            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ClearDataButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to clear all data? This will delete all clipboard history and reset the application to its initial state. Your API key will be preserved.\n\nThis action cannot be undone.",
            "Confirm Data Clear",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // Save the current API key
                var currentSettings = await _dbContext.AppSettings.FirstOrDefaultAsync();
                var apiKey = currentSettings?.ApiKey;

                // Delete all clipboard history
                _dbContext.ClipboardHistory.RemoveRange(_dbContext.ClipboardHistory);
                await _dbContext.SaveChangesAsync();

                // Recreate settings with preserved API key
                if (currentSettings != null)
                {
                    _dbContext.AppSettings.Remove(currentSettings);
                    await _dbContext.SaveChangesAsync();

                    var newSettings = new AppSettings
                    {
                        ApiKey = apiKey,
                        IsClipboardMonitoringEnabled = true,
                        ApiTimeoutSeconds = 30,
                        ShowDetailedNotifications = true
                    };
                    _dbContext.AppSettings.Add(newSettings);
                    await _dbContext.SaveChangesAsync();
                }

                MessageBox.Show(
                    "All data has been cleared successfully. Your API key has been preserved.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Reload settings
                LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error clearing data: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
} 