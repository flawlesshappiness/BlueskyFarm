public partial class Item : Grabbable
{
    public ItemInfo Info { get; set; }
    public ItemInfo PlantInfo { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (GlobalPosition.Y < -50)
        {
            FarmBounds.Instance.ThrowObject(this, FirstPersonController.Instance.GlobalPosition);
        }
    }
}
