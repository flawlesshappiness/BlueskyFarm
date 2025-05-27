using Godot;
using System.Collections;

public partial class ForgeMachine : Node3D
{
    [Export]
    public InteractableLever Lever;

    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public AudioStreamPlayer3D SfxPressMove;

    [Export]
    public AudioStreamPlayer3D SfxLavaMove;

    [Export]
    public SoundInfo SfxPressImpact;

    [Export]
    public SoundInfo SfxSteam;

    [Export]
    public Node3D PressImpactMarker;

    [Export]
    public ParticleInfo ps_dirt_impact;

    [Export]
    public ItemArea ItemArea_Mold;

    [Export]
    public Marker3D MoldMarkerStart;

    [Export]
    public Marker3D MoldMarkerEnd;

    [Export]
    public Marker3D CreatedItemMarker;

    private bool HasMold => _current_mold != null;

    private bool _activated;
    private bool _touched;
    private bool _press_down;
    private Item _current_mold;
    private Item _created_item;
    private ForgeMoldInfo _current_info;

    public override void _Ready()
    {
        base._Ready();

        Lever.SetLeverUp();
        Lever.OnStateChanged += LeverStateChanged;
        Lever.Touchable.OnTouched += LeverTouched;

        ItemArea_Mold.OnItemEntered += ItemEntered_Mold;
    }

    public void SetActivated(bool activated)
    {
        _activated = activated;
        ItemArea_Mold.SetEnabled(activated);
    }

    private void LeverStateChanged(int i)
    {
        if (!_touched) return;
        if (i == 1 && !_press_down) // Down
        {
            if (!_activated)
            {
                //ShowNotActivatedText();
                Lever.Toggle();
                return;
            }

            if (!HasMold)
            {
                //ShowMoldMissingText();
                Lever.Toggle();
                return;
            }

            _press_down = true;
            AnimateForge();
        }
        else if (i == 0 && _press_down) // Up
        {
            _press_down = false;
            AnimateMoveUp();
        }
        else
        {
            Lever.Touchable.Enable();
        }
    }

    public void AnimateDeactivated()
    {
        AnimationPlayer.Play("deactivated");
    }

    public Coroutine AnimateActivate(float delay)
    {
        return this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            yield return AnimationPlayer.PlayAndWaitForAnimation("activate");
        }
    }

    private Coroutine AnimateForge()
    {
        return this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(1f);
            yield return AnimationPlayer.PlayAndWaitForAnimation("move_down");
            yield return new WaitForSeconds(0.5f);
            yield return AnimationPlayer.PlayAndWaitForAnimation("lava_fill");
            yield return new WaitForSeconds(0.5f);
            yield return AnimationPlayer.PlayAndWaitForAnimation("hammer");
            Lever.Touchable.Enable();
        }
    }

    private Coroutine AnimateMoveUp()
    {
        return this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(0.5f);
            yield return AnimationPlayer.PlayAndWaitForAnimation("move_up");
            Lever.Touchable.Enable();
        }
    }

    private void LeverTouched()
    {
        _touched = true;
        Lever.Touchable.Disable();
    }

    private void ItemEntered_Mold(Item item)
    {
        if (_current_mold != null) return;
        if (item.Info.Type != ItemType.ForgeMold) return;

        _current_info = ForgeMoldController.Instance.GetInfo(item.Data.CustomId);
        if (_current_info == null) return;

        _current_mold = item;

        item.SetGrabbable(false);
        item.Freeze = true;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            Lever.Touchable.Disable();
            SoundController.Instance.Play("sfx_stone_impact", MoldMarkerEnd.GlobalPosition);
            yield return item.LerpToNode(0.5f, MoldMarkerStart);
            var asp = SoundController.Instance.Play("sfx_stone_drag_long", item.GlobalPosition);
            yield return item.LerpToNode(1.0f, MoldMarkerEnd, Curves.EaseInQuad);
            asp.Stop();
            Lever.Touchable.Enable();
        }
    }

    public void PlayPressImpactSfx()
    {
        SoundController.Instance.Play(SfxPressImpact, PressImpactMarker);
    }

    public void StartPressMoveSfx()
    {
        SfxPressMove.Fade(0.2f, AudioMath.PercentageToDecibel(1));
    }

    public void StopPressMoveSfx()
    {
        SfxPressMove.FadeOut(0.2f);
    }

    public void StartLavaMoveSfx()
    {
        SfxLavaMove.Fade(1f, 30);
    }

    public void StopLavaMoveSfx()
    {
        SfxLavaMove.FadeOut(2f);
    }

    public void PlaySteamSfx()
    {
        SoundController.Instance.Play(SfxSteam, PressImpactMarker);
    }

    public void PlayDirtImpactPS()
    {
        Particle.PlayOneShot(ps_dirt_impact, GlobalPosition);
    }

    public void DestroyMold()
    {
        if (_current_mold == null) return;

        _current_mold.QueueFree();
        _current_mold = null;
    }

    public void CreateMoldItem()
    {
        if (_current_info == null) return;

        var item = ItemController.Instance.CreateItem(_current_info.ResultItem);
        item.SetGrabbable(false);
        item.Freeze = true;
        item.SetParent(CreatedItemMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;

        _created_item = item;
    }

    public void ReleaseMoldItem()
    {
        if (_created_item == null) return;

        _created_item.SetParent(Scene.Current);
        _created_item.Freeze = false;
        _created_item.SetGrabbable(true);

        _created_item = null;
    }

    private void ShowMoldMissingText()
    {
        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "forge_missing_mold_" + GetInstanceId(),
            Text = "##SOMETHING_MISSING##",
            Target = MoldMarkerStart,
            Offset = new Vector3(0, 0, 0),
            Duration = 3.0f,
        });
    }

    private void ShowNotActivatedText()
    {
        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "forge_not_activated_" + GetInstanceId(),
            Text = "##KILN_NOT_ACTIVATED##",
            Target = MoldMarkerStart,
            Offset = new Vector3(0, 0, 0),
            Duration = 3.0f,
        });
    }

    private void StartImpactScreenShake()
    {
        ScreenEffects.AnimateCameraShake($"{nameof(ForgeMachine)}_{GetInstanceId()}", 0.25f, 0, 0, 1);
    }
}
