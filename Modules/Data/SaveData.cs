public abstract class SaveData
{
    public string Version { get; set; } = string.Empty;
    public bool IsRelease { get; set; } = false;

    public void UpdateVersion() => Version = ApplicationInfo.Instance.Version;
    public void UpdateRelease() => IsRelease = ApplicationInfo.Instance.Release;

    public void Update()
    {
        UpdateVersion();
        UpdateRelease();
    }

    public void Save()
    {
        var type = GetType();
        SaveDataController.Instance.Save(type);
    }
}
