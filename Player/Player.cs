using Godot;
using System.Collections;
using System.Linq;

public partial class Player : FirstPersonController
{
    public static Player Instance { get; private set; }

    [Export]
    public float WalkSpeed;

    [Export]
    public float RunSpeed;

    [NodeName]
    public RayCast3D GroundRaycast;

    [NodeName]
    public Area3D WaterTrigger;

    public bool IsInWater { get; private set; }
    public bool IsRunning => PlayerInput.Run.Held;

    public MultiLock MovementLock = new MultiLock();
    public MultiLock LookLock = new MultiLock();


    private static bool _debug_actions_registered;

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

        RegisterDebugActions();
        LoadData();
    }

    private void RegisterDebugActions()
    {
        if (_debug_actions_registered) return;
        _debug_actions_registered = true;

        var category = "Player";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Unstuck",
            Action = Unstuck
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Replenish near item uses",
            Action = ReplenishCloseItems
        });

        void ReplenishCloseItems(DebugView view)
        {
            var items = Scene.Current
                .GetNodesInChildren<Item>()
                .Where(x => GlobalPosition.DistanceTo(x.GlobalPosition) < 5f);

            items.ForEach(x => x.ReplenishUses(999));

            view.Close();
        }

        void Unstuck(DebugView view)
        {
            var player = Player.Instance;
            var agent = player.Agent;
            var target_position = player.GlobalPosition - player.Camera.GlobalBasis.Z * 2f;
            var nav_position = NavigationServer3D.MapGetClosestPoint(agent.GetNavigationMap(), target_position) - new Vector3(0, agent.PathHeightOffset, 0);
            player.GlobalPosition = nav_position;
            view.Close();
        }
    }

    public void UpdateData()
    {
        Data.Game.PlayerPositionX = GlobalPosition.X;
        Data.Game.PlayerPositionY = GlobalPosition.Y;
        Data.Game.PlayerPositionZ = GlobalPosition.Z;
        Data.Game.PlayerNeckVerticalRotation = NeckVertical.Rotation.X;
        Data.Game.PlayerNeckHorizontalRotation = NeckHorizontal.Rotation.Y;
    }

    public void LoadData()
    {
        GlobalPosition = new Vector3(Data.Game.PlayerPositionX, Data.Game.PlayerPositionY, Data.Game.PlayerPositionZ);
        NeckVertical.Rotation = new Vector3(Data.Game.PlayerNeckVerticalRotation, 0, 0);
        NeckHorizontal.Rotation = new Vector3(0, Data.Game.PlayerNeckHorizontalRotation, 0);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        Input_Look(@event);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_Move();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        PhysicsProcess_Ground();
        PhysicsProcess_WaterRippleParticles();
    }

    private void Input_Look(InputEvent e)
    {
        if (LookLock.IsLocked) return;
        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (e is not InputEventMouseMotion) return;
        if (Grab?.IsRotating ?? false) return;

        var factor = 0.001f;
        var motion = e as InputEventMouseMotion;
        var speed = factor * LookSpeedMultiplier;
        Look(motion, speed);
    }

    private void PhysicsProcess_Ground()
    {
        var collider = GroundRaycast.GetCollider();
        if (!IsInstanceValid(collider)) return;

        _ground = collider as SolidMaterial ?? _ground;
    }

    private void Process_Move()
    {
        var input = PlayerInput.GetMoveInput();
        if (input.Length() > 0)
        {
            var speed = IsRunning ? RunSpeed : WalkSpeed;
            Move(input, speed);
        }
        else
        {
            Stop();
        }
    }

    private void OnDialogueStart()
    {
        var name = nameof(DialogueController);
        InventoryController.Instance.InventoryLock.AddLock(name);
        InteractLock.AddLock(name);
    }

    private void OnDialogueEnd()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var delay = 0.5f;
            yield return new WaitForSeconds(delay);

            var name = nameof(DialogueController);
            InventoryController.Instance.InventoryLock.RemoveLock(nameof(DialogueController));
            InteractLock.RemoveLock(name);
        }
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
