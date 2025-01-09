using System;

public static class AudioMath
{
    public static float PercentageToDecibel(float value)
    {
        var t = Math.Clamp(value, 0.0001f, 1.0f);
        var dcb = Math.Log10(t) * 20;
        return Convert.ToSingle(dcb);
    }
}
