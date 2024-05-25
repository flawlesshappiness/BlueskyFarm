using System;

public abstract partial class View : ControlScript, IComparable<View>
{
    public abstract string Directory { get; }
    private void Create() => Singleton.LoadScene($"{Directory}/{GetType().Name}", GetType());

    public static void CreateAll()
    {
        var views = ReflectiveEnumerator.GetEnumerableOfType<View>();
        foreach (var view in views)
        {
            view.Create();
        }
    }

    public override void _Ready()
    {
        base._Ready();
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;
        VisibilityChanged += OnVisibilityChanged;
    }

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

    public int CompareTo(View other)
    {
        return GetType().Name.CompareTo(other.GetType().Name);
    }
}
