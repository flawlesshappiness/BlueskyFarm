using Godot;

[GlobalClass]
public partial class EnemyInfo : Resource
{
    [Export(PropertyHint.File)]
    public string Path;
}
