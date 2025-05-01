using Godot;
using System;
using System.Collections;

public partial class MainMenuView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(MainMenuView)}";
    public static MainMenuView Instance => View.Get<MainMenuView>();

    [Export]
    public Button NewGameButton;

    [Export]
    public Button ContinueButton;

    [Export]
    public Button ProfilesButton;

    [Export]
    public Button OptionsButton;

    [Export]
    public Button QuitButton;

    [Export]
    public Control MainControl;

    [Export]
    public ColorRect Overlay;

    [Export]
    public Label ProfileLabel;

    public event Action OnGameStarted;
    public event Action OnReturnToMainMenu;

    private OptionsView OptionsView => View.Get<OptionsView>();

    public override void _Ready()
    {
        base._Ready();

        NewGameButton.Pressed += ClickNewGame;
        ContinueButton.Pressed += ClickContinue;
        ProfilesButton.Pressed += ClickProfiles;
        OptionsButton.Pressed += ClickOptions;
        QuitButton.Pressed += ClickQuit;
    }

    protected override void OnShow()
    {
        base.OnShow();
        SetLocks(true);
        InitializeNewGameButton();
        InitializeProfileLabel();
    }

    private void SetLocks(bool locked)
    {
        var id = nameof(MainMenuView);
        InventoryController.Instance.InventoryLock.SetLock(id, locked);
        MouseVisibility.Instance.Lock.SetLock(id, locked);
        PauseView.Instance.ToggleLock.SetLock(id, locked);
    }

    private void InitializeNewGameButton()
    {
        var profile_data_exists = !Data.Game.Deleted;
        NewGameButton.Visible = !profile_data_exists;
        ContinueButton.Visible = profile_data_exists;
    }

    private void InitializeProfileLabel()
    {
        ProfileLabel.Text = $"Profile {Data.Options.SelectedGameProfile}";
    }

    private void ClickNewGame()
    {
        GameProfileController.Instance.CreateGameProfile(Data.Options.SelectedGameProfile);
        ClickContinue();
    }

    private void ClickContinue()
    {
        AnimateTransitionToPlay();
    }

    private void ClickProfiles()
    {
        Hide();
        ProfilesView.Instance.Show();
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
        this.StartCoroutine(Cr, "transition")
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
            SetLocks(false);

            OnGameStarted?.Invoke();

            GameView.Instance.Show();
        }
    }

    public void AnimateTransitionToMainMenu()
    {
        var cr = this.StartCoroutine(Cr, "transition")
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

            OnReturnToMainMenu?.Invoke();

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
