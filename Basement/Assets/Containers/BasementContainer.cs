using Godot;

public partial class BasementContainer : Touchable
{
    [Export]
    public string IdleClosed_AnimationName = "idle_closed";

    [Export]
    public string MoveOpen_AnimationName = "move_open";

    [NodeType]
    public AnimationPlayer Animation;

    protected bool _open;

    public override void _Ready()
    {
        base._Ready();
        Animation.Play(IdleClosed_AnimationName);
    }

    protected override void Touched()
    {
        base.Touched();
        Open();
    }

    protected void Open()
    {
        if (_open) return;
        _open = true;

        AnimateOpen();
        SetCollision_None();
    }

    protected void AnimateOpen()
    {
        Animation.Play(MoveOpen_AnimationName);
    }
}
