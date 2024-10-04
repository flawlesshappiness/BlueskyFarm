using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;

public partial class BarrelMimicEnemy : NavEnemy
{
    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public AnimationStateMachine StateMachine;

    private BoolParameter _param_sitting;
    private BoolParameter _param_moving;
    private BoolParameter _param_scaring;
    private TriggerParameter _param_attacking;

    private enum State { Idle, Debug_Follow, Debug_Scare }
    private State state = State.Idle;

    public override void _Ready()
    {
        base._Ready();
        InitializeAnimations();
        RegisterDebugActions();
    }

    private void InitializeAnimations()
    {
        var sit = StateMachine.CreateAnimation("Armature|sit_idle_pose", false);
        var sit_to_walk = StateMachine.CreateAnimation("Armature|sit_to_walk", false);
        var walk_to_sit = StateMachine.CreateAnimation("Armature|walk_to_sit", false);
        var walk = StateMachine.CreateAnimation("Armature|walk_forward", true);
        var idle = StateMachine.CreateAnimation("Armature|walk_idle", true);
        var scare = StateMachine.CreateAnimation("Armature|walk_scare", true);
        var attack = StateMachine.CreateAnimation("Armature|attack", false);

        _param_sitting = StateMachine.CreateParameter("sitting", true);
        _param_moving = StateMachine.CreateParameter("moving", false);
        _param_scaring = StateMachine.CreateParameter("scaring", false);
        _param_attacking = StateMachine.CreateParameter("attacking");

        AnimationPlayer.SetBlendTime(walk_to_sit.Animation, sit.Animation, 0);
        AnimationPlayer.SetBlendTime(sit.Animation, sit_to_walk.Animation, 0);
        AnimationPlayer.SetBlendTime(sit_to_walk.Animation, idle.Animation, 0);
        AnimationPlayer.SetBlendTime(sit_to_walk.Animation, walk.Animation, 0);

        StateMachine.ConnectFromAnyState(attack.Node, _param_attacking.WhenTriggered());

        StateMachine.Connect(sit.Node, sit_to_walk.Node, _param_sitting.WhenFalse());
        StateMachine.Connect(sit_to_walk.Node, idle.Node); // On animation end
        StateMachine.Connect(idle.Node, walk_to_sit.Node, _param_sitting.WhenTrue());
        StateMachine.Connect(walk_to_sit.Node, sit.Node); // On animation end

        StateMachine.Connect(idle.Node, walk.Node, _param_moving.WhenTrue());
        StateMachine.Connect(walk.Node, idle.Node, _param_moving.WhenFalse());

        StateMachine.Connect(idle.Node, scare.Node, _param_scaring.WhenTrue());
        StateMachine.Connect(scare.Node, idle.Node, _param_scaring.WhenFalse());

        StateMachine.Initialize(AnimationPlayer);
        StateMachine.Start(sit.Node);
    }

    private void RegisterDebugActions()
    {
        var category = "BarrelMimic " + GetInstanceId();

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Toggle animation parameter",
            Action = ToggleParameter
        });

        void ToggleParameter(DebugView view)
        {
            view.HideContent();
            view.PopupStringInput("Parameter name", v =>
            {
                var parameter = StateMachine.GetParameter<bool>(v) as BoolParameter;
                if (parameter != null)
                {
                    parameter.Toggle();
                }
            });
        }

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Follow player",
            Action = _ => SetState(State.Debug_Follow)
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Scare player",
            Action = _ => SetState(State.Debug_Scare)
        });
    }

    protected override void OnVelocityComputed(Vector3 v)
    {
        if (StateMachine.Current.Name == "Armature|walk_forward")
        {
            base.OnVelocityComputed(v);
        }
    }

    private void SetState(State state)
    {
        this.state = state;

        var id = "state";
        switch (state)
        {
            case State.Debug_Follow: StartCoroutine(StateCr_DebugFollow, id); return;
            case State.Debug_Scare: StartCoroutine(StateCr_DebugScare, id); return;
            default: return;
        }
    }

    private IEnumerator StateCr_DebugFollow()
    {
        Coroutine cr_wait = null;

        while (true)
        {
            _param_moving.Set(!Agent.IsNavigationFinished());

            if (!Agent.IsNavigationFinished() && DistanceToPlayer < 3)
            {
                Agent.TargetPosition = GlobalPosition;

                Coroutine.Stop(cr_wait);
                cr_wait = Coroutine.Start(CrWaitThenSit(4));
            }
            else if (DistanceToPlayer > 4)
            {
                Coroutine.Stop(cr_wait);

                _param_sitting.Set(false);
                Agent.TargetPosition = Player.GlobalPosition;
            }
            else if (!_param_sitting.Value)
            {
                TurnTowardsDirection(DirectionToPlayer);
            }

            yield return null;
        }

        IEnumerator CrWaitThenSit(float delay)
        {
            yield return new WaitForSeconds(delay);
            _param_sitting.Set(true);
        }
    }

    private IEnumerator StateCr_DebugScare()
    {
        while (true)
        {
            _param_sitting.Set(DistanceToPlayer > 3);
            _param_scaring.Set(DistanceToPlayer <= 3);

            if (_param_scaring.Value)
            {
                TurnTowardsDirection(DirectionToPlayer);
            }

            yield return null;
        }
    }
}
