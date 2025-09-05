using Avalonia.Controls;
using Mir2.Editor.ViewModels;

namespace Mir2.Editor.Views;

public partial class SetAnimationDialog : Window
{
    public SetAnimationDialog()
    {
        InitializeComponent();
        DataContext = new SetAnimationDialogViewModel();
    }

    public SetAnimationDialog(SetAnimationDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}