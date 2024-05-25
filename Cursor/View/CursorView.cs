using Godot;

public partial class CursorView : View
{
    [NodeName(nameof(Cursor))]
    public TextureRect Cursor;

    [NodeName(nameof(Label))]
    public Label Label;

    [Export(PropertyHint.File, nameof(Texture2D))]
    public Texture2D GrabTexture;

    [Export(PropertyHint.File, nameof(Texture2D))]
    public Texture2D TalkTexture;

    [Export(PropertyHint.File, nameof(Texture2D))]
    public Texture2D DoorTexture;

    [Export(PropertyHint.File, nameof(Texture2D))]
    public Texture2D LookTexture;

    public override string Directory => $"Cursor/View";

    public override void _Ready()
    {
        base._Ready();
        Hide();
    }

    public void SetCursor(CursorType type, string text = "")
    {
        Cursor.Texture = GetTexture(type);
        Label.Text = text;
    }

    public void SetCursorPosition(Vector2 position)
    {
        var size = Cursor.Size * 0.5f;
        Cursor.Position = position - size;
    }

    private Texture2D GetTexture(CursorType type) => type switch
    {
        CursorType.Grab => GrabTexture,
        CursorType.Talk => TalkTexture,
        CursorType.Door => DoorTexture,
        CursorType.Look => LookTexture,
        _ => null
    };
}
