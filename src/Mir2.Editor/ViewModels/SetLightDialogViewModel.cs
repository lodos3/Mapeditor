using ReactiveUI;
using System.Reactive;

namespace Mir2.Editor.ViewModels;

/// <summary>
/// ViewModel for Set Light Properties dialog
/// </summary>
public class SetLightDialogViewModel : ReactiveObject
{
    private byte _light;
    
    /// <summary>
    /// Light level (0-255, where 100-119 are fishing zones)
    /// </summary>
    public byte Light
    {
        get => _light;
        set => this.RaiseAndSetIfChanged(ref _light, value);
    }

    /// <summary>
    /// Result flag indicating if settings were applied
    /// </summary>
    public bool DialogResult { get; private set; }

    /// <summary>
    /// Command to apply the light settings
    /// </summary>
    public ReactiveCommand<Unit, Unit> SetCommand { get; }

    /// <summary>
    /// Command to cancel the operation
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public SetLightDialogViewModel()
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