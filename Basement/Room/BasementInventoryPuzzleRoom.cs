using Godot;

public partial class BasementInventoryPuzzleRoom : Node3DScript
{
    [NodeName]
    public Node CubePositions;

    [NodeName]
    public Node ItemPosition;

    [NodeType]
    public BasementWallGatedHole RewardWall;

    [NodeType]
    public PuzzleCubeWall PuzzleWall;

    public override void _Ready()
    {
        base._Ready();
        CreateCubes();
        CreateRewardItem();

        PuzzleWall.OnSequenceCompleted += SequenceCompleted;
    }

    private void CreateCubes()
    {
        var nodes = CubePositions.GetChildren();

        foreach (var node in nodes)
        {
            var n3 = node as Node3D;
            if (n3 == null) continue;

            var item = ItemController.Instance.CreateItem("PuzzleCube", new CreateItemSettings
            {
                Parent = n3
            });

            item.Position = Vector3.Zero;
        }
    }

    private void CreateRewardItem()
    {
        var item = BlueprintController.Instance.CreateBlueprintRoll("inventory_001");
        item.SetParent(ItemPosition);
        item.Position = Vector3.Zero;
    }

    private void SequenceCompleted()
    {
        RewardWall.AnimateOpenGate();
    }
}
