public static class Data
{
    public static GameSaveData Game => SaveDataController.Instance.Get<GameSaveData>();
    public static OptionsData Options => SaveDataController.Instance.Get<OptionsData>();
}
