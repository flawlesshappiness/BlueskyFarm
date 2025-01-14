using Godot;
using Godot.Collections;

public partial class BasementRoomSection : Node3DScript
{
    [Export]
    public Node3D Open;

    [Export]
    public Node3D Closed;

    [Export]
    public Node3D Start;

    [Export]
    public Node3D NotStart;

    [Export]
    public Array<Node3D> Areas;

    public void SetOpen(bool open)
    {
        if (open)
        {
            SetOpen();
        }
        else
        {
            SetClosed();
        }
    }

    public void SetOpen()
    {
        Open?.Enable();
        Closed?.Disable();
    }

    public void SetClosed()
    {
        Open?.Disable();
        Closed?.Enable();
    }

    public void SetStart()
    {
        Start?.Enable();
        NotStart?.Disable();
    }

    public void SetNotStart()
    {
        Start?.Disable();
        NotStart?.Enable();
    }

    public void SetArea(string area)
    {
        foreach (var node in Areas)
        {
            node.SetEnabled(node.Name == area);
        }
    }
}
