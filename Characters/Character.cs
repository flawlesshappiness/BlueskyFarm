using Godot;
using System.Linq;

public partial class Character : Node3DScript
{
    [Export]
    public string CharacterId;

    [Export]
    public DialogueCharacterInfo DialogueCharacterInfo;

    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public AnimationStateMachine AnimationStateMachine;

    [NodeName]
    public Node3D DialogueSoundOrigin;

    public CharacterData CharacterData { get; private set; }
    protected bool ActiveDialogue { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        InitializeData();
        InitializeDialogue();
    }

    private void InitializeDialogue()
    {
        DialogueController.Instance.OnDialogue += _OnDialogue;
        DialogueController.Instance.OnDialogueEnd += _OnDialogueEnd;
        DialogueController.Instance.OnDialogueCancel += _OnDialogueCancel;
        DialogueController.Instance.OnDialogueTrigger += _OnDialogueTrigger;
    }

    private void InitializeData()
    {
        CharacterData = GetOrCreateData();
    }

    private CharacterData GetOrCreateData()
    {
        Debug.TraceMethod(CharacterId);
        Debug.Indent++;

        if (string.IsNullOrEmpty(CharacterId))
        {
            Debug.LogError("CharacterId was null or empty");
            Debug.Indent--;
            return null;
        }

        var data = Data.Game.Characters.FirstOrDefault(x => x.Id == CharacterId);
        if (data == null)
        {
            data = new CharacterData
            {
                Id = CharacterId
            };

            Data.Game.Characters.Add(data);
        }

        Debug.Indent--;
        return data;
    }

    protected void StartDialogue(string name)
    {
        ActiveDialogue = true;

        DialogueController.Instance.SetCharacter(new DialogueCharacter
        {
            Info = DialogueCharacterInfo,
            SoundOrigin = DialogueSoundOrigin
        });

        DialogueController.Instance.SetNode(name);
    }

    private void _OnDialogue(DialogueNode node)
    {
        if (ActiveDialogue)
        {
            OnDialogue(node);
        }
    }

    private void _OnDialogueEnd(DialogueNode node)
    {
        if (ActiveDialogue)
        {
            OnDialogueEnd(node.id);
            ActiveDialogue = false;
        }
    }

    private void _OnDialogueCancel()
    {
        if (ActiveDialogue)
        {
            ActiveDialogue = false;
        }
    }

    private void _OnDialogueTrigger(string trigger)
    {
        if (ActiveDialogue)
        {
            OnDialogueTrigger(trigger);
        }
    }

    protected virtual void OnDialogue(DialogueNode node)
    {
        OnDialogueAnimation(node.animation);
    }

    protected virtual void OnDialogueEnd(string node)
    {

    }

    protected virtual void OnDialogueAnimation(string animation)
    {
        if (!string.IsNullOrEmpty(animation))
        {
            var state = AnimationStateMachine.Animations.TryGetValue(animation, out var value) ? value : null;
            if (state != null)
            {
                AnimationStateMachine.SetCurrentState(state.Node);
            }
        }
    }

    protected virtual void OnDialogueTrigger(string trigger)
    {

    }

    protected void TurnTowardsPlayer()
    {
        var direction_to_player = GlobalPosition.DirectionTo(Player.Instance.GlobalPosition);
        TurnTowardsDirection(direction_to_player);
    }

    protected void TurnTowardsDirection(Vector3 direction)
    {
        var y = Mathf.LerpAngle(GlobalRotation.Y, Mathf.Atan2(direction.X, direction.Z), 10 * GameTime.DeltaTime);
        GlobalRotation = GlobalRotation.Set(y: y);
    }
}
