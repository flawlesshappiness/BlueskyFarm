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

    [NodeType]
    public FirstPersonInteract Interact;

    [NodeType]
    public FirstPersonGrab Grab;

    [NodeType]
    public FirstPersonStep Step;

    [NodeType]
    public PlayerStepSound StepSound;

    [NodeName]
    public RayCast3D GroundRaycast;

    [NodeName]
    public Area3D WaterTrigger;

    [NodeName]
    public Node3D CameraTarget;

    [NodeType]
    public NavigationAgent3D Agent;

    [NodeName]
    public RigidBody3D CameraRagdoll;

    public bool IsInWater { get; private set; }
    public bool IsRunning => PlayerInput.Run.Held;
    public Vector3 MidPosition => GlobalPosition.Add(y: Capsule.Height * 0.5f);
    public float MoveSpeedMultiplier => _move_multiplier_max.Value;
    public float LookSpeedMultiplier => _look_multiplier_max.Value;

    public MultiLock MovementLock = new MultiLock();
    public MultiLock LookLock = new MultiLock();
    public MultiLock InteractLock = new MultiLock();

    private static bool _debug_actions_registered;

    private SolidMaterial _ground;
    private WaterArea _current_water_area;
    private float _time_next_water_ripple;

    private MultiFloatMax _move_multiplier_max = new MultiFloatMax(1);
    private MultiFloatMax _look_multiplier_max = new MultiFloatMax(1);

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

        FreezeCameraRagdoll();
        ScreenEffects.View.SetCameraTarget(CameraTarget);
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

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Test camera ragdoll",
            Action = v => { Instance.RagdollCameraAndPickUp(Instance.CameraTarget.GlobalBasis * (Vector3.Forward * 4)); v.Close(); }
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
            var target_position = player.GlobalPosition - player.CameraTarget.GlobalBasis.Z * 2f;
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
        Process_Cursor();
        Process_Interact();
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
        var sens = Data.Options.MouseSensitivity;
        var motion = e as InputEventMouseMotion;
        var speed = factor * sens * LookSpeedMultiplier;
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
        if (input.Length() > 0 && !MovementLock.IsLocked)
        {
            var base_speed = IsRunning ? RunSpeed : WalkSpeed;
            var speed = base_speed * MoveSpeedMultiplier;
            Move(input, speed);
        }
        else
        {
            Stop();
        }
    }

    private void Process_Cursor()
    {
        var target = Interact.CurrentInteractable;

        if (InteractLock.IsLocked)
        {
            // Purposefully unhandled
        }
        else if (Grab.TryHandleCursor(target))
        {
            // Handled by grab
        }
        else if (TryHandleCursor_Touch(target))
        {
            // Handled by touch
        }
        else
        {
            Cursor.Hide();
        }
    }

    private void Process_Interact()
    {
        if (InteractLock.IsLocked) return;

        if (PlayerInput.Interact.Pressed)
        {
            var interactable = Interact.CurrentInteractable;
            if (TryGrab(interactable)) return;
            if (TryTouch(interactable)) return;
        }
        else if (PlayerInput.Interact.Released)
        {
            Grab?.Release();
        }
    }

    private bool TryHandleCursor_Touch(IInteractable interactable)
    {
        var touchable = interactable as Touchable;
        if (!IsInstanceValid(touchable)) return false;

        if (touchable.HandleCursor())
        {
            // Handled by touchable
            return true;
        }
        else
        {
            Cursor.Show(new CursorSettings
            {
                Name = "Touch",
                OverrideTexture = interactable.GetHoverIcon(),
                Text = interactable.GetHoverText(),
                Position = interactable.GetHoverIconPosition()
            });
        }

        return true;
    }

    private bool TryGrab(IInteractable interactable)
    {
        if (Grab == null) return false;
        if (interactable == null) return false;

        var grabbable = interactable as Grabbable;
        if (grabbable == null) return false;

        Grab.Grab(grabbable);
        return true;
    }

    private bool TryTouch(IInteractable interactable)
    {
        if (interactable == null) return false;

        var touchable = interactable as Touchable;
        if (touchable == null) return false;

        touchable.Touch();
        return true;
    }

    private void OnDialogueStart()
    {
        var name = nameof(DialogueController);
        InventoryController.Instance.InventoryLock.AddLock(name);
        InteractLock.AddLock(name);
        GameView.Instance.HideAllDynamicUI();
        Cursor.Hide();
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

    public void SetLookSpeedMultiplier(string id, float value)
    {
        _look_multiplier_max.Set(id, Mathf.Clamp(value, 0, 1));
    }

    public void RemoveLookSpeedMultiplier(string id)
    {
        _look_multiplier_max.Remove(id);
    }

    public void SetMoveSpeedMultiplier(string id, float value)
    {
        _move_multiplier_max.Set(id, Mathf.Clamp(value, 0, 1));
    }

    public void RemoveMoveSpeedMultiplier(string id)
    {
        _move_multiplier_max.Remove(id);
    }

    public Coroutine AnimateLookSpeedMultiplier(string id, float value, float duration_in, float duration_on, float duration_out)
    {
        return Coroutine.Start(Cr, $"{nameof(AnimateLookSpeedMultiplier)}_{id}", this);
        IEnumerator Cr()
        {
            var start = _look_multiplier_max.DefaultValue;
            var end = value;
            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                SetLookSpeedMultiplier(id, Mathf.Lerp(start, end, f));
            });

            yield return new WaitForSeconds(duration_on);

            start = value;
            end = _look_multiplier_max.DefaultValue;
            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                SetLookSpeedMultiplier(id, Mathf.Lerp(start, end, f));
            });

            RemoveLookSpeedMultiplier(id);
        }
    }

    public Coroutine AnimateMoveSpeedMultiplier(string id, float value, float duration_in, float duration_on, float duration_out)
    {
        return Coroutine.Start(Cr, $"{nameof(AnimateMoveSpeedMultiplier)}_{id}", this);
        IEnumerator Cr()
        {
            var start = _move_multiplier_max.DefaultValue;
            var end = value;
            yield return LerpEnumerator.Lerp01(duration_in, f =>
            {
                SetMoveSpeedMultiplier(id, Mathf.Lerp(start, end, f));
            });

            yield return new WaitForSeconds(duration_on);

            start = value;
            end = _move_multiplier_max.DefaultValue;
            yield return LerpEnumerator.Lerp01(duration_out, f =>
            {
                SetMoveSpeedMultiplier(id, Mathf.Lerp(start, end, f));
            });

            RemoveMoveSpeedMultiplier(id);
        }
    }

    private void WaterAreaEntered(GodotObject go)
    {
        var area = go as Area3D;
        if (!IsInstanceValid(area)) return;

        if (area is WaterArea water && water != null)
        {
            IsInWater = true;
            _current_water_area = water;

            StepSound.PlayStepSFX(SolidMaterialType.Water, IsRunning);
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

            StepSound.PlayStepSFX(SolidMaterialType.Water, IsRunning);
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

    private void FreezeCameraRagdoll()
    {
        CameraRagdoll.LockPosition_All();
        CameraRagdoll.LockRotation_All();
        CameraRagdoll.SetParent(CameraTarget);
        CameraRagdoll.Position = Vector3.Zero;
        CameraRagdoll.Rotation = Vector3.Zero;
        CameraRagdoll.Disable();
    }

    public Coroutine RagdollCameraAndPickUp(Vector3 velocity, float delay_pickup = 0f)
    {
        return Coroutine.Start(Cr, $"{nameof(RagdollCameraAndPickUp) + GetInstanceId()}", this);
        IEnumerator Cr()
        {
            yield return RagdollCamera(velocity);
            yield return new WaitForSeconds(delay_pickup);
            yield return UnragdollCamera();
        }
    }

    public Coroutine RagdollCamera(Vector3 velocity)
    {
        CameraRagdoll.Enable();
        CameraRagdoll.UnlockPosition_All();
        CameraRagdoll.UnlockRotation_All();
        CameraRagdoll.SetParent(Scene.Current);
        CameraRagdoll.GlobalRotation = CameraTarget.GlobalRotation;
        ScreenEffects.View.SetCameraTarget(CameraRagdoll);
        Cursor.Hide();

        var id_lock = "ragdoll";
        MovementLock.AddLock(id_lock);
        LookLock.AddLock(id_lock);
        InteractLock.AddLock(id_lock);

        CameraRagdoll.LinearVelocity = velocity;
        CameraRagdoll.AngularVelocity = velocity.Normalized();

        return this.StartCoroutine(Cr, nameof(RagdollCamera));
        IEnumerator Cr()
        {
            while (CameraRagdoll.LinearVelocity.Length() > 0.2f)
            {
                yield return null;
            }
        }
    }

    public Coroutine UnragdollCamera()
    {
        GlobalPosition = CameraRagdoll.GlobalPosition;

        return this.StartCoroutine(Cr, nameof(UnragdollCamera));
        IEnumerator Cr()
        {
            var start_position = CameraRagdoll.GlobalPosition;
            var end_position = CameraTarget.GlobalPosition;
            var start_rotation = CameraRagdoll.GlobalRotation;
            var end_rotation = CameraTarget.GlobalRotation;
            var curve = Curves.EaseInOutQuad;

            yield return LerpEnumerator.Lerp01(1.0f, f =>
            {
                var t = curve.Evaluate(f);
                CameraRagdoll.GlobalPosition = start_position.Lerp(end_position, t);
                CameraRagdoll.GlobalRotation = start_rotation.Lerp(end_rotation, t);
            });

            FreezeCameraRagdoll();
            ScreenEffects.View.SetCameraTarget(CameraTarget);

            var id_lock = "ragdoll";
            MovementLock.RemoveLock(id_lock);
            LookLock.RemoveLock(id_lock);
            InteractLock.RemoveLock(id_lock);
        }
    }

    public Coroutine WaitForProgress(float duration, Node3D target)
    {
        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var id = GetInstanceId().ToString();
            MovementLock.AddLock(id);
            InteractLock.AddLock(id);
            StartLookingAt(target, 0.05f);

            var time_start = GameTime.Time;
            var time_end = time_start + duration;
            while (GameTime.Time < time_end)
            {
                var t = (GameTime.Time - time_start) / duration;
                Cursor.Progress(new ProgressSettings
                {
                    Position = target.GlobalPosition,
                    Value = t
                });
                yield return null;
            }

            MovementLock.RemoveLock(id);
            InteractLock.RemoveLock(id);
            StopLookingAt();
        }
    }

    public bool HasAccessToItem(string item_id) => HasAccessToItem(ItemController.Instance.Collection.GetResource(item_id));
    public bool HasAccessToItem(ItemInfo info)
    {
        var item_path = info.ResourcePath;
        var scene_data = Data.Game.Scenes.FirstOrDefault(x => x.Name == nameof(FarmScene));
        var item_in_scene = scene_data.Items.Any(x => x.Info == item_path);
        var item_in_inv = Data.Game.InventoryItems.Any(x => x != null && x.Info == item_path);
        return item_in_scene || item_in_inv;
    }

    public bool HasAccessToBlueprint(string bp_id)
    {
        var scene_data = Data.Game.Scenes.FirstOrDefault(x => x.Name == nameof(FarmScene));
        var bp_in_scene = scene_data.Items.Any(x => x.Blueprint?.Id == bp_id);
        var bp_in_inv = Data.Game.InventoryItems.Any(x => x != null && x.Blueprint?.Id == bp_id);
        var bp_in_progress = Data.Game.BlueprintCraftingData?.Id == bp_id;
        return bp_in_scene || bp_in_inv || bp_in_progress;
    }
}
