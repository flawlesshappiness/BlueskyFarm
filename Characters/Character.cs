using Godot;
using System.Linq;

public partial class Character : Node3DScript
{
    [Export]
    public string CharacterId;

    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public AnimationStateMachine AnimationStateMachine;

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
        DialogueController.Instance.OnDialogueEnd += OnDialogueEnd;
        DialogueController.Instance.OnDialogue += OnDialogue;
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
        DialogueController.Instance.SetNode(name);
    }

    private void OnDialogue(DialogueNode node)
    {
        if (ActiveDialogue)
        {
            OnDialogueNode(node);
        }
    }

    protected virtual void OnDialogueNode(DialogueNode node)
    {
        if (!string.IsNullOrEmpty(node.animation))
        {
            var state = AnimationStateMachine.Animations.TryGetValue(node.animation, out var value) ? value : null;
            if (state != null)
            {
                AnimationStateMachine.SetCurrentState(state.Node);
            }
        }
    }

    protected virtual void OnDialogueEnd()
    {
        ActiveDialogue = false;
    }

    protected bool HasFlag(string id)
    {
        var data = Data.Game.DialogFlags_Bool.FirstOrDefault(x => x.id == id);
        return data?.value ?? false;
    }

    protected int GetFlag(string id)
    {
        var data = Data.Game.DialogFlags_Int.FirstOrDefault(x => x.id == id);
        return data?.value ?? 0;
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
