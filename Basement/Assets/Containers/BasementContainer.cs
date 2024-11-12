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
            yield return OpenOverTime(2f);

            AnimateOpen();
            SetCollision_None();
            SpawnItemCoroutine(0.1f);

            SoundController.Instance.Play("sfx_container_open", GlobalPosition);

            ps_dust.Emitting = true;
        }
    }

    private Coroutine OpenOverTime(float duration)
    {
        return StartCoroutine(Cr, nameof(OpenOverTime));
        IEnumerator Cr()
        {
            var id = GetInstanceId().ToString();
            Player.Instance.MovementLock.AddLock(id);
            Player.Instance.InteractLock.AddLock(id);
            Player.Instance.StartLookingAt(Body, 0.05f);

            var time_start = GameTime.Time;
            var time_end = time_start + duration;
            while (GameTime.Time < time_end)
            {
                var t = (GameTime.Time - time_start) / duration;
                Cursor.Progress(new ProgressSettings
                {
                    Position = Body.GlobalPosition,
                    Value = t
                });
                yield return null;
            }

            Player.Instance.MovementLock.RemoveLock(id);
            Player.Instance.InteractLock.RemoveLock(id);
            Player.Instance.StopLookingAt();
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

        Item.UnlockPosition_All();
        Item.UnlockRotation_All();
        Item.SetEnabled(true);
        Item.GlobalPosition = ItemPosition.GlobalPosition;
        Item.RigidBody.LinearVelocity = ItemPosition.GlobalBasis * Vector3.Forward * 4;
    }
}
