using ReactiveUI;

namespace Mir2.Editor.ViewModels;

public class NewMapDialogViewModel : ReactiveObject
{
    private int _width = 100;
    private int _height = 100;
    private string _name = "New Map";

    public int Width
    {
        get => _width;
        set => this.RaiseAndSetIfChanged(ref _width, Math.Max(1, Math.Min(1000, value)));
    }

    public int Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, Math.Max(1, Math.Min(1000, value)));
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value ?? "New Map");
    }

    public bool IsValid => Width > 0 && Height > 0 && Width <= 1000 && Height <= 1000 && !string.IsNullOrWhiteSpace(Name);
}