using Godot;

public partial class ForestFounderHutRoom : Node3DScript
{
    [Export]
    public Weed_Thorns BlockingWeeds;

    [Export]
    public Node3D BlueprintInventoryNode;

    [Export]
    public BlueprintInfo BlueprintInventory;

    public override void _Ready()
    {
        base._Ready();
        InitializeBlockingWeeds();
        InitializeBlueprintInventory();
    }

    private void InitializeBlockingWeeds()
    {
        BlockingWeeds.SetCut(GameFlagIds.ForestHutWeedsCut.IsTrue());
        BlockingWeeds.OnWeedCut += OnCut;

        void OnCut()
        {
            GameFlagIds.ForestHutWeedsCut.SetTrue();
        }
    }

    private void InitializeBlueprintInventory()
    {
        var bp_id = BlueprintInventory.Id;

        if (Player.HasAccessToBlueprint(bp_id)) return;
        if (Player.HasCraftedBlueprint(bp_id)) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(bp_id);
        item.SetParent(BlueprintInventoryNode);
        item.Position = Vector3.Zero;
    }
}
