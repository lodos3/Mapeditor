using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Mir2.Editor.Views;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
    }

    private void btnClose_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}