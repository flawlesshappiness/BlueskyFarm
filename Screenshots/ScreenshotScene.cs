using Godot;
using System.Collections;

public partial class ScreenshotScene : Scene
{
    [Export]
    public Camera3D Camera;

    [Export]
    public WorldEnvironment Environment;

    [Export(PropertyHint.SaveFile, "*.png")]
    public string ImageFilePath;

    private string ImageFilePathNoExt => ImageFilePath.RemoveExtension();

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
                TakeScreenshots();
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

    private void TakeScreenshots()
    {
        this.StartCoroutine(Cr, nameof(SaveImage))
            .SetRunWhilePaused();

        IEnumerator Cr()
        {
            var current_size = Root.Size;

            PauseLock.AddLock(nameof(ScreenshotScene));

            yield return SaveImageByResolution(new Vector2I(1920, 1080));
            yield return SaveImageByResolution(new Vector2I(1280, 720));

            // Reset resolution
            Root.Size = current_size;
            PauseLock.RemoveLock(nameof(ScreenshotScene));
        }
    }

    private IEnumerator SaveImageByResolution(Vector2I resolution)
    {
        Root.Size = resolution;
        yield return new WaitForSecondsUnscaled(0.5f);
        SaveImage($"{ImageFilePathNoExt}_{resolution.X}x{resolution.Y}.png");
        yield return null;
    }

    private void SaveImage(string filename = null)
    {
        filename ??= ImageFilePath;
        Camera.GetViewport().GetTexture().GetImage().SavePng(filename);
    }
}
