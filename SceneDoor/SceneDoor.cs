using Godot;
using System;
using System.Collections;

public partial class SceneDoor : Node3DScript
{
    [Export]
    public string SceneName;

    [Export]
    public string StartNode;

    [Export]
    public SoundInfo OpenSound;

    [Export]
    public SoundInfo CloseSound;

    [Export]
    public SoundInfo LockedSound;

    [Export]
    public bool PerishableWarning;

    [Export]
    public Touchable Touchable;

    [Export]
    public Texture2D HoverIconLocked;

    [Export]
    public Texture2D HoverIconUnlocked;

    public event Action OnSceneChange;

    private bool _locked;

    public override void _Ready()
    {
        base._Ready();
        Touchable.OnTouched += Touched;
    }

    public void SetLocked(bool locked)
    {
        _locked = locked;

        Touchable.HoverIcon = locked ? HoverIconLocked : HoverIconUnlocked;
    }

    private void Touched()
    {
        Debug.TraceMethod();
        Debug.Indent++;

        if (PerishableWarning && InventoryController.Instance.HasNonFarmItems())
        {
            GameView.Instance.InventoryBar.WarnPerishableItems();
            SoundController.Instance.Play("sfx_throw_light");
            Debug.Indent--;
            return;
        }

        if (string.IsNullOrEmpty(SceneName))
        {
            Debug.LogError("SceneDoor.SceneName is empty");
            Debug.Indent--;
            return;
        }

        if (_locked)
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

        /*
        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "locked_" + GetInstanceId(),
            Text = "##LOCKED##",
            Target = Touchable,
            Offset = new Vector3(0, 0.4f, 0),
            Duration = 3.0f,
            Shake_Duration = 0.4f,
            Shake_Frequency = 0.04f,
            Shake_Strength = 10f,
            Shake_Dampening = 0.9f,
        });
        */
    }

    private void ChangeScene()
    {
        OnSceneChange?.Invoke();

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

            SoundController.Instance.Play(OpenSound?.ResourcePath);

            yield return view.TransitionStartCr(new TransitionSettings
            {
                Duration = 0.5f
            });

            ChangeScene();

            yield return view.TransitionEndCr(new TransitionSettings
            {
                Duration = 0.5f
            });

            SoundController.Instance.Play(CloseSound?.ResourcePath);
        }
    }
}
