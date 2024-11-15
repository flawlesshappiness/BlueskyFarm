using Godot;
using System.Collections;

public partial class SceneDoor : Touchable
{
    [Export]
    public string SceneName;

    [Export]
    public string StartNode;

    [Export]
    public SoundInfo OpenSound;

    [Export]
    public SoundInfo CloseSound;

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
        // Save
        Data.Game.Save();

        // Load scene
        Scene.Goto(SceneName);

        // Teleport player to position
        var node = Scene.Current.GetNodeInChildren<Node3D>(StartNode);
        if (node != null)
        {
            Player.Instance.GlobalPosition = node.GlobalPosition;
            Player.Instance.SetLookRotation(node);
        }
    }

    private void AnimateTransition()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();

            Player.Instance.MovementLock.AddLock(nameof(SceneDoor));
            Player.Instance.LookLock.AddLock(nameof(SceneDoor));
            Player.Instance.InteractLock.AddLock(nameof(SceneDoor));

            SoundController.Instance.Play(OpenSound?.ResourcePath);

            yield return LerpEnumerator.Lerp01(0.5f, f => view.SetBlackOverlayAlpha(Mathf.Lerp(0, 1, f)));
            yield return new WaitForSeconds(0.5f);

            ChangeScene();

            yield return new WaitForSeconds(0.5f);

            Player.Instance.MovementLock.RemoveLock(nameof(SceneDoor));
            Player.Instance.LookLock.RemoveLock(nameof(SceneDoor));
            Player.Instance.InteractLock.RemoveLock(nameof(SceneDoor));

            SoundController.Instance.Play(CloseSound?.ResourcePath);


            yield return LerpEnumerator.Lerp01(0.5f, f => view.SetBlackOverlayAlpha(Mathf.Lerp(1, 0, f)));
        }
    }
}
