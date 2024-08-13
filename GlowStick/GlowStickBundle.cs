using Godot;

public partial class GlowStickBundle : Item
{
    [Export]
    public int Count;

    public override void AddToInventory()
    {
        GlowStickController.Instance.AdjustGlowSticks(3);
    }
}
