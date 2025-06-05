using System.Windows.Input;

namespace Chitti.Models;

public class AppSettings
{
    public int Id { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public bool IsClipboardMonitoringEnabled { get; set; } = true;
    public int ApiTimeoutSeconds { get; set; } = 30;
    public bool ShowDetailedNotifications { get; set; } = true;
    public string DefaultTags { get; set; } = "/formal,/grammar,/concise,/style";
    // Default to Alt + Shift + C
    public int ChatHotkeyModifiers { get; set; } = (int)(System.Windows.Input.ModifierKeys.Alt | System.Windows.Input.ModifierKeys.Shift);
    public int ChatHotkeyKey { get; set; } = (int)Key.C;
}