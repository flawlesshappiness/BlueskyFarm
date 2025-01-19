using Godot;

public partial class FarmShed : Node3D
{
    [Export]
    public UnlockableGroup UnlockGroup;

    private string DebugCategory = "SHED";

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();

        UnlockGroup.SetNotUnlocked();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Debug.RemoveActions(DebugCategory);
    }

    private void RegisterDebugActions()
    {
        Debug.RegisterAction(new DebugAction
        {
            Id = DebugCategory,
            Category = DebugCategory,
            Text = "Unlock Shed",
            Action = v => UnlockGroup.AnimateUnlock()
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = DebugCategory,
            Category = DebugCategory,
            Text = "Lock Shed",
            Action = v => UnlockGroup.SetNotUnlocked()
        });
    }
}
