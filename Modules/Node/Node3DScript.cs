using Godot;
using System;
using System.Collections;

public partial class Node3DScript : Node3D
{
    private bool _initialized = false;

    public override void _Ready()
    {
        base._Ready();

        if (Debug.IsEditor)
        {
            _ReadyEditor();
        }
        else
        {
            NodeScript.FindNodesFromAttribute(this, GetType());
            _ReadyPlayer();
        }
    }

    protected virtual void _ReadyPlayer()
    {

    }

    protected virtual void _ReadyEditor()
    {

    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Debug.IsEditor)
        {
            _ProcessEditor(delta);
        }
        else
        {
            _ProcessPlayer(delta);
        }
    }

    protected virtual void _ProcessPlayer(double delta)
    {
        if (!_initialized)
        {
            _initialized = true;
            Initialize();
        }
    }

    protected virtual void _ProcessEditor(double delta)
    {

    }

    protected virtual void Initialize()
    {
    }

    public Coroutine StartCoroutine(Func<IEnumerator> enumerator, string id = null)
    {
        return StartCoroutine(enumerator(), id);
    }

    public Coroutine StartCoroutine(IEnumerator enumerator, string id = null)
    {
        id = (id ?? string.Empty) + GetInstanceId();
        return Coroutine.Start(enumerator, id, this);
    }
}
