using Godot;
using System;

public abstract partial class SingletonController : Node, IComparable<SingletonController>
{
    public abstract string Directory { get; }
    public SingletonController Create() => Singleton.GetOrCreate($"{Directory}/{GetType().Name}", GetType()) as SingletonController;

    public static void CreateAll()
    {
        var singletons = ReflectiveEnumerator.GetEnumerableOfType<SingletonController>();
        foreach (var singleton in singletons)
        {
            singleton.Create();
        }
    }

    public int CompareTo(SingletonController other)
    {
        return GetType().Name.CompareTo(other.GetType().Name);
    }

    public virtual void Initialize()
    {
        Debug.TraceMethod(GetType());
    }
}
