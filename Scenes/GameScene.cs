using Godot;
using System.Collections;

public partial class GameScene : Scene
{
    public static GameScene Current => Scene.Current as GameScene;

    [NodeType]
    public WorldEnvironment WorldEnvironment;

    private bool _player_is_dying;

    private static bool _registered_debug_actions;

    public override void _Ready()
    {
        base._Ready();
        _player_is_dying = false;

        ScreenEffects.Reset();
        RegisterDebugActions();

        WorldEnvironment.Environment.AdjustmentEnabled = true;
        UpdateBrightness();

        OptionsController.Instance.OnBrightnessChanged += UpdateBrightness;
    }

    private void UpdateBrightness()
    {
        if (!IsInstanceValid(WorldEnvironment)) return;

        WorldEnvironment.Environment.AdjustmentBrightness = Data.Game.Brightness;
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
        OnPlayerDeath();
    }

    protected virtual void OnPlayerDeath()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();
            view.SetOverlayAlpha(1);

            var player = FirstPersonController.Instance;
            player.MovementLock.AddLock("death");
            player.InteractLock.AddLock("death");

            var bus = AudioBus.Get(SoundBus.Master.ToString());
            bus.SetMuted(true);

            ScreenEffects.Reset();

            yield return new WaitForSeconds(2f);

            Goto("farm");

            view.SetOverlayAlpha(0);

            bus.SetMuted(false);

            player = FirstPersonController.Instance;
            player.GlobalPosition = Vector3.Zero;
            player.MovementLock.RemoveLock("death");
            player.InteractLock.RemoveLock("death");
        }
    }
}
