using Godot;
using System.Linq;

public partial class BasementController : SingletonController
{
    public override string Directory => "Basement";

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
            MaxRooms = 10,
            MaxCorridorDepth = 7,
        };

        var grid = BasementGridGenerator.Generate(settings);

        BasementGridGenerator.LogGrid(grid);

        GenerateRooms(grid);
    }

    private void GenerateRooms(Grid<BasementRoomElement> grid)
    {
        Debug.TraceMethod();
        Debug.Indent++;

        var offset = new Vector3(300, 0, 0);
        var room_size = new Vector3(28, 4, 28);

        var elements = grid.Elements;
        foreach (var element in elements)
        {
            var coords = element.Coordinates;
            var x = coords.X * room_size.X;
            var z = -coords.Y * room_size.Z;
            var position = offset + new Vector3(x, 0, z);

            var info = GetRandomRoomInfo(element);
            var room = GDHelper.Instantiate<BasementRoom>(info.Path);
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
