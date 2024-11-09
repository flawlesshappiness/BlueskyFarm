using Godot;
using System;
using System.Collections;

public partial class Weed : Node3DScript
{
    [Export]
    public bool IsTouchable;

    [Export]
    public Curve2D Curve;

    [NodeType]
    public AnimationPlayer Animation;

    [NodeType]
    public Touchable Touchable;

    [NodeName]
    public Node3D Anim;

    [NodeName]
    public Node3D ModelScale;

    public event Action OnWeedCut;
    public event Action OnWeedCutFinished;

    private bool _is_cut;

    public override void _Ready()
    {
        base._Ready();
        Touchable.SetEnabled(IsTouchable);
        Touchable.OnTouched += Touched;
    }

    private void Touched()
    {
        Cut();
    }

    public void Cut()
    {
        if (_is_cut) return;
        _is_cut = true;

        Animation.Play("cut");
        Animation.AnimationFinished += _ => Remove();

        PlayCutSFX();

        Touchable.SetEnabled(false);

        OnWeedCut?.Invoke();
    }

    private void Remove()
    {
        OnWeedCutFinished?.Invoke();
        QueueFree();
    }

    private void PlayCutSFX()
    {
        SoundController.Instance.Play("sfx_weed_cut", GlobalPosition);
    }

    public void RandomizeModel()
    {
        var valid_models = Anim.GetNodesInChildren<MeshInstance3D>(x => x.IsVisibleInTree());
        var selected_model = valid_models.Random();
        valid_models.ForEach(x => x.Visible = x == selected_model);
    }

    public void RandomizeScale()
    {
        var rng = new RandomNumberGenerator();
        ModelScale.Scale = Vector3.One * rng.RandfRange(0.5f, 1f);
    }

    public Coroutine AnimateAppear()
    {
        SoundController.Instance.Play("sfx_weed_cut", GlobalPosition);

        return StartCoroutine(Cr, "animate");
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
}
