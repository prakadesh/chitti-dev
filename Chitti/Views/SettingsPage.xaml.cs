using Chitti.Controls;
using Chitti.Data;
using Chitti.Helpers;
using Chitti.Models;
using Chitti.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace Chitti.Views;


public partial class SettingsPage : UserControl
{
    private readonly ApplicationDbContext _dbContext;
    private bool _isRecordingHotkey;
    private readonly HotkeyManager _hotkeyManager;
    public SettingsPage()
    {
        InitializeComponent();
        _dbContext = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={AppPaths.DatabasePath}")
            .Options);

        LoadSettings();

    }
    private void SetHotkeyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isRecordingHotkey)
        {
            StartRecordingHotkey();
        }
        else
        {
            StopRecordingHotkey();
        }
    }
     private void StartRecordingHotkey()
    {
        _isRecordingHotkey = true;
        SetHotkeyButton.Content = "Press Keys...";
        SetHotkeyButton.Background = new SolidColorBrush(Colors.LightCoral);
        CurrentHotkeyText.Text = "Listening for key combination...";

        // Start listening for key combinations
        this.PreviewKeyDown += HotkeyRecording_PreviewKeyDown;
    }

        private void StopRecordingHotkey()
    {
        _isRecordingHotkey = false;
        SetHotkeyButton.Content = "Set Hotkey";
        SetHotkeyButton.Background = (SolidColorBrush)Application.Current.Resources["SuccessGreen"];
        this.PreviewKeyDown -= HotkeyRecording_PreviewKeyDown;
    }

    private async void HotkeyRecording_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        // Get modifiers
        var modifiers = 0;
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            modifiers |= (int)ModifierKeys.Control;
        if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            modifiers |= (int)ModifierKeys.Alt;
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            modifiers |= (int)ModifierKeys.Shift;
        if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
            modifiers |= (int)ModifierKeys.Windows;

        // Ignore if only modifier keys are pressed
        if (IsModifierKey(e.Key)) return;

        // Don't allow hotkeys without modifiers
        if (modifiers == 0)
        {
            MessageBox.Show("Please include at least one modifier key (Ctrl, Alt, Shift, or Windows)",
                "Invalid Hotkey",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        int key = KeyInterop.VirtualKeyFromKey(e.Key);

        try
        {
            var settings = await _dbContext.AppSettings.FirstOrDefaultAsync() ?? new AppSettings();
            settings.ChatHotkeyModifiers = modifiers;
            settings.ChatHotkeyKey = key;

            if (settings.Id == 0)
                _dbContext.AppSettings.Add(settings);
            else
                _dbContext.AppSettings.Update(settings);

            await _dbContext.SaveChangesAsync();

            // Update the hotkey manager
            _hotkeyManager?.UpdateHotkey(modifiers, key);

            // Update display
            UpdateHotkeyDisplay(modifiers, key);

            StopRecordingHotkey();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error setting hotkey: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StopRecordingHotkey();
        }
    }

    private bool IsModifierKey(Key key)
    {
        return key == Key.LeftCtrl || key == Key.RightCtrl ||
               key == Key.LeftAlt || key == Key.RightAlt ||
               key == Key.LeftShift || key == Key.RightShift ||
               key == Key.LWin || key == Key.RWin;
    }



    private void UpdateHotkeyDisplay(int modifiers, int key)
    {
        var text = new System.Text.StringBuilder("Current: ");

        if ((modifiers & (int)ModifierKeys.Control) != 0) text.Append("Ctrl + ");
        if ((modifiers & (int)ModifierKeys.Alt) != 0) text.Append("Alt + ");
        if ((modifiers & (int)ModifierKeys.Shift) != 0) text.Append("Shift + ");
        if ((modifiers & (int)ModifierKeys.Windows) != 0) text.Append("Win + ");

        text.Append(((System.Windows.Forms.Keys)key).ToString());

        CurrentHotkeyText.Text = text.ToString();
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
            UpdateHotkeyDisplay(settings.ChatHotkeyModifiers, settings.ChatHotkeyKey);

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