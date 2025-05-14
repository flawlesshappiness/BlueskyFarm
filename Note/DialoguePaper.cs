using Godot;

public partial class DialoguePaper : Node3D
{
    [Export(PropertyHint.MultilineText)]
    public string Text;

    [Export]
    public Touchable Touchable;

    [Export]
    public DialogueCharacterInfo DialogueCharacterInfo;

    public override void _Ready()
    {
        base._Ready();
        Touchable.OnTouched += Touched;

        if (string.IsNullOrEmpty(Text))
        {
            Touchable.Disable();
        }
    }

    private void Touched()
    {
        NoteView.Instance.SetText(Text);
        NoteView.Instance.AnimateShow();
        Cursor.Hide();
    }
}
