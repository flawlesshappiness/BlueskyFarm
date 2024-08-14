using Godot;

public partial class GlowStickCounter : DynamicUI
{
    [NodeName]
    public Label GlowStickLabel;

    public override void _Ready()
    {
        base._Ready();
        GlowSticksChanged();
        GlowStickController.Instance.OnGlowSticksChanged += GlowSticksChanged;
    }

    private void GlowSticksChanged()
    {
        Visible = Data.Game.GlowStickCount > 0;
        GlowStickLabel.Text = Data.Game.GlowStickCount.ToString();
        AnimateShow(true);
    }
}
