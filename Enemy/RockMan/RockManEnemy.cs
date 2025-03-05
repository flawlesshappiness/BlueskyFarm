using FlawLizArt.Animation.StateMachine;
using Godot;

public partial class RockManEnemy : NavEnemy
{
    [Export]
    public AnimationPlayer AnimationPlayer;

    [Export]
    public AnimationStateMachine AnimationStateMachine;

    [Export]
    public Skeleton3D Skeleton;

    [Export]
    public SoundInfo SfxStepNear;

    [Export]
    public SoundInfo SfxStepFar;

    [Export]
    public SoundInfo SfxIdle;

    [Export]
    public SoundInfo SfxAmb;

    [Export]
    public SoundInfo SfxAlert;

    [Export]
    public SoundInfo SfxScream;

    protected override string EnemyName => "RockMan";
    //protected override string DefaultState => "Idle";

    private Vector3 LeftArmPosition => GetBoneGlobalPosition("IK.Arm.L");
    private Vector3 RightArmPosition => GetBoneGlobalPosition("IK.Arm.R");
    private float DistanceStepNear => 20f;

    private TriggerParameter _param_idle_look;
    private TriggerParameter _param_attack;
    private BoolParameter _param_moving;
    private BoolParameter _param_scream;

    public override void _Ready()
    {
        base._Ready();
        InitializeAnimations();
    }

    private void InitializeAnimations()
    {
        AnimationStateMachine.Initialize(AnimationPlayer);

        var idle = AnimationStateMachine.CreateAnimation("Armature|idle", true);
        var idle_look = AnimationStateMachine.CreateAnimation("Armature|idle_look", false);
        var attack = AnimationStateMachine.CreateAnimation("Armature|attack", false);
        var walking = AnimationStateMachine.CreateAnimation("Armature|walking", true);
        var scream = AnimationStateMachine.CreateAnimation("Armature|scream", true);

        _param_idle_look = new TriggerParameter("idle_look");
        _param_attack = new TriggerParameter("attack");
        _param_moving = new BoolParameter("moving", false);
        _param_scream = new BoolParameter("scream", false);

        AnimationStateMachine.Connect(idle, idle_look, _param_idle_look.WhenTriggered());
        AnimationStateMachine.Connect(idle_look, idle);

        AnimationStateMachine.Connect(idle, walking, _param_moving.WhenTrue());
        AnimationStateMachine.Connect(idle_look, walking, _param_moving.WhenTrue());
        AnimationStateMachine.Connect(walking, idle, _param_moving.WhenFalse());

        AnimationStateMachine.Connect(idle, attack, _param_attack.WhenTriggered());
        AnimationStateMachine.Connect(idle_look, attack, _param_attack.WhenTriggered());
        AnimationStateMachine.Connect(walking, attack, _param_attack.WhenTriggered());

        AnimationStateMachine.Connect(idle, scream, _param_scream.WhenTrue());
        AnimationStateMachine.Connect(idle_look, scream, _param_scream.WhenTrue());
        AnimationStateMachine.Connect(walking, scream, _param_scream.WhenTrue());
        AnimationStateMachine.Connect(attack, scream, _param_scream.WhenTrue());
        AnimationStateMachine.Connect(scream, idle, _param_scream.WhenFalse());

        AnimationStateMachine.Start(idle.Node);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_Animations();
    }

    private void Process_Animations()
    {
        _param_moving.Set(!Agent.IsNavigationFinished());
    }

    private Vector3 GetBoneGlobalPosition(string id)
    {
        var idx = Skeleton.FindBone(id);
        var position = Skeleton.GetBonePosePosition(idx);
        return Skeleton.GlobalPosition + position;
    }

    public void PlayLeftStepSfx()
    {
        PlayStepSfx(LeftArmPosition);
    }

    public void PlayRightStepSfx()
    {
        PlayStepSfx(RightArmPosition);
    }

    private void PlayStepSfx(Vector3 position)
    {
        if (DistanceToPlayer < DistanceStepNear)
        {
            SfxStepNear.Play(position);
        }
        else
        {
            SfxStepFar.Play(position);
        }
    }
}
