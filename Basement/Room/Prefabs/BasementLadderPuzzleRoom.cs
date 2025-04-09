using Godot;
using System.Collections;

public partial class BasementLadderPuzzleRoom : Node3D
{
    [Export]
    public Marker3D InventoryItemMarker;

    [Export]
    public ItemInfo InventoryItem;

    [Export]
    public AnimationPlayer AnimationPlayer_Ladder;

    [Export]
    public LadderArea LadderArea;

    [Export]
    public CuttableRope CuttableRope;

    [Export]
    public Node3D Rope;

    public override void _Ready()
    {
        base._Ready();
        InitializeInventoryItem();
        InitializeLadder();
    }

    private void InitializeInventoryItem()
    {
        if (GameFlagIds.BasementLadderInventoryItemPicked.IsTrue()) return;

        var item = ItemController.Instance.CreateItem(InventoryItem);
        item.SetParent(InventoryItemMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;

        item.OnPickUp += () =>
        {
            GameFlagIds.BasementLadderInventoryItemPicked.SetTrue();
        };
    }

    private void InitializeLadder()
    {
        var repaired = GameFlagIds.BasementLadderRepaired.IsTrue();
        var anim = repaired ? "idle_down" : "idle_up";
        AnimationPlayer_Ladder.Play(anim);
        LadderArea.SetEnabled(repaired);
        Rope.SetEnabled(!repaired);

        CuttableRope.OnCut += () =>
        {
            GameFlagIds.BasementLadderRepaired.SetTrue();
            Rope.SetEnabled(false);
            this.StartCoroutine(Cr, "cut_rope");
        };

        IEnumerator Cr()
        {
            yield return AnimationPlayer_Ladder.PlayAndWaitForAnimation("move_down");
            LadderArea.SetEnabled(true);
        }
    }
}
