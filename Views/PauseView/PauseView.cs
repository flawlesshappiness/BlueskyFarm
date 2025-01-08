using Godot;

public partial class PauseView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(PauseView)}";
    public static PauseView Instance => View.Get<PauseView>();

    [NodeName]
    public Button ResumeButton;

    [NodeName]
    public Button OptionsButton;

    [NodeName]
    public Button MainMenuButton;

    [NodeName]
    public Control Buttons;

    private OptionsView OptionsView => View.Get<OptionsView>();

    public MultiLock ToggleLock { get; set; } = new();

    public override void _Ready()
    {
        base._Ready();
        ResumeButton.Pressed += ResumePressed;
        OptionsButton.Pressed += OptionsPressed;
        MainMenuButton.Pressed += MainMenuPressed;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (PlayerInput.Pause.Released)
        {
            ToggleView();
            GetViewport().SetInputAsHandled();
        }
    }

    private void ToggleView()
    {
        if (ToggleLock.IsLocked) return;
        if (Visible)
        {
            ResumePressed();
        }
        else
        {
            Show();
        }
    }

    protected override void OnShow()
    {
        base.OnShow();
        Scene.PauseLock.AddLock(nameof(PauseView));
        MouseVisibility.Instance.Lock.AddLock(nameof(PauseView));

        ScreenEffects.SetGaussianBlur(nameof(PauseView), 16);
    }

    protected override void OnHide()
    {
        base.OnHide();
        Scene.PauseLock.RemoveLock(nameof(PauseView));
        MouseVisibility.Instance.Lock.RemoveLock(nameof(PauseView));

        ScreenEffects.RemoveGaussianBlur(nameof(PauseView));
    }

    private void ResumePressed()
    {
        Hide();
    }

    private void OptionsPressed()
    {
        Buttons.Visible = false;
        OptionsView.Show();
        OptionsView.OnBack += OptionsBackPressed;
    }

    private void OptionsBackPressed()
    {
        OptionsView.OnBack -= OptionsBackPressed;
        Buttons.Visible = true;
    }

    private void MainMenuPressed()
    {
        Data.Game.Save();
        Hide();
        MainMenuView.Instance.AnimateTransitionToMainMenu();
    }
}
