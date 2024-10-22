using Godot;
using System;

public partial class Weed : Node3DScript
{
    [Export]
    public bool IsTouchable;

    [NodeType]
    public AnimationPlayer Animation;

    [NodeType]
    public Touchable Touchable;

    [NodeName]
    public Node3D Anim;

    public event Action OnWeedCut;

    private bool _is_cut;

    public override void _Ready()
    {
        base._Ready();
        Touchable.SetEnabled(IsTouchable);
        Touchable.OnTouched += Touched;

        ShowRandomModel();
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
    }

    private void Remove()
    {
        OnWeedCut?.Invoke();
        QueueFree();
    }

    private void PlayCutSFX()
    {
        SoundController.Instance.Play("sfx_weed_cut", new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = GlobalPosition,
            PitchMin = 0.95f,
            PitchMax = 1f
        });
    }

    private void ShowRandomModel()
    {
        var valid_models = Anim.GetNodesInChildren<MeshInstance3D>(x => x.IsVisibleInTree());
        var selected_model = valid_models.Random();
        valid_models.ForEach(x => x.Visible = x == selected_model);
    }
}
