using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;

public partial class RockManEnemy : NavEnemy
{
    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public AnimationStateMachine AnimationStateMachine;

    [Export]
    public Skeleton3D Skeleton;

    [Export]
    public SoundInfo SfxStepNear;

    [Export]
    public SoundInfo SfxStepFar;

    [Export]
    public SoundInfo SfxIdle;

    [Export]
    public SoundInfo SfxAmb;

    [Export]
    public SoundInfo SfxAlert;

    [Export]
    public SoundInfo SfxScream;

    [Export]
    public Marker3D FaceMarker;

    [Export]
    public Marker3D ScreamStartMarker;

    [Export]
    public Marker3D ScreamEndMarker;

    [Export]
    public Marker3D CameraTarget;

    protected override string EnemyName => "RockMan";
    protected override string DefaultState => "Idle";

    private Vector3 LeftArmPosition => GetBoneGlobalPosition("IK.Arm.L");
    private Vector3 RightArmPosition => GetBoneGlobalPosition("IK.Arm.R");
    private Vector3 CameraBonePosition => GetBoneGlobalPosition("Camera");
    private Quaternion CameraBoneRotation => GetBoneGlobalRotation("Camera");

    private TriggerParameter _param_idle_look;
    private TriggerParameter _param_attack;
    private BoolParameter _param_moving;
    private BoolParameter _param_scream;

    private int _count_until_spawn;
    private bool _amb_enabled;

    private BasementRoomElement _current_room;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        base._Ready();
        InitializeAnimations();
        StartAmbience();
    }

    protected override void RegisterDebugActions()
    {
        base.RegisterDebugActions();

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyCategory,
            Text = "Trigger",
            Action = _ => { _count_until_spawn = 0; BreakContainer(); }
        });
    }

    public override void InitializeEnemy()
    {
        base.InitializeEnemy();
        InitializeContainers();
    }

    private void InitializeAnimations()
    {
        AnimationStateMachine.Initialize(AnimationPlayer);

        var idle = AnimationStateMachine.CreateAnimation("Armature|idle", true);
        var idle_look = AnimationStateMachine.CreateAnimation("Armature|idle_look", false);
        var attack = AnimationStateMachine.CreateAnimation("Armature|attack", false);
        var walking = AnimationStateMachine.CreateAnimation("Armature|walking", true);
        var scream = AnimationStateMachine.CreateAnimation("Armature|scream", true);

        _param_idle_look = new TriggerParameter("idle_look");
        _param_attack = new TriggerParameter("attack");
        _param_moving = new BoolParameter("moving", false);
        _param_scream = new BoolParameter("scream", false);

        AnimationStateMachine.Connect(idle, idle_look, _param_idle_look.WhenTriggered());
        AnimationStateMachine.Connect(idle_look, idle);

        AnimationStateMachine.Connect(idle, walking, _param_moving.WhenTrue());
        AnimationStateMachine.Connect(idle_look, walking, _param_moving.WhenTrue());
        AnimationStateMachine.Connect(walking, idle, _param_moving.WhenFalse());

        AnimationStateMachine.Connect(idle, attack, _param_attack.WhenTriggered());
        AnimationStateMachine.Connect(idle_look, attack, _param_attack.WhenTriggered());
        AnimationStateMachine.Connect(walking, attack, _param_attack.WhenTriggered());

        AnimationStateMachine.Connect(idle, scream, _param_scream.WhenTrue());
        AnimationStateMachine.Connect(idle_look, scream, _param_scream.WhenTrue());
        AnimationStateMachine.Connect(walking, scream, _param_scream.WhenTrue());
        AnimationStateMachine.Connect(attack, scream, _param_scream.WhenTrue());
        AnimationStateMachine.Connect(scream, idle, _param_scream.WhenFalse());

        AnimationStateMachine.Start(idle.Node);
    }

    protected override void RegisterStates()
    {
        base.RegisterStates();
        RegisterState("Idle", IdleState);
        RegisterState("Search", SearchState);
        RegisterState("Hunt", HuntingState);
        RegisterState("Attack", AttackState);
        RegisterState("Flee", FleeState);
    }

    private void InitializeContainers()
    {
        var container_count = 0;
        var rooms = GetRooms();
        foreach (var room in rooms)
        {
            var containers = room.Room.GetNodesInChildren<RockContainer>();
            foreach (var container in containers)
            {
                if (!container.IsVisibleInTree()) continue;

                container.onBreak += BreakContainer;
                container_count++;
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_Animations();
        Process_LOS();
        Process_Attack();
    }

    private void Process_Animations()
    {
        _param_moving.Set(!Agent.IsNavigationFinished());
    }

    private void Process_Attack()
    {
        if (IsState("Hunt"))
        {
            if (DistanceToPlayer < 3)
            {
                SetState("Attack");
            }
        }
    }

    private void Process_LOS()
    {
        if (IsState("Search") || IsState("Flee"))
        {
            var too_close = DistanceToPlayer < 8;
            var has_los = CanSeePlayer();

            if (too_close && has_los)
            {
                SetState("Hunt");
            }
        }
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);

        if (debug)
        {

        }
        else
        {
            SetState("Idle");
        }
    }

    private void BreakContainer()
    {
        _amb_enabled = IsState("Idle");
        _count_until_spawn--;

        if (IsState("Search") && DistanceToPlayer < BasementRoom.ROOM_SIZE)
        {
            Agent.TargetPosition = PlayerPosition;
        }
    }

    private IEnumerator IdleState()
    {
        _count_until_spawn = _rng.RandiRange(3, 8);
        _amb_enabled = false;
        GlobalPosition = new Vector3(0, -300, 0);

        while (_count_until_spawn > 0)
        {
            yield return null;
        }

        _amb_enabled = false;

        var player_room = GetClosestRoomElementToPlayer();
        var adjacent_room = player_room.Connections.Random();
        GlobalPosition = adjacent_room.Room.EnemyMarker.GlobalPosition;
        Agent.TargetPosition = GlobalPosition;
        var noise_position = PlayerPosition;

        yield return new WaitForSeconds(1f);

        SfxAlert.Play(GlobalPosition);

        yield return new WaitForSeconds(3f);

        Agent.TargetPosition = noise_position;

        SetState("Search");
    }

    private IEnumerator SearchState()
    {
        while (IsMoving)
        {
            yield return null;
        }

        _current_room = GetClosestRoomElement();
        var count_move = _rng.RandiRange(2, 4);
        for (int i = 0; i < count_move; i++)
        {
            _param_idle_look.Trigger();

            yield return new WaitForSeconds(3f);

            var position = GetRandomPositionInRoom(_current_room.Room);
            Agent.TargetPosition = position;

            while (IsMoving)
            {
                yield return null;
            }
        }

        SetState("Flee");
    }

    private IEnumerator HuntingState()
    {
        StopNavigation();
        StartFacingPlayer();
        SfxAlert.Play(GlobalPosition);

        yield return new WaitForSeconds(2f);

        StopFacingPlayer();

        Agent.TargetPosition = PlayerPosition;
        while (IsMoving)
        {
            var too_far = DistanceToPlayer > 12f;
            var cant_see = !CanSeePlayer();

            if (too_far || cant_see)
            {
                // Move to last seen location
            }
            else
            {
                Agent.TargetPosition = PlayerPosition;
            }

            yield return null;
        }

        SetState("Search");
    }

    private IEnumerator AttackState()
    {
        StopNavigation();
        StartFacingPlayer();

        Player.MovementLock.AddLock(EnemyId);
        Player.LookLock.AddLock(EnemyId);

        Player.StartLookingAt(FaceMarker, 0.2f);

        yield return new WaitForSeconds(0.25f);

        _param_attack.Trigger();

        yield return new WaitForSeconds(0.25f);

        Player.RagdollCamera(DirectionToPlayer.Normalized() * 2f);
        StopFacingPlayer();

        ScreenEffects.AnimateGaussianBlur(EnemyId, 20, 0.25f, 0.5f, 0);

        yield return new WaitForSeconds(0.5f);

        // Blackout
        var view = View.Get<GameView>();
        view.SetBlackOverlayAlpha(1);

        var bus = AudioBus.Get(SoundBus.Master.ToString());
        bus.SetMuted(true);

        _param_scream.Set(true);

        CameraTarget.GlobalPosition = ScreamStartMarker.GlobalPosition;
        ScreenEffects.View.SetCameraTarget(CameraTarget);

        yield return new WaitForSeconds(1f);

        // Scream
        bus.SetMuted(false);
        view.SetBlackOverlayAlpha(0);

        var asp_scream = SfxScream.Play();

        ScreenEffects.SetRadialBlur(EnemyId, 0.02f);
        ScreenEffects.AnimateCameraShake(EnemyId, 0.05f, 0, 2.1f, 0);

        yield return LerpEnumerator.Lerp01(2f, f =>
        {
            CameraTarget.GlobalPosition = ScreamStartMarker.GlobalPosition.Lerp(ScreamEndMarker.GlobalPosition, f);
        });

        ScreenEffects.RemoveRadialBlur(EnemyId);

        asp_scream.Stop();
        asp_scream.QueueFree();

        Player.MovementLock.RemoveLock(EnemyId);
        Player.LookLock.RemoveLock(EnemyId);
        GameScene.Current.KillPlayer();
    }

    private IEnumerator FleeState()
    {
        var room = GetFurthestRoomElementToPlayer();
        var position = GetRandomPositionInRoom(room.Room);
        var far_enough = false;
        Agent.TargetPosition = position;

        while (IsMoving)
        {
            far_enough = DistanceToPlayer > BasementRoom.ROOM_SIZE;
            if (far_enough) break;

            yield return null;
        }

        if (far_enough)
        {
            SetState("Idle");
        }
        else
        {
            SetState("Search");
        }

    }

    private Coroutine StartAmbience()
    {
        _amb_enabled = false;
        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            while (true)
            {
                if (_amb_enabled)
                {
                    var position = AmbienceController.Instance.GetAmbientSoundPosition();
                    SfxAmb.Play(position);
                }

                yield return new WaitForSeconds(15f);
            }
        }
    }

    private Vector3 GetBoneGlobalPosition(string id)
    {
        var idx = Skeleton.FindBone(id);
        var position = Skeleton.GetBonePosePosition(idx);
        return Skeleton.GlobalPosition + position;
    }

    private Quaternion GetBoneGlobalRotation(string id)
    {
        var idx = Skeleton.FindBone(id);
        var rotation = Skeleton.GetBonePoseRotation(idx);
        return Skeleton.Quaternion * rotation;
    }

    public void PlayLeftStepSfx()
    {
        PlayStepSfx(LeftArmPosition);
    }

    public void PlayRightStepSfx()
    {
        PlayStepSfx(RightArmPosition);
    }

    private void PlayStepSfx(Vector3 position)
    {
        var dist_far_min = 10f;
        var dist_far_max = 50f;
        var dist_near_min = 0f;
        var dist_near_max = 20f;
        var dist = DistanceToPlayer;

        var t_far = (dist - dist_far_min) / (dist_far_max);
        var t_near = (dist - dist_near_min) / (dist_near_max);

        var vol_far = Mathf.Lerp(0f, 1.25f, t_far);
        var vol_near = Mathf.Lerp(1f, 0f, t_near);

        SfxStepNear.Play(position, new SoundOverride { Volume = AudioMath.PercentageToDecibel(vol_near) });
        SfxStepFar.Play(position, new SoundOverride { Volume = AudioMath.PercentageToDecibel(vol_far) });
    }
}
