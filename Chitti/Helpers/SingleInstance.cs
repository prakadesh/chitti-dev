using System;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace Chitti.Helpers;

public class SingleInstance : IDisposable
{
    private const string MutexName = "ChittiSingleInstanceMutex";
    private readonly Mutex _mutex;
    private readonly NotifyIcon _notifyIcon;
    private bool _owned = false;

    public SingleInstance(NotifyIcon notifyIcon)
    {
        _notifyIcon = notifyIcon;
        _mutex = new Mutex(true, MutexName, out _owned);
    }

    public bool IsFirstInstance()
    {
        if (_owned)
        {
            return true;
        }

        _notifyIcon.ShowBalloonTip(
            3000,
            "Chitti Already Running",
            "Another instance of Chitti is already running in the system tray.",
            ToolTipIcon.Warning);

        return false;
    }

    public void Dispose()
    {
        if (_owned)
        {
            _mutex.ReleaseMutex();
        }
        _mutex.Dispose();
    }
}