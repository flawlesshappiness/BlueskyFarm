using Godot;
using System;

public static class AudioMath
{
    public static float PercentageToDecibel(float value)
    {
        var t = Math.Max(value, 0.0001f);
        var dcb = Math.Log10(t) * 20;
        return Convert.ToSingle(dcb);
    }

    public static float DecibelToPercentage(float value)
    {
        return Mathf.Pow(10, value / 20f);
    }
}
