using Avalonia.Controls;
using Avalonia.Interactivity;
using Mir2.Editor.ViewModels;
using System.Reactive.Linq;

namespace Mir2.Editor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnInitializeClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.InitializeCommand.Execute().Subscribe();
        }
    }

    private void OnLoadMapClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.LoadMapCommand.Execute().Subscribe();
        }
    }

    private void OnSaveMapClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SaveMapCommand.Execute().Subscribe();
        }
    }

    private void OnNewMapClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.NewMapCommand.Execute().Subscribe();
        }
    }

    private void OnUndoClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.UndoCommand.Execute().Subscribe();
        }
    }

    private void OnRedoClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.RedoCommand.Execute().Subscribe();
        }
    }
}