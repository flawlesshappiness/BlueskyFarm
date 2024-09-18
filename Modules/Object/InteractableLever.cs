using Godot;
using System;

public partial class InteractableLever : Touchable
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
    public string Toggle_SFX;

    [Export]
    public string Pull_SFX;

    [NodeType]
    public AnimationPlayer Animation;

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
        Animation.Play(IdleUp_AnimationName);
    }

    private void SetupEvents()
    {
        if (_first_time_setup) return;
        _first_time_setup = true;

        Animation.AnimationFinished += AnimationFinished;
    }

    private void AnimationFinished(StringName animName)
    {
        OnStateChanged?.Invoke((int)_state);
        _animating = false;
    }

    protected override void Touched()
    {
        base.Touched();

        if (_animating) return;

        SetupEvents();

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
        Animation.Play(animation);

        SoundController.Instance.Play(Toggle_SFX, new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = GlobalPosition,
            PitchMin = 0.95f,
            PitchMax = 1f
        });
    }

    private void Pull()
    {
        _animating = true;
        Animation.Play(MoveBoth_AnimationName);

        SoundController.Instance.Play(Pull_SFX, new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = GlobalPosition,
            PitchMin = 0.95f,
            PitchMax = 1f
        });
    }
}
