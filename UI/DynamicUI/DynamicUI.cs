using Godot;
using System.Collections;

public partial class DynamicUI : ControlScript
{
    [Export]
    public bool HideWhenInvisible;

    public bool KeepShowing { get; set; }

    private Coroutine _cr;
    private bool _showing;
    private bool _hiding;

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
        if (_showing) return;
        _cr = StartCoroutine(Cr, "animate_" + GetInstanceId());
        IEnumerator Cr()
        {
            _showing = true;
            _hiding = false;

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

            while (KeepShowing)
            {
                yield return null;
            }

            _showing = false;
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
        if (_hiding) return;

        _cr = StartCoroutine(Cr, "animate_" + GetInstanceId());
        IEnumerator Cr()
        {
            _hiding = true;
            _showing = false;

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

            _hiding = false;
        }
    }

    public void SetHidden()
    {
        _showing = false;
        _hiding = false;

        Coroutine.Stop(_cr);

        Modulate = Modulate.SetA(0);

        if (HideWhenInvisible)
        {
            Hide();
        }
    }

    public void SetShown()
    {
        _showing = false;
        _hiding = false;

        Coroutine.Stop(_cr);

        Show();
        Modulate = Modulate.SetA(1);
    }
}
