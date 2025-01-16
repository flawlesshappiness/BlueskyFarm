using Godot;

public partial class DialoguePaper : Node3D
{
    [Export]
    public string DialogueNode;

    [Export]
    public Touchable Touchable;

    [Export]
    public DialogueCharacterInfo DialogueCharacterInfo;

    public override void _Ready()
    {
        base._Ready();
        Touchable.OnTouched += Touched;
    }

    private void Touched()
    {
        DialogueController.Instance.SetCharacter(new DialogueCharacter
        {
            Info = DialogueCharacterInfo,
        });

        DialogueController.Instance.SetNode(DialogueNode);
    }
}
