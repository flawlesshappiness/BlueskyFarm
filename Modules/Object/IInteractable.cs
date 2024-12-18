using Godot;

public interface IInteractable
{
    public PhysicsBody3D Body { get; }

    public string GetHoverText();
    public Texture2D GetHoverIcon();
}
