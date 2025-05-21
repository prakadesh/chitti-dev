using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Chitti.Helpers;

public static class ToastNotificationHelper
{
    private static Window? _toastWindow;
    private static TextBlock? _titleTextBlock;
    private static TextBlock? _messageTextBlock;
    private static TextBlock? _appNameTextBlock;
    private static TextBlock? _timestampTextBlock;
    private static Image? _iconImage;
    private const double TOAST_WIDTH = 370;
    private const double TOAST_HEIGHT = 110;
    private const double CORNER_RADIUS = 22;
    private const double ANIMATION_DURATION = 0.4;
    private const string APP_NAME = "Chitti";
    private const string TITLE = "Notification";
    private const string ICON_PATH = "pack://application:,,,/Chitti;component/Assets/logo.png";

    public static void UpdateToastText(string title, string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_toastWindow != null && _titleTextBlock != null && _messageTextBlock != null)
            {
                _titleTextBlock.Text = title;
                _messageTextBlock.Text = message;
                _timestampTextBlock.Text = DateTime.Now.ToString("h:mm tt");
            }
        });
    }

    public static void ShowToast(string title, string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_toastWindow == null)
            {
                CreateToastWindow();
            }

            _titleTextBlock!.Text = title;
            _messageTextBlock!.Text = message;
            _timestampTextBlock!.Text = DateTime.Now.ToString("h:mm tt");

            ShowToastWithAnimation();
        });
    }

    public static void HideToast()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_toastWindow != null)
            {
                HideToastWithAnimation();
            }
        });
    }

    private static void CreateToastWindow()
    {
        _toastWindow = new Window
        {
            Width = TOAST_WIDTH,
            Height = TOAST_HEIGHT,
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = Brushes.Transparent,
            Topmost = true,
            ShowInTaskbar = false,
            ResizeMode = ResizeMode.NoResize
        };

        Brush backgroundBrush;
        try
        {
            // Try to use AcrylicBrush if available (Windows 10+)
            var acrylicType = Type.GetType("System.Windows.Media.AcrylicBrush, PresentationCore");
            if (acrylicType != null)
            {
                var acrylic = (Brush)Activator.CreateInstance(acrylicType);
                acrylicType.GetProperty("BackgroundSource")?.SetValue(acrylic, Enum.Parse(acrylicType.Assembly.GetType("System.Windows.Media.AcrylicBackgroundSource"), "HostBackdrop"));
                acrylicType.GetProperty("TintColor")?.SetValue(acrylic, Color.FromArgb(200, 255, 255, 255));
                acrylicType.GetProperty("TintOpacity")?.SetValue(acrylic, 0.7);
                backgroundBrush = acrylic;
            }
            else
            {
                backgroundBrush = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
            }
        }
        catch
        {
            backgroundBrush = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
        }

        var glassBorder = new Border
        {
            Background = backgroundBrush,
            CornerRadius = new CornerRadius(CORNER_RADIUS),
            BorderThickness = new Thickness(1.5),
            BorderBrush = new SolidColorBrush(Color.FromArgb(80, 200, 200, 200))
        };

        var mainGrid = new Grid
        {
            Margin = new Thickness(0)
        };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        // Top row: icon, app name, timestamp
        var topRow = new Grid { Margin = new Thickness(18, 10, 18, 0) };
        topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

        // App icon
        _iconImage = new Image
        {
            Width = 28,
            Height = 28,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Source = new BitmapImage(new Uri(ICON_PATH, UriKind.Absolute))
        };
        Grid.SetColumn(_iconImage, 0);
        topRow.Children.Add(_iconImage);

        // App name
        _appNameTextBlock = new TextBlock
        {
            Text = APP_NAME,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 0)
        };
        Grid.SetColumn(_appNameTextBlock, 1);
        topRow.Children.Add(_appNameTextBlock);

        // Timestamp
        _timestampTextBlock = new TextBlock
        {
            Text = DateTime.Now.ToString("h:mm tt"),
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140)),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_timestampTextBlock, 2);
        topRow.Children.Add(_timestampTextBlock);

        // Content row: title and message
        var contentStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(18, 2, 18, 12)
        };

        _titleTextBlock = new TextBlock
        {
            Text = TITLE,
            FontWeight = FontWeights.Bold,
            FontSize = 15,
            Foreground = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        _messageTextBlock = new TextBlock
        {
            Text = "",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 0)
        };
        contentStack.Children.Add(_titleTextBlock);
        contentStack.Children.Add(_messageTextBlock);

        Grid.SetRow(topRow, 0);
        Grid.SetRow(contentStack, 1);
        mainGrid.Children.Add(topRow);
        mainGrid.Children.Add(contentStack);

        glassBorder.Child = mainGrid;
        _toastWindow.Content = glassBorder;
    }

    private static void ShowToastWithAnimation()
    {
        if (_toastWindow == null) return;

        // Position the toast in the top-right corner
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        _toastWindow.Left = screenWidth - TOAST_WIDTH - 20;
        _toastWindow.Top = 20;

        // Fade-in animation
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(ANIMATION_DURATION),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        _toastWindow.Opacity = 0;
        _toastWindow.Show();
        _toastWindow.BeginAnimation(Window.OpacityProperty, fadeIn);
    }

    private static void HideToastWithAnimation()
    {
        if (_toastWindow == null) return;

        // Fade-out animation
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(ANIMATION_DURATION),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        fadeOut.Completed += (s, e) =>
        {
            _toastWindow?.Close();
            _toastWindow = null;
        };
        _toastWindow.BeginAnimation(Window.OpacityProperty, fadeOut);
    }
} 