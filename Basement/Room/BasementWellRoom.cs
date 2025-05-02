using Godot;
using System.Collections;
using System.Linq;

public partial class BasementWellRoom : Node3DScript
{
    [Export]
    public BasementRoom Room;

    [NodeType]
    public BasementWell Well;

    [NodeName]
    public Node3D KeyOnRope;

    [NodeName]
    public Touchable KeyOnRopeTouchable;

    [NodeName]
    public Touchable MissingHandleTouchable;

    [NodeName]
    public ItemArea HandleArea;

    [Export]
    public ItemInfo WellHandle;

    private bool _key_collected;

    public override void _Ready()
    {
        base._Ready();

        Well.AttachObjectToRope(KeyOnRope);
        Well.OnRaiseFinished += OnWellRaised;
        Well.OnRaise += OnWellAnimate;
        Well.OnLower += OnWellAnimate;

        KeyOnRopeTouchable.OnTouched += KeyOnRope_Touched;

        HandleArea.OnItemEntered += HandleArea_ItemEntered;
    }

    protected override void Initialize()
    {
        base.Initialize();
        InitializeHandle();
    }

    private void InitializeHandle()
    {
        MissingHandleTouchable.SetEnabled(GameFlagIds.BasementWellRepaired.IsFalse());

        if (GameFlagIds.BasementWellRepaired.IsTrue()) return;

        Well.SetHandleEnabled(false);

        var containers = Room.GetContainers();
        var container = containers
            .Where(x => !x.HasItem)
            .ToList().Random();

        if (container == null)
        {
            container = containers
                .ToList()
                .Random();
        }

        container.SetItem(WellHandle);
    }

    private void OnWellAnimate()
    {
        KeyOnRopeTouchable.Disable();
    }

    private void OnWellRaised()
    {
        if (!_key_collected)
        {
            KeyOnRopeTouchable.Enable();
        }
    }

    private void KeyOnRope_Touched()
    {
        if (_key_collected) return;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return Player.Instance.WaitForProgress(2f, KeyOnRope);
            _key_collected = true;

            var item = ItemController.Instance.CreateItem("Key_Workshop");
            item.GlobalPosition = KeyOnRope.GlobalPosition;
            item.GlobalRotation = KeyOnRope.GlobalRotation;

            var dir = item.GlobalPosition.DirectionTo(Player.Instance.GlobalPosition);
            var vel = dir * 5f + Vector3.Up * 1.0f;
            item.LinearVelocity = vel;

            Particle.PlayOneShot("ps_smoke_puff_small", item.GlobalPosition);
            SoundController.Instance.Play("sfx_throw_light", item.GlobalPosition);

            KeyOnRope.Disable();
        }
    }

    private void HandleArea_ItemEntered(Item item)
    {
        GameFlagIds.BasementWellRepaired.SetTrue();

        item.IsBeingHandled = true;
        item.QueueFree();

        SoundController.Instance.Play("sfx_attach_wood", Well.Handle.GlobalPosition);

        Well.SetHandleEnabled(true);

        HandleArea.Disable();
        MissingHandleTouchable.Disable();
    }
}
