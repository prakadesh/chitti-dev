using System;
using System.IO;

namespace ClipEnhancer.Helpers;

public static class Logger
{
    private static readonly string LogFilePath = AppPaths.LogPath;

    public static void Log(string message)
    {
        try
        {
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllText(LogFilePath, logLine + Environment.NewLine);
        }
        catch { /* Ignore logging errors */ }
    }

    public static string ReadLog()
    {
        try
        {
            if (File.Exists(LogFilePath))
                return File.ReadAllText(LogFilePath);
            return "Log file is empty.";
        }
        catch (Exception ex)
        {
            return $"Error reading log: {ex.Message}";
        }
    }

    public static void ClearLog()
    {
        try { if (File.Exists(LogFilePath)) File.Delete(LogFilePath); } catch { }
    }
} 