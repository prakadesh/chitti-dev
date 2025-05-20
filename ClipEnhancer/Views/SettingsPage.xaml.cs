using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClipEnhancer.Data;
using ClipEnhancer.Models;
using Microsoft.EntityFrameworkCore;
using ClipEnhancer.Helpers;

namespace ClipEnhancer.Views;

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
} 