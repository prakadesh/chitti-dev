using Chitti.Data;
using Chitti.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Chitti.Views;

public partial class ChatWindow : Window
{
    private readonly GeminiService _geminiService;
    private readonly ObservableCollection<ChatMessage> _messages;
    private bool _isDragging;
    private bool _isProcessing;

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            _isProcessing = value;
            LoadingOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            MessageInput.IsEnabled = !value;
            SendButton.IsEnabled = !value;
        }
    }

    // Add minimize button handler
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    public ChatWindow(GeminiService geminiService)
    {
        InitializeComponent();
        _geminiService = geminiService;
        _messages = new ObservableCollection<ChatMessage>();
        ChatList.ItemsSource = _messages;

        // Position window in bottom right corner
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 20;
        Top = workArea.Bottom - Height - 20;
    }

    // Add window dragging functionality
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Double click to maximize/restore
            WindowState = WindowState == WindowState.Maximized ? 
                         WindowState.Normal : WindowState.Maximized;
        }
        else
        {
            _isDragging = true;
            DragMove();
            _isDragging = false;
        }
    }

    // Handle window closing
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // Clean up any resources if needed
        _messages.Clear();
        Close();
    }

    // Update SendButton_Click method
    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MessageInput.Text)) return;

        var userMessage = new ChatMessage
        {
            Content = MessageInput.Text,
            IsUser = true,
            Timestamp = DateTime.Now
        };
        _messages.Add(userMessage);
        MessageInput.Text = string.Empty;

        IsProcessing = true;
        try
        {
            var tags = new List<string> { "@chitti chat" };
            var response = await _geminiService.ProcessText(userMessage.Content, tags);
            _messages.Add(new ChatMessage
            {
                Content = response,
                IsUser = false,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _messages.Add(new ChatMessage
            {
                Content = $"Error: {ex.Message}",
                IsUser = false,
                IsError = true,
                Timestamp = DateTime.Now
            });
        }
        finally
        {
            IsProcessing = false;
            MessageInput.Focus();
            await Task.Delay(100); // Small delay to ensure UI updates
            MessagesScrollViewer.ScrollToBottom();
        }
    }

    private void MessageInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                // Allow Shift+Enter for new line
                return;
            }
            else
            {
                e.Handled = true; // Prevent new line
                SendButton_Click(sender, e);
            }
        }
    }
}

public class ChatMessage
{
    public string Content { get; set; } = "";
    public bool IsUser { get; set; }
    public bool IsError { get; set; }
    public DateTime Timestamp { get; set; }
}