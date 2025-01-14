using Godot;
using Godot.Collections;
using System.Collections;

public partial class GraveContainer : ItemContainer
{
    [Export]
    public Touchable Touchable;

    [Export]
    public Node3D ParticlesNode;

    [Export]
    public Node3D ItemNode;

    [Export]
    public Array<Node3D> Models;

    private int _state;

    public override void _Ready()
    {
        base._Ready();

        Touchable.OnTouched += Touched;
    }

    protected override void Initialize()
    {
        base.Initialize();
        SetState(0);
    }

    private void SetState(int state)
    {
        _state = Mathf.Clamp(state, 0, Models.Count - 1);

        for (int i = 0; i < Models.Count; i++)
        {
            var model = Models[i];
            model.Visible = _state == i;
        }
    }

    private void UpdateState()
    {
        SetState(_state);

        Particle.PlayOneShot("ps_dirt_puff", ParticlesNode.GlobalPosition);
        SoundController.Instance.Play("sfx_dig", ParticlesNode.GlobalPosition);

        if (_state >= Models.Count - 1)
        {
            SpawnItem(ItemNode.GlobalPosition, Vector3.Up * 4);
        }
    }

    private void Touched()
    {
        _state++;
        Touchable.SetEnabled(_state < Models.Count - 1);

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return Player.Instance.WaitForProgress(0.5f, Touchable);
            UpdateState();
        }
    }
}
