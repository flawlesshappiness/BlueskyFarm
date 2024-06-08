using Godot;

public partial class PlayerInput : Node
{
    public static CustomInputAction PickUp = new CustomInputAction("PickUp");
    public static CustomInputAction DropInventory = new CustomInputAction("DropInventory");
}