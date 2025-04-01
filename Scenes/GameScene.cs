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

        if (DialogueController.Instance.GetIntFlag(DialogueFlags.FirstDeath) == 0)
        {
            DialogueController.Instance.SetFlag(DialogueFlags.FirstDeath, 1);
        }

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();
            view.SetBlackOverlayAlpha(1);

            Player.Instance.MovementLock.AddLock(id_lock);
            Player.Instance.InteractLock.AddLock(id_lock);
            PauseView.Instance.ToggleLock.AddLock(id_lock);

            var bus = AudioBus.Get(SoundBus.Master.ToString());
            bus.SetMuted(true);

            yield return new WaitForSeconds(2f);

            Goto(nameof(FarmScene));

            view.SetBlackOverlayAlpha(0);

            bus.SetMuted(false);

            Player.Instance.GlobalPosition = Vector3.Zero;
            Player.Instance.MovementLock.RemoveLock(id_lock);
            Player.Instance.InteractLock.RemoveLock(id_lock);
            PauseView.Instance.ToggleLock.RemoveLock(id_lock);
        }
    }
}
