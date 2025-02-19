using Godot;
using System;

public partial class InteractableLever : Node3D
{
    [Export]
    public AnimationType Type;

    [Export]
    public string IdleUp_AnimationName;

    [Export]
    public string IdleDown_AnimationName;

    [Export]
    public string MoveUp_AnimationName;

    [Export]
    public string MoveDown_AnimationName;

    [Export]
    public string MoveBoth_AnimationName;

    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public Touchable Touchable;

    [Export]
    public AudioStreamPlayer3D sfx_metal_scraping;

    [Export]
    public AudioStreamPlayer3D sfx_metal_click;

    public int CurrentState => (int)_state;

    public event Action<int> OnStateChanged;

    private State _state = State.Up;
    private bool _first_time_setup;
    private bool _animating;

    public enum State
    {
        Up = 0,
        Down = 1,
    }

    public enum AnimationType
    {
        Toggle, Pull
    }

    public override void _Ready()
    {
        base._Ready();
        AnimationPlayer.AnimationFinished += AnimationFinished;
        Touchable.OnTouched += Touched;

        sfx_metal_scraping.VolumeDb = AudioMath.PercentageToDecibel(0);
    }

    public void SetLeverUp()
    {
        AnimationPlayer.Play(IdleUp_AnimationName);
    }

    public void SetLeverDown()
    {
        AnimationPlayer.Play(IdleDown_AnimationName);
    }

    private void AnimationFinished(StringName animName)
    {
        _animating = false;
        OnStateChanged?.Invoke((int)_state);
    }

    private void Touched()
    {
        if (_animating) return;

        if (Type == AnimationType.Toggle)
        {
            Toggle();
        }
        else
        {
            Pull();
        }
    }

    private void Toggle()
    {
        _animating = true;
        _state = _state == State.Up ? State.Down : State.Up;
        var animation = _state == State.Up ? MoveUp_AnimationName : MoveDown_AnimationName;
        AnimationPlayer.Play(animation);
    }

    private void Pull()
    {
        _animating = true;
        AnimationPlayer.Play(MoveBoth_AnimationName);
    }

    public void SfxScrapingStart()
    {
        sfx_metal_scraping.FadeIn(0.2f, 0f);
    }

    public void SfxScrapingStop()
    {
        sfx_metal_scraping.FadeOut(0.2f);
    }

    public void PlaySfxClick()
    {
        sfx_metal_click.Play();
    }
}
