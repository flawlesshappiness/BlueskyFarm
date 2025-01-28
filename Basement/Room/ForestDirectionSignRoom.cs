using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ForestDirectionSignRoom : Node3DScript
{
    [Export]
    public SignDirection Sign;

    protected override void Initialize()
    {
        base.Initialize();
        FindDirections();
    }

    private void FindDirections()
    {
        var area_names = new List<string>() { AreaNames.Forest, AreaNames.Mine, AreaNames.Cult };

        var area_rooms = BasementController.Instance.CurrentBasement.Grid.Elements
            .Where(x => x.IsStart && area_names.Any(area => x.AreaName == area.ToString()))
            .Select(x => x.Room)
            .ToList();

        // Create directions
        foreach (var room in area_rooms)
        {
            var area_name = room.Element.Info.Area == AreaNames.Forest ? AreaNames.Basement : room.Element.Info.Area;
            Sign.CreateDirection($"##{area_name.ToUpper()}##", room.GlobalPosition);
        }
    }
}
