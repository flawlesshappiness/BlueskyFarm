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
    public Marker3D CreatedItemMarker;

    [Export]
    public ItemInfo SwordInfo;

    private bool _touched;
    private bool _press_down;
    private Item _created_item;

    public override void _Ready()
    {
        base._Ready();

        Lever.SetLeverUp();
        Lever.OnStateChanged += LeverStateChanged;
        Lever.Touchable.OnTouched += LeverTouched;

        ItemArea_Mold.OnItemEntered += ItemEntered_Mold;

        InitializeAnimation();
    }

    private void InitializeAnimation()
    {
        if (GameFlagIds.ForgeMoldPlaced.IsTrue())
        {
            AnimationPlayer.Play("mold_idle");
        }
        else if (GameFlagIds.KilnActivated.IsTrue())
        {
            AnimationPlayer.Play("activate");
        }
        else
        {
            AnimationPlayer.Play("deactivated");
        }
    }

    private void LeverStateChanged(int i)
    {
        if (!_touched) return;
        if (i == 1 && !_press_down) // Down
        {
            if (CanForge())
            {
                _press_down = true;
                AnimateForge();
            }
            else
            {
                Lever.Toggle();
            }
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

    private bool CanForge()
    {
        return GameFlagIds.ForgeMoldPlaced.IsTrue() && GameFlagIds.KilnActivated.IsTrue();
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
        if (GameFlagIds.KilnActivated.IsFalse()) return;
        if (GameFlagIds.ForgeMoldPlaced.IsTrue()) return;
        GameFlagIds.ForgeMoldPlaced.SetTrue();

        ItemController.Instance.UntrackItem(item);
        item.QueueFree();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            Lever.Touchable.Disable();
            yield return AnimationPlayer.PlayAndWaitForAnimation("mold_move");
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

    public void CreateMoldItem()
    {
        GameFlagIds.ForgeMoldPlaced.SetFalse();

        var item = ItemController.Instance.CreateItem(SwordInfo);
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

    private void StartImpactScreenShake()
    {
        ScreenEffects.AnimateCameraShake($"{nameof(ForgeMachine)}_{GetInstanceId()}", 0.25f, 0, 0, 1);
    }
}
