using Godot;

public partial class PlayerInput : Node
{
    public static CustomInputAction PickUp = new CustomInputAction("PickUp");
    public static CustomInputAction DropInventory = new CustomInputAction("DropInventory");
    public static CustomInputAction NextInventorySlot = new CustomInputAction("NextInventorySlot");
    public static CustomInputAction PreviousInventorySlot = new CustomInputAction("PreviousInventorySlot");
    public static CustomInputAction UseItem = new CustomInputAction("UseItem");
}