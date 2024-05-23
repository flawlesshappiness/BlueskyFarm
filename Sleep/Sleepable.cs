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
            var view = View.Get<GameView>();

            FirstPersonController.Instance.MovementLock.AddLock(nameof(Sleepable));
            FirstPersonController.Instance.InteractLock.AddLock(nameof(Sleepable));

            yield return LerpEnumerator.Lerp01(0.5f, f => view.SetOverlayAlpha(Mathf.Lerp(0, 1, f)));
            yield return new WaitForSeconds(0.5f);

            FirstPersonController.Instance.MovementLock.RemoveLock(nameof(Sleepable));
            FirstPersonController.Instance.InteractLock.RemoveLock(nameof(Sleepable));

            yield return LerpEnumerator.Lerp01(0.5f, f => view.SetOverlayAlpha(Mathf.Lerp(1, 0, f)));
        }
    }
}
