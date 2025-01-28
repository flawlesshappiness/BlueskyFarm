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

    [NodeType]
    public AnimationStateMachine AnimationStateMachine;

    [NodeType]
    public AnimationPlayer AnimationPlayer;

    protected override string EnemyName => "Cultist";

    private BoolParameter _param_moving;
    private BoolParameter _param_running;
    private AnimationState _anim_startled;

    private BasementRoomElement _current_element;

    private enum State { Wait, Patrol, Flee, Respawn }
    private State _state;

    public override void _Ready()
    {
        base._Ready();
        InitializeAnimations();
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

    protected override void Spawn(bool debug)
    {
        base.Spawn(debug);

        if (debug)
        {

        }
        else
        {
            var room = GetFurthestRoomElementToPlayer();
            var spawn = room.Room.GetNodeInChildren<Node3D>("EnemySpawn");
            GlobalPosition = spawn.GlobalPosition;

            SetState(State.Patrol);
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
        if (_state == State.Flee || _state == State.Respawn) return;

        if (DistanceToPlayer < BasementRoom.ROOM_SIZE * 0.3f)
        {
            SetState(State.Flee);
        }
    }

    private void SetState(State state)
    {
        _state = state;

        var id = "state";
        switch (state)
        {
            case State.Wait: this.StartCoroutine(StateCr_Wait, id); return;
            case State.Patrol: this.StartCoroutine(StateCr_Patrol, id); return;
            case State.Flee: this.StartCoroutine(StateCr_Flee, id); return;
            case State.Respawn: this.StartCoroutine(StateCr_Respawn, id); return;
            default: return;
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

        SetState(State.Patrol);
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
                SetState(State.Wait);
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
                SetState(State.Respawn);
            }

            yield return null;
        }
    }

    private void RagdollPlayer()
    {
        Coroutine.Start(Cr, $"{nameof(CultistEnemy)}_Ragdoll");
        IEnumerator Cr()
        {
            Player.MovementLock.AddLock(EnemyId);
            Player.LookLock.AddLock(EnemyId);

            ScreenEffects.AnimateGaussianBlur(EnemyId, 15, 0.2f, 3f, 5f);
            ScreenEffects.AnimateHeartbeatFrequency(EnemyId, 0.75f, 0, 2f, 5f);

            var v_ragdoll = DirectionToPlayer.Normalized() * 2f;
            yield return Player.Instance.RagdollCameraAndPickUp(v_ragdoll, 3f);

            Player.MovementLock.RemoveLock(EnemyId);
            Player.LookLock.RemoveLock(EnemyId);
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
