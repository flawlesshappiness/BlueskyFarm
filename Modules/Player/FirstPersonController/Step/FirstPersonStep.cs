using Godot;
using System;
using System.Collections;

public partial class FirstPersonStep : Node3D
{
    [Export]
    public float WalkStepTime = 0.7f;

    [Export]
    public float RunStepTime = 0.7f;

    [Export]
    public float StepBounce = 0.2f;

    private float DesiredStepTime => Player.IsRunning ? RunStepTime : WalkStepTime;
    private float StepTimeMultiplier => Player.MoveSpeedMultiplier == 0 ? 0 : (1f / Player.MoveSpeedMultiplier);

    private FirstPersonController Player { get; set; }

    private bool moving;
    private float timeNextStep;
    private Vector3 start_position;
    private Coroutine cr_head_bob;

    public Action OnMoveStart, OnMoveStop, OnMoveStep;

    public override void _Ready()
    {
        base._Ready();
        start_position = Position;
        Player = this.GetNodeInParents<FirstPersonController>();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Process_Movement();
        Process_Step();
    }

    private void Process_Movement()
    {
        if (Player == null) return;

        var velocity = Player.Velocity;

        if (moving != velocity.Length() > 0)
        {
            moving = !moving;

            if (moving) MoveStart();
            else MoveStop();
        }
    }

    private void Process_Step()
    {
        if (!moving) return;
        if (!Player.IsOnFloor()) return;
        if (GameTime.Time < timeNextStep) return;

        MoveStep();
    }

    public void ResetStepTime()
    {
        timeNextStep = GameTime.Time + DesiredStepTime * StepTimeMultiplier;
    }

    private void MoveStep()
    {
        ResetStepTime();
        StepHeadBob();
        OnMoveStep?.Invoke();
    }

    private void MoveStart()
    {
        OnMoveStart?.Invoke();
        MoveStep();
    }

    private void MoveStop()
    {
        OnMoveStop?.Invoke();
        ResetHeadBob();
    }

    private void StepHeadBob()
    {
        Coroutine.Stop(cr_head_bob);
        cr_head_bob = Coroutine.Start(Cr);

        IEnumerator Cr()
        {
            var duration = DesiredStepTime * 0.4f * StepTimeMultiplier;
            var current_position = Position;
            var mid_position = start_position + Vector3.Down * StepBounce;
            var end_position = start_position;

            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                Position = current_position.Lerp(mid_position, Curves.EaseOutQuad.Evaluate(f));
            });

            current_position = Position;
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                Position = current_position.Lerp(end_position, Curves.EaseInOutQuad.Evaluate(f));
            });
        }
    }

    private void ResetHeadBob()
    {
        Coroutine.Stop(cr_head_bob);
        cr_head_bob = Coroutine.Start(Cr);

        IEnumerator Cr()
        {
            var current_position = Position;
            var duration = 0.15f;
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                Position = current_position.Lerp(start_position, Curves.EaseInOutQuad.Evaluate(f));
            });
        }
    }
}
