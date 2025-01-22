using Godot;

public partial class MainMenuScene : Scene
{
    [NodeName]
    public Node3D CameraTarget;

    private MainMenuView _view;

    public override void _Ready()
    {
        base._Ready();
        _view = View.Get<MainMenuView>();
        _view.Show();

        ScreenEffects.View.SetCameraTarget(CameraTarget);

        AmbienceController.Instance.StartAmbienceImmediate(AreaNames.Farm);
    }
}
