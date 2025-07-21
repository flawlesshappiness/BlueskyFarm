using Godot;
using System.Collections;

public partial class MainMenuScene : Scene
{
    [NodeName]
    public Node3D CameraTarget;

    [Export]
    public WorldEnvironment WorldEnvironment;

    private MainMenuView _view;

    public override void _Ready()
    {
        base._Ready();

        ScreenEffects.View.SetCameraTarget(CameraTarget);

        AmbienceController.Instance.StartAmbienceImmediate(AreaNames.Farm);

        UpdateBrightness();
        WorldEnvironment.Environment.AdjustmentEnabled = true;

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

    protected override void Initialize()
    {
        base.Initialize();
        OptionsController.Instance.OnBrightnessChanged += UpdateBrightness;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OptionsController.Instance.OnBrightnessChanged -= UpdateBrightness;
    }

    private void UpdateBrightness()
    {
        if (!IsInstanceValid(WorldEnvironment)) return;

        WorldEnvironment.Environment.AdjustmentBrightness = Data.Options.Brightness;
    }
}
