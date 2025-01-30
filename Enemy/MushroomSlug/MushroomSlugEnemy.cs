using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class MushroomSlugEnemy : NavEnemy
{
    [Export]
    public AnimationStateMachine AnimationStateMachine;

    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public PlayerArea PlayerArea;

    protected override string EnemyName => "MushroomSlug";
    protected override string DefaultState => StateWander;
    private string FxId => $"{nameof(MushroomSlugEnemy)}_{GetInstanceId()}";

    private BoolParameter _param_moving;
    private float _time_player_enter;
    private BasementRoomElement _current_room;

    private const string StateWander = "Wander";
    private const string StateTravel = "Travel";

    public override void InitializeEnemy()
    {
        base.InitializeEnemy();
        PlayerArea.Disable();
        PlayerArea.PlayerEntered += PlayerEntered;
        PlayerArea.PlayerExited += PlayerExited;

        InitializeAnimations();
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);

        if (debug)
        {

        }
        else
        {
            _current_room = GetFurthestRoomElementToPlayer();
            var spawn = _current_room.Room.GetNodeInChildren<Node3D>("EnemySpawn");
            GlobalPosition = spawn.GlobalPosition;

            PlayerArea.Enable();

            SetState(StateTravel);
        }
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

    protected override void RegisterStates()
    {
        base.RegisterStates();
        RegisterState(StateWander, StateCr_Wander);
        RegisterState(StateTravel, StateCr_Travel);
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

    private void PlayerEntered(Player player)
    {
        if (!Spawned) return;

        _time_player_enter = GameTime.Time;

        ScreenEffects.AnimateDistortIn(FxId, 0.08f, 5f);
        ScreenEffects.AnimateGaussianBlurIn(FxId, 20, 5f);
        ScreenEffects.AnimateFogIn(FxId, 1f, 2f);
    }

    private void PlayerExited(Player player)
    {
        if (!Spawned) return;

        var t = Mathf.Clamp((GameTime.Time - _time_player_enter) / 5f, 0, 1);
        var duration = 15f * t;

        ScreenEffects.AnimateDistortOut(FxId, duration);
        ScreenEffects.AnimateGaussianBlurOut(FxId, duration);
        ScreenEffects.AnimateFogOut(FxId, duration);
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
                SetState(StateWander);
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
                    SetState(StateTravel);
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
