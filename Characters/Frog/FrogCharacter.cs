using Godot;

public partial class FrogCharacter : Character
{
    [NodeType]
    public Touchable Touchable;

    [NodeType]
    public FrogBlueprintCrafting BlueprintCrafting;

    private bool _active_dialogue;
    private Vector3 _init_direction;

    public override void _Ready()
    {
        base._Ready();

        _init_direction = GlobalBasis * -Vector3.Forward;

        Touchable.OnTouched += Touched;
        BlueprintCrafting.OnBlueprintCompleted += OnBlueprintCompleted;

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

    private void OnBlueprintCompleted(string id)
    {
        var info = BlueprintController.Instance.GetInfo(id);
        if (info == null) return;

        if (!string.IsNullOrEmpty(info.ResultDialogueNode))
        {
            StartDialogue(info.ResultDialogueNode);
        }
    }

    private void Touched()
    {
        if (HasFlag("frog_intro"))
        {
            StartDialogue("##FROG_INTRO_REPEAT_001##");
        }
        else if (HasFlag("frog_find_seeds"))
        {
            StartDialogue("##FROG_FIND_SEEDS_REPEAT_001##");
        }
        else if (HasFlag("frog_find_weedcutter"))
        {
            StartDialogue("##FROG_BLUEPRINT_WEEDCUTTER_001##");
        }
        else if (HasFlag("frog_find_weedcutter_repeat"))
        {
            StartDialogue("##FROG_BLUEPRINT_WEEDCUTTER_REPEAT_001##");
        }
        else
        {
            StartDialogue("##FROG_INTRO_001##");
        }
    }

    protected override void OnDialogueTrigger(string trigger)
    {
        base.OnDialogueTrigger(trigger);

        switch (trigger)
        {
            case "tutorial_001":
                ThrowSeedToPlayer();
                BlueprintCrafting.SetBlueprint("tutorial_001");
                break;

            case "tutorial_002":
                ThrowBasementKeyToPlayer();
                BlueprintCrafting.SetBlueprint("tutorial_002");
                break;

            case "workshop_key":
                Data.Game.Flag_WorkshopKeyAvailable = true;
                break;
        }
    }

    private void ThrowSeedToPlayer()
    {
        var item = SeedController.Instance.CreateSeed(ItemType.Vegetable);
        item.Data.Seed.OverrideGrowTime = 10;

        ThrowItemToPlayer(item);
    }

    private void ThrowBasementKeyToPlayer()
    {
        var item = ItemController.Instance.CreateItem("Key_Basement");
        ThrowItemToPlayer(item);
    }

    private void ThrowItemToPlayer(Item item)
    {
        var start_position = GlobalPosition.Add(y: 1.5f);
        var direction_to_player = start_position.DirectionTo(Player.Instance.MidPosition).Normalized();
        var direction = direction_to_player.Add(y: 1f);
        var velocity = direction * 2f;
        item.GlobalPosition = start_position;
        item.LinearVelocity = velocity;
    }
}