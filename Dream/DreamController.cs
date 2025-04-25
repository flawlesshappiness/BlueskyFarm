using System.Collections;
using System.Collections.Generic;

public partial class DreamController : SingletonController
{
    public static DreamController Instance => Singleton.Get<DreamController>();
    public override string Directory => "Dream";

    private List<string> _scenes = new List<string> { "Dream_Office", "Dream_Falling" };

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

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Start random dream",
            Action = v => StartRandomDream()
        });

        void StartDreamSearch(DebugView view)
        {
            view.HideContent();
            view.SetContent_Search();

            view.ContentSearch.ClearItems();
            view.ContentSearch.AddItem("Office", () => { view.Close(); StartDream("Dream_Office"); });
            view.ContentSearch.AddItem("Falling", () => { view.Close(); StartDream("Dream_Falling"); });
            view.ContentSearch.UpdateButtons();
        }
    }

    public void StartRandomDream()
    {
        var scene = _scenes.Random();
        StartDream(scene);
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
            var scene = Scene.Goto(scenename);
            var dream_scene = scene as DreamScene;

            yield return new WaitForSeconds(0.5f);
            yield return dream_scene.WaitForReady();

            Cursor.Hide();

            ScreenEffects.SetGaussianBlur(FxId, 20);
            ScreenEffects.SetDistort(FxId, 0.05f);

            dream_scene.StartScene();

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

    public void EndDream(bool immediate = false)
    {
        if (_transitioning) return;
        _transitioning = true;

        var bus = AudioBus.Get(SoundBus.Transition.ToString());

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            if (immediate)
            {
                GameView.Instance.SetBlackOverlayAlpha(1);
                yield return null;
            }
            else
            {
                yield return GameView.Instance.TransitionStartCr(new TransitionSettings
                {
                    Duration = 4f,
                    GaussianBlur = 40f,
                    GaussianBlurStartDuration = 1f,
                });
            }

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
        InventoryController.Instance.InventoryLock.SetLock(FxId, locked);
    }
}
