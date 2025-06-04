using Chitti.Data;
using Chitti.Helpers;
using Chitti.Models;
using Chitti.Services;
using Chitti.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Chitti;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    private NotifyIcon? _notifyIcon;
    private SingleInstance? _singleInstance;


    public App()
    {
        // Ensure app data folder exists
        // AppPaths.EnsureAppDataFolderExists();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Database
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite($"Data Source={AppPaths.DatabasePath}"));

                // Services
                services.AddSingleton<GeminiService>();
                services.AddSingleton<ClipboardMonitorService>();
                services.AddSingleton<ScreenCaptureService>();
                services.AddSingleton<NotifyIcon>(sp =>
                {
                    var iconUri = new Uri("pack://application:,,,/Assets/logo.ico");
                    var resourceStreamInfo = System.Windows.Application.GetResourceStream(iconUri);

                    if (resourceStreamInfo == null)
                        throw new Exception("Icon resource not found.");

                    System.Drawing.Icon iconObj;
                    // Load the icon from resource stream safely
                    using (var stream = resourceStreamInfo.Stream)
                    {
                        iconObj = new System.Drawing.Icon(stream);
                    }

                    var notifyIcon = new NotifyIcon
                    {
                        Icon = iconObj,
                        Visible = true,
                        Text = "Chitti"
                    };

                    // Create context menu
                    var contextMenu = new ContextMenuStrip();

                    var clipboardMonitor = sp.GetRequiredService<ClipboardMonitorService>();

                    var monitoringMenuItem = new ToolStripMenuItem("Pause Monitoring");
                    monitoringMenuItem.Click += (s, e) =>
                    {
                        clipboardMonitor.IsMonitoringEnabled = !clipboardMonitor.IsMonitoringEnabled;
                        monitoringMenuItem.Text = clipboardMonitor.IsMonitoringEnabled ?
                            "Pause Monitoring" : "Resume Monitoring";

                        notifyIcon.ShowBalloonTip(
                            2000,
                            "Clipboard Monitoring",
                            clipboardMonitor.IsMonitoringEnabled ?
                                "Clipboard monitoring resumed" : "Clipboard monitoring paused",
                            ToolTipIcon.Info);
                    };
                    contextMenu.Items.Add(monitoringMenuItem);

                    contextMenu.Items.Add(new ToolStripSeparator());

                    var exitMenuItem = new ToolStripMenuItem("Exit");
                    exitMenuItem.Click += (s, e) =>
                    {
                        notifyIcon.ShowBalloonTip(
                            2000,
                            "Chitti Closing",
                            "Chitti is shutting down...",
                            ToolTipIcon.Info);

                        System.Threading.Thread.Sleep(2000);
                        Current.Shutdown();
                    };
                    contextMenu.Items.Add(exitMenuItem);

                    notifyIcon.ContextMenuStrip = contextMenu;

                    return notifyIcon;
                });


                // Views
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Setup system tray icon first
            using var scope = _host.Services.CreateScope();
            _notifyIcon = scope.ServiceProvider.GetRequiredService<NotifyIcon>();

            // Check for single instance before doing anything else
            _singleInstance = new SingleInstance(_notifyIcon);
            if (!_singleInstance.IsFirstInstance())
            {
                Shutdown();
                return;
            }

            // Start the host
            await _host.StartAsync();

            // Initialize database first, before creating any windows
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Ensure the database directory exists
            var dbDirectory = Path.GetDirectoryName(AppPaths.DatabasePath);
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory!);
            }

            // Create database ONLY if it doesn't exist (remove EnsureDeletedAsync)
            Logger.Log("Checking database...");
            if (!File.Exists(AppPaths.DatabasePath))
            {
                Logger.Log("Database file not found. Creating new database...");
                await dbContext.Database.EnsureCreatedAsync();

                // Initialize default settings for new database only
                Logger.Log("Initializing default settings...");
                dbContext.AppSettings.Add(new AppSettings
                {
                    IsClipboardMonitoringEnabled = true,
                    ApiTimeoutSeconds = 30,
                    ShowDetailedNotifications = true,
                    DefaultTags = string.Empty,
                    ApiKey = string.Empty
                });
                await dbContext.SaveChangesAsync();
            }
            else
            {
                // Just ensure the database is up to date with the current model
                await dbContext.Database.EnsureCreatedAsync();
            }

            // Rest of your startup code...
            var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
            var clipboardMonitor = scope.ServiceProvider.GetRequiredService<ClipboardMonitorService>();
            clipboardMonitor.StartMonitoring();

            // Setup system tray icon behavior
            _notifyIcon.DoubleClick += (s, e) =>
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            };

            // Show main window
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Logger.Log($"Startup error: {ex}");
            System.Windows.MessageBox.Show(
                $"Critical Error During Startup:\n\n{ex.Message}\n\nDetails have been written to the log file at:\n{AppPaths.LogPath}",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            Shutdown(-1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        await _host.StopAsync();
        await _host.WaitForShutdownAsync();

        base.OnExit(e);
    }
} 