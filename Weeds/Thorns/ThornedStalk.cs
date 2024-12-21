using Godot;
using System.Collections;

public partial class ThornedStalk : Node3DScript
{
    [NodeName]
    public Node3D Top;

    [NodeName]
    public Node3D TopAnim;

    [NodeName]
    public Node3D Bottom;

    public void SetCut(bool is_cut)
    {
        Top.Visible = !is_cut;
        TopAnim.Position = Vector3.Zero;
    }

    public Coroutine AnimateCut()
    {
        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var rng = new RandomNumberGenerator();
            var start_position = TopAnim.GlobalPosition;
            var end_position = start_position.Add(y: rng.RandfRange(0.5f, 1f));
            var start_scale = Vector3.One;
            var end_scale = Vector3.One * 0.001f;
            var curve_position = Curves.EaseOutQuad;
            var curve_scale = Curves.EaseInBack;
            var duration_position = rng.RandfRange(0.25f, 0.5f);

            yield return LerpEnumerator.Lerp01(duration_position, f =>
            {
                var t = curve_position.Evaluate(f);
                TopAnim.GlobalPosition = start_position.Lerp(end_position, t);
            });

            yield return LerpEnumerator.Lerp01(0.25f, f =>
            {
                var t = curve_scale.Evaluate(f);
                TopAnim.Scale = start_scale.Lerp(end_scale, t);
            });

            Particle.PlayOneShot("ps_smoke_puff_small", TopAnim.GlobalPosition);
            SoundController.Instance.Play("sfx_weed_disappear", TopAnim.GlobalPosition);

            Top.Hide();
        }
    }
}
