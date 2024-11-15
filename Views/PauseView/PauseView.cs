using Godot;

public partial class PauseView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(PauseView)}";

    [NodeName]
    public Button ResumeButton;

    [NodeName]
    public Button OptionsButton;

    [NodeName]
    public Button MainMenuButton;

    [NodeName]
    public Control Buttons;

    private OptionsView OptionsView { get; set; }

    public override void _Ready()
    {
        base._Ready();
        ResumeButton.Pressed += ResumePressed;
        OptionsButton.Pressed += OptionsPressed;
        MainMenuButton.Pressed += MainMenuPressed;

        OptionsView = Get<OptionsView>();
        OptionsView.OnBack += OptionsBackPressed;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (PlayerInput.Pause.Pressed)
        {
            if (Get<OptionsView>().Visible) return;
            if (Visible)
            {
                ResumePressed();
            }
            else
            {
                Show();
            }
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
    }

    private void OptionsBackPressed()
    {
        Buttons.Visible = true;
    }

    private void MainMenuPressed()
    {
        Data.Game.Save();
        Scene.Tree.Quit();
    }
}
