using Avalonia.Controls;
using Mir2.Editor.ViewModels;

namespace Mir2.Editor.Views;

public partial class JumpDialog : Window
{
    public JumpDialog()
    {
        InitializeComponent();
        DataContext = new JumpDialogViewModel();
    }

    public JumpDialog(JumpDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}