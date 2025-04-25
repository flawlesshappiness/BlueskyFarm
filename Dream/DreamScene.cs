using Godot;
using System.Collections;

public partial class DreamScene : Scene
{
    [Export]
    public Camera3D Camera;

    protected string DebugCategory = "DREAM - SCENE";
    protected string DebugId => DebugCategory + GetInstanceId();

    private bool _dream_completed;

    public override void _Ready()
    {
        base._Ready();

        ScreenEffects.View.SetCameraTarget(Camera);

        RegisterDebugActions();
    }

    public virtual IEnumerator WaitForReady()
    {
        yield return null;
    }

    public virtual void StartScene()
    {

    }

    protected virtual void RegisterDebugActions()
    {
        Debug.RegisterAction(new DebugAction
        {
            Id = DebugId,
            Category = DebugCategory,
            Text = "Complete dream",
            Action = v => { v.Close(); CompleteDream(); }
        });
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Debug.RemoveActions(DebugId);
    }

    protected void CompleteDream(bool immediate = false)
    {
        if (_dream_completed) return;
        _dream_completed = true;

        DreamController.Instance.EndDream(immediate);
    }
}
