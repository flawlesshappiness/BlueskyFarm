using Godot;

public partial class FarmForge : Node3D
{
    [Export]
    public ForgeKiln Kiln;

    [Export]
    public ForgeMachine Forge;

    public override void _Ready()
    {
        base._Ready();
        Kiln.OnActivated += KilnActivated;
        Forge.Lever.Touchable.OnTouched += Touched_Forge;
        Kiln.Lever.Touchable.OnTouched += Touched_Forge;
    }

    private void KilnActivated()
    {
        DialogueFlags.SetFlagMin(DialogueFlags.FrogForge, 3);
        Forge.AnimateActivate(2f);
    }

    private void Touched_Forge()
    {
        DialogueFlags.SetFlagMin(DialogueFlags.FrogForge, 1);
    }
}
