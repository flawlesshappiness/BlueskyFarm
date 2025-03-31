using Godot;
using System.Collections;

public partial class CultTreeRoom : Node3D
{
    [Export]
    public BasementRoom Room;

    [Export]
    public PlayerArea PlayerAreaEnterHole;

    [Export]
    public Touchable TouchableExitCore;

    [Export]
    public Marker3D StartHole;

    [Export]
    public Marker3D StartCore;

    [Export]
    public RootCore RootCore;

    private string DebugId => GetInstanceId().ToString();

    public override void _Ready()
    {
        base._Ready();
        PlayerAreaEnterHole.OnPlayerEntered += PlayerEntered_EnterHole;
        TouchableExitCore.OnTouched += OnTouched_ExitCore;
        RootCore.OnSwordEntered += SwordEntered;

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

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();
            yield return view.TransitionStartCr(nameof(SceneDoor));

            Player.Instance.GlobalPosition = StartCore.GlobalPosition;
            Player.Instance.SetLookRotation(StartCore);
            EnvironmentController.Instance.SetEnvironment(AreaNameType.Core);
            AmbienceController.Instance.StartAmbienceImmediate(AreaNames.Core);
            SetPlayerLockEnabled(false);

            yield return view.TransitionEndCr(nameof(SceneDoor));
        }
    }

    private void OnTouched_ExitCore()
    {
        SetPlayerLockEnabled(true);

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var view = View.Get<GameView>();
            yield return view.TransitionStartCr(nameof(SceneDoor));

            Player.Instance.GlobalPosition = StartHole.GlobalPosition;
            Player.Instance.SetLookRotation(StartHole);
            EnvironmentController.Instance.SetEnvironment(AreaNameType.Cult);
            AmbienceController.Instance.StartAmbienceImmediate(AreaNames.Cult);
            SetPlayerLockEnabled(false);

            yield return view.TransitionEndCr(nameof(SceneDoor));
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
        Debug.Log("BAD END");
    }
}
