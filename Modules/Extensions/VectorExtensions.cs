using Godot;

public static class VectorExtensions
{
    #region VECTOR3
    public static Vector3 Set(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        v.X = x ?? v.X;
        v.Y = y ?? v.Y;
        v.Z = z ?? v.Z;
        return v;
    }

    public static Vector3 Add(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        v.X += x ?? 0;
        v.Y += y ?? 0;
        v.Z += z ?? 0;
        return v;
    }

    public static Vector2 ToVector2(this Vector3 v)
    {
        return new Vector2 { X = v.X, Y = v.Y };
    }

    public static Vector3 RandomNormalized(this Vector3 v)
    {
        var rng = new RandomNumberGenerator();
        return new Vector3(rng.Randf(), rng.Randf(), rng.Randf()).Normalized();
    }

    public static Vector3 WrappedEulerAngles(this Vector3 eulerAngles)
    {
        return EulerMath.WrappedEulerAngles(eulerAngles);
    }

    public static Vector3 ClosestEulerAngle(this Vector3 from, Vector3 to)
    {
        return EulerMath.ClosestEulerAngles(from.WrappedEulerAngles(), to.WrappedEulerAngles());
    }
    #endregion

    #region VECTOR2
    public static Vector2 Set(this Vector2 v, float? x = null, float? y = null)
    {
        v.X = x ?? v.X;
        v.Y = y ?? v.Y;
        return v;
    }

    public static Vector3 ToVector3(this Vector2 v)
    {
        return new Vector3 { X = v.X, Y = v.Y };
    }
    #endregion

    #region VECTOR2I
    public static float DistanceTo(this Vector2I v1, Vector2I v2)
    {
        float deltaX = v2.X - v1.X;
        float deltaY = v2.Y - v1.Y;
        float distance = (float)Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
        return distance;
    }
    #endregion
}
