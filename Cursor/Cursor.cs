using Godot;

public static class Cursor
{
    public static CursorView View => Singleton.LoadScene<CursorView>($"Cursor/View/{nameof(CursorView)}.tscn");

    public static void Show(CursorType type, string text = "")
    {
        View.SetCursor(type, text);
        View.Show();
    }

    public static void Hide()
    {
        View.Hide();
    }

    public static void Position(Node3D node)
    {
        if (!GodotObject.IsInstanceValid(node)) return;

        var viewport_position = node.GetViewport().GetCamera3D().UnprojectPosition(node.GlobalPosition);
        View.SetCursorPosition(viewport_position);
    }
}
