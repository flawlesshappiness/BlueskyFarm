using Godot;
using System;

public interface IGrabbable
{
    public event Action OnGrabbed, OnReleased;
    public Node3D Node { get; }
    public bool IsGrabbed { get; }
    public void Grabbed();
    public void Released();
    public void SetPosition(Vector3 position);
    public void SetRotation(Vector3 rotation);
}
