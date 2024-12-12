using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class PuzzleCubeWallHole : Node3DScript
{
    [NodeName]
    public ItemArea AreaHole;

    [NodeName]
    public Node3D OutsidePosition;

    [NodeName]
    public Node3D InsidePosition;

    [NodeName]
    public Touchable EjectTouchable;

    private PuzzleCube _current_cube;

    private List<CubeAlignment> _alignments = new()
    {
        new CubeAlignment {
            Direction = Vector3.Back,
            Type = CubeTypes.Diagonal,
            Rotation = new Vector3(0, 0, 0)
        },

        new CubeAlignment {
            Direction = Vector3.Right,
            Type = CubeTypes.Arrow,
            Rotation = new Vector3(0, -90, 0)
        },

        new CubeAlignment {
            Direction = Vector3.Forward,
            Type = CubeTypes.F,
            Rotation = new Vector3(0, 180, 0)
        },

        new CubeAlignment {
            Direction = Vector3.Left,
            Type = CubeTypes.Roots,
            Rotation = new Vector3(0, 90, 0)
        },

        new CubeAlignment {
            Direction = Vector3.Up,
            Type = CubeTypes.Tree,
            Rotation = new Vector3(0, 90, 90)
        },

        new CubeAlignment {
            Direction = Vector3.Down,
            Type = CubeTypes.Leaf,
            Rotation = new Vector3(0, 90, -90)
        },
    };

    private class CubeAlignment
    {
        public Vector3 Direction { get; set; }
        public Vector3 Rotation { get; set; }
        public CubeTypes Type { get; set; }
    }

    private enum CubeTypes
    {
        Diagonal, // north
        Arrow, // east
        F, // south
        Roots, // west
        Leaf, // top
        Tree, // bottom
    }

    public override void _Ready()
    {
        base._Ready();
        AreaHole.OnItemEntered += AreaHoleEntered;

        EjectTouchable.Disable();
        EjectTouchable.OnTouched += Touched_Eject;
    }

    private void AreaHoleEntered(Item item)
    {
        var cube = item as PuzzleCube;
        if (cube == null) return;

        _current_cube = cube;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            AreaHole.Disable();
            cube.IsBeingHandled = true;
            cube.SetGrabbable(false);
            cube.SetCollisionEnabled(false);
            cube.RigidBody.LockPosition_All();
            cube.RigidBody.LockRotation_All();

            var alignment = GetAlignment(cube);

            var end_rotation = WrappedEulerAngles(alignment.Rotation + GlobalRotationDegrees);
            var start_rotation = ClosestEulerAngles(WrappedEulerAngles(cube.RigidBody.GlobalRotationDegrees), end_rotation);

            Debug.Log($"{start_rotation} > {end_rotation}");

            var start_position = cube.GlobalPosition;
            var curve_out = Curves.EaseOutQuad;
            var curve_in_out = Curves.EaseInOutQuad;

            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                var t = curve_out.Evaluate(f);
                cube.RigidBody.GlobalRotationDegrees = start_rotation.Lerp(end_rotation, t);
                cube.GlobalPosition = start_position.Lerp(OutsidePosition.GlobalPosition, t);
            });

            yield return LerpEnumerator.Lerp01(1, f =>
            {
                var t = curve_in_out.Evaluate(f);
                cube.GlobalPosition = OutsidePosition.GlobalPosition.Lerp(InsidePosition.GlobalPosition, t);
            });

            EjectTouchable.Enable();
        }
    }

    private Vector3 WrappedEulerAngles(Vector3 v)
    {
        return new Vector3(
            WrappedEulerAngle(v.X),
            WrappedEulerAngle(v.Y),
            WrappedEulerAngle(v.Z));
    }

    private float WrappedEulerAngle(float v)
    {
        v %= 360;
        v += v < -180 ? 360 : v > 180 ? -360 : 0;
        return v;
    }

    private Vector3 ClosestEulerAngles(Vector3 from, Vector3 to)
    {
        return new Vector3(
            ClosestEulerAngle(from.X, to.X),
            ClosestEulerAngle(from.Y, to.Y),
            ClosestEulerAngle(from.Z, to.Z)
            );
    }

    private float ClosestEulerAngle(float from, float to)
    {
        var list = new List<float> { from, from - 360, from + 360 };
        var closest = list
            .OrderBy(x => Mathf.Abs(x - to))
            .First();
        return closest;
    }

    private CubeAlignment GetAlignment(PuzzleCube cube)
    {
        var wall_normal = GlobalBasis * Vector3.Back;
        var closest = _alignments
            .OrderByDescending(x => (cube.RigidBody.GlobalBasis * x.Direction).Dot(wall_normal))
            .First();
        return closest;
    }

    private void Touched_Eject()
    {
        EjectTouchable.Disable();

        if (_current_cube == null) return;

        var cube = _current_cube;
        _current_cube = null;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var curve = Curves.EaseOutQuad;
            yield return LerpEnumerator.Lerp01(1f, f =>
            {
                var t = curve.Evaluate(f);
                cube.GlobalPosition = InsidePosition.GlobalPosition.Lerp(OutsidePosition.GlobalPosition, t);
            });

            cube.IsBeingHandled = false;
            cube.SetGrabbable(true);
            cube.SetCollisionEnabled(true);
            cube.RigidBody.UnlockPosition_All();
            cube.RigidBody.UnlockRotation_All();

            yield return new WaitForSeconds(1);

            AreaHole.Enable();
        }
    }
}
