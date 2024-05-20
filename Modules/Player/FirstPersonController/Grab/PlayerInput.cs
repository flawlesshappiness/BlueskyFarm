using Godot;

public partial class PlayerInput : Node
{
    public static CustomInputAction Rotate = new CustomInputAction("Rotate");
    public static CustomInputAction ZoomIn = new CustomInputAction("ZoomIn");
    public static CustomInputAction ZoomOut = new CustomInputAction("ZoomOut");
}
