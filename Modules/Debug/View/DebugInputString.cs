using Godot;

public partial class DebugInputString : ControlScript
{
    [NodeType]
    public Label Label;

    [NodeType]
    public LineEdit Text;

    public string Id { get; set; }
}
