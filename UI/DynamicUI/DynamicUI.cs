using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class DynamicUI : ControlScript
{
    [Export]
    public bool HideWhenInvisible;

    private List<DynamicUI> _children = new();

    private Coroutine _cr_show;
    private float _time_hide;

    public override void _Ready()
    {
        base._Ready();
        RegisterChildren();
        RegisterInParents();

        if (HideWhenInvisible)
        {
            Hide();
        }
    }

    private void RegisterInParents()
    {
        this.GetNodesInParents<DynamicUI>()
            .ForEach(x => x.RegisterChild(this));
    }

    private void RegisterChildren()
    {
        this.GetNodesInChildren<DynamicUI>()
            .ToList()
            .ForEach(x => RegisterChild(x));
    }

    public void RegisterChild(DynamicUI child)
    {
        _children.Add(child);
    }

    public void AnimateShow(bool include_children, float duration = 5f)
    {
        if (include_children)
        {
            foreach (var child in _children)
            {
                child.AnimateShow(false);
            }
        }

        _time_hide = GameTime.Time + duration;
        _cr_show ??= StartCoroutine(Cr, nameof(AnimateShow) + GetInstanceId());

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

            while (GameTime.Time < _time_hide)
            {
                yield return null;
            }

            _cr_show = null;

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
}
