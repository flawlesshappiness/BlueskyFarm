using Godot;
using System.Collections.Generic;
using System.Linq;

public static class BasementGridGenerator
{
    public static Grid<BasementRoomElement> Generate(BasementSettings settings)
    {
        Debug.TraceMethod();
        Debug.Indent++;

        var grid = new Grid<BasementRoomElement>();
        var root = new BasementRoomElement();
        root.IsStart = true;

        grid.Set(0, 0, root);

        var remaining_rooms = settings.MaxRooms - 1;
        while (remaining_rooms > 0)
        {
            var current = grid.Elements.ToList().Random();
            var depth = 0;
            while (depth < settings.MaxCorridorDepth && remaining_rooms > 0)
            {
                var available_coords = grid.GetEmptyNeighbourCoordinates(current.Coordinates);
                if (!available_coords.Any()) break;

                var coords = available_coords.ToList().Random();
                var next = new BasementRoomElement
                {
                    Coordinates = coords,
                };

                grid.Set(coords, next);

                current.Connections.Add(next);
                next.Connections.Add(current);

                current = next;

                remaining_rooms--;
                depth++;
            }
        }


        Debug.Indent--;
        return grid;
    }

    public static void LogGrid(Grid<BasementRoomElement> grid)
    {
        var x_lowest = grid.Elements.OrderBy(x => x.Coordinates.X).First().Coordinates.X;
        var x_highest = grid.Elements.OrderByDescending(x => x.Coordinates.X).First().Coordinates.X;
        var y_lowest = grid.Elements.OrderBy(x => x.Coordinates.Y).First().Coordinates.Y;
        var y_highest = grid.Elements.OrderByDescending(x => x.Coordinates.Y).First().Coordinates.Y;

        for (int y = y_highest + 1; y > y_lowest - 2; y--)
        {
            var s = "";
            for (int x = x_lowest - 1; x < x_highest + 2; x++)
            {
                var p = new Vector2I(x, y);
                grid.TryGet(p, out var room);
                s += room == null ? "X" : "_";
            }
            Debug.Log(s);
        }
    }
}

public class BasementSettings
{
    public int MaxCorridorDepth { get; set; }
    public int MaxRooms { get; set; }
    public List<string> SpawnItemInfoPaths { get; set; }
    public List<string> SeedPlantInfoPaths { get; set; }
}

public class BasementRoomElement
{
    public Vector2I Coordinates { get; set; }
    public List<BasementRoomElement> Connections { get; set; } = new();
    public BasementRoom Room { get; set; }
    public BasementRoomInfo Info { get; set; }
    public bool IsStart { get; set; }

    public bool HasNorth => Connections.Any(x => x.Coordinates.Y > Coordinates.Y);
    public bool HasSouth => Connections.Any(x => x.Coordinates.Y < Coordinates.Y);
    public bool HasEast => Connections.Any(x => x.Coordinates.X > Coordinates.X);
    public bool HasWest => Connections.Any(x => x.Coordinates.X < Coordinates.X);
}
