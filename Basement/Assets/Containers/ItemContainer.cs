using Godot;

public partial class ItemContainer : Node3DScript
{
    public Item Item { get; set; }

    protected void SpawnItem(Vector3 position, Vector3 velocity)
    {
        if (!IsInstanceValid(Item)) return;

        Item.Freeze = false;
        Item.SetEnabled(true);
        Item.GlobalPosition = position;
        Item.LinearVelocity = velocity;
    }
}
