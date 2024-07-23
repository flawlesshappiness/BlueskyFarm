using Godot;
using System;
using System.Collections;

public partial class Node3DScript : Node3D
{
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
}
