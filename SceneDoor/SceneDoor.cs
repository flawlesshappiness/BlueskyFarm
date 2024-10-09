using Godot;
using System.Collections;

public partial class SceneDoor : Touchable
{
    [Export]
    public string SceneName;

    [Export]
    public string StartNode;

    [Export]
    public AudioStream OpenSound;

    [Export]
    public AudioStream CloseSound;

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

            FirstPersonController.Instance.MovementLock.AddLock(nameof(SceneDoor));
            FirstPersonController.Instance.InteractLock.AddLock(nameof(SceneDoor));

            SoundController.Instance.Play(OpenSound?.ResourcePath, new SoundSettings
            {
                Volume = -12,
                Bus = SoundBus.SFX
            });

            yield return LerpEnumerator.Lerp01(0.5f, f => view.SetBlackOverlayAlpha(Mathf.Lerp(0, 1, f)));
            yield return new WaitForSeconds(0.5f);

            ChangeScene();

            yield return new WaitForSeconds(0.5f);

            FirstPersonController.Instance.MovementLock.RemoveLock(nameof(SceneDoor));
            FirstPersonController.Instance.InteractLock.RemoveLock(nameof(SceneDoor));

            SoundController.Instance.Play(CloseSound?.ResourcePath, new SoundSettings
            {
                Volume = -12,
                Bus = SoundBus.SFX
            });


            yield return LerpEnumerator.Lerp01(0.5f, f => view.SetBlackOverlayAlpha(Mathf.Lerp(1, 0, f)));
        }
    }
}
