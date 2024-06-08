public partial class Item : Grabbable
{
    public ItemInfo Info { get; set; }
    public ItemInfo PlantInfo { get; set; }
    public bool IsBeingHandled { get; set; } // If the item is currently being handled by a script, other scripts will ignore this item

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (GlobalPosition.Y < -50)
        {
            FarmBounds.Instance.ThrowObject(this, FirstPersonController.Instance.GlobalPosition);
        }
    }
}
