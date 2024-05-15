using Godot;
using System;

public class InputDirection
{
    private string xNegative, xPositive, yNegative, yPositive;
    private float deadzone;
    private bool started;

    public Action<Vector2> OnStarted, OnHeld, OnEnded;

    public InputDirection(string xNegative, string xPositive, string yNegative, string yPositive, float deadzone = -1)
    {
        this.xNegative = xNegative;
        this.xPositive = xPositive;
        this.yNegative = yNegative;
        this.yPositive = yPositive;
        this.deadzone = deadzone;
    }

    public void ProcessInput()
    {
        var input = Input.GetVector(xNegative, xPositive, yNegative, yPositive, deadzone);
        if (input.Length() > 0)
        {
            if (!started)
            {
                started = true;
                OnStarted?.Invoke(input);
            }

            OnHeld?.Invoke(input);
        }
        else
        {
            if (started)
            {
                started = false;
                OnEnded?.Invoke(input);
            }
        }
    }
}
