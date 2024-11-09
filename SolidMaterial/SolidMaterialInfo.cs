using Godot;

[GlobalClass]
public partial class SolidMaterialInfo : Resource
{
    [Export]
    public SolidMaterialType Type;

    [Export]
    public SoundInfo WalkSound;

    [Export]
    public SoundInfo RunSound;
}
