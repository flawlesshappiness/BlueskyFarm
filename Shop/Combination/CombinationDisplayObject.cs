using Godot;

public partial class CombinationDisplayObject : Node3DScript
{
    [NodeName]
    public Node3D Red;

    [NodeName]
    public Node3D Green;

    [NodeName]
    public Node3D Blue;

    public void SetDisplayType(string input)
    {
        Clear();

        var obj = input switch
        {
            "R" => Red,
            "G" => Green,
            "B" => Blue,
            _ => null
        };

        if (obj != null)
        {
            obj.Visible = true;
        }
    }

    public void Clear()
    {
        Red.Visible = false;
        Green.Visible = false;
        Blue.Visible = false;
    }
}
