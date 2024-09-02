using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class BasementInteriorGroup : Node3DScript
{
    private IEnumerable<BasementInterior> _interiors;
    private IEnumerable<BasementInterior> _active_interiors;

    public override void _Ready()
    {
        base._Ready();

        _interiors = this.GetNodesInChildren<BasementInterior>();
        _active_interiors = _interiors
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
        var interior = _active_interiors.ToList().Random();
        SetInterior(interior);
    }
}
