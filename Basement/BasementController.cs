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
        OnBasementGenerated?.Invoke(CurrentBasement);
    }

    private void GenerateGrid(Basement basement)
    {
        var grid = BasementGridGenerator.Generate(basement.Settings);
        basement.Grid = grid;

        GenerateRooms(basement);
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
            var parent = IsInstanceValid(basement.Settings.RoomParent) ? basement.Settings.RoomParent : Scene.Current;
            var room = GDHelper.Instantiate<BasementRoom>(info.Path, parent);
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

    private BasementRoomInfo GetRandomRoomInfo(BasementRoomElement element)
    {
        if (element.IsStart)
        {
            var room = BasementRoomController.Instance.Collection.Resources
                .Where(x => !x.Disabled)
                .FirstOrDefault(x => x.IsStartRoom);
            return room;
        }
        else
        {
            var room = BasementRoomController.Instance.Collection.Resources
                .Where(x => !x.Disabled && !x.IsStartRoom)
                .ToList().Random();

            return room;
        }
    }
}
