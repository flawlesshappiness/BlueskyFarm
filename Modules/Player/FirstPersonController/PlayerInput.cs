using Godot;

public partial class PlayerInput : Node
{
    public static CustomInputAction Forward = new CustomInputAction("Forward");
    public static CustomInputAction Back = new CustomInputAction("Back");
    public static CustomInputAction Left = new CustomInputAction("Left");
    public static CustomInputAction Right = new CustomInputAction("Right");
    public static CustomInputAction Jump = new CustomInputAction("Jump");
    public static CustomInputAction Interact = new CustomInputAction("Interact");
    public static CustomInputAction Pause = new CustomInputAction("Pause");
    public static CustomInputAction Run = new CustomInputAction("Run");

    public static Vector2 GetMoveInput()
    {
        return Input.GetVector(
            Left.Name,
            Right.Name,
            Forward.Name,
            Back.Name);
    }
}
