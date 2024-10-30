using Godot;

public partial class WateringCanRefillArea : Node3DScript
{
    [NodeType]
    public Area3D Area;

    public override void _Ready()
    {
        base._Ready();
        Area.BodyEntered += v => CallDeferred(nameof(BodyEntered), v);
    }

    private void BodyEntered(GodotObject go)
    {
        if (!IsInstanceValid(go)) return;

        var node = go as Node;
        var wc = node.GetNodeInParents<WateringCan>();
        if (wc == null) return;

        RefillWateringCan(wc);
    }

    private void RefillWateringCan(WateringCan wc)
    {
        if (wc.IsFull) return;

        wc.ReplenishUses(wc.MaxUses);

        SoundController.Instance.Play("sfx_watering_can_refill", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = wc.GlobalPosition
        });
    }
}
