public static class DialogueFlags
{
    public static string FrogFirstDeath = "first_death";
    public static string FrogIntro = "frog_intro";
    public static string FrogForestWeeds = "frog_forest_weeds";
    public static string FrogStone = "frog_stone";
    public static string FrogForge = "frog_forge";
    public static string FrogCore = "frog_core";
    public static string FounderQuest = "founder_quest";

    private static string ModifyId(string id) => $"dialogue_flag_{id}";
    public static int GetFlag(string id) => GameFlags.GetFlag(ModifyId(id));
    public static bool HasFlag(string id) => GameFlags.HasFlag(ModifyId(id));
    public static bool IsFlag(string id, int value) => GameFlags.IsFlag(ModifyId(id), value);
    public static void SetFlag(string id, int value) => GameFlags.SetFlag(ModifyId(id), value);

    /// <summary>
    /// Sets flag with id to value, if current value is lower
    /// </summary>
    public static void SetFlagMin(string id, int value)
    {
        if (GetFlag(id) < value)
        {
            SetFlag(id, value);
        }
    }
}