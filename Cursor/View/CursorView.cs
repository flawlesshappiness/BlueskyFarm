using Godot;

public partial class CursorView : View
{
    [NodeName("Cursor")]
    public TextureRect Cursor;

    [Export(PropertyHint.File, nameof(Texture2D))]
    public Texture2D GrabTexture;

    [Export(PropertyHint.File, nameof(Texture2D))]
    public Texture2D TalkTexture;

    [Export(PropertyHint.File, nameof(Texture2D))]
    public Texture2D DoorTexture;

    [Export(PropertyHint.File, nameof(Texture2D))]
    public Texture2D LookTexture;

    public override void _Ready()
    {
        base._Ready();
        Hide();
    }

    public void SetCursor(CursorType type)
    {
        Cursor.Texture = GetTexture(type);
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
