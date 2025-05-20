namespace Chitti.Models;

public class AppSettings
{
    public int Id { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public bool IsClipboardMonitoringEnabled { get; set; } = true;
    public int ApiTimeoutSeconds { get; set; } = 30;
    public bool ShowDetailedNotifications { get; set; } = true;
    public string DefaultTags { get; set; } = "/formal,/grammar,/concise,/style";
} 