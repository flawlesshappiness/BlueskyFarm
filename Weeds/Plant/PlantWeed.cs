using Godot;
using System.Collections;
using System.Linq;

public partial class PlantWeed : Weed
{
    [NodeName]
    public Node3D Models;

    [NodeName]
    public Node3D Anim;

    public override void Cut()
    {
        base.Cut();
        AnimateCut();
    }

    public void RandomizeModel()
    {
        var children = Models
            .GetChildren()
            .Select(x => x as Node3D)
            .Where(x => x != null)
            .ToList();

        var model = children.Random();

        children.ForEach(x => x.Hide());
        model.Show();
    }

    public void RandomizeScale()
    {
        var rng = new RandomNumberGenerator();
        Models.Scale = Vector3.One * rng.RandfRange(0.8f, 1.2f);
    }

    public Coroutine AnimateAppear()
    {
        SoundController.Instance.Play("sfx_weed_cut", GlobalPosition);

        return this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            var duration = 0.5f;
            var curve = AnimationCurves.WobbleOut(0.5f, 3);
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var t = curve.Sample(f);
                Anim.Scale = Vector3.One * t;
            });
        }
    }

    private Coroutine AnimateCut()
    {
        SoundController.Instance.Play("sfx_weed_cut", GlobalPosition);

        return this.StartCoroutine(Cr, "animate");
        IEnumerator Cr()
        {
            var start_position = Anim.GlobalPosition;
            var end_position = start_position.Add(y: 1);
            var move_curve = Curves.EaseOutQuad;

            var start_scale = Anim.Scale;
            var end_scale = Vector3.One * 0.0001f;
            var scale_curve = Curves.EaseInBack;

            yield return LerpEnumerator.Lerp01(0.3f, f =>
            {
                var t_pos = move_curve.Evaluate(f);
                var t_scale = scale_curve.Evaluate(f);

                Anim.GlobalPosition = start_position.Lerp(end_position, t_pos);
                Anim.Scale = start_scale.Lerp(end_scale, t_scale);
            });

            Models.Hide();

            QueueFree();
        }
    }
}
