using Godot;
using System.Collections;

public partial class BasementContainer : Touchable
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

    public Item Item { get; set; }

    protected bool _open;

    public override void _Ready()
    {
        base._Ready();
        this.SetEnabled(IsVisibleInTree());
        Animation.Play(IdleClosed_AnimationName);
    }

    protected override void Touched()
    {
        base.Touched();
        Open();
    }

    protected void Open()
    {
        if (_open) return;
        _open = true;

        StartCoroutine(Cr, nameof(Open));
        IEnumerator Cr()
        {
            yield return Player.Instance.WaitForProgress(2f, Body);

            AnimateOpen();
            SetCollision_None();
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
            SpawnItem();
        }
    }

    private void SpawnItem()
    {
        if (!IsInstanceValid(Item)) return;

        Item.RigidBody.UnlockPosition_All();
        Item.RigidBody.UnlockRotation_All();
        Item.SetEnabled(true);
        Item.GlobalPosition = ItemPosition.GlobalPosition;
        Item.RigidBody.LinearVelocity = ItemPosition.GlobalBasis * Vector3.Forward * 4;
    }
}
