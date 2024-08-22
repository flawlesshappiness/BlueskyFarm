using Godot;

[GlobalClass]
public partial class CursorInfo : Resource
{
    [Export]
    public string Name { get; set; }

    [Export]
    public Texture2D Texture { get; set; }
}
