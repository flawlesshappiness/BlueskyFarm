using Godot;

public partial class GameView : View
{
    [NodeName]
    public ColorRect ColorRectOverlay;

    [NodeType]
    public Minimap Minimap;

    [NodeName]
    public DynamicUI DynamicUI;

    public override string Directory => $"{Paths.ViewDirectory}/{nameof(GameView)}";

    public override void _Ready()
    {
        base._Ready();
        SetOverlayAlpha(0);
        Show();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (PlayerInput.ShowUI.Pressed)
        {
            DynamicUI.AnimateShow(true);
        }
    }

    public void SetOverlayAlpha(float alpha)
    {
        var color = ColorRectOverlay.Color;
        var new_color = new Color(color.R, color.G, color.B, alpha);
        ColorRectOverlay.Color = new_color;
    }
}
