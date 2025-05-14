using Godot;
using System;
using System.Collections;
using System.Linq;

public partial class GameScene : Scene
{
    public static new GameScene Current => Scene.Current as GameScene;

    [Export]
    public WorldEnvironment WorldEnvironment;

    [Export]
    public DirectionalLight3D DirectionalLight;

    public SceneData SceneData { get; set; }

    private bool _player_is_dying;
    private static bool _registered_debug_actions;

    public static event Action<GameScene> OnSceneLoaded;

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();

        _player_is_dying = false;

        UpdateBrightness();
        WorldEnvironment.Environment.AdjustmentEnabled = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        LoadData();
        Data.Game.OnBeforeSave += BeforeSave;
        OptionsController.Instance.OnBrightnessChanged += UpdateBrightness;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Data.Game.OnBeforeSave -= BeforeSave;
        OptionsController.Instance.OnBrightnessChanged -= UpdateBrightness;
    }

    private void UpdateBrightness()
    {
        if (!IsInstanceValid(WorldEnvironment)) return;

        WorldEnvironment.Environment.AdjustmentBrightness = Data.Options.Brightness;
    }

    protected virtual void BeforeSave()
    {

    }

    protected virtual void LoadData()
    {
        SceneData ??= GetOrCreateData();

        OnSceneLoaded?.Invoke(this);
    }

    private SceneData GetOrCreateData()
    {
        var data = Data.Game.Scenes.FirstOrDefault(x => x.Name == Name);
        if (data == null)
        {
            data = new SceneData { Name = Name };
            Data.Game.Scenes.Add(data);
        }

        return data;
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

        Player.Instance.Interrupt();
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
