using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class RootMimicEnemy : NavEnemy
{
    [NodeName]
    public AudioStreamPlayer3D SfxThreat;

    [NodeName]
    public AudioStreamPlayer3D SfxGrowl;

    [NodeName]
    public SkeletonIK3D ik_leg1_R;

    [NodeName]
    public SkeletonIK3D ik_leg2_R;

    [NodeName]
    public SkeletonIK3D ik_leg3_R;

    [NodeName]
    public SkeletonIK3D ik_leg1_L;

    [NodeName]
    public SkeletonIK3D ik_leg2_L;

    [NodeName]
    public SkeletonIK3D ik_leg3_L;

    [NodeName]
    public SkeletonIK3D ik_neck;

    [NodeName]
    public Node3D Leg1_R;

    [NodeName]
    public Node3D Leg2_R;

    [NodeName]
    public Node3D Leg3_R;

    [NodeName]
    public Node3D Leg1_L;

    [NodeName]
    public Node3D Leg2_L;

    [NodeName]
    public Node3D Leg3_L;

    [NodeName]
    public Node3D Neck;

    [NodeName]
    public Node3D Neck_ThreatPosition;

    [NodeName]
    public Node3D Neck_WalkPosition;

    [NodeName]
    public Node3D Neck_WaitPosition;

    [NodeName]
    public Node3D Leg3_L_ThreatPosition;

    [NodeName]
    public Node3D Leg3_R_ThreatPosition;

    private enum State
    {
        Wander, Waiting, Threat, Fleeing, Attacking,
        Debug_Follow
    }

    private State _state;
    private float _time_step_sfx;
    private float _time_move_pause;
    private float _time_move_pause_check;
    private bool _spawned;
    private bool _ignore_move_pause;
    private bool _state_change_disabled;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private BasementRoomElement _current_room;

    private List<Limb> _limbs = new();

    private const float CHANCE_THREAT = 0.5f;
    private const float CHANCE_WANDER = 0.5f;
    private const float DIST_WAIT_NEAR = 24;
    private const float DIST_WAIT_FAR = 28;
    private const float DIST_THREAT = 5;
    private const float DIST_THREAT_ATTACK = 2;

    private class Limb
    {
        public SkeletonIK3D IK { get; }
        public Node3D Target { get; }
        public Vector3 LocalPosition { get; }
        public Vector3 LocalRotation { get; }
        public Vector3 TargetPosition { get; set; }
        public float MaxDistance { get; set; }
        public float TargetSpeed { get; set; }

        private float _init_max_distance;
        private RandomNumberGenerator _rng = new RandomNumberGenerator();

        public Limb(SkeletonIK3D ik, Node3D target)
        {
            IK = ik;
            Target = target;
            LocalPosition = Target.Position;
            LocalRotation = Target.Rotation;
            MaxDistance = 1f;
            TargetSpeed = 15f;

            _init_max_distance = MaxDistance;
        }

        public void UpdateTargetPosition()
        {
            Target.GlobalPosition = Target.GlobalPosition.Lerp(TargetPosition, TargetSpeed * GameTime.DeltaTime);
        }

        public void RandomizeMaxDistance()
        {
            var range = 0.1f;
            MaxDistance = _rng.RandfRange(_init_max_distance - range, _init_max_distance + range);
        }
    }

    public override void _Ready()
    {
        base._Ready();

        RegisterDebugActions();
        InitializeLimbs();
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (IsDebug)
        {
            SpawnDebug();
        }
        else
        {
            Spawn();
        }
    }

    private void RegisterDebugActions()
    {
        var category = "Enemy (RoomMimic)";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Teleport to player",
            Action = _ => DebugTeleportToPlayer()
        });

        void DebugTeleportToPlayer()
        {
            GlobalPosition = Player.GlobalPosition;
            Agent.TargetPosition = GlobalPosition;
            SetState(State.Waiting);
            _state_change_disabled = true;
        }

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Follow player",
            Action = _ => SetState(State.Debug_Follow)
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Set state (Waiting)",
            Action = _ => SetState(State.Waiting)
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Set state (Threat)",
            Action = _ => SetState(State.Threat)
        });
    }

    private void InitializeLimbs()
    {
        _limbs = new List<Limb>
        {
            new Limb(ik_leg1_L, Leg1_L),
            new Limb(ik_leg2_L, Leg2_L),
            new Limb(ik_leg3_L, Leg3_L),
            new Limb(ik_leg1_R, Leg1_R),
            new Limb(ik_leg2_R, Leg2_R),
            new Limb(ik_leg3_R, Leg3_R),
        };

        foreach (var limb in _limbs)
        {
            limb.IK.Start();
            limb.Target.SetParent(GetParent());
        }

        ik_neck.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Process_Limbs();
    }

    private void Spawn()
    {
        _current_room = GetFurthestRoomElementToPlayer(AreaNames.Basement);
        var spawn = _current_room.Room.GetNodeInChildren<Node3D>("EnemySpawn");
        GlobalPosition = spawn.GlobalPosition;

        _spawned = true;

        SetState(State.Wander);
    }

    private void SpawnDebug()
    {
        GlobalPosition = Player.GlobalPosition;
        _spawned = true;
    }

    private bool IsValidRoomElement(BasementRoomElement element)
    {
        var valid_area = element.AreaName == AreaNames.Basement;
        return valid_area;
    }

    private void SetState(State state)
    {
        if (_state_change_disabled) return;
        _state = state;

        var id = "state";
        switch (state)
        {
            case State.Wander: StartCoroutine(StateCr_Wander, id); return;
            case State.Waiting: StartCoroutine(StateCr_Waiting, id); return;
            case State.Threat: StartCoroutine(StateCr_Threat, id); return;
            case State.Fleeing: StartCoroutine(StateCr_Fleeing, id); return;
            case State.Attacking: StartCoroutine(StateCr_Attacking, id); return;
            case State.Debug_Follow: StartCoroutine(StateCr_Debug_Follow, id); return;
            default: return;
        }
    }

    private IEnumerator StateCr_Wander()
    {
        _ignore_move_pause = false;

        var move_to_room_center = false;
        var time_wait = GameTime.Time;
        while (true)
        {
            if (move_to_room_center)
            {
                if (DistanceToPlayer < DIST_WAIT_NEAR * 0.75f || Agent.IsNavigationFinished())
                {
                    SetState(State.Waiting);
                    break;
                }

                yield return null;
                continue;
            }

            if (DistanceToPlayer < DIST_WAIT_NEAR)
            {
                var closest_room = GetClosestRoomElements(AreaNames.Basement)
                    .Take(2)
                    .OrderByDescending(x => x.Room.GlobalPosition.DistanceTo(PlayerPosition)) // Get room furthest from the player
                    .FirstOrDefault();

                if (GlobalPosition.DistanceTo(closest_room.Room.GlobalPosition) > BasementRoom.ROOM_SIZE * 0.5f)
                {
                    _current_room = closest_room;
                    Agent.TargetPosition = GetRandomPositionInRoom(_current_room.Room);
                    move_to_room_center = true;
                    yield return null;
                    continue;
                }

                SetState(State.Waiting);
                break;
            }

            if (GameTime.Time > time_wait)
            {
                if (_rng.RandfRange(0, 1) < CHANCE_WANDER)
                {
                    _current_room = GetClosestRoomElementToPlayer(AreaNames.Basement);
                    Agent.TargetPosition = GetRandomPositionInRoom(_current_room.Room);
                }

                time_wait = GameTime.Time + _rng.RandfRange(2, 10);
            }

            yield return null;
        }
    }

    private IEnumerator StateCr_Waiting()
    {
        StopNavigation();
        AnimatePose_Wait();

        while (true)
        {
            if (IsDebug)
            {
                yield return null;
                continue;
            }

            if (DistanceToPlayer > DIST_WAIT_FAR)
            {
                SetState(State.Wander);
                break;
            }

            if (DistanceToPlayer < DIST_THREAT && CanSeePlayer())
            {
                if (_rng.RandfRange(0, 1) < CHANCE_THREAT)
                {
                    SetState(State.Threat);
                }
                else
                {
                    ScreenEffects.AnimateRadialBlur(nameof(RootMimicEnemy) + GetInstanceId(), 0.02f, 0.1f, 0f, 1f);
                    SetState(State.Fleeing);
                }

                break;
            }

            yield return null;
        }
    }

    private IEnumerator StateCr_Threat()
    {
        StopNavigation();
        AnimatePose_Threat();

        SfxThreat.Play();
        SfxGrowl.Play();

        var duration = 4f;
        Player.StartLookingAt(this, 0.05f);
        ScreenEffects.AnimateRadialBlur(nameof(RootMimicEnemy) + GetInstanceId(), 0.02f, 0.1f, duration, 1f);
        SoundController.Instance.Play("sfx_horror_chord");
        SoundController.Instance.Play("sfx_horror_boom");

        var time_wait = GameTime.Time + duration;
        while ((GameTime.Time < time_wait && DistanceToPlayer > DIST_THREAT_ATTACK) || IsDebug)
        {
            TurnTowardsDirection(DirectionToPlayer);
            yield return null;
        }

        if (DistanceToPlayer < DIST_THREAT)
        {
            Player.StopLookingAt();
            SetState(State.Attacking);
        }
        else
        {
            Player.StopLookingAt();
            AnimatePose_Walk();
            SetState(State.Fleeing);
        }
    }

    private IEnumerator StateCr_Fleeing()
    {
        _ignore_move_pause = true;
        _current_room = GetFurthestRoomElementToPlayer(AreaNames.Basement);
        Agent.TargetPosition = GetRandomPositionInRoom(_current_room.Room);
        SfxThreat.Play();

        while (true)
        {
            if (Agent.IsNavigationFinished())
            {
                SetState(State.Wander);
            }

            yield return null;
        }
    }

    private IEnumerator StateCr_Attacking()
    {
        _ignore_move_pause = true;

        var id_lock = nameof(RootMimicEnemy) + GetInstanceId();
        Player.MovementLock.AddLock(id_lock);

        SfxGrowl.Play();

        while (DistanceToPlayer > DIST_THREAT_ATTACK)
        {
            Move(DirectionToPlayer * MoveSpeed * 2);
            yield return null;
        }

        Player.MovementLock.RemoveLock(id_lock);
        GameScene.Current.KillPlayer();
    }

    private IEnumerator StateCr_Debug_Follow()
    {
        while (true)
        {
            if (DistanceToPlayer < DIST_THREAT_ATTACK)
            {
                Agent.TargetPosition = GlobalPosition;
            }
            else if (DistanceToPlayer > DIST_THREAT)
            {
                Agent.TargetPosition = Player.GlobalPosition;
            }

            yield return null;
        }
    }

    protected override void OnVelocityComputed(Vector3 v)
    {
        if (GameTime.Time > _time_move_pause_check)
        {
            _time_move_pause_check = GameTime.Time + _rng.RandfRange(2f, 4f);
            _time_move_pause = GameTime.Time + _rng.RandfRange(0.5f, 1.5f);
        }

        if (!_ignore_move_pause && GameTime.Time < _time_move_pause) return;

        base.OnVelocityComputed(v);
    }

    private void Process_Limbs()
    {
        foreach (var limb in _limbs)
        {
            if (IsThreatLimb(limb))
            {
                Process_Limb_Threat(limb);
            }
            else
            {
                Process_Limb_Walk(limb);
            }
        }

        bool IsThreatLimb(Limb limb) => (_state == State.Threat || _state == State.Attacking) && (limb.Target == Leg3_L || limb.Target == Leg3_R);
    }

    private void Process_Limb_Walk(Limb limb)
    {
        var new_position = GlobalPosition + GlobalBasis * limb.LocalPosition;
        var distance = limb.Target.GlobalPosition.DistanceTo(new_position);

        if (distance > limb.MaxDistance)
        {
            limb.TargetPosition = new_position + (GlobalBasis * Vector3.Back * limb.MaxDistance * 0.5f);
            limb.Target.GlobalRotation = GlobalRotation + GlobalBasis * limb.LocalRotation;
            limb.RandomizeMaxDistance();

            PlayStepSfx(limb.TargetPosition);
        }

        limb.UpdateTargetPosition();
    }

    private void Process_Limb_Threat(Limb limb)
    {
        var L = limb.Target == Leg3_L;
        var new_position = L ? Leg3_L_ThreatPosition : Leg3_R_ThreatPosition;
        limb.TargetPosition = new_position.GlobalPosition;
        limb.Target.GlobalRotation = new_position.GlobalRotation;
        limb.UpdateTargetPosition();
    }

    private void PlayStepSfx(Vector3 position)
    {
        if (GameTime.Time < _time_step_sfx) return;
        _time_step_sfx = GameTime.Time + 0.1f;

        SoundController.Instance.Play("sfx_root_mimic_walk", position);
    }

    private void AnimatePose_Threat()
    {
        StartCoroutine(Cr, "pose");
        IEnumerator Cr()
        {
            var curve = Curves.EaseOutQuad;
            var start_position = Neck.Position;
            var start_rotation = Neck.Rotation;
            yield return LerpEnumerator.Lerp01(0.25f, f =>
            {
                Neck.Position = start_position.Lerp(Neck_ThreatPosition.Position, curve.Evaluate(f));
                Neck.Rotation = start_rotation.Lerp(Neck_ThreatPosition.Rotation, curve.Evaluate(f));
            });
        }
    }

    private void AnimatePose_Walk()
    {
        StartCoroutine(Cr, "pose");
        IEnumerator Cr()
        {
            var curve = Curves.EaseOutQuad;
            var start_position = Neck.Position;
            var start_rotation = Neck.Rotation;
            yield return LerpEnumerator.Lerp01(0.25f, f =>
            {
                Neck.Position = start_position.Lerp(Neck_WalkPosition.Position, curve.Evaluate(f));
                Neck.Rotation = start_rotation.Lerp(Neck_WalkPosition.Rotation, curve.Evaluate(f));
            });
        }
    }

    private void AnimatePose_Wait()
    {
        StartCoroutine(Cr, "pose");
        IEnumerator Cr()
        {
            var curve = Curves.EaseOutQuad;
            var start_position = Neck.Position;
            var start_rotation = Neck.Rotation;
            yield return LerpEnumerator.Lerp01(0.25f, f =>
            {
                Neck.Position = start_position.Lerp(Neck_WaitPosition.Position, curve.Evaluate(f));
                Neck.Rotation = start_rotation.Lerp(Neck_WaitPosition.Rotation, curve.Evaluate(f));
            });
        }
    }

    private Vector3 GetRandomPositionInRoom(BasementRoom room)
    {
        var room_size = BasementRoom.ROOM_SIZE;
        var min = room_size * 0.2f;
        var max = room_size * 0.5f;
        var x = _rng.RandfRange(min, max);
        var z = _rng.RandfRange(min, max);
        return room.GlobalPosition + new Vector3(x, 0, z);
    }
}
