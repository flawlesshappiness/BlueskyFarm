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
        var activated = GameFlagIds.MineKilnActivated.IsTrue();
        Kiln.SetActivated(activated);
        Forge.SetActivated(activated);
        Kiln.OnActivated += KilnActivated;

        if (!activated)
        {
            Forge.AnimateDeactivated();
        }
    }

    private void KilnActivated()
    {
        GameFlagIds.MineKilnActivated.SetTrue();
        DialogueFlags.SetFlagMin(DialogueFlags.FrogForge, 3);
        Forge.SetActivated(true);
        Forge.AnimateActivate(2f);
    }

    private void Touched_Forge()
    {
        DialogueFlags.SetFlagMin(DialogueFlags.FrogForge, 1);
    }
}
