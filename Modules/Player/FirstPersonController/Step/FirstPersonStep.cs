using Godot;
using System;
using System.Collections;

public partial class FirstPersonStep : Node3D
{
    [Export]
    public float WalkStepTime;

    [Export]
    public float RunStepTime;

    [Export]
    public float StepBounce;

    private float DesiredStepTime => Player.Instance.IsRunning ? RunStepTime : WalkStepTime;
    private float StepTimeMultiplier => Player.Instance.MoveSpeedMultiplier == 0 ? 0 : (1f / Player.Instance.MoveSpeedMultiplier);

    private bool moving;
    private float timeNextStep;
    private Vector3 start_position;
    private Coroutine cr_head_bob;

    public Action OnMoveStep;

    public override void _Ready()
    {
        base._Ready();
        start_position = Position;

        var player = this.GetNodeInParents<Player>();
        player.OnMoveStart += MoveStart;
        player.OnMoveStop += MoveStop;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Process_Step();
    }

    private void Process_Step()
    {
        if (!moving) return;
        if (!Player.Instance.IsOnFloor()) return;
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
        moving = true;
        ResetStepTime();
        StepHeadBob();
    }

    private void MoveStop()
    {
        moving = false;
        ResetHeadBob();
    }

    private void StepHeadBob()
    {
        Coroutine.Stop(cr_head_bob);
        cr_head_bob = this.StartCoroutine(Cr);

        IEnumerator Cr()
        {
            var duration = DesiredStepTime * 0.4f * StepTimeMultiplier;
            var current_position = Position;
            var mid_position = start_position + Vector3.Down * StepBounce * Data.Options.HeadbobAmount;
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
        cr_head_bob = this.StartCoroutine(Cr);

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
