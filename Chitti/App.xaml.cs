using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Chitti.Data;
using Chitti.Services;
using Chitti.Views;
using Chitti.Helpers;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

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
                    var icon = new NotifyIcon
                    {
                        Icon = new System.Drawing.Icon(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/logo.ico")),
                        Visible = true,
                        Text = "Chitti"
                    };

                    // Create context menu
                    var contextMenu = new ContextMenuStrip();

                    // Get clipboard monitor service
                    var clipboardMonitor = sp.GetRequiredService<ClipboardMonitorService>();

                    // Pause/Resume menu item
                    var monitoringMenuItem = new ToolStripMenuItem("Pause Monitoring");
                    monitoringMenuItem.Click += (s, e) =>
                    {
                        clipboardMonitor.IsMonitoringEnabled = !clipboardMonitor.IsMonitoringEnabled;
                        monitoringMenuItem.Text = clipboardMonitor.IsMonitoringEnabled ?
                            "Pause Monitoring" : "Resume Monitoring";

                        icon.ShowBalloonTip(
                            2000,
                            "Clipboard Monitoring",
                            clipboardMonitor.IsMonitoringEnabled ?
                                "Clipboard monitoring resumed" : "Clipboard monitoring paused",
                            ToolTipIcon.Info);
                    };
                    contextMenu.Items.Add(monitoringMenuItem);

                    // Separator
                    contextMenu.Items.Add(new ToolStripSeparator());

                    // Exit menu item
                    var exitMenuItem = new ToolStripMenuItem("Exit");
                    exitMenuItem.Click += (s, e) =>
                    {
                        icon.ShowBalloonTip(
                            2000,
                            "Chitti Closing",
                            "Chitti is shutting down...",
                            ToolTipIcon.Info);

                        // Give the balloon tip time to show
                        System.Threading.Thread.Sleep(2000);
                        Current.Shutdown();
                    };
                    contextMenu.Items.Add(exitMenuItem);

                    icon.ContextMenuStrip = contextMenu;
                    return icon;
                });

                // Views
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Setup system tray icon first
        using var scope = _host.Services.CreateScope();
        _notifyIcon = scope.ServiceProvider.GetRequiredService<NotifyIcon>();
        var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();


        // Check for single instance
        _singleInstance = new SingleInstance(_notifyIcon);
        if (!_singleInstance.IsFirstInstance())
        {
            Shutdown();
            return;
        }

        await _host.StartAsync();

        // Initialize database
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Start clipboard monitoring
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
        var mainWindowShow = scope.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindowShow.Show();

        base.OnStartup(e);
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