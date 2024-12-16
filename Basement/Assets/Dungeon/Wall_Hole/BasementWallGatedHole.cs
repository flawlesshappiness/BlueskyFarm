using Godot;
using System.Collections;

public partial class BasementWallGatedHole : Node3DScript
{
    [NodeName]
    public Node3D GateAnim;

    private bool _is_open;
    private bool _animating;

    public void AnimateOpenGate()
    {
        if (_is_open) return;
        if (_animating) return;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var start_position = GateAnim.GlobalPosition;
            var end_position = new Vector3(0, 1.25f, 0);

            SoundController.Instance.Play("sfx_stone_drag_long", GateAnim.GlobalPosition);

            yield return LerpEnumerator.Lerp01(2f, f =>
            {
                GateAnim.Position = start_position.Lerp(end_position, f);
            });

            SoundController.Instance.Play("sfx_stone_impact", GateAnim.GlobalPosition);
        }
    }

    public void AnimateCloseGate()
    {
        if (!_is_open) return;
        if (_animating) return;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var start_position = GateAnim.GlobalPosition;
            var end_position = Vector3.Zero;

            SoundController.Instance.Play("sfx_stone_drag_long", GateAnim.GlobalPosition);

            yield return LerpEnumerator.Lerp01(2f, f =>
            {
                GateAnim.Position = start_position.Lerp(end_position, f);
            });

            SoundController.Instance.Play("sfx_stone_impact", GateAnim.GlobalPosition);
        }
    }
}
