using Godot;
using System;
using System.Linq;

public partial class BasementController : SingletonController
{
    public override string Directory => "Basement";
    public static BasementController Instance => Singleton.Get<BasementController>();
    public Basement CurrentBasement { get; set; }

    public Action<Basement> OnBasementGenerated;

    public void GenerateBasement(BasementSettings settings)
    {
        CurrentBasement = new Basement();
        CurrentBasement.Settings = settings;
        GenerateGrid(CurrentBasement);
        GenerateRooms(CurrentBasement);
        OnBasementGenerated?.Invoke(CurrentBasement);
    }

    private void GenerateGrid(Basement basement)
    {
        var grid = BasementGridGenerator.Generate(basement.Settings);
        basement.Grid = grid;

        // Set priority rooms
        var workshop_element = GetRandomElement(AreaNames.Basement);
        workshop_element.Info = BasementRoomController.Instance.Collection.GetResource("Basement_Workshop");

        // Add special rooms
        /*
        if (!Data.Game.Flag_MaterialProcessorFixed)
        {
            var room = BasementGridGenerator.AddRoom(grid, new AddRoomSettings
            {
                AreaName = AreaNames.Basement
            });

            room.Info = GD.Load<BasementRoomInfo>(BasementRoomController.Instance.Collection.MaterialProcessorOilRoom);
        }
        */
    }

    private void UpdateRoomConnection(Basement basement, BasementRoomElement element)
    {
        if (element == null) return;
        if (!element.Info.IsConnectedToAllFromSameArea) return;

        var neis = basement.Grid.GetNeighbours(element.Coordinates, x => x.AreaName == element.AreaName);
        foreach (var nei in neis)
        {
            if (element.Connections.Contains(nei)) continue;
            element.Connections.Add(nei);
        }
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

            element.Info ??= GetRandomRoomInfo(element);
            UpdateRoomConnection(basement, element);

            var parent = IsInstanceValid(basement.Settings.RoomParent) ? basement.Settings.RoomParent : Scene.Current;
            var room = GDHelper.Instantiate<BasementRoom>(element.Info.Path, parent);
            element.Room = room;
            room.Element = element;
            room.GlobalPosition = position;

            room.North.SetOpen(element.HasNorth);
            room.East.SetOpen(element.HasEast);
            room.South.SetOpen(element.HasSouth);
            room.West.SetOpen(element.HasWest);

            room.North.SetAreaConnection(element.AreaName, element.ConnectedAreaName, element.NorthElement);
            room.East.SetAreaConnection(element.AreaName, element.ConnectedAreaName, element.EastElement);
            room.South.SetAreaConnection(element.AreaName, element.ConnectedAreaName, element.SouthElement);
            room.West.SetAreaConnection(element.AreaName, element.ConnectedAreaName, element.WestElement);
        }

        Player.Instance.GlobalPosition = elements.FirstOrDefault(x => x.IsStart).Room.GlobalPosition;

        Debug.Indent--;
    }

    private BasementRoomInfo GetRandomRoomInfo(BasementRoomElement element)
    {
        if (element.IsStart)
        {
            var room = BasementRoomController.Instance.Collection.Resources
                .Where(x => !x.Disabled && x.IsStartRoom &&
                ((x.Area == element.AreaName && x.ConnectedArea == element.ConnectedAreaName) || (x.Area == element.ConnectedAreaName && x.ConnectedArea == element.AreaName)))
                .FirstOrDefault();
            return room;
        }
        else
        {
            var room = BasementRoomController.Instance.Collection.Resources
                .Where(x => !x.Disabled && !x.IsStartRoom && x.Area == element.AreaName)
                .ToList().Random();

            return room;
        }
    }

    private BasementRoomElement GetRandomElement(string area_name)
    {
        return CurrentBasement.Grid.Elements
            .Where(x => x.AreaName == area_name && !x.IsStart && x.Info == null)
            .ToList().Random();
    }
}
