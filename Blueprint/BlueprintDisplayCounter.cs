using Godot;

public partial class BlueprintDisplayCounter : Node3DScript
{
    [NodeType]
    public Sprite3D Icon;

    [NodeType]
    public Label3D Label;

    public void SetValue(int value)
    {
        Label.Text = $"x{value}";
        Icon.Modulate = Icon.Modulate.SetA(value > 0 ? 1 : 0.5f);
    }
}
