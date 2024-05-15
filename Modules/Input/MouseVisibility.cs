using Godot;

public partial class MouseVisibility : Node
{
    public static MouseVisibility Instance { get; private set; }

    public readonly MultiLock Lock = new MultiLock();

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        InitializeMouse();
    }

    private void InitializeMouse()
    {
        Lock.OnLocked += OnMouseVisibleLocked;
        Lock.OnFree += OnMouseVisibleFree;

        if (Lock.IsLocked)
        {
            OnMouseVisibleLocked();
        }
        else
        {
            OnMouseVisibleFree();
        }
    }

    private void OnMouseVisibleFree()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void OnMouseVisibleLocked()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public static void Show(string lock_name)
    {
        Instance.Lock.AddLock(lock_name);
    }

    public static void Hide(string lock_name)
    {
        Instance.Lock.RemoveLock(lock_name);
    }
}
