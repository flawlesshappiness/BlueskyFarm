using Godot;

public partial class SporeMushroomModel : Node3DScript
{
    [NodeType]
    public AnimationPlayer Animator;

    public override void _Ready()
    {
        base._Ready();
        AnimateIdle();
    }

    public void AnimateIdle()
    {
        Animator.Play("idle");
    }

    public void AnimateAppear()
    {
        Animator.Play("appear");
    }
}
