using Godot;
using System.Collections;

public partial class SceneDoor : Touchable
{
    [Export]
    public string SceneName;

    [Export]
    public string StartNode;

    public override void _Ready()
    {
        base._Ready();
        OnTouched += Touched;
    }

    private void Touched()
    {
        Debug.LogMethod();
        Debug.Indent++;

        if (string.IsNullOrEmpty(SceneName))
        {
            Debug.LogError("SceneDoor.SceneName is empty");
            Debug.Indent--;
            return;
        }

        AnimateTransition();

        Debug.Indent--;
    }

    private void ChangeScene()
    {
        // Load scene
        Scene.Goto(SceneName);

        // Teleport player to position
        var node = Scene.Current.GetNodeInChildren<Node3D>(StartNode);
        if (node != null)
        {
            FirstPersonController.Instance.GlobalPosition = node.GlobalPosition;
            FirstPersonController.Instance.SetLookRotation(node);
        }
    }

    private void AnimateTransition()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();

            FirstPersonController.Instance.MovementLock.AddLock(nameof(Sleepable));
            FirstPersonController.Instance.InteractLock.AddLock(nameof(Sleepable));

            yield return LerpEnumerator.Lerp01(0.5f, f => view.SetOverlayAlpha(Mathf.Lerp(0, 1, f)));
            yield return new WaitForSeconds(0.5f);

            ChangeScene();

            FirstPersonController.Instance.MovementLock.RemoveLock(nameof(Sleepable));
            FirstPersonController.Instance.InteractLock.RemoveLock(nameof(Sleepable));

            yield return LerpEnumerator.Lerp01(0.5f, f => view.SetOverlayAlpha(Mathf.Lerp(1, 0, f)));
        }
    }
}
