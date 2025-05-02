using Godot;
using System.Collections;

public partial class ScreenshotController : SingletonController
{
    public static ScreenshotController Instance => Singleton.Get<ScreenshotController>();
    public override string Directory => "Screenshots";

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "SCREENSHOTS";

        Debug.RegisterAction(new DebugAction
        {
            Id = category,
            Category = category,
            Text = "Goto",
            Action = Goto
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = category,
            Category = category,
            Text = "Take screenshots",
            Action = DebugTakeScreenshots
        });

        void Goto(DebugView view)
        {
            view.HideContent();
            view.SetContent_Search();

            view.ContentSearch.AddItem("Basement", () => { view.Close(); Scene.Goto("Screenshot_001"); });
            view.ContentSearch.AddItem("Basement Corridor", () => { view.Close(); Scene.Goto("Screenshot_005"); });
            view.ContentSearch.AddItem("Forest", () => { view.Close(); Scene.Goto("Screenshot_002"); });
            view.ContentSearch.AddItem("Cult Hallway", () => { view.Close(); Scene.Goto("Screenshot_003"); });
            view.ContentSearch.AddItem("Mine Corridor", () => { view.Close(); Scene.Goto("Screenshot_004"); });
            view.ContentSearch.UpdateButtons();
        }

        void DebugTakeScreenshots(DebugView view)
        {
            view.PopupStringInput("Filename", s =>
            {
                view.Close();
                TakeScreenshots($"res://Screenshots/Images/{s}");
            });
        }
    }

    public void TakeScreenshots(string image_file_path)
    {
        this.StartCoroutine(Cr, nameof(SaveImage))
            .SetRunWhilePaused();

        IEnumerator Cr()
        {
            var current_size = Scene.Root.Size;

            Scene.PauseLock.AddLock(nameof(ScreenshotScene));

            var file_path_no_ext = image_file_path.RemoveExtension();
            yield return SaveImageByResolution(new Vector2I(3840, 1240), file_path_no_ext);
            yield return SaveImageByResolution(new Vector2I(1920, 1080), file_path_no_ext);
            yield return SaveImageByResolution(new Vector2I(1280, 720), file_path_no_ext);

            // Reset resolution
            Scene.Root.Size = current_size;
            Scene.PauseLock.RemoveLock(nameof(ScreenshotScene));
        }
    }

    private IEnumerator SaveImageByResolution(Vector2I resolution, string file_path_no_ext)
    {
        Scene.Root.Size = resolution;
        yield return new WaitForSecondsUnscaled(0.5f);
        SaveImage($"{file_path_no_ext}_{resolution.X}x{resolution.Y}.png");
        yield return null;
    }

    private void SaveImage(string filename)
    {
        ScreenEffects.View.Camera.GetViewport().GetTexture().GetImage().SavePng(filename);
    }
}
