public partial class FrogCharacter : Character
{
    [NodeType]
    public Touchable Touchable;

    private bool _active_dialogue;

    public override void _Ready()
    {
        base._Ready();
        InitializeAnimations();

        Touchable.OnTouched += Touched;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (ActiveDialogue)
        {
            TurnTowardsPlayer();
        }
    }

    private void InitializeAnimations()
    {
        AnimationPlayer.PlaybackDefaultBlendTime = 0.25f;
        AnimationStateMachine.Initialize(AnimationPlayer);

        var anim_idle = AnimationStateMachine.CreateAnimation("CharacterArmature|Idle", true);
        var anim_yes = AnimationStateMachine.CreateAnimation("CharacterArmature|Yes", false);
        var anim_no = AnimationStateMachine.CreateAnimation("CharacterArmature|No", false);
        var anim_wave = AnimationStateMachine.CreateAnimation("CharacterArmature|Wave", false);

        AnimationStateMachine.Connect(anim_yes.Node, anim_idle.Node);
        AnimationStateMachine.Connect(anim_no.Node, anim_idle.Node);
        AnimationStateMachine.Connect(anim_wave.Node, anim_idle.Node);

        AnimationStateMachine.Start(anim_idle.Node);
    }

    private void Touched()
    {
        StartDialogue("##FROG_TEST_001##");
    }
}