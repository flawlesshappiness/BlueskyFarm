using Godot;

public partial class Dreamcatcher : Node3D
{
    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public float MinAnimationSpeed = 1f;

    [Export]
    public float MaxAnimationSpeed = 1f;

    [Export]
    public string InitialAnimation;

    public override void _Ready()
    {
        base._Ready();
        var rng = new RandomNumberGenerator();
        var speed = rng.RandfRange(MinAnimationSpeed, MaxAnimationSpeed);
        AnimationPlayer.SpeedScale = speed;
        AnimationPlayer.Play(InitialAnimation);

        var rot = rng.RandfRange(0, 360);
        GlobalRotationDegrees = Vector3.Up * rot;
    }
}
