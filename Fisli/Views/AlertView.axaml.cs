using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Fisli.Views;

public partial class AlertView : UserControl
{
    public AlertView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}