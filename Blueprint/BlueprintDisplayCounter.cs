using Godot;

public partial class BlueprintDisplayCounter : Node3DScript
{
    [Export]
    public Texture2D VegetableIcon;

    [NodeType]
    public Sprite3D Icon;

    [NodeType]
    public Label3D Label;

    public ItemType CurrentType { get; private set; }

    private Texture2D GetIcon(ItemType type) => type switch
    {
        ItemType.Vegetable => VegetableIcon,
        _ => null
    };

    public void SetType(ItemType type)
    {
        Icon.Texture = GetIcon(type);
    }

    public void SetValue(int value)
    {
        Label.Text = value > 0 ? $"x{value}" : string.Empty;
        Icon.Modulate = Icon.Modulate.SetA(value > 0 ? 1 : 0.5f);
    }
}
