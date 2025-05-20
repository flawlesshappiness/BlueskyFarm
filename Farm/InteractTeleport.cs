using Godot;
using System.Collections;

public partial class InteractTeleport : Touchable
{
    [Export]
    public Marker3D DestinationMarker;

    [Export]
    public AreaNameType Area;

    [Export]
    public SoundInfo SfxTeleportStart;

    [Export]
    public SoundInfo SfxTeleportEnd;

    protected override void Touched()
    {
        base.Touched();
        OnTouched_Teleport();
    }

    private void OnTouched_Teleport()
    {
        SetPlayerLockEnabled(true);

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            SfxTeleportStart?.Play(GlobalPosition);

            var view = View.Get<GameView>();
            yield return view.TransitionStartCr(new TransitionSettings
            {
                Duration = 1f
            });

            Player.Instance.GlobalPosition = DestinationMarker.GlobalPosition;
            Player.Instance.SetLookRotation(DestinationMarker);
            EnvironmentController.Instance.SetEnvironment(Area);
            AmbienceController.Instance.StartAmbienceImmediate(Area.ToString());
            SetPlayerLockEnabled(false);

            yield return view.TransitionEndCr(new TransitionSettings
            {
                Duration = 1f
            });

            SfxTeleportEnd?.Play();
        }
    }

    private void SetPlayerLockEnabled(bool enabled)
    {
        var id = nameof(InteractTeleport) + GetInstanceId();
        PauseView.Instance.ToggleLock.SetLock(id, enabled);
        Player.Instance.MovementLock.SetLock(id, enabled);
        Player.Instance.LookLock.SetLock(id, enabled);
        Player.Instance.InteractLock.SetLock(id, enabled);
    }
}
