using Godot;
using System.Collections.Generic;

public partial class SignDirection : Node3D
{
    [Export]
    public SignDirectionBoard BoardTemplate;

    [Export]
    public float MaxHeight;

    [Export]
    public float Spacing;

    private List<SignDirectionBoard> _boards = new();

    public override void _Ready()
    {
        base._Ready();
        BoardTemplate.Disable();
    }

    public void CreateDirection(string text, Vector3 target)
    {
        var board = BoardTemplate.Duplicate() as SignDirectionBoard;
        board.SetParent(this);
        board.Enable();
        board.Text = text;
        board.GlobalPosition = GlobalPosition.Add(y: GetBoardHeight(_boards.Count));
        var direction = GlobalPosition.Set(y: 0).DirectionTo(target.Set(y: 0)).Normalized();
        board.GlobalRotation = RotationTowards(direction);
        _boards.Add(board);
    }

    private float GetBoardHeight(int i)
    {
        return MaxHeight - Spacing * i;
    }

    private Vector3 RotationTowards(Vector3 target)
    {
        var angle = Vector3.Forward.AngleTo(target);
        return new Vector3(0, angle, 0);
    }
}
