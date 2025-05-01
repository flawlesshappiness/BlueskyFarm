using Godot;
using System.Collections;

public partial class MainMenuScene : Scene
{
    [NodeName]
    public Node3D CameraTarget;

    private MainMenuView _view;

    public override void _Ready()
    {
        base._Ready();

        ScreenEffects.View.SetCameraTarget(CameraTarget);

        AmbienceController.Instance.StartAmbienceImmediate(AreaNames.Farm);

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            // wait for save data
            while (!GameProfileController.Instance.GameProfilesLoaded)
            {
                yield return null;
            }

            _view = View.Get<MainMenuView>();
            _view.Show();
        }
    }
}
