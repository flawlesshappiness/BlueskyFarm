using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class CultistEnemy : NavEnemy
{
    [Export]
    public float RunSpeed;

    [Export]
    public Node3D Model;

    [Export]
    public SoundInfo SfxWalk;

    [Export]
    public SoundInfo SfxRun;

    [Export]
    public AnimationStateMachine AnimationStateMachine;

    [Export]
    public AnimationPlayer AnimationPlayer;

    protected override string EnemyName => "Cultist";
    protected override string DefaultState => StatePatrol;

    private BoolParameter _param_moving;
    private BoolParameter _param_running;
    private AnimationState _anim_startled;

    private BasementRoomElement _current_element;

    private const string StateWait = "Wait";
    private const string StatePatrol = "Patrol";
    private const string StateFlee = "Flee";
    private const string StateRespawn = "Respawn";

    protected override void Initialize()
    {
        base.Initialize();
        InitializeAnimations();
        Spawn();
    }

    private void InitializeAnimations()
    {
        AnimationPlayer.PlaybackDefaultBlendTime = 0.15f;
        AnimationStateMachine.Initialize(AnimationPlayer);

        var anim_idle = AnimationStateMachine.CreateAnimation("Armature|Idle_Cult", true);
        var anim_walk = AnimationStateMachine.CreateAnimation("Armature|Walk_Cult", true);
        var anim_run = AnimationStateMachine.CreateAnimation("Armature|Run_Cult", true);
        _anim_startled = AnimationStateMachine.CreateAnimation("Armature|Startled_Cult", false);

        _param_moving = new BoolParameter("moving", false);
        _param_running = new BoolParameter("running", false);

        AnimationStateMachine.Connect(anim_walk.Node, anim_idle.Node, _param_moving.WhenFalse());
        AnimationStateMachine.Connect(anim_run.Node, anim_idle.Node, _param_moving.WhenFalse());
        AnimationStateMachine.Connect(_anim_startled.Node, anim_idle.Node);

        AnimationStateMachine.Connect(anim_idle.Node, anim_walk.Node, _param_moving.WhenTrue(), _param_running.WhenFalse());
        AnimationStateMachine.Connect(anim_run.Node, anim_walk.Node, _param_moving.WhenTrue(), _param_running.WhenFalse());

        AnimationStateMachine.Connect(anim_idle.Node, anim_run.Node, _param_moving.WhenTrue(), _param_running.WhenTrue());
        AnimationStateMachine.Connect(anim_walk.Node, anim_run.Node, _param_moving.WhenTrue(), _param_running.WhenTrue());

        AnimationStateMachine.Start(anim_idle.Node);
    }

    protected override void RegisterStates()
    {
        base.RegisterStates();
        RegisterState(StatePatrol, StateCr_Patrol);
        RegisterState(StateWait, StateCr_Wait);
        RegisterState(StateFlee, StateCr_Flee);
        RegisterState(StateRespawn, StateCr_Respawn);
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);

        if (debug)
        {

        }
        else
        {
            _current_element = GetFurthestRoomElementToPlayer();
            var spawn = _current_element.Room.GetNodeInChildren<Node3D>("EnemySpawn");
            GlobalPosition = spawn.GlobalPosition;

            SetState(StatePatrol);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessAnimations();
        ProcessFlee();
    }

    private void ProcessAnimations()
    {
        _param_moving.Set(!Agent.IsNavigationFinished());
    }

    private void ProcessFlee()
    {
        if (IsState(StateFlee, StateRespawn)) return;

        if (DistanceToPlayer < BasementRoom.ROOM_SIZE * 0.3f)
        {
            SetState(StateFlee);
        }
    }

    private IEnumerator StateCr_Wait()
    {
        _param_running.Set(false);

        Agent.TargetPosition = GlobalPosition;

        var time = GameTime.Time + 3f;
        while (GameTime.Time < time)
        {
            yield return null;
        }

        SetState(StatePatrol);
    }

    private IEnumerator StateCr_Patrol()
    {
        _param_running.Set(false);
        CurrentSpeed = MoveSpeed;

        _current_element = GetClosestRoomElements(x => x != _current_element).FirstOrDefault();
        Agent.TargetPosition = GetRandomPositionInRoom(_current_element.Room);
        Model.Show();

        while (true)
        {
            if (Agent.IsNavigationFinished())
            {
                SetState(StateWait);
            }

            yield return null;
        }
    }

    private IEnumerator StateCr_Flee()
    {
        Agent.TargetPosition = GlobalPosition;
        AnimationStateMachine.SetCurrentState(_anim_startled.Node);
        SoundController.Instance.Play("sfx_horror_chord");
        SoundController.Instance.Play("sfx_horror_boom");

        yield return new WaitForSeconds(0.5f);

        _param_running.Set(true);
        CurrentSpeed = RunSpeed;

        // Run towards player
        var timeout = GameTime.Time + 3f;
        while (DistanceToPlayer > 2f && GameTime.Time < timeout)
        {
            Agent.TargetPosition = PlayerPosition;
            yield return null;
        }

        RagdollPlayer();

        // Run away
        _current_element = GetFurthestRoomElementToPlayer();
        Agent.TargetPosition = _current_element.Room.GlobalPosition;

        while (true)
        {
            if (Agent.IsNavigationFinished())
            {
                SetState(StateRespawn);
            }

            yield return null;
        }
    }

    private void RagdollPlayer()
    {
        Coroutine.Start(Cr, $"{nameof(CultistEnemy)}_Ragdoll");
        IEnumerator Cr()
        {
            ScreenEffects.AnimateGaussianBlur(EnemyId, 15, 0.2f, 3f, 5f);
            ScreenEffects.AnimateHeartbeatFrequency(EnemyId, 0.75f, 0, 2f, 5f);

            var v_ragdoll = DirectionToPlayer.Normalized() * 2f;
            yield return Player.Instance.RagdollCameraAndPickUp(v_ragdoll, 3f);
        }
    }

    private IEnumerator StateCr_Respawn()
    {
        _param_running.Set(true);

        while (true)
        {
            Model.Hide();
            yield return new WaitForSeconds(30);
            Spawn(false);
        }
    }

    public void PlayWalkSfx()
    {
        SoundController.Instance.Play(SfxWalk, GlobalPosition.Add(y: 0.5f), new SoundOverride
        {
            Volume = -20,
            Distance = SoundDistance.Far
        });
    }

    public void PlayRunSfx()
    {
        SoundController.Instance.Play(SfxRun, GlobalPosition.Add(y: 0.5f), new SoundOverride
        {
            Volume = -20,
            Distance = SoundDistance.Far
        });
    }
}
