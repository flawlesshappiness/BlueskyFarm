using Godot;

public partial class Weed : Node3DScript
{
    [NodeType]
    public AnimationPlayer Animation;

    private bool _is_cut;

    public void Cut()
    {
        if (_is_cut) return;
        _is_cut = true;

        Animation.Play("cut");
        Animation.AnimationFinished += _ => Remove();
    }

    private void Remove()
    {
        QueueFree();
    }
}
