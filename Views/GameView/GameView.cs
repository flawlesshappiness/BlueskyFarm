using Godot;

public partial class GameView : View
{
    [NodeName]
    public ColorRect ColorRectOverlay;

    [NodeType]
    public Minimap Minimap;

    public override string Directory => $"{Paths.ViewDirectory}/{nameof(GameView)}";

    public override void _Ready()
    {
        base._Ready();
        SetOverlayAlpha(0);
        Show();
    }

    public void SetOverlayAlpha(float alpha)
    {
        var color = ColorRectOverlay.Color;
        var new_color = new Color(color.R, color.G, color.B, alpha);
        ColorRectOverlay.Color = new_color;
    }
}
