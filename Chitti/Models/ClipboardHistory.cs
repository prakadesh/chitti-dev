using System;

namespace Chitti.Models;

public class ClipboardHistory
{
    public int Id { get; set; }
    public string OriginalText { get; set; } = string.Empty;
    public string ProcessedText { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
} 