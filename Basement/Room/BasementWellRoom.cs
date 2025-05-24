using Godot;
using System.Collections;
using System.Linq;

public partial class BasementWellRoom : Node3DScript
{
    [Export]
    public BasementRoom Room;

    [Export]
    public BasementWell Well;

    [Export]
    public Node3D KeyOnRope;

    [Export]
    public Node3D VegetableOnRope;

    [Export]
    public Node3D BoneOnRope;

    [Export]
    public Node3D CrystalOnRope;

    [Export]
    public Touchable MissingHandleTouchable;

    [Export]
    public ItemArea HandleArea;

    [Export]
    public ItemArea RopeEndArea;

    [Export]
    public ItemInfo WellHandle;

    [Export]
    public ItemInfo WorkshopKey;

    [Export]
    public ItemInfo VegetableItem;

    [Export]
    public ItemInfo BoneItem;

    [Export]
    public ItemInfo CrystalItem;

    private ItemType item_type_on_rope = ItemType.Other;

    public override void _Ready()
    {
        base._Ready();

        HandleArea.OnItemEntered += HandleArea_ItemEntered;
        RopeEndArea.OnItemEntered += RopeEndArea_ItemEntered;
        Well.OnRopeEndTouched += WellRopeEnd_Touched;
        Well.OnLowered += Well_Lowered;
    }

    protected override void Initialize()
    {
        base.Initialize();
        InitializeHandle();
        InitializeKey();
        InitializeVegetables();
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

    private void InitializeKey()
    {
        var has_key = Player.HasAccessToItem(WorkshopKey);
        var used_key = GameFlagIds.BasementWorkshopDoorUnlocked.IsTrue();
        var active_key = !has_key && !used_key;

        if (active_key)
        {
            Well.CanEnableRopeEndTouchable = true;
            Well.TryEnableRopeEndTouchable();
            Well.AttachObjectToRope(KeyOnRope);
            KeyOnRope.Enable();
            RopeEndArea.Disable();
        }
        else
        {
            KeyOnRope.Disable();
            Well.CanEnableRopeEndTouchable = false;
            Well.DisableRopeEndTouchable();
            RopeEndArea.Enable();

            TryAttachSpecialItem();
        }
    }

    private void InitializeVegetables()
    {
        Well.AttachObjectToRope(VegetableOnRope);
        Well.AttachObjectToRope(BoneOnRope);
        Well.AttachObjectToRope(CrystalOnRope);

        VegetableOnRope.Disable();
        BoneOnRope.Disable();
        CrystalOnRope.Disable();
    }

    private void WellRopeEnd_Touched()
    {
        KeyOnRope_Touched();
    }

    private void KeyOnRope_Touched()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return Player.Instance.WaitForProgress(2f, KeyOnRope);

            var item = ItemController.Instance.CreateItem(WorkshopKey);
            item.GlobalPosition = KeyOnRope.GlobalPosition;
            item.GlobalRotation = KeyOnRope.GlobalRotation;

            var dir = item.GlobalPosition.DirectionTo(Player.Instance.GlobalPosition);
            var vel = dir * 5f + Vector3.Up * 1.0f;
            item.LinearVelocity = vel;

            Particle.PlayOneShot("ps_smoke_puff_small", item.GlobalPosition);
            SoundController.Instance.Play("sfx_throw_light", item.GlobalPosition);

            KeyOnRope.Disable();
            RopeEndArea.Enable();
            Well.CanEnableRopeEndTouchable = false;
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

    private void RopeEndArea_ItemEntered(Item item)
    {
        if (!Well.IsRaised) return;

        var type = item.Info.Type;
        var on_rope = GetOnRopeObject(type);

        if (on_rope == null) return;

        item.Disable();
        item.QueueFree();
        on_rope.Enable();
        RopeEndArea.Disable();

        item_type_on_rope = type;

        SoundController.Instance.Play("sfx_throw_light", Well.RopeEndPosition);
    }

    private Node3D GetOnRopeObject(ItemType type) => type switch
    {
        ItemType.Crop_Vegetable => VegetableOnRope,
        ItemType.Crop_Bone => BoneOnRope,
        ItemType.Crop_Stone => CrystalOnRope,
        _ => null
    };

    private void Well_Lowered()
    {
        VegetableOnRope.Disable();
        BoneOnRope.Disable();
        CrystalOnRope.Disable();

        RopeEndArea.Enable();

        if (item_type_on_rope == ItemType.Crop_Vegetable)
        {
            GameFlagIds.BasementWellVegetableOffered.SetTrue();
        }
        else if (item_type_on_rope == ItemType.Crop_Bone)
        {
            GameFlagIds.BasementWellBoneOffered.SetTrue();
        }
        else if (item_type_on_rope == ItemType.Crop_Stone)
        {
            GameFlagIds.BasementWellCrystalOffered.SetTrue();
        }

        item_type_on_rope = ItemType.Other;

        TryAttachSpecialItem();
    }

    private void TryAttachSpecialItem()
    {
        if (item_type_on_rope != ItemType.Other) return;
        if (GameFlagIds.BasementWellVegetableOffered.IsFalse()) return;
        if (GameFlagIds.BasementWellBoneOffered.IsFalse()) return;
        if (GameFlagIds.BasementWellCrystalOffered.IsFalse()) return;
        // Player has access to item?

        // Attach item
    }
}
