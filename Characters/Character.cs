using Godot;

public partial class Character : Node3DScript
{
    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public AnimationStateMachine AnimationStateMachine;

    protected bool ActiveDialogue { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        DialogueController.Instance.OnDialogueEnd += OnDialogueEnd;
        DialogueController.Instance.OnDialogue += OnDialogue;
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
