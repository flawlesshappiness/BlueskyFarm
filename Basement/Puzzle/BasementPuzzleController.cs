public partial class BasementPuzzleController : ResourceController<BasementPuzzleInfoCollection, BasementPuzzleInfo>
{
    public override string Directory => "Basement/Puzzle";

    public override void _Ready()
    {
        base._Ready();
        BasementController.Instance.OnBasementGenerated += BasementGenerated;
    }

    private BasementPuzzleInfo GetRandomPuzzleInfo()
    {
        return Collection.Resources.Random();
    }

    private BasementPuzzle CreatePuzzle(BasementPuzzleInfo info)
    {
        var puzzle = GDHelper.Instantiate<BasementPuzzle>(info.Path, Scene.Current);
        puzzle.SetParent(Scene.Current);
        return puzzle;
    }

    private void BasementGenerated(Basement basement)
    {
        for (int i = 0; i < basement.Settings.PuzzleCount; i++)
        {
            var info = GetRandomPuzzleInfo();
            var puzzle = CreatePuzzle(info);
        }
    }
}
