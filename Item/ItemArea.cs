using Godot;
using System;

[GlobalClass]
public partial class ItemArea : Area3D
{
    [Export]
    public string CustomId;

    public event Action<Item> OnItemEntered;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += go => CallDeferred(nameof(_BodyEntered), go);
    }

    private void _BodyEntered(GodotObject go)
    {
        if (!IsInstanceValid(go)) return;

        var item = go as Item;
        if (item == null) return;

        if (ValidateItem(item))
        {
            OnItemEntered?.Invoke(item);
        }
    }

    private bool ValidateItem(Item item)
    {
        var valid_id = string.IsNullOrEmpty(CustomId) || item.Data.CustomId == CustomId;
        return valid_id;
    }
}
