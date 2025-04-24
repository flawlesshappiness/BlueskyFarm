using Godot;
using System.Collections;

public partial class GameScene : Scene
{
    public static new GameScene Current => Scene.Current as GameScene;

    [Export]
    public WorldEnvironment WorldEnvironment;

    [Export]
    public DirectionalLight3D DirectionalLight;

    private bool _player_is_dying;

    private static bool _registered_debug_actions;

    public override void _Ready()
    {
        base._Ready();
        _player_is_dying = false;

        RegisterDebugActions();

        WorldEnvironment.Environment.AdjustmentEnabled = true;
        UpdateBrightness();

        OptionsController.Instance.OnBrightnessChanged += UpdateBrightness;
    }

    private void UpdateBrightness()
    {
        if (!IsInstanceValid(WorldEnvironment)) return;

        WorldEnvironment.Environment.AdjustmentBrightness = Data.Options.Brightness;
    }

    protected override void Initialize()
    {
        base.Initialize();
        Data.Game.OnBeforeSave += BeforeSave;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Data.Game.OnBeforeSave -= BeforeSave;
    }

    protected virtual void BeforeSave()
    {

    }

    private void RegisterDebugActions()
    {
        if (_registered_debug_actions) return;
        _registered_debug_actions = true;

        var category = "SCENE";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Kill player",
            Action = v =>
            {
                GameScene.Current.KillPlayer();
                v.Close();
            }
        });
    }

    public void KillPlayer()
    {
        if (_player_is_dying) return;

        _player_is_dying = true;
        InventoryController.Instance.ResetInventory();
        Data.Game.Save();
        OnPlayerDeath();
    }

    protected virtual void OnPlayerDeath()
    {
        string id_lock = "death";

        DialogueFlags.SetFlagMin(DialogueFlags.FrogFirstDeath, 1);

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();
            view.SetBlackOverlayAlpha(1);

            SetLocks(true);

            var bus = AudioBus.Get(SoundBus.Transition.ToString());
            bus.SetVolume(AudioMath.PercentageToDecibel(0));

            yield return new WaitForSeconds(2f);

            SetLocks(false);
            DreamController.Instance.StartRandomDream();
        }

        void SetLocks(bool locked)
        {
            Player.Instance.MovementLock.SetLock(id_lock, locked);
            Player.Instance.InteractLock.SetLock(id_lock, locked);
            PauseView.Instance.ToggleLock.SetLock(id_lock, locked);
            InventoryController.Instance.InventoryLock.SetLock(id_lock, locked);
        }
    }
}
