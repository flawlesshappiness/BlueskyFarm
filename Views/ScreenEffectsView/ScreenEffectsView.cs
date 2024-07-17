using Godot;

public partial class ScreenEffectsView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(ScreenEffectsView)}";

    [NodeName]
    public ColorRect BlurVignette;

    public override void _Ready()
    {
        base._Ready();
        Visible = true;
    }

    public void SetBlurVignetteEnabled(bool enabled)
    {
        BlurVignette.Visible = enabled;
    }
}
