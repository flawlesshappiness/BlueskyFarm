using Godot;

public partial class Player : FirstPersonController
{
    public static Player Instance { get; private set; }

    [NodeName]
    public RayCast3D GroundRaycast;

    [NodeName]
    public Area3D WaterTrigger;

    public bool IsInWater { get; private set; }

    private SolidMaterial _ground;
    private WaterArea _current_water_area;
    private float _time_next_water_ripple;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;

        WaterTrigger.AreaEntered += v => CallDeferred(nameof(WaterAreaEntered), v);
        WaterTrigger.AreaExited += v => CallDeferred(nameof(WaterAreaExited), v);

        DialogueController.Instance.OnDialogueStart += OnDialogueStart;
        DialogueController.Instance.OnDialogueEnd += OnDialogueEnd;
    }

    private void OnDialogueStart()
    {
        var name = nameof(DialogueController);
        InventoryController.Instance.InventoryLock.AddLock(name);
        InteractLock.AddLock(name);
    }

    private void OnDialogueEnd()
    {
        var name = nameof(DialogueController);
        InventoryController.Instance.InventoryLock.RemoveLock(nameof(DialogueController));
        InteractLock.RemoveLock(name);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        PhysicsProcess_Ground();
        PhysicsProcess_WaterRippleParticles();
    }

    private void PhysicsProcess_Ground()
    {
        var collider = GroundRaycast.GetCollider();
        if (!IsInstanceValid(collider)) return;

        _ground = collider as SolidMaterial ?? _ground;
    }

    public SolidMaterial GetGround()
    {
        return _ground;
    }

    private void WaterAreaEntered(GodotObject go)
    {
        var area = go as Area3D;
        if (!IsInstanceValid(area)) return;

        if (area is WaterArea water && water != null)
        {
            IsInWater = true;
            _current_water_area = water;
        }
    }

    private void WaterAreaExited(GodotObject go)
    {
        var area = go as Area3D;
        if (!IsInstanceValid(area)) return;

        if (area is WaterArea water && water != null)
        {
            IsInWater = false;
            _current_water_area = null;
        }
    }

    private void PhysicsProcess_WaterRippleParticles()
    {
        if (!IsInWater) return;
        if (Velocity.Length() < 0.5f) return;
        if (GameTime.Time < _time_next_water_ripple) return;

        _time_next_water_ripple = GameTime.Time + 0.01f;
        PlaySplashParticle();
    }

    private void PlaySplashParticle()
    {
        if (!IsInstanceValid(_current_water_area)) return;

        var position = GlobalPosition.Set(y: _current_water_area.GlobalWaterHeight + 0.01f);
        Particle.PlayOneShot("ps_water_ripple", position);
    }
}
