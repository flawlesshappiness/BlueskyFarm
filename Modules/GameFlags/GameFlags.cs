public static class GameFlags
{
    private static GameFlagsController Controller => GameFlagsController.Instance;

    public static int GetFlag(string id) => Controller.GetOrCreateFlag(id).Value;
    public static bool HasFlag(string id) => Controller.HasFlag(id);
    public static bool IsFlag(string id, int value) => Controller.IsFlag(id, value);
    public static void SetFlag(string id, int value) => Controller.SetFlag(id, value);

    public static void IncrementFlag(string id)
    {
        Controller.GetOrCreateFlag(id).Value++;
    }

    public static void DecrementFlag(string id)
    {
        Controller.GetOrCreateFlag(id).Value--;
    }
}
