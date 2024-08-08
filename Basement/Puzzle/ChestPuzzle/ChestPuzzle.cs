using Godot;
using System.Linq;

public partial class ChestPuzzle : BasementPuzzle
{
    [NodeName]
    public KeyItem Key;

    public override void _Ready()
    {
        base._Ready();
        SetRandomChestAndKey();
    }

    private void SetRandomChestAndKey()
    {
        Key.Hide();
        Key.SetCollisionEnabled(false);

        var elements = BasementController.Instance.CurrentBasement.Grid.Elements;
        var rooms = elements.Select(x => x.Room);
        var chests = rooms.SelectMany(x => x.GetNodesInChildren<ChestPuzzleChest>())
            .Where(x => x.Visible == false);
        var boards = rooms.SelectMany(x => x.GetNodesInChildren<ChestPuzzleBoard>())
            .Where(x => x.Visible == false);
        var chest = chests.ToList().Random();
        var boards_ordered = boards
            .OrderByDescending(x => chest.GlobalPosition.DistanceTo(x.GlobalPosition));

        var board = boards_ordered.Count() < 5 ? boards_ordered.FirstOrDefault() : boards_ordered.Take(5).ToList().Random();

        if (!IsInstanceValid(chest)) return;
        if (!IsInstanceValid(board)) return;

        chest.Activate();
        board.Activate();

        var key_position = board.GetRandomKeyPosition();
        Key.GlobalPosition = key_position.GlobalPosition;
        Key.GlobalRotation = key_position.GlobalRotation;

        Key.Show();
        Key.SetCollisionEnabled(true);
    }
}
