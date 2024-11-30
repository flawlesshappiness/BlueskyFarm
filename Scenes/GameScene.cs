using Godot;
using System.Collections;

public partial class GameScene : Scene
{
    public static new GameScene Current => Scene.Current as GameScene;

    [NodeType]
    public WorldEnvironment WorldEnvironment;

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
                v.SetVisible(false);
            }
        });
    }

    public void KillPlayer()
    {
        if (_player_is_dying) return;

        _player_is_dying = true;
        InventoryController.Instance.ClearInventory();
        OnPlayerDeath();
    }

    protected virtual void OnPlayerDeath()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();
            view.SetBlackOverlayAlpha(1);

            Player.Instance.MovementLock.AddLock("death");
            Player.Instance.InteractLock.AddLock("death");

            var bus = AudioBus.Get(SoundBus.Master.ToString());
            bus.SetMuted(true);

            yield return new WaitForSeconds(2f);

            Goto(nameof(FarmScene));

            view.SetBlackOverlayAlpha(0);

            bus.SetMuted(false);

            Player.Instance.GlobalPosition = Vector3.Zero;
            Player.Instance.MovementLock.RemoveLock("death");
            Player.Instance.InteractLock.RemoveLock("death");
        }
    }
}
