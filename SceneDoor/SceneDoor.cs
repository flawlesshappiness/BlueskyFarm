using Godot;
using System.Collections;

public partial class SceneDoor : Node3DScript
{
    [Export]
    public string SceneName;

    [Export]
    public string StartNode;

    [Export]
    public bool Locked;

    [Export]
    public SoundInfo OpenSound;

    [Export]
    public SoundInfo CloseSound;

    [Export]
    public SoundInfo LockedSound;

    [NodeName]
    public Touchable Touchable;

    public override void _Ready()
    {
        base._Ready();
        Touchable.OnTouched += Touched;
    }

    private void Touched()
    {
        Debug.TraceMethod();
        Debug.Indent++;

        if (string.IsNullOrEmpty(SceneName))
        {
            Debug.LogError("SceneDoor.SceneName is empty");
            Debug.Indent--;
            return;
        }

        if (Locked)
        {
            PlayLockedSFX();
        }
        else
        {
            AnimateTransition();
        }

        Debug.Indent--;
    }

    private void PlayLockedSFX()
    {
        if (LockedSound != null)
        {
            SoundController.Instance.Play(LockedSound, GlobalPosition);
        }

        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "locked_" + GetInstanceId(),
            Text = "##LOCKED##",
            Target = Touchable,
            Offset = new Vector3(0, 0.4f, 0),
            Duration = 5.0f,
            Shake_Duration = 0.4f,
            Shake_Frequency = 0.04f,
            Shake_Strength = 10f,
            Shake_Dampening = 0.9f,
        });
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
            var bus = AudioBus.Get(SoundBus.Transition.ToString());

            Player.Instance.MovementLock.AddLock(nameof(SceneDoor));
            Player.Instance.LookLock.AddLock(nameof(SceneDoor));
            Player.Instance.InteractLock.AddLock(nameof(SceneDoor));
            Cursor.Hide();
            view.HideAllDynamicUI();

            SoundController.Instance.Play(OpenSound?.ResourcePath);

            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                view.SetBlackOverlayAlpha(Mathf.Lerp(0, 1, f));
            });

            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                var volume = SoundController.Instance.PercentageToDecibel(Mathf.Lerp(1f, 0f, f));
                bus.SetVolume(volume);
            });

            yield return new WaitForSeconds(0.25f);

            ChangeScene();

            yield return new WaitForSeconds(0.25f);

            Player.Instance.MovementLock.RemoveLock(nameof(SceneDoor));
            Player.Instance.LookLock.RemoveLock(nameof(SceneDoor));
            Player.Instance.InteractLock.RemoveLock(nameof(SceneDoor));

            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                var volume = SoundController.Instance.PercentageToDecibel(Mathf.Lerp(0f, 1f, f));
                bus.SetVolume(volume);
            });

            SoundController.Instance.Play(CloseSound?.ResourcePath);

            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                view.SetBlackOverlayAlpha(Mathf.Lerp(1, 0, f));
            });
        }
    }
}
