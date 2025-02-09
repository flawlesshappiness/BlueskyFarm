using Godot;

public partial class PickaxeItem : Item
{
    [Export]
    public PickaxeTool PickaxeTool;

    public override void Use()
    {
        base.Use();
        PickaxeTool.AnimateUse();
    }
}
