using Godot;

public partial class FrogCharacter : Character
{
    [NodeType]
    public Touchable Touchable;

    private bool _active_dialogue;
    private Vector3 _init_direction;

    public override void _Ready()
    {
        base._Ready();

        _init_direction = GlobalBasis * -Vector3.Forward;

        Touchable.OnTouched += Touched;

        InitializeAnimations();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        PhysicsProcess_Turn();
    }

    private void PhysicsProcess_Turn()
    {
        if (ActiveDialogue)
        {
            TurnTowardsPlayer();
        }
        else
        {
            TurnTowardsDirection(_init_direction);
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
        if (HasFlag("frog_intro"))
        {
            StartDialogue("##FROG_INTRO_005##");
        }
        else
        {
            StartDialogue("##FROG_INTRO_001##");
        }
    }
}