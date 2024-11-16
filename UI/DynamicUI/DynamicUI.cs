using Godot;
using System.Collections;

public partial class DynamicUI : ControlScript
{
    [Export]
    public bool HideWhenInvisible;

    public override void _Ready()
    {
        base._Ready();

        if (HideWhenInvisible)
        {
            Hide();
        }
    }

    public void AnimateShow(float duration = 5f)
    {
        StartCoroutine(Cr, "animate_" + GetInstanceId());
        IEnumerator Cr()
        {
            if (HideWhenInvisible)
            {
                Show();
            }

            var start = Modulate;
            var end = start.SetA(1);
            yield return LerpEnumerator.Lerp01(0.25f, f =>
            {
                Modulate = start.Lerp(end, f);
            });

            yield return new WaitForSeconds(duration);

            start = Modulate;
            end = start.SetA(0);
            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                Modulate = start.Lerp(end, f);
            });

            if (HideWhenInvisible)
            {
                Hide();
            }
        }
    }

    public void AnimateHide()
    {
        StartCoroutine(Cr, "animate_" + GetInstanceId());
        IEnumerator Cr()
        {
            if (HideWhenInvisible)
            {
                Show();
            }

            var start = Modulate;
            var end = start.SetA(0);
            yield return LerpEnumerator.Lerp01(0.25f, f =>
            {
                Modulate = start.Lerp(end, f);
            });

            if (HideWhenInvisible)
            {
                Hide();
            }
        }
    }
}
