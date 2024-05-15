public class CollisionMask
{
    private uint value;

    private CollisionMask()
    {

    }

    public static CollisionMask Create(params int[] layers)
    {
        var mask = new CollisionMask();

        foreach (var layer in layers)
        {
            mask.value |= (uint)(1 << (layer - 1));
        }

        return mask;
    }

    public static implicit operator uint(CollisionMask mask)
    {
        return mask.value;
    }
}
