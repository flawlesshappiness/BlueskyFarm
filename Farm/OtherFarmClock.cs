using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class OtherFarmClock : Node3D
{
    [Export]
    public Array<ClockRock> Rocks;

    [Export]
    public Array<ClockRock> SolutionRocks;

    public Action OnSolved;

    public override void _Ready()
    {
        base._Ready();
        Rocks.ForEach(x => x.OnToggled += RockToggled);
    }

    private void RockToggled(bool toggle)
    {
        var valid_solution = Rocks.All(x => SolutionRocks.Contains(x) == x.Toggled);

        if (valid_solution)
        {
            OnSolved?.Invoke();
        }
    }
}
