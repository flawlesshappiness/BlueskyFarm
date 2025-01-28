using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class CultistEnemy : NavEnemy
{
    [Export]
    public float RunSpeed;

    [NodeType]
    public AnimationStateMachine AnimationStateMachine;

    [NodeType]
    public AnimationPlayer AnimationPlayer;

    private TriggerParameter _param_startled;
    private BoolParameter _param_moving;
    private BoolParameter _param_running;

    private BasementRoomElement _current_element;

    private enum State { Wait, Patrol, Flee }
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
        var anim_startled = AnimationStateMachine.CreateAnimation("Armature|Startled_Cult", false);

        _param_startled = new TriggerParameter("startled");
        _param_moving = new BoolParameter("moving", false);
        _param_running = new BoolParameter("running", false);

        AnimationStateMachine.Connect(anim_walk.Node, anim_idle.Node, _param_moving.WhenFalse());
        AnimationStateMachine.Connect(anim_run.Node, anim_idle.Node, _param_moving.WhenFalse());
        AnimationStateMachine.Connect(anim_startled.Node, anim_idle.Node, _param_moving.WhenFalse());

        AnimationStateMachine.Connect(anim_idle.Node, anim_walk.Node, _param_moving.WhenTrue(), _param_running.WhenFalse());
        AnimationStateMachine.Connect(anim_run.Node, anim_walk.Node, _param_moving.WhenTrue(), _param_running.WhenFalse());
        AnimationStateMachine.Connect(anim_startled.Node, anim_walk.Node, _param_moving.WhenTrue(), _param_running.WhenFalse());

        AnimationStateMachine.Connect(anim_idle.Node, anim_run.Node, _param_moving.WhenTrue(), _param_running.WhenTrue());
        AnimationStateMachine.Connect(anim_walk.Node, anim_run.Node, _param_moving.WhenTrue(), _param_running.WhenTrue());
        AnimationStateMachine.Connect(anim_startled.Node, anim_run.Node, _param_moving.WhenTrue(), _param_running.WhenTrue());

        AnimationStateMachine.Connect(anim_idle.Node, anim_startled.Node, _param_startled.WhenTriggered());
        AnimationStateMachine.Connect(anim_walk.Node, anim_startled.Node, _param_startled.WhenTriggered());
        AnimationStateMachine.Connect(anim_run.Node, anim_startled.Node, _param_startled.WhenTriggered());

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
    }

    private void ProcessAnimations()
    {
        _param_moving.Set(!Agent.IsNavigationFinished());
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
        _param_running.Set(true);
        CurrentSpeed = RunSpeed;

        while (true)
        {
            yield return null;
        }
    }
}
