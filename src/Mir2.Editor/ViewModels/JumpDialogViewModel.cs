using ReactiveUI;
using System.Reactive;

namespace Mir2.Editor.ViewModels;

/// <summary>
/// ViewModel for Jump dialog
/// </summary>
public class JumpDialogViewModel : ReactiveObject
{
    private int _x;
    private int _y;
    
    /// <summary>
    /// X coordinate to jump to
    /// </summary>
    public int X
    {
        get => _x;
        set => this.RaiseAndSetIfChanged(ref _x, value);
    }

    /// <summary>
    /// Y coordinate to jump to
    /// </summary>
    public int Y
    {
        get => _y;
        set => this.RaiseAndSetIfChanged(ref _y, value);
    }

    /// <summary>
    /// Result flag indicating if jump was confirmed
    /// </summary>
    public bool DialogResult { get; private set; }

    /// <summary>
    /// Command to perform the jump
    /// </summary>
    public ReactiveCommand<Unit, Unit> JumpCommand { get; }

    /// <summary>
    /// Command to cancel the jump
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public JumpDialogViewModel()
    {
        JumpCommand = ReactiveCommand.Create(Jump);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void Jump()
    {
        DialogResult = true;
        // In Avalonia, we'll need to close the window from code-behind or through other means
    }

    private void Cancel()
    {
        DialogResult = false;
        // In Avalonia, we'll need to close the window from code-behind or through other means
    }
}