using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;

namespace Fisli.ViewModels;

public class AlertViewModel : ViewModelBase
{
    private readonly string WindowID = string.Empty;
    private string _alertMessage = "Something went wrong!";

    public AlertViewModel(string aMessage, string windowID)
    {
        AlertMessage = aMessage;
        WindowID = windowID;
    }

    public AlertViewModel()
    {
    }

    public string AlertMessage
    {
        get => _alertMessage;
        set => this.RaiseAndSetIfChanged(ref _alertMessage, value);
    }

    public void CloseAlertWindow()
    {
        foreach (var window in ((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!)
                 .Windows)
        {
            if (window.Name == "AlertWindow" + WindowID)
            {
                window.Close();
            }
        }
    }
}