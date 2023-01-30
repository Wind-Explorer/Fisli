using Avalonia.Controls;
using Avalonia.Interactivity;
using Fisli.ViewModels;

namespace Fisli.Views;

public partial class MainContentView : UserControl
{
    public MainContentView()
    {
        InitializeComponent();
        DataContext = new MainContentViewModel();
    }

    private void FilesListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        RemoveFileFromListButton.IsVisible = FilesListBox.SelectedIndex > -1;

    private void RemoveFileFromListButton_OnClick(object? sender, RoutedEventArgs e) =>
        RemoveFileFromListButton.IsVisible = false;
}