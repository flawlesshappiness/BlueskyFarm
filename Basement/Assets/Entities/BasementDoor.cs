using Godot;

public partial class BasementDoor : Node3DScript
{
    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public Touchable Touchable;

    [Export]
    public string LockedSFX = "sfx_locked";

    [Export]
    public string UnlockSFX = "sfx_unlock";

    public bool Locked { get; set; }

    private bool _open;

    public override void _Ready()
    {
        base._Ready();
        AnimationPlayer.AnimationFinished += AnimationFinished;
        Touchable.OnTouched += Touched;
        SetOpen(false);
    }

    private void AnimationFinished(StringName name)
    {
        Touchable.Enable();
    }

    private void Touched()
    {
        if (_open)
        {
            Close();
        }
        else
        {
            TryOpen();
        }
    }

    public void TryOpen()
    {
        if (Locked)
        {
            PlayLockedFX();
        }
        else
        {
            Open();
        }
    }

    public void SetOpen(bool open)
    {
        var anim = open ? "opened" : "closed";
        AnimationPlayer.Play(anim);
        _open = open;
    }

    public void Open()
    {
        AnimationPlayer.Play("open");
        Touchable.Disable();
        _open = true;
    }

    public void Close()
    {
        AnimationPlayer.Play("close");
        Touchable.Disable();
        _open = false;
    }

    public void PlayLockedFX()
    {
        SoundController.Instance.Play(LockedSFX, Touchable.GlobalPosition);
        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "locked_" + GetInstanceId(),
            Text = "##LOCKED##",
            Target = Touchable,
            Offset = new Vector3(0, 0.2f, 0),
            Duration = 5.0f,
            Shake_Duration = 0.4f,
            Shake_Frequency = 0.04f,
            Shake_Strength = 10f,
            Shake_Dampening = 0.9f,
        });
    }

    public void Unlock()
    {
        Locked = false;
        SoundController.Instance.Play(UnlockSFX, GlobalPosition);
    }
}
