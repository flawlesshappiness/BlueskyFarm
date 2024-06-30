using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BasementController : SingletonController
{
    public override string Directory => "Basement";
    public static BasementController Instance => Singleton.Get<BasementController>();
    public Basement CurrentBasement { get; set; }

    public Action<Basement> OnBasementGenerated;

    public override void _Ready()
    {
        base._Ready();

        //RegisterDebugActions();
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
        CurrentBasement = new Basement();
        GenerateGrid(CurrentBasement);
        OnBasementGenerated?.Invoke(CurrentBasement);
    }

    private void GenerateGrid(Basement basement)
    {
        basement.Settings = new BasementSettings
        {
            MaxRooms = 5,
            MaxCorridorDepth = 3,
            SpawnItemPaths = new List<string> { ItemController.Instance.Collection.Seed }
        };

        var grid = BasementGridGenerator.Generate(basement.Settings);
        basement.Grid = grid;

        GenerateRooms(basement);
        GenerateItems(basement);
    }

    private void GenerateRooms(Basement basement)
    {
        Debug.TraceMethod();
        Debug.Indent++;

        var offset = new Vector3(0, 0, 0);
        var room_size = new Vector3(28, 4, 28);

        var elements = basement.Grid.Elements;
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

    private void GenerateItems(Basement basement)
    {
        // Find positions
        var item_positions = new List<Node3D>();
        foreach (var element in basement.Grid.Elements)
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

            var item_path = basement.Settings.SpawnItemPaths.Random();
            var item = ItemController.Instance.CreateItem(item_path);
            item.GlobalPosition = position.GlobalPosition;

            basement.Items.Add(item);

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
