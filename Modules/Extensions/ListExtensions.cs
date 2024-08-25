using Godot;
using System.Collections.Generic;

public static class ListExtensions
{
    public static T Random<T>(this List<T> list, RandomNumberGenerator rng = null)
    {
        rng ??= new RandomNumberGenerator();
        return list.Count > 0 ? list.Get(rng.RandiRange(0, list.Count - 1)) : default(T);
    }

    public static T GetClamped<T>(this List<T> list, int index)
    {
        return list[Mathf.Clamp(index, 0, list.Count - 1)];
    }

    public static T Get<T>(this List<T> list, int index)
    {
        if (index < 0 || index >= list.Count) return default(T);
        return list[index];
    }
}
