using System;
using System.Text.Json.Serialization;

public abstract class SaveData
{
    public string Version { get; set; } = string.Empty;
    public bool IsRelease { get; set; } = false;
    public string TimeUpdated { get; set; } = string.Empty;

    public void UpdateVersion() => Version = ApplicationInfo.Instance.Version;
    public void UpdateRelease() => IsRelease = ApplicationInfo.Instance.Release;
    public void UpdateTimestamp() => TimeUpdated = DateTime.UtcNow.ToString();

    [JsonIgnore]
    public DateTime DateTimeUpdated => DateTime.TryParse(TimeUpdated, out var result) ? result : new DateTime();

    public void Update()
    {
        UpdateVersion();
        UpdateRelease();
        UpdateTimestamp();
    }

    public void Save()
    {
        var type = GetType();
        SaveDataController.Instance.Save(type);
    }
}
