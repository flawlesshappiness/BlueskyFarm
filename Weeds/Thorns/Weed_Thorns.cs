using Godot;
using System.Collections.Generic;

public partial class Weed_Thorns : Weed
{
    [NodeName]
    public Node3D Models;

    private List<ThornedStalk> _thorns = new();

    public override void _Ready()
    {
        base._Ready();
        _thorns = Models.GetNodesInChildren<ThornedStalk>();
    }

    public override void Cut()
    {
        base.Cut();

        foreach (var thorn in _thorns)
        {
            thorn.AnimateCut();
        }
    }

    public void SetCut(bool is_cut)
    {
        foreach (var thorn in _thorns)
        {
            thorn.SetCut(is_cut);
        }

        Touchable.SetEnabled(!is_cut);
    }
}
