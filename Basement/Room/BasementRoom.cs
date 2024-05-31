using Godot;

public partial class BasementRoom : Node3DScript
{
    public Section North { get; private set; }
    public Section East { get; private set; }
    public Section South { get; private set; }
    public Section West { get; private set; }

    public class Section
    {
        public Node3D Root { get; set; }
        public Node3D Open { get; set; }
        public Node3D Closed { get; set; }

        public void SetOpen(bool open)
        {
            Open.SetEnabled(open);
            Closed.SetEnabled(!open);
        }
    }

    public override void _Ready()
    {
        base._Ready();

        North = FindSection(nameof(North));
        East = FindSection(nameof(East));
        South = FindSection(nameof(South));
        West = FindSection(nameof(West));
    }

    private Section FindSection(string name)
    {
        var root = GetNode<Node3D>(name);
        var open = root.GetNode<Node3D>("Open");
        var closed = root.GetNode<Node3D>("Closed");

        var section = new Section
        {
            Root = root,
            Open = open,
            Closed = closed,
        };

        return section;
    }
}
