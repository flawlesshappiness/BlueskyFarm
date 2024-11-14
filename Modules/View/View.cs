using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract partial class View : ControlScript, IComparable<View>
{
    [Export]
    public int ChildOrder;

    public abstract string Directory { get; }
    private View Create() => Singleton.LoadScene($"{Directory}/{GetType().Name}", GetType()) as View;

    public static void CreateAll()
    {
        var types = ReflectiveEnumerator.GetEnumerableOfType<View>();
        var views = new List<View>();

        // Create views
        foreach (var type in types)
        {
            var v = type.Create();
            views.Add(v);
        }

        // Order views
        var ordered_views = views.OrderBy(x => x.ChildOrder).ToList();
        var child_count = Scene.Root.GetChildCount();
        var idx_start = child_count - views.Count - 1;

        for (int i = 0; i < ordered_views.Count; i++)
        {
            var idx = idx_start + i;
            var view = ordered_views[i];
            Scene.Root.MoveChild(view, idx);
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
        Singleton.TryGet<T>(out var view) ? view : null;

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
