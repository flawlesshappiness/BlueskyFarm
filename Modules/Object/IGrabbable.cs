using Godot;

public interface IGrabbable
{
    public Node3D Node { get; }
    public bool IsGrabbed { get; }
    public bool IsGrabbable { get; set; }
    public void Grabbed();
    public void Released();
    public void SetPosition(Vector3 position);
    public void SetRotation(Vector3 rotation);
}
