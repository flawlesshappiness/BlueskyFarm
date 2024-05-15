public partial class View : ControlScript
{
    public override void _Ready()
    {
        base._Ready();
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;
        VisibilityChanged += OnVisibilityChanged;
    }

    private static string GetPath<T>() where T : View
    {
        var type = typeof(T).Name;
        var path = $"{Paths.ViewDirectory}/{type}/{type}";
        return path;
    }

    public static T Instantiate<T>() where T : View =>
        GDHelper.Instantiate<T>(GetPath<T>());

    public static T LoadSingleton<T>() where T : View =>
        Singleton.Load<T>(GetPath<T>());

    public static T Get<T>() where T : View =>
        Singleton.Get<T>();

    public static void Show<T>() where T : View =>
        Get<T>().Show();

    protected virtual void OnVisibilityChanged()
    {
        if (Visible)
        {
            OnShow();
        }
        else
        {
            OnHide();
        }
    }

    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}
