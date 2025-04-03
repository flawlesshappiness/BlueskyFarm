using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class BasementRoom : Node3DScript
{
    public const int SECTION_SIZE = 4;
    public const int SECTION_COUNT = 7;
    public static int ROOM_SIZE => SECTION_SIZE * SECTION_COUNT;

    [Export]
    public Node3D Ceiling;

    [Export]
    public Node3D Floor;

    [Export]
    public Node3D Walls;

    [Export]
    public PlayerArea PlayerArea;

    [Export]
    public BasementRoomSection North;

    [Export]
    public BasementRoomSection East;

    [Export]
    public BasementRoomSection South;

    [Export]
    public BasementRoomSection West;

    [Export]
    public Node3D EnemyMarker;

    public BasementRoomElement Element { get; set; }

    public override void _Ready()
    {
        base._Ready();

        PlayerArea.OnPlayerEntered += PlayerEntered;
        PlayerArea.OnPlayerExited += PlayerExited;

        if (IsInstanceValid(Ceiling))
        {
            Ceiling.Visible = true;
        }

        if (IsInstanceValid(Floor))
        {
            Floor.Visible = true;
        }

        if (IsInstanceValid(Walls))
        {
            Walls.Visible = true;
        }
    }

    public void InitializeAfterGeneration()
    {
        North.SetOpen(Element.HasNorth);
        East.SetOpen(Element.HasEast);
        South.SetOpen(Element.HasSouth);
        West.SetOpen(Element.HasWest);

        UpdateSectionConnection(North, Element.NorthElement);
        UpdateSectionConnection(East, Element.EastElement);
        UpdateSectionConnection(South, Element.SouthElement);
        UpdateSectionConnection(West, Element.WestElement);
    }

    private void UpdateSectionConnection(BasementRoomSection section, BasementRoomElement neighbour)
    {
        if (neighbour == null) return;

        UpdateSectionAreaConnection(section, neighbour);
        UpdateSectionStartConnection(section, neighbour);
    }

    private void UpdateSectionAreaConnection(BasementRoomSection section, BasementRoomElement neighbour)
    {
        if (section.Areas.Any(x => x.Name == neighbour.AreaName))
        {
            section.SetArea(neighbour.AreaName);
        }
    }

    private void UpdateSectionStartConnection(BasementRoomSection section, BasementRoomElement neighbour)
    {
        if (neighbour.IsStart)
        {
            section.SetStart();
        }
        else
        {
            section.SetNotStart();
        }
    }

    private void PlayerEntered(Player player)
    {
        BasementController.Instance.EnterRoom(Element);
    }

    private void PlayerExited(Player player)
    {
    }

    public IEnumerable<ItemContainer> GetContainers()
    {
        return this.GetNodesInChildren<ItemContainer>()
            .Where(x => x.IsVisibleInTree());
    }
}
