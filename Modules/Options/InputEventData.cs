using Godot;

public abstract class InputEventData
{
    public string Action { get; set; }
}

public class InputEventKeyData : InputEventData
{
    public Key Key { get; set; }

    public static InputEventKeyData Create(string action, InputEventKey e)
    {
        return new InputEventKeyData { Action = action, Key = e.Keycode };
    }

    public InputEvent ToEvent()
    {
        return new InputEventKey
        {
            Keycode = Key,
        };
    }
}

public class InputEventMouseButtonData : InputEventData
{
    public MouseButton Button { get; set; }

    public static InputEventMouseButtonData Create(string action, InputEventMouseButton e)
    {
        return new InputEventMouseButtonData { Action = action, Button = e.ButtonIndex };
    }

    public InputEvent ToEvent()
    {
        return new InputEventMouseButton
        {
            ButtonIndex = Button
        };
    }
}