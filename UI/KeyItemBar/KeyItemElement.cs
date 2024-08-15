using Godot;

public partial class KeyItemElement : ControlScript
{
    [NodeType]
    public TextureRect TextureRect;

    [NodeName]
    public Label CountLabel;

    public string Id { get; set; }
    public bool AlwaysShowCount { get; set; }
    public Texture2D Icon { set { TextureRect.Texture = value; } }

    public void SetCount(int i)
    {
        CountLabel.Text = i.ToString();
        CountLabel.Visible = i > 0 || AlwaysShowCount;
    }
}
