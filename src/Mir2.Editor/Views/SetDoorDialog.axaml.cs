using Avalonia.Controls;
using Mir2.Editor.ViewModels;

namespace Mir2.Editor.Views;

public partial class SetDoorDialog : Window
{
    public SetDoorDialog()
    {
        InitializeComponent();
        DataContext = new SetDoorDialogViewModel();
    }

    public SetDoorDialog(SetDoorDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}