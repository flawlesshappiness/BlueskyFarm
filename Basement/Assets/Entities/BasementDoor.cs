using Godot;

public partial class BasementDoor : Node3DScript
{
    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public Touchable Touchable;

    [Export]
    public string OpenSFX = "sfx_door_open";

    [Export]
    public string CloseSFX = "sfx_door_close";

    [Export]
    public string LockedSFX = "sfx_locked";

    [Export]
    public string UnlockSFX = "sfx_unlock";

    public bool Locked { get; set; }

    private bool _open;

    public override void _Ready()
    {
        base._Ready();
        Touchable.OnTouched += Touched;
        SetOpen(false);
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
        SoundController.Instance.Play(OpenSFX, GlobalPosition);
        _open = true;
    }

    public void Close()
    {
        AnimationPlayer.Play("close");
        SoundController.Instance.Play(CloseSFX, GlobalPosition);
        _open = false;
    }

    public void PlayLockedFX()
    {
        SoundController.Instance.Play(LockedSFX, GlobalPosition);
        DialogueController.Instance.SetNode("##LOCKED##");
    }

    public void Unlock()
    {
        Locked = false;
        SoundController.Instance.Play(UnlockSFX, GlobalPosition);
    }
}
