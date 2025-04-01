using Godot;
using System.Collections;

public partial class MainMenuView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(MainMenuView)}";
    public static MainMenuView Instance => View.Get<MainMenuView>();

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

    public override void _Ready()
    {
        base._Ready();

        PlayButton.Pressed += ClickPlay;
        OptionsButton.Pressed += ClickOptions;
        QuitButton.Pressed += ClickQuit;
    }

    protected override void OnShow()
    {
        base.OnShow();
        InventoryController.Instance.InventoryLock.AddLock(nameof(MainMenuView));
        MouseVisibility.Instance.Lock.AddLock(nameof(MainMenuView));
        PauseView.Instance.ToggleLock.AddLock(nameof(MainMenuView));
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
        OptionsView.OptionsControl.OnBack += ClickOptionsBack;
        OptionsView.Show();
        MainControl.Hide();
    }

    private void ClickQuit()
    {
        Scene.Tree.Quit();
    }

    private void ClickOptionsBack()
    {
        OptionsView.OptionsControl.OnBack -= ClickOptionsBack;
        MainControl.Show();
    }

    private void AnimateTransitionToPlay()
    {
        Coroutine.Start(Cr)
            .SetRunWhilePaused(true);

        IEnumerator Cr()
        {
            var bus = AudioBus.Get(SoundBus.Transition.ToString());

            MainControl.SetMouseFilterRec(MouseFilterEnum.Ignore);
            MouseVisibility.Instance.Lock.RemoveLock(nameof(MainMenuView));

            Overlay.Color = Overlay.Color.SetA(0);
            Overlay.Show();

            yield return LerpEnumerator.Lerp01_Unscaled(1f, f =>
            {
                var a = Mathf.Lerp(0f, 1f, f);
                Overlay.Color = Overlay.Color.SetA(a);

                var volume = AudioMath.PercentageToDecibel(Mathf.Lerp(1f, 0f, f));
                bus.SetVolume(volume);
            });

            MainControl.Hide();
            Scene.Goto<FarmScene>();

            yield return LerpEnumerator.Lerp01_Unscaled(1f, f =>
            {
                var a = Mathf.Lerp(1f, 0f, f);
                Overlay.Color = Overlay.Color.SetA(a);

                var volume = AudioMath.PercentageToDecibel(Mathf.Lerp(0f, 1f, f));
                bus.SetVolume(volume);
            });

            Overlay.Hide();
            Hide();
            GameView.Instance.Show();
        }
    }

    public void AnimateTransitionToMainMenu()
    {
        var cr = Coroutine.Start(Cr)
            .SetRunWhilePaused(true);

        IEnumerator Cr()
        {
            var bus = AudioBus.Get(SoundBus.Transition.ToString());
            Scene.PauseLock.AddLock(nameof(MainMenuView));
            Cursor.Hide();

            Show();
            MainControl.Hide();

            Overlay.Color = Overlay.Color.SetA(0);
            Overlay.Show();

            yield return LerpEnumerator.Lerp01_Unscaled(1f, f =>
            {
                var a = Mathf.Lerp(0f, 1f, f);
                Overlay.Color = Overlay.Color.SetA(a);

                var volume = AudioMath.PercentageToDecibel(Mathf.Lerp(1f, 0f, f));
                bus.SetVolume(volume);
            });

            MainControl.Show();
            Scene.Goto<MainMenuScene>();
            Scene.PauseLock.RemoveLock(nameof(MainMenuView));
            MainControl.SetMouseFilterRec(MouseFilterEnum.Stop);

            yield return null;
            Cursor.Hide();

            yield return LerpEnumerator.Lerp01_Unscaled(1f, f =>
            {
                var a = Mathf.Lerp(1f, 0f, f);
                Overlay.Color = Overlay.Color.SetA(a);

                var volume = AudioMath.PercentageToDecibel(Mathf.Lerp(0f, 1f, f));
                bus.SetVolume(volume);
            });

            Overlay.Hide();
        }
    }
}
