using System;

namespace Chitti.Services;

public class ClipboardMonitoringState
{
    public event EventHandler<bool>? MonitoringStateChanged;
    private bool _isMonitoringEnabled = true;

    public bool IsMonitoringEnabled
    {
        get => _isMonitoringEnabled;
        set
        {
            if (_isMonitoringEnabled != value)
            {
                _isMonitoringEnabled = value;
                MonitoringStateChanged?.Invoke(this, value);
            }
        }
    }
}