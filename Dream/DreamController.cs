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
        GameView.Instance.SetBlackOverlayAlpha(1);
        var bus = AudioBus.Get(SoundBus.Transition.ToString());

        AmbienceController.Instance.StopAmbience();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                var vol = AudioMath.LerpPercentageToDecibel(1f, 0f, f);
                bus.SetVolume(vol);
            });

            var scene = Scene.Goto(scenename);
            var dream_scene = scene as DreamScene;

            yield return new WaitForSeconds(0.5f);

            ScreenEffects.SetGaussianBlur(FxId, 20);
            ScreenEffects.SetDistort(FxId, 0.05f);

            yield return GameView.Instance.TransitionEndCr(new TransitionSettings
            {
                Duration = 4f,
                GaussianBlur = 40f,
                GaussianBlurStartDuration = 1f,
            });

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
            yield return GameView.Instance.TransitionStartCr(new TransitionSettings
            {
                Duration = 4f,
                GaussianBlur = 40f,
                GaussianBlurStartDuration = 1f,
            });

            ScreenEffects.RemoveGaussianBlur(FxId);
            ScreenEffects.RemoveDistort(FxId);

            Data.Game.PlayerPositionNode = "StartBed";
            Scene.Goto<FarmScene>();

            yield return null;

            yield return GameView.Instance.TransitionEndCr(new TransitionSettings
            {
                Duration = 4f,
                GaussianBlur = 20f,
                GaussianBlurStartDuration = 1f,
            });

            _transitioning = false;
        }
    }

    private void SetTransitionLocks(bool locked)
    {
        PauseView.Instance.ToggleLock.SetLock(FxId, locked);
    }
}
