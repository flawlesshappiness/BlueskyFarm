using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ChestPuzzleBoard : Node3DScript
{
    [NodeName]
    public Node3D KeyPositions;

    private List<Node3D> _key_positions;

    public override void _Ready()
    {
        base._Ready();
        Hide();
        this.SetCollisionEnabled(false);

        _key_positions = KeyPositions.GetNodesInChildren<Node3D>().ToList();
    }

    public void Activate()
    {
        Show();
        this.SetCollisionEnabled(true);
    }

    public Node3D GetRandomKeyPosition()
    {
        return _key_positions.Random();
    }
}
