using Godot;
using System.Collections;

public partial class CultTreeRoom : Node3D
{
    [Export]
    public BasementRoom Room;

    [Export]
    public PlayerArea PlayerAreaEnterHole;

    [Export]
    public ItemArea ItemAreaEnterHole;

    [Export]
    public Touchable TouchableExitCore;

    [Export]
    public Marker3D StartHole;

    [Export]
    public Marker3D StartCore;

    [Export]
    public RootCore RootCore;

    [Export]
    public Cutscene_EndingA CutsceneA;

    [Export]
    public Cutscene_EndingB CutsceneB;

    [Export]
    public PlayerArea PlayerAreaEnterRoom;

    private string DebugId => GetInstanceId().ToString();

    private bool cutscene_started;

    public override void _Ready()
    {
        base._Ready();
        PlayerAreaEnterHole.OnPlayerEntered += PlayerEntered_EnterHole;
        ItemAreaEnterHole.OnItemEntered += ItemEntered_EnterHole;
        PlayerAreaEnterRoom.OnPlayerEntered += PlayerEntered_EnterRoom;
        TouchableExitCore.OnTouched += OnTouched_ExitCore;
        RootCore.OnSwordEntered += SwordEntered;
        RootCore.OnPotionEntered += PotionEntered;

        RegisterDebugActions();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Debug.RemoveActions(DebugId);
    }

    private void RegisterDebugActions()
    {
        var category = "BASEMENT - CULT TREE";

        Debug.RegisterAction(new DebugAction
        {
            Id = DebugId,
            Category = category,
            Text = "Teleport to",
            Action = _ => TeleportToForge()
        });

        void TeleportToForge()
        {
            Player.Instance.GlobalPosition = Room.EnemyMarker.GlobalPosition;
        }
    }

    private void PlayerEntered_EnterHole(Player player)
    {
        SetPlayerLockEnabled(true);
        DialogueFlags.SetFlagMin(DialogueFlags.FrogCore, 3);

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();
            yield return view.TransitionStartCr(new TransitionSettings
            {
                Duration = 1f
            });

            Player.Instance.GlobalPosition = StartCore.GlobalPosition;
            Player.Instance.SetLookRotation(StartCore);
            EnvironmentController.Instance.SetEnvironment(AreaNameType.Core);
            AmbienceController.Instance.StartAmbienceImmediate(AreaNames.Core);
            SetPlayerLockEnabled(false);

            yield return view.TransitionEndCr(new TransitionSettings
            {
                Duration = 1f
            });
        }
    }

    private void OnTouched_ExitCore()
    {
        SetPlayerLockEnabled(true);

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();
            yield return view.TransitionStartCr(new TransitionSettings
            {
                Duration = 1f
            });

            Player.Instance.GlobalPosition = StartHole.GlobalPosition;
            Player.Instance.SetLookRotation(StartHole);
            EnvironmentController.Instance.SetEnvironment(AreaNameType.Cult);
            AmbienceController.Instance.StartAmbienceImmediate(AreaNames.Cult);
            SetPlayerLockEnabled(false);

            yield return view.TransitionEndCr(new TransitionSettings
            {
                Duration = 1f
            });
        }
    }

    private void SetPlayerLockEnabled(bool enabled)
    {
        var id = nameof(CultTreeRoom);
        PauseView.Instance.ToggleLock.SetLock(id, enabled);
        Player.Instance.MovementLock.SetLock(id, enabled);
        Player.Instance.LookLock.SetLock(id, enabled);
        Player.Instance.InteractLock.SetLock(id, enabled);
    }

    private void SwordEntered()
    {
        if (cutscene_started) return;
        cutscene_started = true;

        CutsceneA.StartEnding();
    }

    private void PotionEntered()
    {
        if (cutscene_started) return;
        cutscene_started = true;

        CutsceneB.StartEnding();
    }

    private void PlayerEntered_EnterRoom(Player player)
    {
        DialogueFlags.SetFlagMin(DialogueFlags.FrogCore, 1);
    }

    private void ItemEntered_EnterHole(Item item)
    {
        item.LinearVelocity = Vector3.Zero;
        item.GlobalPosition = StartCore.GlobalPosition;
    }
}
