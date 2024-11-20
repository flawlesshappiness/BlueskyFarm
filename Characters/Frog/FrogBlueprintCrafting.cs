using Godot;
using System.Collections;

public partial class FrogBlueprintCrafting : Node3DScript
{
    [NodeName]
    public Area3D ItemArea;

    [NodeType]
    public BlueprintDisplay Display;

    private FrogCharacter _character;

    public override void _Ready()
    {
        base._Ready();
        ItemArea.BodyEntered += go => CallDeferred(nameof(BodyEntered), go);
        Display.OnCancel += OnCancel;
        _character = this.GetNodeInParents<FrogCharacter>();

        LoadData();
    }

    private void LoadData()
    {
        var has_data = Data.Game.BlueprintCraftingData != null;
        Display.SetData(Data.Game.BlueprintCraftingData);
        Display.SetVisible(has_data);
        Display.SetCancelEnabled(has_data);
    }

    private void BodyEntered(GodotObject go)
    {
        var node = go as Node3D;
        if (node == null) return;

        var item = node.GetNodeInParents<Item>();
        if (IsItemValid(item))
        {
            SetBlueprint(item);
        }
    }

    private void OnCancel()
    {
        StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            Display.SetCancelEnabled(false);
            yield return Display.AnimateHide();

            ItemArea.Disable();
            ThrowBlueprintToPlayer(Data.Game.BlueprintCraftingData);
            Data.Game.BlueprintCraftingData = null;

            yield return new WaitForSeconds(2f);

            ItemArea.Enable();
        }
    }

    private void ThrowBlueprintToPlayer(BlueprintCraftingData data)
    {
        if (data == null) return;

        var item_bp = BlueprintController.Instance.CreateBlueprintRoll(data.Id);
        ThrowItemToPlayer(item_bp);

        SoundController.Instance.Play("sfx_throw_light", item_bp.GlobalPosition);
    }

    private bool IsItemValid(Item item)
    {
        return item != null && !string.IsNullOrEmpty(item.Data.Blueprint?.Id);
    }

    private void SetBlueprint(Item item)
    {
        var bp_info = BlueprintController.Instance.GetInfo(item.Data.Blueprint.Id);
        Data.Game.BlueprintCraftingData = new BlueprintCraftingData
        {
            Id = item.Data.Blueprint.Id,
            VegetableCount = bp_info.VegetableCount
        };

        Display.SetData(Data.Game.BlueprintCraftingData);

        StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            yield return item.AnimateDisappearAndQueueFree();
            yield return Display.AnimateShow();
            Display.SetCancelEnabled(true);
        }
    }

    private void ThrowItemToPlayer(Item item)
    {
        var start_position = _character.GlobalPosition.Add(y: 1);
        var direction_to_player = start_position.DirectionTo(Player.Instance.MidPosition).Normalized();
        var direction = direction_to_player.Add(y: 0.5f);
        var velocity = direction * 6f;

        item.GlobalPosition = start_position;
        item.RigidBody.LinearVelocity = velocity;
    }
}
