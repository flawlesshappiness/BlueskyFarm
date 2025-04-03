using Godot;
using System.Collections;

public partial class BasementWellRoom : Node3DScript
{
    [NodeType]
    public BasementWell Well;

    [NodeName]
    public Node3D KeyOnRope;

    [NodeName]
    public Node3D HandleItemPosition;

    [NodeName]
    public Touchable KeyOnRopeTouchable;

    [NodeName]
    public Touchable MissingHandleTouchable;

    [NodeName]
    public Touchable SomethingInWellTouchable;

    [NodeName]
    public ItemArea HandleArea;

    private bool _key_collected;

    public override void _Ready()
    {
        base._Ready();

        Well.AttachObjectToRope(KeyOnRope);
        Well.OnRaiseFinished += OnWellRaised;
        Well.OnRaise += OnWellAnimate;
        Well.OnLower += OnWellAnimate;

        KeyOnRopeTouchable.OnTouched += KeyOnRope_Touched;
        SomethingInWellTouchable.OnTouched += SomethingInWell_Touched;

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
        SomethingInWellTouchable.SetEnabled(GameFlagIds.BasementWellRepaired.IsFalse());

        if (GameFlagIds.BasementWellRepaired.IsTrue()) return;

        Well.Handle.Disable();
        Well.HandleTouchable.Disable();

        var item_handle = ItemController.Instance.CreateItem("WellHandle", new CreateItemSettings
        {
            Parent = HandleItemPosition
        });

        item_handle.Position = Vector3.Zero;
        item_handle.Rotation = Vector3.Zero;
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

        Well.Handle.Enable();
        Well.HandleTouchable.Enable();

        HandleArea.Disable();
        MissingHandleTouchable.Disable();
        SomethingInWellTouchable.Disable();
    }

    private void SomethingInWell_Touched()
    {
        GameView.Instance.CreateText(new CreateTextSettings
        {
            Id = "something_in_well_" + GetInstanceId(),
            Duration = 3.0f,
            Target = SomethingInWellTouchable,
            Offset = new Vector3(0, 0.2f, 0),
            Text = "##SOMETHING_DOWN_THERE##"
        });
    }
}
