using Godot;

public partial class CursorView : View
{
    [NodeName]
    public Control Cursor;

    [NodeType]
    public TextureRect Icon;

    [NodeName]
    public Label Label;

    [NodeType]
    public ProgressBar ProgressBar;

    public override string Directory => $"{Paths.Modules}/Cursor/View";

    public override void _Ready()
    {
        base._Ready();
        Hide();
    }

    public void SetCursorTexture(Texture2D texture)
    {
        Icon.Texture = texture;
        Icon.Show();
        ProgressBar.Hide();
    }

    public void SetCursorText(string text)
    {
        Label.Text = text;
    }

    public void SetCursorPosition(Vector2 position)
    {
        var size = Cursor.Size * 0.5f;
        Cursor.Position = position - size;
    }

    public void SetCursorPosition(Vector3 position)
    {
        var viewport_position = ScreenEffects.View.Camera.UnprojectPosition(position);
        SetCursorPosition(viewport_position);
    }

    public void SetProgress(float t)
    {
        ProgressBar.Value = t;
        Icon.Hide();
        ProgressBar.Show();
    }
}
