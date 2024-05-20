using Godot;

public interface IInteractable
{
    public Node3D Node { get; }
    public void SetCollisionMode(InteractCollisionMode mode);
}
