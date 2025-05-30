using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Collections.Generic;

public partial class FrogCharacter : Character
{
    [NodeType]
    public Touchable Touchable;

    [NodeType]
    public FrogBlueprintCrafting BlueprintCrafting;

    private bool _active_dialogue;
    private Vector3 _init_direction;

    private AnimationState _anim_grab;
    private TriggerParameter _param_wave;
    private IntParameter _param_idle;
    private List<AnimationState> _anims_dialogue = new();

    public override void _Ready()
    {
        base._Ready();

        _init_direction = GlobalBasis * -Vector3.Forward;

        Touchable.OnTouched += Touched;
        BlueprintCrafting.OnBlueprintStarted += OnBlueprintStarted;
        BlueprintCrafting.OnBlueprintCompleted += OnBlueprintCompleted;
        BlueprintCrafting.OnMaterialReceived += OnMaterialReceived;
        AnimationPlayer.AnimationStarted += AnimationStarted;

        InitializeAnimations();
    }

    private void InitializeAnimations()
    {
        AnimationPlayer.PlaybackDefaultBlendTime = 0.15f;
        AnimationStateMachine.Initialize(AnimationPlayer);

        var anim_idle = AnimationStateMachine.CreateAnimation("Armature|Idle_Stand", true);
        var anim_idle_var1 = AnimationStateMachine.CreateAnimation("Armature|Idle_Stand_Variant1", true);
        var anim_idle_var2 = AnimationStateMachine.CreateAnimation("Armature|Idle_Stand_Variant2", true);
        _anim_grab = AnimationStateMachine.CreateAnimation("Armature|Idle_Stand_Grab", false);
        var anim_wave = AnimationStateMachine.CreateAnimation("Armature|Idle_Stand_Wave", false);
        var anim_dialogue_hand = AnimationStateMachine.CreateAnimation("Armature|Idle_Stand_Dialogue_Hand", false);
        var anim_dialogue_nod = AnimationStateMachine.CreateAnimation("Armature|Idle_Stand_Dialogue_Nod", false);
        var anim_dialogue_tilt = AnimationStateMachine.CreateAnimation("Armature|Idle_Stand_Dialogue_Tilt", false);

        _anims_dialogue.Add(anim_dialogue_hand);
        _anims_dialogue.Add(anim_dialogue_nod);
        _anims_dialogue.Add(anim_dialogue_tilt);

        _param_wave = new TriggerParameter("wave");
        _param_idle = new IntParameter("idle", 0);

        AnimationStateMachine.Connect(anim_idle_var1.Node, anim_idle.Node);
        AnimationStateMachine.Connect(anim_idle_var2.Node, anim_idle.Node);
        AnimationStateMachine.Connect(_anim_grab.Node, anim_idle.Node);
        AnimationStateMachine.Connect(anim_wave.Node, anim_idle.Node);
        AnimationStateMachine.Connect(anim_dialogue_hand.Node, anim_idle.Node);
        AnimationStateMachine.Connect(anim_dialogue_nod.Node, anim_idle.Node);
        AnimationStateMachine.Connect(anim_dialogue_tilt.Node, anim_idle.Node);

        AnimationStateMachine.Connect(anim_idle.Node, anim_idle_var1.Node, _param_idle.When(ComparisonType.Equal, 1));
        AnimationStateMachine.Connect(anim_idle.Node, anim_idle_var2.Node, _param_idle.When(ComparisonType.Equal, 2));
        AnimationStateMachine.Connect(anim_idle_var1.Node, anim_idle.Node, _param_idle.When(ComparisonType.Equal, 0));
        AnimationStateMachine.Connect(anim_idle_var2.Node, anim_idle.Node, _param_idle.When(ComparisonType.Equal, 0));

        AnimationStateMachine.Start(anim_idle.Node);
    }

    protected override void OnDialogueAnimation(string animation)
    {
        if (string.IsNullOrEmpty(animation))
        {
            var anim = _anims_dialogue.Random();
            AnimationStateMachine.SetCurrentState(anim.Node);
            _param_idle.Set(0);
        }
        else
        {
            base.OnDialogueAnimation(animation);
        }
    }

    private void AnimationStarted(StringName animName)
    {
        if (animName.ToString().Contains("Idle_Stand"))
        {
            this.StartCoroutine(IdleCr, "animation");
        }

        IEnumerator IdleCr()
        {
            var rng = new RandomNumberGenerator();
            var delay = rng.RandfRange(5, 20);
            yield return new WaitForSeconds(delay);

            if (_param_idle.Value == 0)
            {
                _param_idle.Set(rng.RandiRange(1, 2));
            }
            else
            {
                _param_idle.Set(0);
            }
        }
    }

    private void OnBlueprintStarted()
    {
        AnimationStateMachine.SetCurrentState(_anim_grab.Node);
        _param_idle.Set(0);
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

    private void OnMaterialReceived()
    {
        AnimationStateMachine.SetCurrentState(_anim_grab.Node);
        _param_idle.Set(0);
    }

    private void Touched()
    {
        if (DialogueFlags.IsFlag(DialogueFlags.FrogCore, 1))
        {
            // After entering the upper core room
            // Tells the player to jump into the hole
            StartDialogue("##FROG_CORE_001##");
        }
        if (DialogueFlags.IsFlag(DialogueFlags.FrogForge, 1))
        {
            // After touching the forge
            // Tells the player to fuel the furnace
            StartDialogue("##FROG_FORGE_001##");
        }
        else if (DialogueFlags.IsFlag(DialogueFlags.FrogStone, 1))
        {
            // After touching the rocks in the mines
            // Tells the player to look for a pickaxe
            StartDialogue("##FROG_STONE_001##");
        }
        else if (DialogueFlags.IsFlag(DialogueFlags.FrogForestWeeds, 1))
        {
            // After touching the weeds in the basement
            // Tells player to look for workshop
            StartDialogue("##FROG_FOREST_WEEDS_001##");
        }
        else if (DialogueFlags.IsFlag(DialogueFlags.FrogFirstDeath, 1))
        {
            // After player dies first time
            // Tells player not to get too close to enemies
            StartDialogue("##FROG_FIRST_DEATH_001##");
        }
        else if (DialogueFlags.IsFlag(DialogueFlags.FrogIntro, 1))
        {
            // After given the first task
            // Repeats: Player must grow a crop
            StartDialogue("##FROG_INTRO_REPEAT_001##");
        }
        else if (DialogueFlags.IsFlag(DialogueFlags.FrogIntro, 2))
        {
            // After given the second task
            // Repeats: Player must go to basement to collect more seeds
            StartDialogue("##FROG_FIND_SEEDS_REPEAT_001##");
        }
        else if (DialogueFlags.IsFlag(DialogueFlags.FrogIntro, 3))
        {
            // After given the third task
            // Repeats: Player must forge the sword, then destroy the tree
            StartDialogue("##FROG_DESTROY_TREE_REPEAT_001##");
        }
        else
        {
            // The first thing said to the player
            // Gives the first task of growing a crop
            StartDialogue("##FROG_INTRO_001##");
        }
    }

    protected override void OnDialogueEnd(string node)
    {
        base.OnDialogueEnd(node);

        if (node.Contains("FROG_INTRO_0"))
        {
            ThrowSeedToPlayer();
            BlueprintCrafting.SetBlueprint("tutorial_001");
        }
        else if (node.Contains("FROG_FIND_SEEDS_0"))
        {
            ThrowBasementKeyToPlayer();
            BlueprintCrafting.SetBlueprint("tutorial_002");
        }
    }

    private void ThrowSeedToPlayer()
    {
        var item = SeedController.Instance.CreateSeed(ItemType.Crop_Vegetable);
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