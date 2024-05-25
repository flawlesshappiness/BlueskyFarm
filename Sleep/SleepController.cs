using Godot;
using System;

public partial class SleepController : Node
{
    public static SleepController Instance => Singleton.GetOrCreate<SleepController>($"Sleep/{nameof(SleepController)}");

    public int CurrentTicks => Data.Game.SleepTicks;

    public Action OnTicksChanged;

    public void AddTicks(int value)
    {
        Debug.LogMethod(value);
        Debug.Indent++;

        Data.Game.SleepTicks += value;
        OnTicksChanged?.Invoke();

        Debug.Log($"Total sleep ticks: {Data.Game.SleepTicks}");
        Debug.Indent--;
    }
}
