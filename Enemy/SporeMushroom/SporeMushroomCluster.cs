using Godot;
using System.Collections;

public partial class SporeMushroomCluster : Node3DScript
{
    [NodeType]
    public PsMushroomSmoke PsSmoke;

    [NodeName]
    public Area3D Trigger;

    private bool _triggered;

    public override void _Ready()
    {
        base._Ready();
        Hide();

        Trigger.BodyEntered += TriggerEntered;
    }

    private void TriggerEntered(Node3D body)
    {
        if (!Visible) return;
        if (_triggered) return;

        ScreenEffects.AnimateBlur(20, 0.2f, 0f, 15f);
        ScreenEffects.AnimateDistort(0.1f, 0.25f, new Vector2(5, 1), 1f, 5f, 15f);
        AnimateMoveSpeed();

        PsSmoke.PlayPuff();
        PsSmoke.PlayIdle();
        _triggered = true;
    }

    private void AnimateMoveSpeed()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            FirstPersonController.Instance.MoveSpeedMultiplier = 0.25f;
            FirstPersonController.Instance.LookSpeedMultiplier = 0.25f;

            yield return new WaitForSeconds(2f);

            var move_start = FirstPersonController.Instance.MoveSpeedMultiplier;
            var look_start = FirstPersonController.Instance.LookSpeedMultiplier;

            yield return LerpEnumerator.Lerp01(2f, f =>
            {
                FirstPersonController.Instance.MoveSpeedMultiplier = Mathf.Lerp(move_start, 1f, f);
                FirstPersonController.Instance.LookSpeedMultiplier = Mathf.Lerp(look_start, 1f, f);
            });
        }
    }
}
