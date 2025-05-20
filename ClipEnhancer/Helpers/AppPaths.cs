using System;
using System.IO;

namespace ClipEnhancer.Helpers;

public static class AppPaths
{
    private static readonly string AppDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ClipEnhancer");

    public static string DatabasePath => Path.Combine(AppDataPath, "clipenhancer.db");
    public static string LogPath => Path.Combine(AppDataPath, "app.log");

    static AppPaths()
    {
        // Ensure the app data directory exists
        if (!Directory.Exists(AppDataPath))
        {
            Directory.CreateDirectory(AppDataPath);
        }
    }
} 