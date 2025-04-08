using Godot;

public partial class FrogCutscene : Node3D
{
    [Export]
    public string StartAnimation = "Armature|Idle";

    [Export]
    public AnimationPlayer AnimationPlayer;

    public override void _Ready()
    {
        base._Ready();
        AnimationPlayer.Play(StartAnimation);
    }
}
