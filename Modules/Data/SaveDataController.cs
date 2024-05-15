using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class SaveDataController : Node
{
    public static SaveDataController Instance => Singleton.Create<SaveDataController>($"{Paths.Modules}/Data/{nameof(SaveDataController)}");

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
    }

    public T Get<T>() where T : SaveData, new()
    {
        if (data_objects.ContainsKey(typeof(T)))
        {
            return data_objects[typeof(T)] as T;
        }
        else
        {
            var filename = typeof(T).Name;
            var path = $"user://{filename}.save";

            EnsureFileExists(path);

            var json = FileAccess.GetFileAsString(path);
            Debug.Log("json: " + json);

            T data = string.IsNullOrEmpty(json) ? new T() : JsonSerializer.Deserialize<T>(json);
            data_objects.Add(typeof(T), data);

            Save<T>();

            return data;
        }
    }

    public void SaveAll()
    {
        Debug.LogMethod();
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
        var data = data_objects[type];
        var json = JsonSerializer.Serialize(data);
        var filename = type.Name;
        var path = $"user://{filename}.save";
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreLine(json);
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
