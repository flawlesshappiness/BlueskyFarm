using Godot;
using System.Collections;

public partial class MainMenuView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(MainMenuView)}";

    [Export]
    public Button PlayButton;

    [Export]
    public Button OptionsButton;

    [Export]
    public Button QuitButton;

    [Export]
    public Control MainControl;

    [Export]
    public ColorRect Overlay;

    private OptionsView OptionsView => View.Get<OptionsView>();

    protected override void OnShow()
    {
        base.OnShow();
        InventoryController.Instance.InventoryLock.AddLock(nameof(MainMenuView));
        MouseVisibility.Instance.Lock.AddLock(nameof(MainMenuView));
        PauseView.Instance.ToggleLock.AddLock(nameof(MainMenuView));

        PlayButton.Pressed += ClickPlay;
        OptionsButton.Pressed += ClickOptions;
        QuitButton.Pressed += ClickQuit;

        MainControl.Show();
        MainControl.SetMouseFilterRec(MouseFilterEnum.Stop);

        InitializeButton(PlayButton);
        InitializeButton(OptionsButton);
        InitializeButton(QuitButton);
    }

    private void InitializeButton(Button button)
    {
        button.MouseEntered += () => SoundController.Instance.Play("sfx_ui_hover");
        button.Pressed += () => SoundController.Instance.Play("sfx_ui_click");
    }

    protected override void OnHide()
    {
        base.OnHide();
        InventoryController.Instance.InventoryLock.RemoveLock(nameof(MainMenuView));
        MouseVisibility.Instance.Lock.RemoveLock(nameof(MainMenuView));
        PauseView.Instance.ToggleLock.RemoveLock(nameof(MainMenuView));
    }

    private void ClickPlay()
    {
        AnimateTransitionToPlay();
    }

    private void ClickOptions()
    {
        OptionsView.OnBack += ClickOptionsBack;
        OptionsView.Show();
        MainControl.Hide();
    }

    private void ClickQuit()
    {
        Scene.Tree.Quit();
    }

    private void ClickOptionsBack()
    {
        OptionsView.OnBack -= ClickOptionsBack;
        MainControl.Show();
    }

    private void AnimateTransitionToPlay()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var bus = AudioBus.Get(SoundBus.Transition.ToString());

            MainControl.SetMouseFilterRec(MouseFilterEnum.Ignore);
            MouseVisibility.Instance.Lock.RemoveLock(nameof(MainMenuView));

            Overlay.Color = Overlay.Color.SetA(0);
            Overlay.Show();

            yield return LerpEnumerator.Lerp01(1f, f =>
            {
                var a = Mathf.Lerp(0f, 1f, f);
                Overlay.Color = Overlay.Color.SetA(a);

                var volume = SoundController.Instance.PercentageToDecibel(Mathf.Lerp(1f, 0f, f));
                bus.SetVolume(volume);
            });

            MainControl.Hide();
            Scene.Goto<FarmScene>();

            yield return LerpEnumerator.Lerp01(1f, f =>
            {
                var a = Mathf.Lerp(1f, 0f, f);
                Overlay.Color = Overlay.Color.SetA(a);

                var volume = SoundController.Instance.PercentageToDecibel(Mathf.Lerp(0f, 1f, f));
                bus.SetVolume(volume);
            });

            Overlay.Hide();
            Hide();
            GameView.Instance.Show();
        }
    }
}
