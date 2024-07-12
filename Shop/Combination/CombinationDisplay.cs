using Godot;
using System.Collections.Generic;

public partial class CombinationDisplay : Node3DScript
{
    [Export]
    public int CombinationLength;

    [Export]
    public float MaxDisplayWidth;

    [NodeName]
    public CombinationDisplayObject DisplayObject;

    private List<CombinationDisplayObject> _objects = new();

    public override void _Ready()
    {
        base._Ready();
        InitializeObjects();
    }

    public void InitializeObjects()
    {
        var parent = DisplayObject.GetParent();
        for (int i = 0; i < CombinationLength; i++)
        {
            var obj = DisplayObject.Duplicate() as CombinationDisplayObject;
            obj.SetParent(parent);
            obj.Show();
            obj.Clear();
            obj.GlobalPosition = GetObjectPosition(i);
            _objects.Add(obj);
        }

        DisplayObject.Visible = false;
    }

    public void UpdateDisplay(string combination)
    {
        ClearDisplay();

        var chars = combination.ToCharArray();
        var length = Mathf.Min(chars.Length, _objects.Count);
        for (int i = 0; i < length; i++)
        {
            var input = chars[i].ToString();
            var obj = _objects[i];
            obj.SetDisplayType(input);
        }
    }

    public void ClearDisplay()
    {
        foreach (var obj in _objects)
        {
            obj.Clear();
        }
    }

    private Vector3 GetObjectPosition(int index)
    {
        var w = MaxDisplayWidth;
        var wh = w * 0.5f;
        var start = GlobalPosition.X - wh;
        var wper = MaxDisplayWidth / (CombinationLength - 1);
        var x = start + wper * index;
        return GlobalPosition + new Vector3(x, 0, 0);
    }
}
