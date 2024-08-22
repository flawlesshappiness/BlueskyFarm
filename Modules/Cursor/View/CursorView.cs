using Godot;

public partial class CursorView : View
{
    [NodeName(nameof(Cursor))]
    public TextureRect Cursor;

    [NodeName(nameof(Label))]
    public Label Label;

    public override string Directory => $"{Paths.Modules}/Cursor/View";

    public override void _Ready()
    {
        base._Ready();
        Hide();
    }

    public void SetCursorTexture(Texture2D texture)
    {
        Cursor.Texture = texture;
    }

    public void SetCursorText(string text)
    {
        Label.Text = text;
    }

    public void SetCursor(CursorType type, string text = "")
    {
        SetCursorTexture(null);
        SetCursorText(text);
    }

    public void SetCursorPosition(Vector2 position)
    {
        var size = Cursor.Size * 0.5f;
        Cursor.Position = position - size;
    }

    public void SetCursorPosition(Vector3 position)
    {
        var viewport_position = GetViewport().GetCamera3D().UnprojectPosition(position);
        SetCursorPosition(viewport_position);
    }
}
