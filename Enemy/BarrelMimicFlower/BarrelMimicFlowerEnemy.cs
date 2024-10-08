using FlawLizArt.Animation.StateMachine;
using Godot;

public partial class BarrelMimicFlowerEnemy : NavEnemy
{
    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public AnimationStateMachine StateMachine;

    private TriggerParameter _param_close;
    private TriggerParameter _param_touched;

    public override void _Ready()
    {
        base._Ready();
        InitializeAnimations();
        RegisterDebugActions();
    }

    private void InitializeAnimations()
    {
        var pose_closed = StateMachine.CreateAnimation("Armature|closed_pose", false);
        var pose_open = StateMachine.CreateAnimation("Armature|open_pose", false);
        var closed_to_open = StateMachine.CreateAnimation("Armature|closed_to_open", false);

        _param_close = StateMachine.CreateParameter("close");
        _param_touched = StateMachine.CreateParameter("touched");

        StateMachine.Connect(pose_open.Node, pose_closed.Node, _param_close.WhenTriggered());
        StateMachine.Connect(pose_closed.Node, closed_to_open.Node, _param_touched.WhenTriggered());
        StateMachine.Connect(closed_to_open.Node, pose_open.Node);

        StateMachine.Initialize(AnimationPlayer);
        StateMachine.Start(pose_closed.Node);
    }

    private void RegisterDebugActions()
    {
        var category = $"BarrelMimicFlower ({GetInstanceId()})";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Close pose",
            Action = v => { _param_close.Trigger(); v.SetVisible(false); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Debug touched",
            Action = v => { _param_touched.Trigger(); v.SetVisible(false); }
        });
    }
}
