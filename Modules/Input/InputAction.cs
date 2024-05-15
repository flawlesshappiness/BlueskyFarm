using Godot;
using System;

public class InputAction
{
    public string Key { get; set; }
    public bool Pressed { get; set; }
    public Action OnPressed { get; set; }
    public Action OnHeld { get; set; }
    public Action OnReleased { get; set; }

    public InputAction(string key)
    {
        Key = key;
    }

    public void ProcessInput(InputEvent @event)
    {
        if (@event.IsActionPressed(Key))
        {
            if (!Pressed)
            {
                Pressed = true;
                OnPressed?.Invoke();
            }

            OnHeld?.Invoke();
        }
        else if (@event.IsActionReleased(Key))
        {
            if (Pressed)
            {
                Pressed = false;
                OnReleased?.Invoke();
            }
        }
    }
}
