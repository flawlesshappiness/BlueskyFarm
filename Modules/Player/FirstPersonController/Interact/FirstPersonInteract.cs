using Godot;

public partial class FirstPersonInteract : RayCast3D, IPlayerInteract
{
    private IInteractable current_interactable;
    private Node3D current_collider;

    public IInteractable CurrentInteractable => current_interactable;

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessInteractRaycast();
    }

    private void ProcessInteractRaycast()
    {
        if (IsColliding())
        {
            current_collider = GetCollider() as Node3D;
            var interactable = current_collider.GetNodeInParents<IInteractable>();
            SetInteractable(interactable);
        }
        else
        {
            current_collider = null;
            current_interactable = null;
        }
    }

    private void SetInteractable(IInteractable interactable)
    {
        if (interactable == null) return;
        if (current_interactable == interactable) return;
        current_interactable = interactable;
    }
}
