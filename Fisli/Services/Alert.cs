using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Fisli.ViewModels;

namespace Fisli.Services;

public static class Alert
{
    public static void ShowAlert(string Message, string WindowID = "", bool IsFatal = false)
    {
        var alertWindow = new Window
        {
            Content = new AlertViewModel(Message, WindowID),
            ExtendClientAreaToDecorationsHint = true,
            ExtendClientAreaTitleBarHeightHint = 30,
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome,
            Name = "AlertWindow" + WindowID,
            Width = 400,
            Height = 150,
            CanResize = false
        };
        alertWindow.ShowDialog(((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!)
            .MainWindow);
    }
}