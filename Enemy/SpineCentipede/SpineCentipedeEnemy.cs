using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class SpineCentipedeEnemy : NavEnemy
{
    [Export]
    public SoundInfo SfxWheezeShort;

    [Export]
    public SoundInfo SfxWheezeLong;

    [Export]
    public SoundInfo SfxTick;

    [Export]
    public SoundInfo SfxAttack;

    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public AnimationStateMachine AnimationStateMachine;

    [Export]
    public AudioStreamPlayer3D SfxMoving;

    [Export]
    public PlayerArea ChargeArea;

    [Export]
    public PlayerArea AttackArea;

    protected override string EnemyName => "SpineCentipede";
    protected override string DefaultState => StatePatrol;
    private bool IsAttacking => IsState(StateCharge, StateAttack);

    private BoolParameter _param_moving;
    private BoolParameter _param_charge;
    private TriggerParameter _param_attack;
    private float _time_next_wheeze;
    private bool _killed_player;

    private BasementRoomElement _start_room;
    private BasementRoomElement _end_room;
    private bool _patrol_to_end;
    private Vector3 _attack_position;

    private const string StatePatrol = "Patrol";
    private const string StateCharge = "Charge";
    private const string StateAttack = "Attack";

    public override void InitializeEnemy()
    {
        base.InitializeEnemy();

        InitializeAnimations();

        ChargeArea.PlayerEntered += ChargeArea_PlayerEntered;
        AttackArea.PlayerEntered += AttackArea_PlayerEntered;

        AttackArea.Disable();
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);

        if (debug)
        {
            SetState(StateDebugIdle);
        }
        else
        {
            var grid = BasementController.Instance.CurrentBasement.Grid;

            _start_room = GetRooms()
                .OrderBy(x => grid.GetNeighbours(x.Coordinates).Count)
                .FirstOrDefault();

            _end_room = GetRooms()
                .OrderByDescending(x => x.Room.GlobalPosition.DistanceSquaredTo(_start_room.Room.GlobalPosition))
                .FirstOrDefault();

            var spawn = _start_room.Room.GetNodeInChildren<Node3D>("EnemySpawn");
            GlobalPosition = spawn.GlobalPosition;

            SetState(DefaultState);
        }
    }

    private void InitializeAnimations()
    {
        var idle = AnimationStateMachine.CreateAnimation("Armature|idle", true);
        var moving = AnimationStateMachine.CreateAnimation("Armature|moving", true);
        var charging = AnimationStateMachine.CreateAnimation("Armature|charging", false);
        var attacking = AnimationStateMachine.CreateAnimation("Armature|attacking", false);

        _param_moving = AnimationStateMachine.CreateParameter("moving", false);
        _param_charge = AnimationStateMachine.CreateParameter("charging", false);
        _param_attack = AnimationStateMachine.CreateParameter("attack");

        AnimationStateMachine.Connect(idle.Node, moving.Node, _param_moving.WhenTrue());
        AnimationStateMachine.Connect(moving.Node, idle.Node, _param_moving.WhenFalse());

        AnimationStateMachine.Connect(idle.Node, charging.Node, _param_charge.WhenTrue());
        AnimationStateMachine.Connect(moving.Node, charging.Node, _param_charge.WhenTrue());
        AnimationStateMachine.Connect(charging.Node, idle.Node, _param_charge.WhenFalse());

        AnimationStateMachine.Connect(charging.Node, attacking.Node, _param_attack.WhenTriggered());
        AnimationStateMachine.Connect(idle.Node, attacking.Node, _param_attack.WhenTriggered());
        AnimationStateMachine.Connect(moving.Node, attacking.Node, _param_attack.WhenTriggered());
        AnimationStateMachine.Connect(attacking.Node, idle.Node);

        AnimationStateMachine.Initialize(AnimationPlayer);
        AnimationStateMachine.Start(idle.Node);
    }

    protected override void RegisterStates()
    {
        base.RegisterStates();
        RegisterState(StatePatrol, StateCr_Patrol);
        RegisterState(StateCharge, StateCr_Charge);
        RegisterState(StateAttack, StateCr_Attack);
    }

    protected override void OnVelocityComputed(Vector3 v)
    {
        if (IsState(StateCharge, StateAttack)) return;
        base.OnVelocityComputed(v);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessAnimations();
        ProcessWheeze();
    }

    private void ProcessAnimations()
    {
        var moving = !Agent.IsNavigationFinished() && !IsAttacking;
        _param_moving.Set(moving);

        SfxMoving.SetPlaying(moving);
    }

    private void ProcessWheeze()
    {
        if (GameTime.Time > _time_next_wheeze)
        {
            var rng = new RandomNumberGenerator();
            _time_next_wheeze = GameTime.Time + rng.RandfRange(10, 20);

            if (!IsAttacking)
            {
                PlayWheezeShortSfx();
            }
        }
    }

    private IEnumerator StateCr_Debug_Idle()
    {
        while (true)
        {
            if (!Agent.IsNavigationFinished())
            {
                Agent.TargetPosition = GlobalPosition;
            }

            yield return null;
        }
    }

    private IEnumerator StateCr_Debug_Follow()
    {
        while (true)
        {
            if (DistanceToPlayer < 2)
            {
                Agent.TargetPosition = GlobalPosition;
            }
            else if (DistanceToPlayer > 3)
            {
                Agent.TargetPosition = Player.GlobalPosition;
            }

            yield return null;
        }
    }

    private IEnumerator StateCr_Patrol()
    {
        UpdateTargetPosition();
        ChargeArea.Enable();

        while (true)
        {
            if (Agent.IsNavigationFinished())
            {
                yield return new WaitForSeconds(5f);

                _patrol_to_end = !_patrol_to_end;
                UpdateTargetPosition();
            }

            yield return null;
        }

        void UpdateTargetPosition()
        {
            var room = _patrol_to_end ? _end_room : _start_room;
            var position = GetRandomPositionInRoom(room.Room);
            Agent.TargetPosition = position;
        }
    }

    private IEnumerator StateCr_Charge()
    {
        _param_charge.Set(true);

        var duration = 2f;
        ScreenEffects.AnimateRadialBlur(nameof(SpineCentipedeEnemy) + GetInstanceId(), 0.02f, 0.1f, duration, 1f);
        ScreenEffects.AnimateHeartbeatFrequency(nameof(SpineCentipedeEnemy) + GetInstanceId(), 0.5f, 0, 1f, 5f);
        SoundController.Instance.Play("sfx_horror_chord");
        SoundController.Instance.Play("sfx_horror_boom");

        var time_end = GameTime.Time + duration;
        var time_charge = GameTime.Time + duration * 0.5f;
        while (GameTime.Time < time_end)
        {
            if (GameTime.Time < time_charge)
            {
                _attack_position = Player.Instance.GlobalPosition;
            }

            TurnTowardsDirection(GlobalPosition.DirectionTo(_attack_position), 2f);
            yield return null;
        }

        _param_charge.Set(false);
        SetState(StateAttack);
    }

    private IEnumerator StateCr_Attack()
    {
        _param_attack.Trigger();

        var distance = 6f;
        var dir = GlobalPosition.DirectionTo(_attack_position);
        var world_position = GlobalPosition + dir.Normalized() * distance;
        var end_position = NavigationServer3D.MapGetClosestPoint(GetWorld3D().NavigationMap, world_position).Add(y: -Agent.PathHeightOffset);
        var start_position = GlobalPosition;

        while (true)
        {
            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                GlobalPosition = start_position.Lerp(end_position, f);
            });

            yield return new WaitForSeconds(3f);

            if (!_killed_player)
            {
                SetState(StatePatrol);
            }
        }
    }

    public void PlayWheezeShortSfx()
    {
        SoundController.Instance.Play(SfxWheezeShort, this);
    }

    public void PlayWheezeLongSfx()
    {
        SoundController.Instance.Play(SfxWheezeLong, this);
    }

    public void PlayTickSfx()
    {
        SoundController.Instance.Play(SfxTick, this);
    }

    public void PlayAttackSfx()
    {
        SoundController.Instance.Play(SfxAttack, this);
    }

    public void EnableAttackArea()
    {
        AttackArea.Enable();
    }

    public void DisableAttackArea()
    {
        AttackArea.Disable();
    }

    private void ChargeArea_PlayerEntered(Player player)
    {
        if (!Spawned) return;
        if (!IsState(StatePatrol)) return;

        ChargeArea.Disable();
        _attack_position = player.GlobalPosition;
        SetState(StateCharge);
    }

    private void AttackArea_PlayerEntered(Player player)
    {
        _killed_player = true;
        KillPlayer();
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
}
