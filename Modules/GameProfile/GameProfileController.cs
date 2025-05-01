using System.Collections.Generic;

public partial class GameProfileController : SingletonController
{
    public override string Directory => $"{Paths.Modules}/GameProfile";
    public static GameProfileController Instance => Singleton.Get<GameProfileController>();
    private Dictionary<int, GameSaveData> Profiles { get; set; } = new();
    public bool GameProfilesLoaded { get; private set; }

    protected override void Initialize()
    {
        base.Initialize();
        PreloadGameProfiles();
    }

    private void PreloadGameProfiles()
    {
        Debug.TraceMethod();
        for (int i = 0; i < 3; i++)
        {
            var profile = i + 1;
            var data = PreloadGameProfile(profile);
            Profiles.Add(profile, data);
        }

        GameProfilesLoaded = true;
    }

    private GameSaveData PreloadGameProfile(int profile)
    {
        Debug.TraceMethod(profile);

        var data = GetNewestLocalOrCloudData(profile);
        SetupGameProfile(data);
        return data;
    }

    private GameSaveData GetNewestLocalOrCloudData(int profile)
    {
        Debug.TraceMethod(profile);

        var local = SaveDataController.Instance.GetOrCreate<GameSaveData>(profile);
        var filename = GetSteamFileName<GameSaveData>(profile);
        if (SteamController.Instance.TryReadData<GameSaveData>(filename, out var cloud))
        {
            Debug.Trace("Successfully read data from Steam. Using newest version.");
            return GetMostRecentlyUpdated(local, cloud);
        }
        else
        {
            Debug.Trace("Failed to read data from Steam. Using local data.");
            return local;
        }
    }

    private void SetupGameProfile(GameSaveData data)
    {
        data.OnAfterSave += () =>
        {
            var filename = GetSteamFileName<GameSaveData>(data.Profile ?? 1);
            SteamController.Instance.TryWriteData(filename, data);
        };
    }

    public void CreateGameProfile(int profile)
    {
        Debug.TraceMethod(profile);
        var data = GetGameProfile(profile);
        data.Deleted = false;
        data.Save();
    }

    public void DeleteGameProfile(int profile)
    {
        Debug.TraceMethod(profile);
        var data = SaveDataController.Instance.Create<GameSaveData>(profile);
        Profiles[profile] = data;
    }

    public GameSaveData GetSelectedGameProfile()
    {
        var profile = Data.Options.SelectedGameProfile;
        var data = GetGameProfile(profile);
        return data;
    }

    public GameSaveData GetGameProfile(int profile)
    {
        return Profiles.TryGetValue(profile, out var data) ? data : null;
    }

    private GameSaveData GetMostRecentlyUpdated(GameSaveData data1, GameSaveData data2)
    {
        var first_is_newest = data1.DateTimeUpdated > data2.DateTimeUpdated;
        return first_is_newest ? data1 : data2;
    }

    private string GetSteamFileName<T>(int profile)
        where T : SaveData
    {
        var type = typeof(T).Name;
        var name = $"{type}{profile}";
        return name;
    }
}
