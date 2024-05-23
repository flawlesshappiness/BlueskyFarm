using Godot;
using System.Collections;

public partial class Sleepable : Touchable
{
    [Export]
    public int SleepTicks;

    public override void _Ready()
    {
        base._Ready();
        OnTouched += Sleep;
    }

    private void Sleep()
    {
        Debug.LogMethod();
        Debug.Indent++;

        SleepController.Instance.AddTicks(SleepTicks);
        Data.Game.Save();

        AnimateSleep();

        Debug.Indent--;
    }

    private void AnimateSleep()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            FirstPersonController.Instance.MovementLock.AddLock(nameof(Sleepable));
            FirstPersonController.Instance.InteractLock.AddLock(nameof(Sleepable));

            var view = View.Get<GameView>();
            view.SetOverlayAlpha(1);
            yield return new WaitForSeconds(1f);
            view.SetOverlayAlpha(0);

            FirstPersonController.Instance.MovementLock.RemoveLock(nameof(Sleepable));
            FirstPersonController.Instance.InteractLock.RemoveLock(nameof(Sleepable));
        }
    }
}
