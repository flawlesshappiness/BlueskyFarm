using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;

public partial class SpineCentipedeEnemy : NavEnemy
{
    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public AnimationStateMachine AnimationStateMachine;

    private BoolParameter _param_moving;
    private BoolParameter _param_charge;
    private TriggerParameter _param_attack;
    private State _state = State.Wander;

    public enum State
    {
        Wander, Travel,
        Debug_Idle, Debug_Follow,
    }

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
        InitializeAnimations();
    }

    private void RegisterDebugActions()
    {
        Debug.RegisterAction(new DebugAction
        {
            Category = DebugCategory,
            Text = "Idle",
            Action = v => SetState(State.Debug_Idle)
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = DebugCategory,
            Text = "Follow",
            Action = v => SetState(State.Debug_Follow)
        });
    }

    private void InitializeAnimations()
    {
        var idle = AnimationStateMachine.CreateAnimation("Armature|idle", true);
        var moving = AnimationStateMachine.CreateAnimation("Armature|moving", true);
        var charging = AnimationStateMachine.CreateAnimation("Armature|charging", true);
        var attacking = AnimationStateMachine.CreateAnimation("Armature|attacking", true);

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

    private void SetState(State state)
    {
        _state = state;

        var id = "state";
        switch (state)
        {
            case State.Debug_Idle: this.StartCoroutine(StateCr_Debug_Idle, id); return;
            case State.Debug_Follow: this.StartCoroutine(StateCr_Debug_Follow, id); return;
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
}
