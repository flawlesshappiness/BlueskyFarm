using Godot;

public partial class SingletonController : Node
{
    protected static T GetController<T>(string directory) where T : SingletonController => Singleton.TryGet<T>(out var instance) ? instance : Create<T>(directory);
    private static T Create<T>(string directory) where T : SingletonController => Singleton.Create<T>($"{directory}/{typeof(T).Name}");

    public virtual void Initialize()
    {
        Debug.TraceMethod(this.GetType());
    }
}
