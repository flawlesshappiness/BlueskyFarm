using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class RootMimicEnemy : NavEnemy
{
    [NodeType]
    public AnimationStateMachine AnimationStateMachine;

    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeName]
    public AudioStreamPlayer3D SfxThreat;

    [NodeName]
    public AudioStreamPlayer3D SfxGrowl;

    private enum State
    {
        Wander, Waiting, Threat, Fleeing, Attacking,
        Debug_Follow
    }

    private State _state;
    private float _time_step_sfx;
    private bool _spawned;
    private bool _state_change_disabled;
    private bool _debug_force_attack;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private BasementRoomElement _current_room;

    private AnimationState _anim_walk;
    private AnimationState _anim_threat;
    private AnimationState _anim_waiting;
    private AnimationState _anim_attack;

    private BoolParameter _param_moving;
    private BoolParameter _param_threat;
    private TriggerParameter _param_attack;

    private const float CHANCE_THREAT = 0.5f;
    private const float DIST_WAIT_NEAR = 24;
    private const float DIST_WAIT_FAR = 28;
    private const float DIST_THREAT = 6;
    private const float DIST_THREAT_CLOSE = 4;
    private const float DIST_THREAT_ATTACK = 2;

    protected override void Initialize()
    {
        base.Initialize();

        InitializeAnimations();

        if (IsDebug)
        {
            SpawnDebug();
        }
        else
        {
            Spawn();
        }
    }

    protected override void OnVelocityComputed(Vector3 v)
    {
        if (_state != State.Attacking)
        {
            base.OnVelocityComputed(v);
        }
    }

    private void InitializeAnimations()
    {
        _anim_waiting = AnimationStateMachine.CreateAnimation("Armature|waiting", false);
        _anim_walk = AnimationStateMachine.CreateAnimation("Armature|walk", true);
        _anim_threat = AnimationStateMachine.CreateAnimation("Armature|threat", true);
        _anim_attack = AnimationStateMachine.CreateAnimation("Armature|attack", false);

        _param_moving = AnimationStateMachine.CreateParameter("moving", false);
        _param_threat = AnimationStateMachine.CreateParameter("threat", false);
        _param_attack = AnimationStateMachine.CreateParameter("attack");

        AnimationStateMachine.Connect(_anim_waiting.Node, _anim_walk.Node, _param_moving.WhenTrue());
        AnimationStateMachine.Connect(_anim_walk.Node, _anim_waiting.Node, _param_moving.WhenFalse());

        AnimationStateMachine.Connect(_anim_walk.Node, _anim_threat.Node, _param_threat.WhenTrue());
        AnimationStateMachine.Connect(_anim_waiting.Node, _anim_threat.Node, _param_threat.WhenTrue());
        AnimationStateMachine.Connect(_anim_threat.Node, _anim_walk.Node, _param_threat.WhenFalse());

        AnimationStateMachine.Connect(_anim_waiting.Node, _anim_attack.Node, _param_attack.WhenTriggered());
        AnimationStateMachine.Connect(_anim_walk.Node, _anim_attack.Node, _param_attack.WhenTriggered());
        AnimationStateMachine.Connect(_anim_threat.Node, _anim_attack.Node, _param_attack.WhenTriggered());

        AnimationStateMachine.Initialize(AnimationPlayer);
        AnimationStateMachine.Start(_anim_waiting.Node);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessAnimations();
    }

    private void ProcessAnimations()
    {
        _param_moving.Set(!Agent.IsNavigationFinished());
    }

    protected override void RegisterDebugActions()
    {
        base.RegisterDebugActions();

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyId,
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
            Id = EnemyId,
            Category = EnemyId,
            Text = "Follow player",
            Action = _ => SetState(State.Debug_Follow)
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyId,
            Text = "Set state (Waiting)",
            Action = _ => SetState(State.Waiting)
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyId,
            Text = "Set state (Threat)",
            Action = _ => SetState(State.Threat)
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyId,
            Text = "Force attack",
            Action = _ => _debug_force_attack = true
        });
    }

    private void Spawn()
    {
        _current_room = GetFurthestRoomElementToPlayer();
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

    private void SetState(State state)
    {
        if (_state_change_disabled) return;
        _state = state;

        var id = "state";
        switch (state)
        {
            case State.Wander: this.StartCoroutine(StateCr_Wander, id); return;
            case State.Waiting: this.StartCoroutine(StateCr_Waiting, id); return;
            case State.Threat: this.StartCoroutine(StateCr_Threat, id); return;
            case State.Fleeing: this.StartCoroutine(StateCr_Fleeing, id); return;
            case State.Attacking: this.StartCoroutine(StateCr_Attacking, id); return;
            case State.Debug_Follow: this.StartCoroutine(StateCr_Debug_Follow, id); return;
            default: return;
        }
    }

    private IEnumerator StateCr_Wander()
    {
        var time_wait = GameTime.Time;
        while (true)
        {
            if (DistanceToPlayer < DIST_WAIT_NEAR)
            {
                SetState(State.Waiting);
                break;
            }

            if (GameTime.Time > time_wait)
            {
                var next_room = BasementController.Instance.CurrentBasement.Grid
                        .GetNeighbours(_current_room.Coordinates) // Get neighbours
                        .OrderBy(x => x.Room.GlobalPosition.DistanceTo(Player.Instance.GlobalPosition)) // that is closest to player
                        .Where(x => x.Room.GlobalPosition.DistanceTo(Player.Instance.GlobalPosition) > BasementRoom.ROOM_SIZE * 0.5f) // that player is not in
                        .FirstOrDefault();

                if (next_room != null)
                {
                    _current_room = next_room;
                    Agent.TargetPosition = GetRandomPositionInRoom(_current_room.Room);
                }

                time_wait = GameTime.Time + _rng.RandfRange(15, 30);
            }

            yield return null;
        }
    }

    private IEnumerator StateCr_Waiting()
    {
        StopNavigation();

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
                if (_rng.RandfRange(0, 1) < CHANCE_THREAT || _debug_force_attack)
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

        _param_threat.Set(true);
        SfxGrowl.Play();

        var duration = 4f;
        AnimatePlayerLookAt(1f);
        ScreenEffects.AnimateRadialBlur(nameof(RootMimicEnemy) + GetInstanceId(), 0.02f, 0.1f, duration, 1f);
        ScreenEffects.AnimateHeartbeatFrequency(nameof(RootMimicEnemy) + GetInstanceId(), 0.5f, 0, 1f, 10f);
        SoundController.Instance.Play("sfx_horror_chord");
        SoundController.Instance.Play("sfx_horror_boom");

        var time_wait = GameTime.Time + duration;
        while ((GameTime.Time < time_wait && DistanceToPlayer > DIST_THREAT_ATTACK) || IsDebug)
        {
            TurnTowardsDirection(DirectionToPlayer);
            yield return null;
        }

        if (DistanceToPlayer < DIST_THREAT_CLOSE)
        {
            SetState(State.Attacking);
        }
        else
        {
            _param_threat.Set(false);
            SetState(State.Fleeing);
        }
    }

    private Coroutine AnimatePlayerLookAt(float duration)
    {
        return this.StartCoroutine(Cr, nameof(AnimatePlayerLookAt));
        IEnumerator Cr()
        {
            var id = $"{nameof(AnimatePlayerLookAt)}{EnemyId}";
            Player.LookLock.AddLock(id);
            Player.StartLookingAt(this, 0.05f);

            yield return new WaitForSeconds(duration);

            Player.LookLock.RemoveLock(id);
            Player.StopLookingAt(this);
        }
    }

    private IEnumerator StateCr_Fleeing()
    {
        _current_room = GetFurthestRoomElementToPlayer(x => x != _current_room);
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
        var dir = -DirectionToPlayer.Normalized();
        var start_position = GlobalPosition;
        var attack_position = Player.Instance.GlobalPosition + dir * 3f;

        Player.MovementLock.AddLock(EnemyId);
        Player.LookLock.AddLock(EnemyId);
        Player.Instance.StartLookingAt(this, 0.1f);

        yield return LerpEnumerator.Lerp01(0.25f, f =>
        {
            GlobalPosition = start_position.Lerp(attack_position, f);
        });

        SfxThreat.Play();
        _param_attack.Trigger();
    }

    public void KillPlayer()
    {
        this.StartCoroutine(Cr, nameof(KillPlayer));
        IEnumerator Cr()
        {
            Player.Instance.StopLookingAt();
            Player.Instance.RagdollCamera(DirectionToPlayer.Normalized() * 2);

            yield return new WaitForSeconds(2.0f);

            Player.MovementLock.RemoveLock(EnemyId);
            Player.LookLock.RemoveLock(EnemyId);
            GameScene.Current.KillPlayer();
        }
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

    public void PlayStepSfx()
    {
        SoundController.Instance.Play("sfx_root_mimic_walk", GlobalPosition);
    }
}
