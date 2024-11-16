using Godot;

public static class RigidBodyExtensions
{
    public static void LockPosition(this RigidBody3D rigidbody, bool x = false, bool y = false, bool z = false)
    {
        rigidbody.AxisLockLinearX = x;
        rigidbody.AxisLockLinearY = y;
        rigidbody.AxisLockLinearZ = z;
    }

    public static void LockPosition_All(this RigidBody3D rigidbody) => rigidbody.LockPosition(x: true, y: true, z: true);
    public static void UnlockPosition_All(this RigidBody3D rigidbody) => rigidbody.LockPosition();

    public static void LockRotation(this RigidBody3D rigidbody, bool x = false, bool y = false, bool z = false)
    {
        rigidbody.AxisLockAngularX = x;
        rigidbody.AxisLockAngularY = y;
        rigidbody.AxisLockAngularZ = z;
    }

    public static void LockRotation_All(this RigidBody3D rigidbody) => rigidbody.LockRotation(x: true, y: true, z: true);
    public static void UnlockRotation_All(this RigidBody3D rigidbody) => rigidbody.LockRotation();
}
