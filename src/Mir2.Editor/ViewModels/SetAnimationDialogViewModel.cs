using ReactiveUI;
using System.Reactive;

namespace Mir2.Editor.ViewModels;

/// <summary>
/// ViewModel for Set Animation Properties dialog
/// </summary>
public class SetAnimationDialogViewModel : ReactiveObject
{
    private bool _blend;
    private byte _frame;
    private byte _tick;
    
    /// <summary>
    /// Whether to blend the animation
    /// </summary>
    public bool Blend
    {
        get => _blend;
        set => this.RaiseAndSetIfChanged(ref _blend, value);
    }

    /// <summary>
    /// Animation frame (0-255)
    /// </summary>
    public byte Frame
    {
        get => _frame;
        set => this.RaiseAndSetIfChanged(ref _frame, value);
    }

    /// <summary>
    /// Animation tick (0-255)
    /// </summary>
    public byte Tick
    {
        get => _tick;
        set => this.RaiseAndSetIfChanged(ref _tick, value);
    }

    /// <summary>
    /// Result flag indicating if settings were applied
    /// </summary>
    public bool DialogResult { get; private set; }

    /// <summary>
    /// Command to apply the animation settings
    /// </summary>
    public ReactiveCommand<Unit, Unit> SetCommand { get; }

    /// <summary>
    /// Command to cancel the operation
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public SetAnimationDialogViewModel()
    {
        SetCommand = ReactiveCommand.Create(Set);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void Set()
    {
        DialogResult = true;
    }

    private void Cancel()
    {
        DialogResult = false;
    }
}