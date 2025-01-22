using Godot;

public static class Cursor
{
    public static CursorView View => Singleton.LoadScene<CursorView>($"Cursor/View/{nameof(CursorView)}.tscn");

    public static void Show(CursorSettings settings)
    {
        //Debug.Log("show");
        var texture = settings.OverrideTexture ?? CursorController.Instance.GetCursorTexture(settings.Name);
        View.SetCursorTexture(texture);
        View.SetCursorText(settings.Text);
        View.SetCursorPosition(settings.Position);
        View.Show();
    }

    public static void Progress(ProgressSettings settings)
    {
        View.SetProgress(settings.Value);
        View.SetCursorText(settings.Text);
        View.SetCursorPosition(settings.Position);
        View.Show();
    }

    public static void Hide()
    {
        //Debug.Log("hide");
        View.Hide();
    }
}

public class CursorSettings
{
    public string Name { get; set; }
    public Vector3 Position { get; set; }
    public string Text { get; set; }
    public Texture2D OverrideTexture { get; set; }
}

public class ProgressSettings
{
    public Vector3 Position { get; set; }
    public string Text { get; set; }
    public float Value { get; set; }
}