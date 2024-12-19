using Godot;
using System.Linq;

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
        var nodes = CubePositions
            .GetChildren()
            .Select(x => x as Node3D)
            .Where(x => x != null)
            .TakeRandom(3);

        foreach (var node in nodes)
        {
            var item = ItemController.Instance.CreateItem("PuzzleCube", new CreateItemSettings
            {
                Parent = node
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
