using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows;

namespace Chitti.Services;

public class ScreenCaptureService
{
    public Image? CaptureScreen()
    {
        try
        {
            // Get the screen dimensions
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            
            // Create a bitmap to store the screenshot
            using Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            
            // Create a graphics object to capture the screen
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
            }

            // Return a copy of the bitmap
            return new Bitmap(bitmap);
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show($"Error capturing screen: {ex.Message}", "Screen Capture Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            return null;
        }
    }

    public Image? CaptureActiveWindow()
    {
        try
        {
            // Get the active window handle
            IntPtr handle = GetForegroundWindow();
            
            // Get the window dimensions
            GetWindowRect(handle, out RECT rect);
            
            // Create a bitmap to store the screenshot
            using Bitmap bitmap = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top);
            
            // Create a graphics object to capture the window
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(rect.Right - rect.Left, rect.Bottom - rect.Top));
            }

            // Return a copy of the bitmap
            return new Bitmap(bitmap);
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show($"Error capturing window: {ex.Message}", "Window Capture Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            return null;
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
} 