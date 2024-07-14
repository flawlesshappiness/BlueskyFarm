using Godot;
using System.Collections;

public partial class DemoView : View
{
    public override string Directory => $"{Paths.ViewDirectory}/{nameof(DemoView)}";

    [NodeName]
    public ColorRect Banner;

    public override void _Ready()
    {
        base._Ready();

        Banner.Modulate = Colors.Transparent;
    }

    public void AnimateDemoCompleted()
    {
        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return AnimateBannerShow();
            yield return new WaitForSeconds(3f);
            yield return AnimateBannerHide();
        }
    }

    public Coroutine AnimateBannerShow() => AnimateBannerVisibility(true);
    public Coroutine AnimateBannerHide() => AnimateBannerVisibility(false);
    public Coroutine AnimateBannerVisibility(bool show)
    {
        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var start = Banner.Modulate;
            var end = show ? Colors.White : Colors.Transparent;
            yield return LerpEnumerator.Lerp01(1f, f =>
            {
                Banner.Modulate = start.Lerp(end, f);
            });
        }
    }
}
