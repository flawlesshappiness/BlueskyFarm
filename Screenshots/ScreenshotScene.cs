using Godot;

public partial class ScreenshotScene : Scene
{
    [Export]
    public Camera3D Camera;

    [Export]
    public WorldEnvironment Environment;

    [Export(PropertyHint.SaveFile, "*.png")]
    public string ImageFilePath;

    public override void _Ready()
    {
        base._Ready();
        AmbienceController.Instance.StopAmbience();
        HideViews();

        ScreenEffects.View.SetCameraTarget(Camera);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventKey key && key.IsReleased())
        {
            if (key.Keycode == Key.Space)
            {
                ScreenshotController.Instance.TakeScreenshots(ImageFilePath);
            }
        }
    }

    private void AdjustBrightness(float value)
    {
        Environment.Environment.AdjustmentBrightness += value;
    }

    private void HideViews()
    {
        MainMenuView.Instance.Hide();
        GameView.Instance.Hide();
    }
}
