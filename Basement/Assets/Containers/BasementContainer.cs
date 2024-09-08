using Godot;
using Godot.Collections;
using System.Collections;

public partial class BasementContainer : Touchable
{
    [Export]
    public string IdleClosed_AnimationName = "idle_closed";

    [Export]
    public string MoveOpen_AnimationName = "move_open";

    [Export]
    public Array<string> Open_SFX;

    [NodeType]
    public AnimationPlayer Animation;

    [NodeName]
    public Node3D ItemPosition;

    public Item Item { get; set; }

    protected bool _open;

    public override void _Ready()
    {
        base._Ready();
        this.SetCollisionEnabled(IsVisibleInTree());
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

        AnimateOpen();
        SetCollision_None();
        SpawnItemCoroutine(0.1f);

        var sfx = Open_SFX.PickRandom();
        SoundController.Instance.Play(sfx, new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = GlobalPosition,
            PitchMin = 0.95f,
            PitchMax = 1.05f,
        });
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

        Item.UnlockPosition_All();
        Item.UnlockRotation_All();
        Item.SetEnabled(true);
        Item.GlobalPosition = ItemPosition.GlobalPosition;
        Item.RigidBody.LinearVelocity = ItemPosition.GlobalBasis * Vector3.Forward * 4;
    }
}
