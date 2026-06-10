namespace UI.avalonia.Services;

public sealed class OverlayGeometry
{
    public bool HasPosition { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Opacity { get; set; } = 0.9;
}
