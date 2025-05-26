using Godot;
using Godot.Collections;

public static class ArrayExtensions
{
    public static T GetClamped<[MustBeVariant] T>(this Array<T> array, int i)
    {
        return array[Mathf.Clamp(i, 0, array.Count - 1)];
    }
}
