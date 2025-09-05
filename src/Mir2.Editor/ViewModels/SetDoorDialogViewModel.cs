using ReactiveUI;
using System.Reactive;

namespace Mir2.Editor.ViewModels;

/// <summary>
/// ViewModel for Set Door Properties dialog
/// </summary>
public class SetDoorDialogViewModel : ReactiveObject
{
    private bool _isDoor;
    private byte _index;
    private byte _offset;
    
    /// <summary>
    /// Whether this is a door
    /// </summary>
    public bool IsDoor
    {
        get => _isDoor;
        set => this.RaiseAndSetIfChanged(ref _isDoor, value);
    }

    /// <summary>
    /// Door index (0-255)
    /// </summary>
    public byte Index
    {
        get => _index;
        set => this.RaiseAndSetIfChanged(ref _index, value);
    }

    /// <summary>
    /// Door offset (0-255)
    /// </summary>
    public byte Offset
    {
        get => _offset;
        set => this.RaiseAndSetIfChanged(ref _offset, value);
    }

    /// <summary>
    /// Result flag indicating if settings were applied
    /// </summary>
    public bool DialogResult { get; private set; }

    /// <summary>
    /// Command to apply the door settings
    /// </summary>
    public ReactiveCommand<Unit, Unit> SetCommand { get; }

    /// <summary>
    /// Command to cancel the operation
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public SetDoorDialogViewModel()
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