using Avalonia.Controls;
using Avalonia.Interactivity;
using Mir2.Editor.ViewModels;

namespace Mir2.Editor.Views;

public partial class NewMapDialog : Window
{
    public NewMapDialog()
    {
        InitializeComponent();
        DataContext = new NewMapDialogViewModel();
    }
    
    public NewMapDialog(NewMapDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public NewMapDialogViewModel? ViewModel => DataContext as NewMapDialogViewModel;

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}