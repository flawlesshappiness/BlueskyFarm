using Godot;
using System;

public interface ITouchable
{
    public event Action OnTouched;
    public Node3D Node { get; }
    public void Touch();
}
