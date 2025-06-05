using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Chitti.Data;
using Microsoft.EntityFrameworkCore;
using Chitti.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Interop;
using Chitti.Helpers;
using System.Windows;

namespace Chitti.Services;

public class HotkeyManager : IDisposable
{
    // Windows message for hotkey
    private const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly ApplicationDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;
    private IntPtr _windowHandle;
    private const int HOTKEY_ID = 9000;
    private HwndSource? _source;

    // Define modifier keys constants
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;

    public HotkeyManager(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _serviceProvider = serviceProvider;

        // Wait for main window to be created
        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            _windowHandle = new WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(WndProc);
            RegisterHotkey();
        }));
    }

    private async void RegisterHotkey()
    {
        try
        {
            var settings = await _dbContext.AppSettings.FirstOrDefaultAsync();
            if (settings != null)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID); // Unregister existing

                // Convert modifiers to Windows API format
                uint modifiers = 0;
                if ((settings.ChatHotkeyModifiers & (int)ModifierKeys.Alt) != 0) modifiers |= MOD_ALT;
                if ((settings.ChatHotkeyModifiers & (int)ModifierKeys.Control) != 0) modifiers |= MOD_CONTROL;
                if ((settings.ChatHotkeyModifiers & (int)ModifierKeys.Shift) != 0) modifiers |= MOD_SHIFT;
                if ((settings.ChatHotkeyModifiers & (int)ModifierKeys.Windows) != 0) modifiers |= MOD_WIN;

                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifiers, (uint)settings.ChatHotkeyKey);
                if (!success)
                {
                    Logger.Log($"Failed to register hotkey. Modifiers: {modifiers}, Key: {settings.ChatHotkeyKey}");
                }
                else
                {
                    Logger.Log("Hotkey registered successfully");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Error registering hotkey: {ex.Message}");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var chatWindow = new ChatWindow(_serviceProvider.GetRequiredService<GeminiService>());
                    chatWindow.Show();
                    chatWindow.Activate(); // Ensure window comes to front
                    Logger.Log("Chat window opened via hotkey");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error opening chat window: {ex.Message}");
                }
            });
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void UpdateHotkey(int modifiers, int key)
    {
        try
        {
            UnregisterHotKey(_windowHandle, HOTKEY_ID);

            // Convert modifiers to Windows API format
            uint winModifiers = 0;
            if ((modifiers & (int)ModifierKeys.Alt) != 0) winModifiers |= MOD_ALT;
            if ((modifiers & (int)ModifierKeys.Control) != 0) winModifiers |= MOD_CONTROL;
            if ((modifiers & (int)ModifierKeys.Shift) != 0) winModifiers |= MOD_SHIFT;
            if ((modifiers & (int)ModifierKeys.Windows) != 0) winModifiers |= MOD_WIN;

            bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, winModifiers, (uint)key);
            if (!success)
            {
                Logger.Log($"Failed to update hotkey. Modifiers: {winModifiers}, Key: {key}");
            }
            else
            {
                Logger.Log("Hotkey updated successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Error updating hotkey: {ex.Message}");
        }
    }

    public void Dispose()
    {
        UnregisterHotKey(_windowHandle, HOTKEY_ID);
        _source?.RemoveHook(WndProc);
        _source?.Dispose();
    }
}