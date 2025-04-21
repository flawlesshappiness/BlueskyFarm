using System.Collections;

public partial class DreamController : SingletonController
{
    public static DreamController Instance => Singleton.Get<DreamController>();

    public override string Directory => "Dream";

    private bool _transitioning;
    private string FxId => nameof(DreamController);

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "DREAM";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Start dream",
            Action = StartDreamSearch
        });

        void StartDreamSearch(DebugView view)
        {
            view.HideContent();
            view.SetContent_Search();

            view.ContentSearch.ClearItems();
            view.ContentSearch.AddItem("Office", () => { view.Close(); StartDream("Dream_Office"); });
            view.ContentSearch.UpdateButtons();
        }
    }

    public void StartRandomDream()
    {
        StartDream("Dream_Office");
    }

    private void StartDream(string scenename)
    {
        if (_transitioning) return;
        _transitioning = true;

        SetTransitionLocks(true);
        MainMenuView.Instance.Hide();
        GameView.Instance.Show();
        GameView.Instance.SetBlackOverlayAlpha(1);
        var bus = AudioBus.Get(SoundBus.Transition.ToString());

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                var vol = AudioMath.LerpPercentageToDecibel(1f, 0f, f);
                bus.SetVolume(vol);
            });

            AmbienceController.Instance.StopAmbience();

            var scene = Scene.Goto(scenename);
            var dream_scene = scene as DreamScene;

            yield return new WaitForSeconds(0.5f);

            yield return LerpEnumerator.Lerp01(1f, f =>
            {
                var vol = AudioMath.LerpPercentageToDecibel(0f, 1f, f);
                bus.SetVolume(vol);
            });

            GameView.Instance.AnimateBlackOverlay(false, 4f);
            ScreenEffects.AnimateGaussianBlur(FxId, 40, 0, 1f, 4f);
            ScreenEffects.SetGaussianBlur(FxId, 20);
            ScreenEffects.SetDistort(FxId, 0.05f);

            SetTransitionLocks(false);

            _transitioning = false;
        }
    }

    public void EndDream()
    {
        if (_transitioning) return;
        _transitioning = true;

        var bus = AudioBus.Get(SoundBus.Transition.ToString());

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            ScreenEffects.AnimateGaussianBlur(FxId, 40, 4f, 1f, 0f);
            yield return new WaitForSeconds(1f);
            GameView.Instance.AnimateBlackOverlay(true, 4f);

            yield return LerpEnumerator.Lerp01(4f, f =>
            {
                var vol = AudioMath.LerpPercentageToDecibel(1f, 0f, f);
                bus.SetVolume(vol);
            });

            ScreenEffects.RemoveGaussianBlur(FxId);
            ScreenEffects.RemoveDistort(FxId);

            Data.Game.PlayerPositionNode = "StartBed";
            Scene.Goto<FarmScene>();

            yield return null;

            GameView.Instance.AnimateBlackOverlay(false, 2f);
            ScreenEffects.AnimateGaussianBlur(FxId, 20, 0f, 1f, 4f);

            yield return LerpEnumerator.Lerp01(4f, f =>
            {
                var vol = AudioMath.LerpPercentageToDecibel(0f, 1f, f);
                bus.SetVolume(vol);
            });

            _transitioning = false;
        }
    }

    private void SetTransitionLocks(bool locked)
    {
        PauseView.Instance.ToggleLock.SetLock(FxId, locked);
    }
}
