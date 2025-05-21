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
                        Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location),
                        Visible = true,
                        Text = "Chitti"
                    };
                    return icon;
                });

                // Views
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Initialize database
        using var scope = _host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Start clipboard monitoring
        var clipboardMonitor = scope.ServiceProvider.GetRequiredService<ClipboardMonitorService>();
        clipboardMonitor.StartMonitoring();

        // Setup system tray icon
        _notifyIcon = scope.ServiceProvider.GetRequiredService<NotifyIcon>();
        _notifyIcon.DoubleClick += (s, e) =>
        {
            var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
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