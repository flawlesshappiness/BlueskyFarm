using Godot;

public partial class ScreenshotAnimationPlayer : AnimationPlayer
{
    [Export]
    public string Animation;

    public override void _Ready()
    {
        base._Ready();
        Play(Animation);
    }
}
