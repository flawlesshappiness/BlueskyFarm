using Godot;

public partial class BasementInventoryPuzzleRoom : Node3DScript
{
    [NodeName]
    public Node CubePositions;

    protected override void Initialize()
    {
        base.Initialize();
        CreateCubes();
    }

    private void CreateCubes()
    {
        var cube_positions = CubePositions
            .GetNodesInChildren<Node3D>()
            .TakeRandom(3);

        foreach (var node_position in cube_positions)
        {
            var item = ItemController.Instance.CreateItem("PuzzleCube");
            item.SetParent(node_position);
            item.ResetBodyPosition();
        }
    }
}
