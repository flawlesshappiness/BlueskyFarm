using Godot;

public interface IInteractable
{
    public Node3D Node { get; }
    public string InteractableText { get; }
    public void SetCollisionMode(InteractCollisionMode mode);
}

public enum InteractCollisionMode
{
    None, All, Collision, Interact
}
