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

    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public AnimationStateMachine AnimationStateMachine;

    [NodeName]
    public AudioStreamPlayer3D SfxMoving;

    [NodeName]
    public PlayerArea ChargeArea;

    [NodeName]
    public PlayerArea AttackArea;

    private BoolParameter _param_moving;
    private BoolParameter _param_charge;
    private TriggerParameter _param_attack;
    private State _state = State.Patrol;
    private float _time_next_wheeze;
    private bool _killed_player;

    private BasementRoomElement _start_room;
    private BasementRoomElement _end_room;
    private bool _patrol_to_end;
    private bool _spawned;
    private Vector3 _attack_position;

    public enum State
    {
        Patrol, Charge, Attack,
        Debug_Idle, Debug_Follow,
    }

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
        InitializeAnimations();

        ChargeArea.PlayerEntered += ChargeArea_PlayerEntered;
        AttackArea.PlayerEntered += AttackArea_PlayerEntered;

        AttackArea.Disable();
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (IsDebug)
        {
            SetState(State.Debug_Idle);
        }
        else
        {
            Spawn();
        }
    }

    private void RegisterDebugActions()
    {
        Debug.RegisterAction(new DebugAction
        {
            Category = EnemyId,
            Text = "Idle",
            Action = v => SetState(State.Debug_Idle)
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = EnemyId,
            Text = "Follow",
            Action = v => SetState(State.Debug_Follow)
        });
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

    protected override void OnVelocityComputed(Vector3 v)
    {
        if (_state == State.Patrol || _state == State.Debug_Follow)
        {
            base.OnVelocityComputed(v);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessAnimations();
        ProcessWheeze();
    }

    private void ProcessAnimations()
    {
        var attacking = _state == State.Charge || _state == State.Attack;
        var moving = !Agent.IsNavigationFinished() && !attacking;
        _param_moving.Set(moving);

        SfxMoving.SetPlaying(moving);
    }

    private void ProcessWheeze()
    {
        if (GameTime.Time > _time_next_wheeze)
        {
            var rng = new RandomNumberGenerator();
            _time_next_wheeze = GameTime.Time + rng.RandfRange(10, 20);

            if (_state != State.Charge && _state != State.Attack)
            {
                PlayWheezeShortSfx();
            }
        }
    }

    private void Spawn()
    {
        var grid = BasementController.Instance.CurrentBasement.Grid;

        _start_room = grid.Elements
            .Where(x => x.AreaName == AreaNames.Basement && !x.IsStart)
            .OrderBy(x => grid.GetNeighbours(x.Coordinates).Count)
            .FirstOrDefault();

        _end_room = grid.Elements
            .Where(x => x.AreaName == AreaNames.Basement && !x.IsStart)
            .OrderByDescending(x => x.Room.GlobalPosition.DistanceSquaredTo(_start_room.Room.GlobalPosition))
            .FirstOrDefault();

        var spawn = _start_room.Room.GetNodeInChildren<Node3D>("EnemySpawn");
        GlobalPosition = spawn.GlobalPosition;

        SetState(State.Patrol);

        _spawned = true;
    }

    private void SetState(State state)
    {
        _state = state;

        var id = "state";
        switch (state)
        {
            case State.Debug_Idle: this.StartCoroutine(StateCr_Debug_Idle, id); return;
            case State.Debug_Follow: this.StartCoroutine(StateCr_Debug_Follow, id); return;
            case State.Patrol: this.StartCoroutine(StateCr_Patrol, id); return;
            case State.Charge: this.StartCoroutine(StateCr_Charge, id); return;
            case State.Attack: this.StartCoroutine(StateCr_Attack, id); return;
            default: return;
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

        var time_end = GameTime.Time + 2f;
        while (GameTime.Time < time_end)
        {
            TurnTowardsDirection(GlobalPosition.DirectionTo(_attack_position), 2f);
            yield return null;
        }

        _param_charge.Set(false);
        SetState(State.Attack);
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
                SetState(State.Patrol);
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
        if (!_spawned) return;

        if (_state == State.Patrol)
        {
            ChargeArea.Disable();
            _attack_position = player.GlobalPosition;
            SetState(State.Charge);
        }
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
