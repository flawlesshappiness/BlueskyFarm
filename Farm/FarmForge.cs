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
        InitializeKiln();

        Forge.Lever.Touchable.OnTouched += Touched_Forge;
        Kiln.Lever.Touchable.OnTouched += Touched_Forge;
    }

    private void InitializeKiln()
    {
        Kiln.SetActivated(GameFlagIds.MineKilnActivated.IsTrue());
        Forge.SetActivated(GameFlagIds.MineKilnActivated.IsTrue());
        Kiln.OnActivated += KilnActivated;
    }

    private void KilnActivated()
    {
        GameFlagIds.MineKilnActivated.SetTrue();
        DialogueFlags.SetFlagMin(DialogueFlags.FrogForge, 3);
        Forge.SetActivated(true);
    }

    private void Touched_Forge()
    {
        DialogueFlags.SetFlagMin(DialogueFlags.FrogForge, 1);
    }
}
