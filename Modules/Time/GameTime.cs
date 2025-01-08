using Godot;
using System;

public partial class GameTime : Node
{
    public static float DeltaTime { get; private set; }
    public static float Time { get; private set; }
    public static float UnscaledTime { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        DeltaTime = Convert.ToSingle(delta);
        Time += Scene.PauseLock.IsFree ? DeltaTime : 0;
        UnscaledTime += DeltaTime;
    }

    public static float T(float start, float duration)
    {
        return (Time - start) / duration;
    }
}
