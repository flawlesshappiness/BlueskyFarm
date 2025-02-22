using Godot;
using System;
using System.Collections;
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
        if (cube.IsBeingHandled) return;
        cube.IsBeingHandled = true;

        var alignment = GetAlignment(cube);

        _current_alignment = alignment;
        _current_cube = cube;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            AreaHole.Disable();
            cube.SetGrabbable(false);
            cube.SetCollisionEnabled(false);
            cube.Freeze = true;

            var end_rotation = EulerMath.WrappedEulerAngles(alignment.Rotation + GlobalRotationDegrees);
            var start_rotation = EulerMath.ClosestEulerAngles(EulerMath.WrappedEulerAngles(cube.GlobalRotationDegrees), end_rotation);
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
            cube.Freeze = false;

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
