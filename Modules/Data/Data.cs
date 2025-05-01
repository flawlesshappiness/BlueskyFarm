public static class Data
{
    public static GameSaveData Game => GameProfileController.Instance.GetSelectedGameProfile();
    public static OptionsData Options => _options ?? (_options = SaveDataController.Instance.GetOrCreate<OptionsData>());
    private static OptionsData _options;
}
