using Godot;
using System;
using System.Collections;

public partial class ForgeKiln : Node3D
{
    [Export]
    public InteractableLever Lever;

    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public ItemArea CoalItemArea;

    [Export]
    public AudioStreamPlayer3D SfxMachine_1;

    [Export]
    public AudioStreamPlayer3D SfxMachine_2;

    [Export]
    public Marker3D CoalFillMarker;

    [Export]
    public SoundInfo SfxCoalFill;

    public event Action OnActivated;

    private bool _lever_touched;
    private int _count_coal = 0;

    public override void _Ready()
    {
        base._Ready();
        Lever.OnStateChanged += LeverStateChanged;
        Lever.Touchable.OnTouched += LeverTouched;
        CoalItemArea.OnItemEntered += ItemEntered_Coal;
    }

    public void SetActivated(bool activated)
    {
        SfxMachine_1.VolumeDb = activated ? -20 : -80;
        SfxMachine_2.VolumeDb = activated ? 0 : -80;
        AnimationPlayer.Play(activated ? "activated" : "RESET");
    }

    private void LeverStateChanged(int state)
    {
        if (!_lever_touched) return;

        if (state == 0) // up
        {
            Lever.Touchable.Enable();
        }
        else if (state == 1) // down
        {
            if (_count_coal < 3)
            {
                //ShowCoalMissingText();
                Lever.Toggle();
            }
            else
            {
                AnimateActivate();
            }
        }
    }

    private void LeverTouched()
    {
        _lever_touched = true;
        Lever.Touchable.Disable();
    }

    private Coroutine AnimateActivate()
    {
        OnActivated?.Invoke();
        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return AnimationPlayer.PlayAndWaitForAnimation("activate");
        }
    }

    private void ItemEntered_Coal(Item item)
    {
        _count_coal++;

        item.QueueFree();
        SfxCoalFill.Play(CoalFillMarker);
    }

    public void StartMachineSfx()
    {
        SfxMachine_1.Fade(4f, -20);
        SfxMachine_2.Fade(4f, 0);
    }

    private void ShowCoalMissingText()
    {
        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "kiln_missing_coal_" + GetInstanceId(),
            Text = "##NOT_ENOUGH_COAL##",
            Target = CoalFillMarker,
            Offset = new Vector3(0, 0, 0),
            Duration = 3.0f,
        });
    }
}
