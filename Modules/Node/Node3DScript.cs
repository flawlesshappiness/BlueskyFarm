using Godot;
using System;
using System.Collections;

public partial class Node3DScript : Node3D
{
    private bool _initialized = false;

    public override void _Ready()
    {
        NodeScript.FindNodesFromAttribute(this, GetType());

        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!_initialized)
        {
            _initialized = true;
            Initialize();
        }
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
