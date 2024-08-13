using Godot;

public partial class GameView : View
{
    [NodeName]
    public ColorRect ColorRectOverlay;

    [NodeType]
    public Minimap Minimap;

    [NodeName]
    public Control GlowStick;

    [NodeName]
    public Label GlowStickLabel;

    public override string Directory => $"{Paths.ViewDirectory}/{nameof(GameView)}";

    public override void _Ready()
    {
        base._Ready();
        SetOverlayAlpha(0);
        Show();

        GlowSticksChanged();

        GlowStickController.Instance.OnGlowSticksChanged += GlowSticksChanged;
    }

    private void GlowSticksChanged()
    {
        GlowStick.Visible = Data.Game.GlowStickCount > 0;
        GlowStickLabel.Text = Data.Game.GlowStickCount.ToString();
    }

    public void SetOverlayAlpha(float alpha)
    {
        var color = ColorRectOverlay.Color;
        var new_color = new Color(color.R, color.G, color.B, alpha);
        ColorRectOverlay.Color = new_color;
    }
}
