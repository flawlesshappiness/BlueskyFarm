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
        for (int i = 0; i < settings.Areas.Count; i++)
        {
            var area = settings.Areas[i];
            GenerateArea(grid, area);
        }

        Debug.Indent--;
        return grid;
    }

    private static bool GenerateArea(Grid<BasementRoomElement> grid, BasementSettingsArea area)
    {
        var start = CreateStartElement(grid, area);
        var remaining_rooms = area.MaxRooms;
        while (remaining_rooms > 0)
        {
            var current = grid.Elements
                .Where(x => x.AreaName == area.AreaName)
                .ToList()
                .Random() ?? start;

            var depth = 0;
            while (depth < area.MaxCorridorDepth && remaining_rooms > 0)
            {
                var available_coords = grid.GetEmptyNeighbourCoordinates(current.Coordinates);
                if (!available_coords.Any()) break;

                var coords = available_coords.ToList().Random();
                var next = new BasementRoomElement
                {
                    AreaName = area.AreaName,
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

        return true;
    }

    private static BasementRoomElement CreateStartElement(Grid<BasementRoomElement> grid, BasementSettingsArea area)
    {
        if (string.IsNullOrEmpty(area.ConnectedAreaName))
        {
            var start = new BasementRoomElement
            {
                AreaName = area.AreaName,
                Coordinates = new Vector2I(0, 0),
                IsStart = true
            };

            grid.Set(0, 0, start);
            return start;
        }
        else
        {
            var elements = grid.Elements.Where(x => x.AreaName == area.ConnectedAreaName && !x.IsStart);
            var empties = elements.SelectMany(x => grid.GetEmptyNeighbourCoordinates(x.Coordinates))
                .OrderByDescending(x => grid.GetEmptyNeighbourCoordinates(x).Count());
            var position = empties.FirstOrDefault();
            var start = new BasementRoomElement
            {
                AreaName = area.AreaName,
                ConnectedAreaName = area.ConnectedAreaName,
                Coordinates = position,
                IsStart = true
            };

            var connected_element = grid.GetNeighbours(position).FirstOrDefault();
            start.Connections.Add(connected_element);
            connected_element.Connections.Add(start);

            grid.Set(position, start);
            return start;
        }
    }

    public static BasementRoomElement AddRoom(Grid<BasementRoomElement> grid, AddRoomSettings settings)
    {
        if (settings.RoomInfo == null)
        {
            Debug.LogError("Added basement room with no info");
        }

        var valid_rooms = grid.Elements
            .Where(x => !x.IsStart)
            .Where(x => string.IsNullOrEmpty(settings.AreaName) || x.AreaName == settings.AreaName)
            .Where(x => grid.GetEmptyNeighbourCoordinates(x.Coordinates).Count() > 0);

        var valid_room = valid_rooms.ToList().Random();

        var coord = grid.GetEmptyNeighbourCoordinates(valid_room.Coordinates).ToList().Random();

        var new_room = new BasementRoomElement
        {
            Coordinates = coord,
            AreaName = valid_room.AreaName
        };

        grid.Set(coord, new_room);
        new_room.Connections.Add(valid_room);
        valid_room.Connections.Add(new_room);

        new_room.Info = settings.RoomInfo;

        return new_room;
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

public class AddRoomSettings
{
    public string AreaName { get; set; }
    public BasementRoomInfo RoomInfo { get; set; }
}

public class BasementSettings
{
    public Node RoomParent { get; set; }
    public List<BasementSettingsArea> Areas { get; set; }
}

public class BasementSettingsArea
{
    public string AreaName { get; set; }
    public string ConnectedAreaName { get; set; }
    public int MaxCorridorDepth { get; set; }
    public int MaxRooms { get; set; }
}

public class BasementRoomElement
{
    public Vector2I Coordinates { get; set; }
    public List<BasementRoomElement> Connections { get; set; } = new();
    public BasementRoom Room { get; set; }
    public BasementRoomInfo Info { get; set; }
    public string AreaName { get; set; }
    public string ConnectedAreaName { get; set; }
    public bool IsStart { get; set; }

    public bool HasNorth => NorthElement != null;
    public bool HasSouth => SouthElement != null;
    public bool HasEast => EastElement != null;
    public bool HasWest => WestElement != null;

    public BasementRoomElement NorthElement => Connections.FirstOrDefault(x => x.Coordinates.Y > Coordinates.Y);
    public BasementRoomElement SouthElement => Connections.FirstOrDefault(x => x.Coordinates.Y < Coordinates.Y);
    public BasementRoomElement EastElement => Connections.FirstOrDefault(x => x.Coordinates.X > Coordinates.X);
    public BasementRoomElement WestElement => Connections.FirstOrDefault(x => x.Coordinates.X < Coordinates.X);
}
