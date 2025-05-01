using System;

public static class Data
{
    public static GameSaveData Game => GetSelectedGameSaveData();
    private static GameSaveData _game;
    public static OptionsData Options => _options ?? (_options = LoadOrCreateOptionsData());
    private static OptionsData _options;

    public static event Action OnGameSaveDataSelected;
    public static event Action OnGameSaveDataRemoved;

    public static void CreateGameSaveData(int profile)
    {
        Debug.LogMethod(profile);
        _game = SaveDataController.Instance.Create<GameSaveData>(profile);
    }

    public static void SetSelectedGameSaveData(GameSaveData data)
    {
        Debug.LogMethod(data.Profile);
        _game = data;
        OnGameSaveDataSelected?.Invoke();
    }

    public static void RemoveSelectedGameSaveData()
    {
        _game = null;
        OnGameSaveDataRemoved?.Invoke();
    }

    private static GameSaveData GetSelectedGameSaveData()
    {
        if (_game == null)
        {
            throw new Exception("Attempted to get GameSaveData without a selected profile");
        }
        else
        {
            return _game;
        }
    }

    private static OptionsData LoadOrCreateOptionsData()
    {
        if (SaveDataController.Instance.TryLoad<OptionsData>(out var data))
        {
            return data;
        }
        else
        {
            return SaveDataController.Instance.Create<OptionsData>();
        }
    }
}
