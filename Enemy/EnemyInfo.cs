using Godot;

[GlobalClass]
public partial class EnemyInfo : Resource
{
    [Export]
    public PackedScene Scene;

    [Export]
    public bool Enabled;

    [Export]
    public bool IsDangerous;
}
