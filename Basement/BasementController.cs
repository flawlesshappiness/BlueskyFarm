using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BasementController : SingletonController
{
    public override string Directory => "Basement";
    public static BasementController Instance => Singleton.Get<BasementController>();

    public Action<Grid<BasementRoomElement>> OnGridGenerated;

    private Grid<BasementRoomElement> current_grid { get; set; }

    public override void _Ready()
    {
        base._Ready();

        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "Basement";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Generate",
            Action = _ => DebugGenerateBasement()
        });
    }

    public void DebugGenerateBasement()
    {
        var settings = new BasementSettings
        {
            MaxRooms = 25,
            MaxCorridorDepth = 7,
        };

        current_grid = BasementGridGenerator.Generate(settings);

        BasementGridGenerator.LogGrid(current_grid);

        GenerateRooms(current_grid);
        GenerateItems(current_grid);

        OnGridGenerated?.Invoke(current_grid);
    }

    private void GenerateRooms(Grid<BasementRoomElement> grid)
    {
        Debug.TraceMethod();
        Debug.Indent++;

        var offset = new Vector3(0, 0, 0);
        var room_size = new Vector3(28, 4, 28);

        var elements = grid.Elements;
        foreach (var element in elements)
        {
            var coords = element.Coordinates;
            var x = coords.X * room_size.X;
            var z = -coords.Y * room_size.Z;
            var position = offset + new Vector3(x, 0, z);

            var info = GetRandomRoomInfo(element);
            var room = GDHelper.Instantiate<BasementRoom>(info.Path, Scene.Current);
            room.GlobalPosition = position;

            room.North.SetOpen(element.HasNorth);
            room.East.SetOpen(element.HasEast);
            room.South.SetOpen(element.HasSouth);
            room.West.SetOpen(element.HasWest);

            element.Room = room;
            element.Info = info;
        }

        FirstPersonController.Instance.GlobalPosition = elements.FirstOrDefault(x => x.IsStart).Room.GlobalPosition;

        Debug.Indent--;
    }

    private void GenerateItems(Grid<BasementRoomElement> grid)
    {
        // Find positions
        var item_positions = new List<Node3D>();
        foreach (var element in grid.Elements)
        {
            var items_nodes = element.Room.GetNodesInChildren<Node3D>(n => n.Name == "Items");

            foreach (var items_node in items_nodes)
            {
                var positions = items_node.GetChildren().Cast<Node3D>();
                item_positions.AddRange(positions);
            }
        }

        // Spawn
        var item_count = 10;
        while (item_positions.Count > 0 && item_count > 0)
        {
            var position = item_positions.Random();
            item_positions.Remove(position);

            var item = ItemController.Instance.SpawnCoin();
            item.GlobalPosition = position.GlobalPosition;

            item_count--;
        }
    }

    private BasementRoomInfo GetRandomRoomInfo(BasementRoomElement element)
    {
        if (element.IsStart)
        {
            var room = BasementRoomController.Instance.Collection.Resources.FirstOrDefault(x => x.IsStartRoom);
            return room;
        }
        else
        {
            var room = BasementRoomController.Instance.Collection.Resources
                .Where(x => !x.IsStartRoom)
                .ToList().Random();

            return room;
        }
    }
}
