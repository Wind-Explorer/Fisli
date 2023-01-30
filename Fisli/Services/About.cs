using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Fisli.Views;

namespace Fisli.Services;

public static class About
{
    public static void ShowAbout()
    {
        var aboutWindow = new Window
        {
            Title = "About Fisli",
            ExtendClientAreaToDecorationsHint = true,
            ExtendClientAreaTitleBarHeightHint = 30,
            Content = new AboutFisliView(),
            Width = 300,
            Height = 200,
            CanResize = false
        };
        aboutWindow.ShowDialog(((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!)
            .MainWindow);
    }
}