using Godot;

public partial class SceneDoor : Touchable
{
    [Export]
    public string SceneName;

    [Export]
    public string StartNode;

    public override void _Ready()
    {
        base._Ready();
        OnTouched += Touched;
    }

    private void Touched()
    {
        Debug.LogMethod();
        Debug.Indent++;

        if (string.IsNullOrEmpty(SceneName))
        {
            Debug.LogError("SceneDoor.SceneName is empty");
            Debug.Indent--;
            return;
        }

        // Load scene
        Scene.Goto(SceneName);

        // Teleport player to position
        var node = Scene.Current.GetNodeInChildren<Node3D>(StartNode);
        if (node != null)
        {
            FirstPersonController.Instance.GlobalPosition = node.GlobalPosition;
            FirstPersonController.Instance.SetLookRotation(node);
        }

        Debug.Indent--;
    }
}
