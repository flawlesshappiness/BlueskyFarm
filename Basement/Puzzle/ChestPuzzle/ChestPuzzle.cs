public partial class ChestPuzzle : BasementPuzzle
{
    /*
    public override void _Ready()
    {
        base._Ready();
        SetRandomChestAndKey();
    }

    private void SetRandomChestAndKey()
    {
        var key = CreateKey();
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
        key.GlobalPosition = key_position.GlobalPosition;
        key.GlobalRotation = key_position.GlobalRotation;
        key.Data.CustomId = chest.CustomId;
        key.RigidBody.LockPosition_All();
        key.RigidBody.LockRotation_All();

        key.OnGrabbed += UnlockKey;

        void UnlockKey()
        {
            key.RigidBody.UnlockPosition_All();
            key.RigidBody.UnlockRotation_All();
            key.OnGrabbed -= UnlockKey;
        }
    }

    private Item CreateKey()
    {
        var item = ItemController.Instance.CreateItem(ItemController.Instance.Collection.Key_Brown, track_item: false);
        return item;
    }
    */
}
