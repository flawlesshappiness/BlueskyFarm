using Godot;

public partial class Scene : NodeScript
{
    private bool _initialized;

    public bool IsPaused => GetTree().Paused;

    public static Scene Current { get; set; }
    public static SceneTree Tree { get; set; }
    public static Window Root { get; set; }
    public static MultiLock PauseLock { get; } = new();
    public static bool AutoSave { get; set; } = true;

    protected virtual void OnInitialize() { }
    protected virtual void OnDestroy() { }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!_initialized)
            Initialize();
    }

    private void Initialize()
    {
        _initialized = true;
        OnInitialize();
    }

    public static T Instantiate<T>(string path) where T : Scene =>
        GDHelper.Instantiate<T>(path);

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
            if (AutoSave)
            {
                // Save
            }

            Current.QueueFree();
        }

        Current = Instantiate<Scene>($"Scenes/{scene_name}");
        // Load

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