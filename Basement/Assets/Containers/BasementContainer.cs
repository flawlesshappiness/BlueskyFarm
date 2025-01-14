using Godot;
using System.Collections;

public partial class BasementContainer : ItemContainer
{
    [Export]
    public string IdleClosed_AnimationName = "idle_closed";

    [Export]
    public string MoveOpen_AnimationName = "move_open";

    [NodeType]
    public AnimationPlayer Animation;

    [NodeName]
    public Node3D ItemPosition;

    [NodeName]
    public GpuParticles3D ps_dust;

    [NodeName]
    public Touchable Touchable;

    protected bool _open;

    public override void _Ready()
    {
        base._Ready();

        Touchable.OnTouched += Touched;

        this.SetEnabled(IsVisibleInTree());
        Animation.Play(IdleClosed_AnimationName);
    }

    private void Touched()
    {
        Open();
    }

    protected void Open()
    {
        if (_open) return;
        _open = true;

        this.StartCoroutine(Cr, nameof(Open));
        IEnumerator Cr()
        {
            yield return Player.Instance.WaitForProgress(2f, ItemPosition);

            AnimateOpen();
            Touchable.ClearCollisionLayerAndMask();
            SpawnItemCoroutine(0.1f);

            SoundController.Instance.Play("sfx_container_open", GlobalPosition);

            ps_dust.Emitting = true;
        }
    }

    protected void AnimateOpen()
    {
        Animation.Play(MoveOpen_AnimationName);
    }

    private Coroutine SpawnItemCoroutine(float delay)
    {
        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            SpawnItem(ItemPosition.GlobalPosition, ItemPosition.GlobalBasis * Vector3.Forward * 4);
        }
    }
}
