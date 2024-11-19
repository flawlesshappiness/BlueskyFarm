using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class SaveDataController : Node
{
    public static SaveDataController Instance => Singleton.GetOrCreate<SaveDataController>($"{Paths.Modules}/Data/{nameof(SaveDataController)}");

    private Dictionary<Type, SaveData> data_objects = new();

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "SAVE DATA";

        Debug.RegisterAction(new DebugAction
        {
            Text = "Clear ALL save data",
            Category = category,
            Action = v => ClearAllSaveData()
        });

        Debug.RegisterAction(new DebugAction
        {
            Text = "Clear GAME save data",
            Category = category,
            Action = v => { ClearSaveData(typeof(GameSaveData)); GetTree().Quit(); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Text = "Clear OPTIONS save data",
            Category = category,
            Action = v => { ClearSaveData(typeof(OptionsData)); GetTree().Quit(); }
        });
    }

    public T Get<T>() where T : SaveData, new()
    {
        if (data_objects.ContainsKey(typeof(T)))
        {
            return data_objects[typeof(T)] as T;
        }
        else
        {
            Debug.TraceMethod(typeof(T));
            Debug.Indent++;

            T data = null;

            try
            {
                var filename = typeof(T).Name;
                var path = $"user://{filename}.save";

                EnsureFileExists(path);

                var json = FileAccess.GetFileAsString(path);
                data = string.IsNullOrEmpty(json) ? new T() : JsonSerializer.Deserialize<T>(json);

                var current_version = Version.Parse(ApplicationInfo.Instance.Version);
                var data_version = Version.Parse(data.Version);
                var is_lesser_version = data_version.CompareTo(current_version) < 0;

                if (!data.IsRelease && is_lesser_version)
                {
                    Debug.Log($"Data version {data_version} < current version {current_version} - Creating new save");
                    data = new();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load data: {e.Message}");
                data = new T();
            }
            finally
            {
                data_objects.Add(typeof(T), data);
            }

            Debug.Indent--;
            return data;
        }
    }

    public void SaveAll()
    {
        Debug.TraceMethod();
        Debug.Indent++;

        foreach (var kvp in data_objects)
        {
            Debug.Log($"{kvp.Key}");
            Save(kvp.Key);
        }

        Debug.Indent--;
    }

    public void Save<T>() where T : SaveData, new()
    {
        var data = Get<T>();
        Save(typeof(T));
    }

    public void Save(Type type)
    {
        Debug.TraceMethod(type.Name);
        Debug.Indent++;

        var data = data_objects[type];
        data.Update();

        var json = JsonSerializer.Serialize(data, data.GetType(), new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
        var filename = type.Name;
        var path = $"user://{filename}.save";
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreLine(json);

        Debug.Indent--;
    }

    private void EnsureFileExists(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            Debug.Log($"Created file at path: {path}");
            using (FileAccess.Open(path, FileAccess.ModeFlags.Write)) { }
        }
    }

    private void ClearSaveData(Type type)
    {
        var data = Activator.CreateInstance(type) as SaveData;
        data_objects[type] = data;
        Save(type);
    }

    private void ClearAllSaveData()
    {
        foreach (var kvp in data_objects)
        {
            ClearSaveData(kvp.Key);
        }

        GetTree().Quit();
    }
}
