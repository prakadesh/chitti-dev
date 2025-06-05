using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chitti.Controls;

public partial class HotkeyControl : UserControl
{
    private bool _isRecording;
    private int _currentModifiers;
    private int _currentKey;

    public delegate void HotkeyChangedEventHandler(int modifiers, int key);
    public event HotkeyChangedEventHandler? HotkeyChanged;

    public HotkeyControl()
    {
        InitializeComponent();
    }

    public void SetCurrentHotkey(int modifiers, int key)
    {
        _currentModifiers = modifiers;
        _currentKey = key;
        UpdateDisplayText();
    }

    private void RecordButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        _isRecording = true;
        RecordButton.Content = "Press Keys...";
        HotkeyDisplay.Text = "Listening for key combination...";
        RecordButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightCoral);
    }

    private void StopRecording()
    {
        _isRecording = false;
        RecordButton.Content = "Record Hotkey";
        RecordButton.Background = null;
        UpdateDisplayText();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!_isRecording) return;

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

        // Ignore just modifier keys
        if (IsModifierKey(e.Key)) return;

        _currentModifiers = modifiers;
        _currentKey = (int)KeyInterop.VirtualKeyFromKey(e.Key);
        
        HotkeyChanged?.Invoke(_currentModifiers, _currentKey);
        StopRecording();
    }

    private bool IsModifierKey(Key key)
    {
        return key == Key.LeftCtrl || key == Key.RightCtrl ||
               key == Key.LeftAlt || key == Key.RightAlt ||
               key == Key.LeftShift || key == Key.RightShift ||
               key == Key.LWin || key == Key.RWin;
    }

    private void UpdateDisplayText()
    {
        var text = new System.Text.StringBuilder();
        
        if ((_currentModifiers & (int)ModifierKeys.Control) != 0) text.Append("Ctrl + ");
        if ((_currentModifiers & (int)ModifierKeys.Alt) != 0) text.Append("Alt + ");
        if ((_currentModifiers & (int)ModifierKeys.Shift) != 0) text.Append("Shift + ");
        if ((_currentModifiers & (int)ModifierKeys.Windows) != 0) text.Append("Win + ");
        
        text.Append(((System.Windows.Forms.Keys)_currentKey).ToString());
        
        HotkeyDisplay.Text = text.ToString();
    }
}