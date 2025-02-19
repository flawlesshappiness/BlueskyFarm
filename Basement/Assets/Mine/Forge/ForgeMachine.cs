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

    private bool _touched;

    public override void _Ready()
    {
        base._Ready();

        AnimationPlayer.Play("idle_up");

        Lever.SetLeverUp();
        Lever.OnStateChanged += LeverStateChanged;
        Lever.Touchable.OnTouched += LeverTouched;
    }

    private void LeverStateChanged(int i)
    {
        if (!_touched) return;
        if (i == 1) // Down
        {
            AnimateForge();
        }
        else
        {
            AnimateMoveUp();
        }
    }

    private Coroutine AnimateForge()
    {
        return Coroutine.Start(Cr);
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
        return Coroutine.Start(Cr);
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

    public void PlayPressImpactSfx()
    {
        SoundController.Instance.Play(SfxPressImpact, PressImpactMarker);
    }

    public void StartPressMoveSfx()
    {
        SfxPressMove.FadeIn(0.2f, AudioMath.PercentageToDecibel(1));
    }

    public void StopPressMoveSfx()
    {
        SfxPressMove.FadeOut(0.2f);
    }

    public void StartLavaMoveSfx()
    {
        SfxLavaMove.FadeIn(1f, 30);
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
}
