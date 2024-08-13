using Godot;

public partial class GlowStick : Item
{
    [NodeType]
    public OmniLight3D Light;

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_Light();
    }

    private void Process_Light()
    {
        Light.GlobalPosition = GlobalPosition + Vector3.Up * 0.5f;
    }
}
