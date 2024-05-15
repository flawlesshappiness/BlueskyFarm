using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class DebugContentList : ControlScript
{
    [NodeName("Label")]
    public Label Label;

    private List<Node> elements = new();

    public override void _Ready()
    {
        base._Ready();
        Label.Visible = false;
    }

    public void Clear()
    {
        elements.ToList().ForEach(e => e.QueueFree());
        elements.Clear();
    }

    public void AddText(string text)
    {
        var label = Label.Duplicate() as Label;
        label.SetParent(Label.GetParent());
        label.Visible = true;
        label.Text = text;

        elements.Add(label);
    }
}
