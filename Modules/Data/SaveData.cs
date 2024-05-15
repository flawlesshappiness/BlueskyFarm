public abstract class SaveData
{
    public string Version { get; set; } = string.Empty;
    public bool IsRelease { get; set; } = false;

    public void Serialize()
    {
        var type = GetType();
        SaveDataController.Instance.Save(type);
    }
}
