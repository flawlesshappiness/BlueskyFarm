using Godot;
using System.Collections;

[GlobalClass]
public partial class LadderArea : Touchable
{
    [Export]
    public float Extents;

    [Export]
    public SolidMaterialType SolidMaterialType;

    [Export]
    public Node3D TopExitNode;

    [Export]
    public Node3D BottomExitNode;

    private Vector3 Center => GlobalPosition;
    private Vector3 Top => Center + Vector3.Up * Extents;
    private Vector3 Bottom => Center + Vector3.Down * Extents;
    private Vector3 PlayerRotationDegrees => GlobalRotationDegrees.Set(x: 0, z: 0);

    private bool _animating;
    private bool _attached;
    private float _time_detach;
    private float _time_sfx_step;

    private const float STEP_SFX_DELAY = 0.5f;

    private Node3D _enter_node;
    private SolidMaterialInfo _material_info;

    public override void _Ready()
    {
        base._Ready();
        _material_info = SolidMaterialController.Instance.GetInfo(SolidMaterialType);

        HoverIconPosition = new Node3D();
        AddChild(HoverIconPosition);

        _enter_node = new Node3D();
        AddChild(_enter_node);
    }

    protected override void Touched()
    {
        base.Touched();
        StartLadder();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (PlayerInput.Interact.Pressed || PlayerInput.Jump.Pressed)
        {
            EndLadder();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Process_Moving();
        Process_StepSound();
        Process_HoverNode();
    }

    private void Process_Moving()
    {
        if (!_attached) return;
        if (_animating) return;

        var up_held = PlayerInput.Forward.Held;
        var down_held = PlayerInput.Back.Held;
        var move_dir = up_held ? Vector3.Up : down_held ? Vector3.Down : Vector3.Zero;
        var velocity = move_dir * Player.Instance.WalkSpeed * 0.5f;

        Player.Instance.Velocity = velocity;

        var y = Player.Instance.GlobalPosition.Y;
        var at_top = y > Top.Y;
        var at_bottom = y < Bottom.Y;
        if (at_bottom || at_top)
        {
            var exit_node = at_top ? TopExitNode : at_bottom ? BottomExitNode : null;
            EndLadder(exit_node);
        }
    }

    private void Process_StepSound()
    {
        var moving = PlayerInput.Forward.Held || PlayerInput.Back.Held;
        if (!moving)
        {
            _time_sfx_step = GameTime.Time + STEP_SFX_DELAY;
            return;
        }

        if (!_attached) return;
        if (_animating) return;
        if (GameTime.Time < _time_sfx_step) return;
        _time_sfx_step = GameTime.Time + STEP_SFX_DELAY;

        SoundController.Instance.Play(_material_info.WalkSound);
    }

    private void Process_HoverNode()
    {
        var target = Player.Instance.CameraTarget;
        var position = target.GlobalPosition + target.GlobalBasis * Vector3.Forward;
        HoverIconPosition.GlobalPosition = position;
    }

    private void StartLadder()
    {
        if (_attached) return;
        if (_animating) return;
        if (GameTime.Time < _time_detach) return;

        var id_lock = nameof(LadderArea);
        Player.Instance.MovementLock.AddLock(id_lock);
        Player.Instance.InteractLock.AddLock(id_lock);
        Player.Instance.LookLock.AddLock(id_lock);
        Player.Instance.GravityLock.AddLock(id_lock);
        Player.Instance.PlayerCollisionShape.Disable();
        Cursor.Hide();

        _attached = true;
        _animating = true;

        SoundController.Instance.Play(_material_info.WalkSound);

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            _enter_node.GlobalPosition = GetPlayerLadderPosition();
            _enter_node.GlobalRotationDegrees = EulerMath.WrappedEulerAngles(PlayerRotationDegrees);
            yield return AnimateToNode(_enter_node, 0.5f);
            _animating = false;

            Player.Instance.PlayerCollisionShape.Enable();
            Player.Instance.LookLock.RemoveLock(id_lock);
        }
    }

    private void EndLadder(Node3D exit_node = null)
    {
        if (!_attached) return;
        if (_animating) return;

        var id_lock = nameof(LadderArea);
        Player.Instance.LookLock.AddLock(id_lock);
        _animating = true;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            if (IsInstanceValid(exit_node))
            {
                yield return AnimateToNode(exit_node, 1.0f);
                Player.Instance.Velocity = Vector3.Zero;
            }

            Player.Instance.MovementLock.RemoveLock(id_lock);
            Player.Instance.InteractLock.RemoveLock(id_lock);
            Player.Instance.LookLock.RemoveLock(id_lock);
            Player.Instance.GravityLock.RemoveLock(id_lock);

            _animating = false;
            _attached = false;
            _time_detach = GameTime.Time + 0.2f;
        }
    }

    private Coroutine AnimateToNode(Node3D target, float duration)
    {
        _animating = true;
        return Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            var start_position = Player.Instance.GlobalPosition;
            var end_position = target.GlobalPosition;
            var end_rotation = EulerMath.WrappedEulerAngles(target.GlobalRotationDegrees);
            var start_rotation = EulerMath.ClosestEulerAngles(EulerMath.WrappedEulerAngles(Player.Instance.NeckHorizontal.GlobalRotationDegrees), end_rotation);
            var curve = Curves.EaseOutQuad;
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var t = curve.Evaluate(f);
                Player.Instance.GlobalPosition = start_position.Lerp(end_position, t);
                Player.Instance.NeckHorizontal.GlobalRotationDegrees = start_rotation.Lerp(end_rotation, t);
            });

            _animating = false;
        }
    }

    private Vector3 GetPlayerLadderPosition()
    {
        var player_pos = Player.Instance.GlobalPosition;
        var y = Mathf.Clamp(player_pos.Y, Bottom.Y + 0.1f, Top.Y - 0.1f);
        return Center.Set(y: y);
    }
}
