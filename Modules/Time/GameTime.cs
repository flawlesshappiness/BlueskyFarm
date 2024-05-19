using Godot;
using System;

public partial class GameTime : Node
{
    public static float delta_time { get; private set; }

    public override void _Process(double delta)
    {
        base._Process(delta);
        delta_time = Convert.ToSingle(delta);
    }
}
