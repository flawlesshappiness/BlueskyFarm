using Godot;
using Godot.Collections;

public partial class MineFounderRoom : Node3D
{
    [Export]
    public Array<Touchable> DialogueTouchables;

    public override void _Ready()
    {
        base._Ready();
        DialogueTouchables.ForEach(x => Touched_Dialogue());
    }

    private void Touched_Dialogue()
    {

    }
}
