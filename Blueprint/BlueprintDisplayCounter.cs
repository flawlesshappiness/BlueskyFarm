using Godot;

public partial class BlueprintDisplayCounter : Node3DScript
{
    [Export]
    public Texture2D VegetableIcon;

    [Export]
    public Texture2D BoneIcon;

    [Export]
    public Texture2D StoneIcon;

    [Export]
    public Texture2D PotionRedIcon;

    [Export]
    public Texture2D PotionOrangeIcon;

    [Export]
    public Texture2D PotionGreenIcon;

    [NodeType]
    public Sprite3D Icon;

    [NodeType]
    public Label3D Label;

    public ItemType CurrentType { get; private set; }

    private Texture2D GetIcon(ItemType type) => type switch
    {
        ItemType.Crop_Vegetable => VegetableIcon,
        ItemType.Crop_Bone => BoneIcon,
        ItemType.Crop_Stone => StoneIcon,
        ItemType.Potion_Red => PotionRedIcon,
        ItemType.Potion_Orange => PotionOrangeIcon,
        ItemType.Potion_Green => PotionGreenIcon,
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
