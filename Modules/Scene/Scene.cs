using Godot;
using System;
using System.Linq;

public partial class Scene : NodeScript
{
    private bool _initialized;

    public SceneData SceneData { get; set; }
    public bool IsPaused => GetTree().Paused;

    public static Scene Current { get; set; }
    public static SceneTree Tree { get; set; }
    public static Window Root { get; set; }
    public static MultiLock PauseLock { get; } = new();

    public static Action OnSceneLoaded;

    protected virtual void OnDestroy() { }

    public static T Instantiate<T>(string path) where T : Scene =>
        GDHelper.Instantiate<T>(path);

    private void LoadData()
    {
        Debug.LogMethod();
        Debug.Indent++;

        SceneData ??= GetOrCreateData();
        OnSceneLoaded?.Invoke();

        Debug.Indent--;
    }

    private SceneData GetOrCreateData()
    {
        var data = Data.Game.Scenes.FirstOrDefault(x => x.Name == Name);
        if (data == null)
        {
            data = new SceneData { Name = Name };
            Data.Game.Scenes.Add(data);
        }

        return data;
    }

    #region SCENE
    public static Scene Goto(string scene_name)
    {
        Debug.LogMethod(scene_name);
        Debug.Indent++;

        if (string.IsNullOrEmpty(scene_name))
        {
            Debug.LogError("Scene name was empty");
            Debug.Indent--;
            return Current;
        }

        if (Current != null)
        {
            Current.Destroy();
        }

        Current = Instantiate<Scene>($"Scenes/{scene_name}");
        Debug.TraceMethod($"Current: {Current}");

        Current.LoadData();

        Debug.Indent--;
        return Current;
    }

    public static T Goto<T>() where T : Scene =>
        Goto(typeof(T).Name) as T;

    public void Destroy() => Destroy(this);

    public static void Destroy(Scene scene)
    {
        scene.OnDestroy();
        scene.QueueFree();
    }
    #endregion
}
