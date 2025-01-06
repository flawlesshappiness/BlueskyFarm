using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class MushroomSlugEnemy : NavEnemy
{
    [NodeType]
    public AnimationStateMachine AnimationStateMachine;

    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public PlayerArea PlayerArea;

    private string FxId => $"{nameof(MushroomSlugEnemy)}_{GetInstanceId()}";

    private State _state = State.Wander;
    private BoolParameter _param_moving;
    private float _time_player_enter;
    private BasementRoomElement _current_room;
    private bool _spawned;

    public enum State
    {
        Wander, Travel,
        Debug_Idle, Debug_Follow,
    }

    public override void _Ready()
    {
        base._Ready();

        PlayerArea.Disable();
        PlayerArea.PlayerEntered += PlayerEntered;
        PlayerArea.PlayerExited += PlayerExited;

        RegisterDebugActions();
        InitializeAnimations();
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (IsDebug)
        {
        }
        else
        {
            Spawn();
        }
    }

    protected override void RegisterDebugActions()
    {
        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyId,
            Text = "Idle",
            Action = v => SetState(State.Debug_Idle)
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyId,
            Text = "Follow",
            Action = v => SetState(State.Debug_Follow)
        });
    }

    private void InitializeAnimations()
    {
        var idle = AnimationStateMachine.CreateAnimation("Armature|idle", true);
        var moving = AnimationStateMachine.CreateAnimation("Armature|moving", true);

        _param_moving = AnimationStateMachine.CreateParameter("moving", false);

        AnimationStateMachine.Connect(idle.Node, moving.Node, _param_moving.WhenTrue());
        AnimationStateMachine.Connect(moving.Node, idle.Node, _param_moving.WhenFalse());

        AnimationStateMachine.Initialize(AnimationPlayer);
        AnimationStateMachine.Start(idle.Node);
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

    private void Spawn()
    {
        _current_room = GetFurthestRoomElementToPlayer();
        var spawn = _current_room.Room.GetNodeInChildren<Node3D>("EnemySpawn");
        GlobalPosition = spawn.GlobalPosition;

        PlayerArea.Enable();

        SetState(State.Travel);

        _spawned = true;
    }

    private void PlayerEntered(Player player)
    {
        if (!_spawned) return;

        _time_player_enter = GameTime.Time;

        ScreenEffects.AnimateDistortIn(FxId, 0.08f, 5f);
        ScreenEffects.AnimateGaussianBlurIn(FxId, 20, 5f);
        ScreenEffects.AnimateFogIn(FxId, 1f, 2f);
    }

    private void PlayerExited(Player player)
    {
        if (!_spawned) return;

        var t = Mathf.Clamp((GameTime.Time - _time_player_enter) / 5f, 0, 1);
        var duration = 15f * t;

        ScreenEffects.AnimateDistortOut(FxId, duration);
        ScreenEffects.AnimateGaussianBlurOut(FxId, duration);
        ScreenEffects.AnimateFogOut(FxId, duration);
    }

    private void SetState(State state)
    {
        _state = state;

        var id = "state";
        switch (state)
        {
            case State.Debug_Idle: this.StartCoroutine(StateCr_Debug_Idle, id); return;
            case State.Debug_Follow: this.StartCoroutine(StateCr_Debug_Follow, id); return;
            case State.Travel: this.StartCoroutine(StateCr_Travel, id); return;
            case State.Wander: this.StartCoroutine(StateCr_Wander, id); return;
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

    private IEnumerator StateCr_Travel()
    {
        var room = GetRooms(x => x != _current_room)
            .ToList().Random();

        Agent.TargetPosition = GetRandomPositionInRoom(room.Room);

        while (true)
        {
            if (Agent.IsNavigationFinished())
            {
                _current_room = room;
                SetState(State.Wander);
                break;
            }

            yield return null;
        }
    }

    private IEnumerator StateCr_Wander()
    {
        var rng = new RandomNumberGenerator();
        var count = rng.RandiRange(2, 3);

        var time_wait = GameTime.Time;

        while (true)
        {
            if (Agent.IsNavigationFinished())
            {
                if (time_wait > GameTime.Time)
                {
                    yield return null;
                }

                if (count == 0)
                {
                    SetState(State.Travel);
                    break;
                }
                else
                {
                    Agent.TargetPosition = GetRandomPositionInRoom(_current_room.Room);
                    time_wait = 5f;
                    count--;
                }
            }

            yield return null;
        }
    }

    public void PlayMoveSfx()
    {
        SoundController.Instance.Play("sfx_slug_move", GlobalPosition);
    }
}
