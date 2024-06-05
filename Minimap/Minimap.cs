using Godot;
using System.Collections.Generic;

public partial class Minimap : ControlScript
{
    [NodeName(nameof(Scroll))]
    public Control Scroll;

    [NodeName(nameof(Rotate))]
    public Control Rotate;

    [NodeName(nameof(RoomTemplate))]
    public Control RoomTemplate;

    [NodeName(nameof(Player))]
    public Control Player;

    public static int RoomSectionWorldSize => BasementRoom.SECTION_SIZE;
    public static int RoomSectionCount => BasementRoom.SECTION_COUNT;
    public static int RoomWorldSize => RoomSectionCount * RoomSectionWorldSize;
    public static int RoomSectionMapSize => MinimapRoom.SECTION_SIZE;
    public static int RoomMapSize => RoomSectionCount * MinimapRoom.SECTION_SIZE;

    private class MinimapRoom
    {
        public const int SECTION_SIZE = 8;
        public BasementRoomElement GridElement { get; set; }
        public Control Control { get; set; }
    }

    public override void _Ready()
    {
        base._Ready();

        RoomTemplate.Visible = false;

        BasementController.Instance.OnGridGenerated += GridCreated;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var pos_player = -FirstPersonController.Instance.GlobalPosition / RoomSectionWorldSize * RoomSectionMapSize;
        Scroll.Position = new Vector2(pos_player.X, pos_player.Z) + Player.Position;

        Rotate.Rotation = FirstPersonController.Instance.Neck.Rotation.Y;
    }

    private void GridCreated(Grid<BasementRoomElement> grid)
    {
        var minimap_rooms = new List<MinimapRoom>();
        foreach (var element in grid.Elements)
        {
            var minimap_room = CreateMinimapRoom(element);
            minimap_rooms.Add(minimap_room);
        }
    }

    private MinimapRoom CreateMinimapRoom(BasementRoomElement element)
    {
        // Create map room control
        var control = CreateRoomControl(element);
        control.Position = new Vector2(element.Coordinates.X, -element.Coordinates.Y) * RoomMapSize;
        control.Name = $"Room {element.Coordinates}";

        // Offset based on half the size of the map room
        var v_offset = (-RoomMapSize * 0.5f) + (RoomSectionMapSize * 0.5f);
        var offset = new Vector2(v_offset, v_offset);
        control.Position += offset;

        // Create MinimapRoom
        var minimap_room = new MinimapRoom
        {
            GridElement = element,
            Control = control
        };

        return minimap_room;
    }

    private Control CreateRoomControl(BasementRoomElement element)
    {
        var control = RoomTemplate.Duplicate() as Control;
        control.SetParent(RoomTemplate.GetParent());
        control.Visible = true;

        var section_template = control.GetNodeInChildren<ColorRect>("SectionTemplate");
        section_template.Visible = false;

        var section_parent = section_template.GetParent();

        var layout = element.Info.RoomLayout?.Replace("\n", "") ?? "";

        for (int y = 0; y < RoomSectionCount; y++)
        {
            for (int x = 0; x < RoomSectionCount; x++)
            {
                var i_layout = x + y * RoomSectionCount;
                var is_section_in_layout = IsSectionActiveInLayout(layout, i_layout, element);

                var section = section_template.Duplicate() as ColorRect;
                section.SetParent(section_parent);
                section.Visible = true;
                section.Color = is_section_in_layout ? section.Color : Colors.Transparent;
            }
        }

        return control;
    }

    private bool IsSectionActiveInLayout(string layout, int i, BasementRoomElement element)
    {
        var c = layout.Length > i ? layout[i] : '0';
        var is_north = c == 'N' && element.Room.North.IsOpen;
        var is_east = c == 'E' && element.Room.East.IsOpen;
        var is_south = c == 'S' && element.Room.South.IsOpen;
        var is_west = c == 'W' && element.Room.West.IsOpen;
        var is_active = c == '1';
        return is_north || is_east || is_south || is_west || is_active;
    }
}
