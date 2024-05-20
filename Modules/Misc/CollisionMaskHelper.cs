public static class CollisionMaskHelper
{
    public static uint Create(params int[] layers)
    {
        uint value = default(uint);

        foreach (var layer in layers)
        {
            value |= (uint)(1 << (layer - 1));
        }

        return value;
    }
}
