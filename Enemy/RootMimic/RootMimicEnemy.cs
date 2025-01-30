using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class RootMimicEnemy : NavEnemy
{
    [Export]
    public AnimationStateMachine AnimationStateMachine;

    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public SoundInfo SfxThreat;

    [Export]
    public SoundInfo SfxGrowl;

    protected override string EnemyName => "RootMimic";
    protected override string DefaultState => StateWander;

    private const string StateWander = "Wander";
    private const string StateWaiting = "Waiting";
    private const string StateThreat = "Threat";
    private const string StateFleeing = "Fleeing";
    private const string StateAttacking = "Attacking";

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

    public override void InitializeEnemy()
    {
        base.InitializeEnemy();
        InitializeAnimations();
    }

    protected override void OnVelocityComputed(Vector3 v)
    {
        if (!IsState(StateAttacking))
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

    protected override void RegisterStates()
    {
        base.RegisterStates();
        RegisterState(StateWander, StateCr_Wander);
        RegisterState(StateWaiting, StateCr_Waiting);
        RegisterState(StateThreat, StateCr_Threat);
        RegisterState(StateFleeing, StateCr_Fleeing);
        RegisterState(StateAttacking, StateCr_Attacking);
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
            Category = EnemyCategory,
            Text = "Force attack",
            Action = _ => _debug_force_attack = true
        });
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);

        if (debug)
        {
            GlobalPosition = Player.GlobalPosition;
        }
        else
        {
            _current_room = GetFurthestRoomElementToPlayer();
            var spawn = _current_room.Room.GetNodeInChildren<Node3D>("EnemySpawn");
            GlobalPosition = spawn.GlobalPosition;

            SetState(StateWander);
        }
    }

    private IEnumerator StateCr_Wander()
    {
        var time_wait = GameTime.Time;
        while (true)
        {
            if (DistanceToPlayer < DIST_WAIT_NEAR)
            {
                SetState(StateWaiting);
                break;
            }

            if (GameTime.Time > time_wait)
            {
                var next_room = BasementController.Instance.CurrentBasement.Grid
                        .GetNeighbours(_current_room.Coordinates) // Get neighbours
                        .Where(x => x.Info.Area == TargetArea) // In target area
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
                SetState(StateWander);
                break;
            }

            if (DistanceToPlayer < DIST_THREAT && CanSeePlayer())
            {
                if (_rng.RandfRange(0, 1) < CHANCE_THREAT || _debug_force_attack)
                {
                    SetState(StateThreat);
                }
                else
                {
                    ScreenEffects.AnimateRadialBlur(nameof(RootMimicEnemy) + GetInstanceId(), 0.02f, 0.1f, 0f, 1f);
                    SetState(StateFleeing);
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
            SetState(StateAttacking);
        }
        else
        {
            _param_threat.Set(false);
            SetState(StateFleeing);
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
                SetState(StateWander);
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

    public void PlayStepSfx()
    {
        SoundController.Instance.Play("sfx_root_mimic_walk", GlobalPosition);
    }
}
