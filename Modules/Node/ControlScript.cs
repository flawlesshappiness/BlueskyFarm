using Godot;
using System;
using System.Collections;

public partial class ControlScript : Control
{
    private bool _visible;

    public override void _Ready()
    {
        NodeScript.FindNodesFromAttribute(this, GetType());

        base._Ready();
    }

    public Coroutine StartCoroutine(Func<IEnumerator> enumerator, string id = null)
    {
        id = (id ?? string.Empty) + GetInstanceId();
        return Coroutine.Start(enumerator, this, id);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_Visible();
    }

    private void Process_Visible()
    {
        if (Visible != _visible)
        {
            _visible = Visible;
            if (Visible)
            {
                OnShow();
            }
            else
            {
                OnHide();
            }
        }
    }

    protected virtual void OnShow()
    {

    }

    protected virtual void OnHide()
    {

    }
}
