using Avalonia.Controls;
using Mir2.Editor.ViewModels;

namespace Mir2.Editor.Views;

public partial class SetLightDialog : Window
{
    public SetLightDialog()
    {
        InitializeComponent();
        DataContext = new SetLightDialogViewModel();
    }

    public SetLightDialog(SetLightDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}