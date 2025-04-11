using Godot;
using Godot.Collections;

public partial class BasementDoor : Node3DScript
{
    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public Array<Touchable> Touchables;

    [Export]
    public Marker3D SoundMarker;

    [Export]
    public SoundInfo SfxLocked;

    [Export]
    public SoundInfo SfxUnlock;

    public bool Locked { get; set; }

    private bool _open;
    private bool _animating;

    public override void _Ready()
    {
        base._Ready();
        AnimationPlayer.AnimationFinished += AnimationFinished;
        Touchables.ForEach(x => x.OnTouched += Touched);
        SetOpen(false);
    }

    private void AnimationFinished(StringName name)
    {
        _animating = false;
        Touchables.ForEach(x => x.Enabled = true);
    }

    private void Touched()
    {
        if (_animating) return;

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
        Touchables.ForEach(x => x.Enabled = false);
        _animating = true;
        _open = true;
    }

    public void Close()
    {
        AnimationPlayer.Play("close");
        Touchables.ForEach(x => x.Enabled = false);
        _animating = true;
        _open = false;
    }

    public void PlayLockedFX()
    {
        SoundController.Instance.Play(SfxLocked, SoundMarker.GlobalPosition);

        /*
        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "locked_" + GetInstanceId(),
            Text = "##LOCKED##",
            Target = SoundMarker,
            Offset = new Vector3(0, 0.2f, 0),
            Duration = 3.0f,
            Shake_Duration = 0.4f,
            Shake_Frequency = 0.04f,
            Shake_Strength = 10f,
            Shake_Dampening = 0.9f,
        });
        */
    }

    public void Unlock()
    {
        Locked = false;
        SoundController.Instance.Play(SfxUnlock, GlobalPosition);
    }
}
