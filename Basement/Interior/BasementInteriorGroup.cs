using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class BasementInteriorGroup : Node3DScript
{
    private IEnumerable<BasementInterior> _interiors;

    public override void _Ready()
    {
        base._Ready();

        _interiors = this.GetNodesInChildren<BasementInterior>()
            .Where(x => !x.ExcludedFromGroup);

        SetRandomInterior();
    }

    public void HideAllInteriors()
    {
        foreach (var node in _interiors)
        {
            node.Hide();
            node.SetCollisionEnabled(false);
        }
    }

    public void SetInterior(Node3D node)
    {
        if (node == null) return;

        HideAllInteriors();
        node.Show();
        node.SetCollisionEnabled(true);
    }

    public void SetRandomInterior()
    {
        var interior = _interiors.ToList().Random();
        SetInterior(interior);
    }
}
