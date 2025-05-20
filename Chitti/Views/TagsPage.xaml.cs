using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using UserControl = System.Windows.Controls.UserControl;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using Orientation = System.Windows.Controls.Orientation;

namespace Chitti.Views;

public partial class TagsPage : UserControl
{
    private readonly HashSet<string> _selectedTags = new();
    private readonly Dictionary<Button, string> _tagButtons = new();

    public TagsPage()
    {
        InitializeComponent();
        InitializeTagButtons();
    }

    private void InitializeTagButtons()
    {
        // Find all buttons in the visual tree and store them
        var buttons = FindVisualChildren<Button>(this);
        foreach (var button in buttons)
        {
            if (button.Content is string tag && (tag.StartsWith("/") || tag.StartsWith("@")))
            {
                _tagButtons[button] = tag;
                button.Click += TagButton_Click;
            }
        }
    }

    private void TagButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && _tagButtons.TryGetValue(button, out string tag))
        {
            if (tag == "@chitti")
            {
                // Special handling for @chitti tag
                var prompt = ShowchittiPromptDialog();
                if (!string.IsNullOrEmpty(prompt))
                {
                    var customTag = $"@chitti {prompt}";
                    _selectedTags.Add(customTag);
                    AddTagToPanel(customTag);
                    button.Background = new SolidColorBrush(Colors.LightBlue);
                }
            }
            else if (_selectedTags.Contains(tag))
            {
                // Remove tag if already selected
                _selectedTags.Remove(tag);
                button.Background = new SolidColorBrush(Colors.Transparent);
                RemoveTagFromPanel(tag);
            }
            else
            {
                // Add tag if not selected
                _selectedTags.Add(tag);
                button.Background = new SolidColorBrush(Colors.LightBlue);
                AddTagToPanel(tag);
            }
        }
    }

    private string ShowchittiPromptDialog()
    {
        var dialog = new Window
        {
            Title = "Enter Custom Prompt",
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };

        var grid = new Grid { Margin = new Thickness(10) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 0, 0, 10)
        };
        Grid.SetRow(textBox, 0);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right
        };
        Grid.SetRow(buttonPanel, 1);

        var okButton = new Button
        {
            Content = "OK",
            Width = 75,
            Margin = new Thickness(0, 0, 10, 0)
        };
        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 75
        };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        grid.Children.Add(textBox);
        grid.Children.Add(buttonPanel);

        dialog.Content = grid;

        string result = null;
        okButton.Click += (s, e) =>
        {
            result = textBox.Text.Trim();
            dialog.Close();
        };
        cancelButton.Click += (s, e) => dialog.Close();

        dialog.ShowDialog();
        return result;
    }

    private void AddTagToPanel(string tag)
    {
        var tagButton = new Button
        {
            Content = tag,
            Margin = new Thickness(0, 0, 10, 10),
            Padding = new Thickness(10, 5, 10, 5),
            Background = new SolidColorBrush(Colors.LightBlue)
        };
        tagButton.Click += (s, e) => RemoveTag(tag);
        SelectedTagsPanel.Children.Add(tagButton);
    }

    private void RemoveTagFromPanel(string tag)
    {
        var buttonToRemove = SelectedTagsPanel.Children.OfType<Button>()
            .FirstOrDefault(b => b.Content.ToString() == tag);
        if (buttonToRemove != null)
        {
            SelectedTagsPanel.Children.Remove(buttonToRemove);
        }
    }

    private void RemoveTag(string tag)
    {
        _selectedTags.Remove(tag);
        RemoveTagFromPanel(tag);
        
        // Reset the original button's background
        var originalButton = _tagButtons.FirstOrDefault(kvp => kvp.Value == tag).Key;
        if (originalButton != null)
        {
            originalButton.Background = new SolidColorBrush(Colors.Transparent);
        }
    }

    private void CopyCombinedTags_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTags.Count == 0)
        {
            MessageBox.Show("Please select at least one tag.", "No Tags Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var combinedTags = string.Join(" ", _selectedTags.OrderBy(t => t));
        System.Windows.Forms.Clipboard.SetText(combinedTags);
        MessageBox.Show($"Combined tags copied to clipboard: {combinedTags}", "Tags Copied", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ClearTags_Click(object sender, RoutedEventArgs e)
    {
        _selectedTags.Clear();
        SelectedTagsPanel.Children.Clear();
        
        // Reset all button backgrounds
        foreach (var button in _tagButtons.Keys)
        {
            button.Background = new SolidColorBrush(Colors.Transparent);
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                    yield return (T)child;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
} 