using Godot;
using System;
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

    public PuzzleCube.Type? SelectedType => _current_alignment?.Type;
    public PuzzleCube.Color? SelectedColor => _current_cube?.Display.CurrentColor;

    public event Action OnCubeChanged;
    public event Action OnCubeEntered;
    public event Action OnCubeExited;

    private PuzzleCube _current_cube;
    private PuzzleCube.Alignment _current_alignment;

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

        var alignment = GetAlignment(cube);

        _current_alignment = alignment;
        _current_cube = cube;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            AreaHole.Disable();
            cube.IsBeingHandled = true;
            cube.SetGrabbable(false);
            cube.SetCollisionEnabled(false);
            cube.LockPosition_All();
            cube.LockRotation_All();

            var end_rotation = WrappedEulerAngles(alignment.Rotation + GlobalRotationDegrees);
            var start_rotation = ClosestEulerAngles(WrappedEulerAngles(cube.GlobalRotationDegrees), end_rotation);
            var start_position = cube.GlobalPosition;
            var curve_out = Curves.EaseOutQuad;
            var curve_in_out = Curves.EaseInOutQuad;

            SoundController.Instance.Play("sfx_stone_impact", GlobalPosition);

            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                var t = curve_out.Evaluate(f);
                cube.GlobalRotationDegrees = start_rotation.Lerp(end_rotation, t);
                cube.GlobalPosition = start_position.Lerp(OutsidePosition.GlobalPosition, t);
            });

            var sfx_drag = SoundController.Instance.Play("sfx_stone_drag_long", GlobalPosition);

            yield return LerpEnumerator.Lerp01(1, f =>
            {
                var t = curve_in_out.Evaluate(f);
                cube.GlobalPosition = OutsidePosition.GlobalPosition.Lerp(InsidePosition.GlobalPosition, t);
            });

            sfx_drag.FadeOut(0.5f);

            EjectTouchable.Enable();

            OnCubeEntered?.Invoke();
            OnCubeChanged?.Invoke();
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

    private PuzzleCube.Alignment GetAlignment(PuzzleCube cube)
    {
        var wall_normal = GlobalBasis * Vector3.Back;
        var closest = PuzzleCube.Alignments
            .OrderByDescending(x => (cube.GlobalBasis * x.Direction).Dot(wall_normal))
            .First();
        return closest;
    }

    private void Touched_Eject()
    {
        EjectTouchable.Disable();

        if (_current_cube == null) return;

        var cube = _current_cube;
        _current_cube = null;
        _current_alignment = null;

        OnCubeExited?.Invoke();
        OnCubeChanged?.Invoke();

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var sfx_drag = SoundController.Instance.Play("sfx_stone_drag_long", GlobalPosition);

            var curve = Curves.EaseOutQuad;
            yield return LerpEnumerator.Lerp01(1f, f =>
            {
                var t = curve.Evaluate(f);
                cube.GlobalPosition = InsidePosition.GlobalPosition.Lerp(OutsidePosition.GlobalPosition, t);
            });

            sfx_drag.FadeOut(0.5f);

            cube.IsBeingHandled = false;
            cube.SetGrabbable(true);
            cube.SetCollisionEnabled(true);
            cube.UnlockPosition_All();
            cube.UnlockRotation_All();

            yield return new WaitForSeconds(1);

            AreaHole.Enable();
        }
    }

    public void SetCubeColorDisabled()
    {
        if (_current_cube == null) return;

        _current_cube.Display.SetColor(PuzzleCube.Color.Disabled);
        _current_cube.Light.Visible = false;
    }
}
