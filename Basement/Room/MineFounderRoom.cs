using Godot;
using Godot.Collections;

public partial class MineFounderRoom : Node3DScript
{
    [Export]
    public DialogueCharacterInfo DialogueCharacter;

    [Export]
    public Marker3D BlueprintItemMarker;

    [Export]
    public BlueprintInfo BlueprintInfo;

    [Export]
    public Array<Touchable> DialogueTouchables;

    [Export]
    public Array<BasementDoor> Doors;

    private Touchable door_touched;

    public override void _Ready()
    {
        base._Ready();
        DialogueTouchables.ForEach(x => x.OnTouched += () => Touched_Dialogue(x));
        Doors.ForEach(x => x.Locked = true);
        DialogueController.Instance.OnDialogueTrigger += DialogueTrigger;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        DialogueController.Instance.OnDialogueTrigger -= DialogueTrigger;
    }

    protected override void Initialize()
    {
        base.Initialize();
        InitializeTouchable();
        InitializeBlueprintItem();
    }

    private void InitializeTouchable()
    {
        if (IsAllPuzzlesCompleted())
        {
            DisableDialogueTouchable();
            OpenDoor(true);
        }
        else
        {

        }
    }

    private void InitializeBlueprintItem()
    {
        if (Player.HasAccessToBlueprint(BlueprintInfo.Id)) return;
        if (!Player.HasAccessToItem(BlueprintInfo.ResultItemInfo)) return;

        var item = BlueprintController.Instance.CreateBlueprintRoll(BlueprintInfo.Id);
        item.SetParent(BlueprintItemMarker);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;
    }

    private void Touched_Dialogue(Touchable touchable)
    {
        door_touched = touchable;

        if (DialogueFlags.IsFlag(DialogueFlags.FounderQuest, 1))
        {
            // After having heard the intro
            if (GameFlagIds.SecretPuzzleWellCompleted.IsFalse())
            {
                StartDialogue("##FOUNDER_POEM_WELL_001##");
            }
            else if (GameFlagIds.SecretPuzzleForestCompleted.IsFalse())
            {
                StartDialogue("##FOUNDER_POEM_FOREST_001##");
            }
            else if (GameFlagIds.SecretPuzzleClockCompleted.IsFalse())
            {
                StartDialogue("##FOUNDER_POEM_CLOCK_001##");
            }
            else
            {
                StartDialogue("##FOUNDER_QUEST_COMPLETE_001##");
            }
        }
        else
        {
            // First time
            StartDialogue("##FOUNDER_QUEST_001##");
        }
    }

    private void StartDialogue(string name)
    {
        DialogueController.Instance.SetCharacter(new DialogueCharacter
        {
            Info = DialogueCharacter,
            SoundOrigin = door_touched
        });

        DialogueController.Instance.SetNode(name);
    }

    private void DialogueTrigger(string trigger)
    {
        if (trigger == "quest_complete")
        {
            DisableDialogueTouchable();
            OpenDoor(false);

            GameFlagIds.SecretPuzzleFounderCompleted.SetTrue();
        }
    }

    private void DisableDialogueTouchable()
    {
        DialogueTouchables.ForEach(x => x.Disable());
    }

    private void OpenDoor(bool immediate)
    {
        foreach (var door in Doors)
        {
            if (!door.IsVisibleInTree()) continue;

            door.Locked = false;

            if (immediate)
            {
                door.SetOpen(true);
            }
            else
            {
                door.Open();
            }
        }
    }

    private bool IsAllPuzzlesCompleted()
    {
        return GameFlagIds.SecretPuzzleWellCompleted.IsTrue() &&
            GameFlagIds.SecretPuzzleForestCompleted.IsTrue() &&
            GameFlagIds.SecretPuzzleClockCompleted.IsTrue() &&
            GameFlagIds.SecretPuzzleFounderCompleted.IsTrue();
    }
}
