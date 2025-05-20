using System;
using System.IO;

namespace Chitti.Helpers;

public static class AppPaths
{
    public static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Chitti");

    public static string DatabasePath => Path.Combine(AppDataFolder, "Chitti.db");
    public static string LogPath => Path.Combine(AppDataFolder, "app.log");

    static AppPaths()
    {
        // Ensure the app data directory exists
        if (!Directory.Exists(AppDataFolder))
        {
            Directory.CreateDirectory(AppDataFolder);
        }
    }
} 