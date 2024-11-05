using Godot;

public partial class Player : FirstPersonController
{
    public static Player Instance { get; private set; }

    [NodeName]
    public RayCast3D GroundRaycast;

    [NodeName]
    public Area3D WaterArea;

    public bool IsInWater { get; private set; }

    private SolidMaterial _ground;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;

        WaterArea.AreaEntered += v => CallDeferred(nameof(WaterAreaEntered), v);
        WaterArea.AreaExited += v => CallDeferred(nameof(WaterAreaExited), v);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        PhysicsProcess_Ground();
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
        }
    }

    private void WaterAreaExited(GodotObject go)
    {
        var area = go as Area3D;
        if (!IsInstanceValid(area)) return;

        if (area is WaterArea water && water != null)
        {
            IsInWater = false;
        }
    }
}
