using Godot;
using System.Collections;

[GlobalClass]
public partial class LadderArea : Touchable
{
    [Export]
    public float Extents;

    [Export]
    public SolidMaterialType SolidMaterialType;

    private Vector3 Center => GlobalPosition;
    private Vector3 Top => Center + Vector3.Up * Extents;
    private Vector3 Bottom => Center + Vector3.Down * Extents;
    private Vector3 PlayerRotationDegrees => GlobalRotationDegrees.Set(x: 0, z: 0);

    private bool _animating;
    private bool _attached;
    private float _time_sfx_step;

    private const float STEP_SFX_DELAY = 0.5f;

    private SolidMaterialInfo _material_info;

    public override void _Ready()
    {
        base._Ready();
        _material_info = SolidMaterialController.Instance.GetInfo(SolidMaterialType);

        HoverIconPosition = new Node3D();
        AddChild(HoverIconPosition);
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
        if (y > Top.Y || y < Bottom.Y)
        {
            EndLadder();
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
        var y_player = Player.Instance.CameraTarget.GlobalPosition.Y;
        var y = Mathf.Clamp(y_player, Bottom.Y, Top.Y);
        HoverIconPosition.GlobalPosition = Center.Set(y: y);
    }

    private void StartLadder()
    {
        if (_attached) return;
        if (_animating) return;

        var id_lock = nameof(LadderArea);
        Player.Instance.MovementLock.AddLock(id_lock);
        Player.Instance.InteractLock.AddLock(id_lock);
        Player.Instance.LookLock.AddLock(id_lock);
        Player.Instance.GravityLock.AddLock(id_lock);
        Player.Instance.PlayerCollisionShape.Disable();
        Cursor.Hide();

        _attached = true;

        SoundController.Instance.Play(_material_info.WalkSound);

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            _animating = true;
            var start_position = Player.Instance.GlobalPosition;
            var end_position = GetPlayerLadderPosition();
            var end_rotation = EulerMath.WrappedEulerAngles(PlayerRotationDegrees);
            var start_rotation = EulerMath.ClosestEulerAngles(EulerMath.WrappedEulerAngles(Player.Instance.NeckHorizontal.GlobalRotationDegrees), end_rotation);
            var curve = Curves.EaseOutQuad;
            yield return LerpEnumerator.Lerp01(0.5f, f =>
            {
                var t = curve.Evaluate(f);
                Player.Instance.GlobalPosition = start_position.Lerp(end_position, t);
                Player.Instance.NeckHorizontal.GlobalRotationDegrees = start_rotation.Lerp(end_rotation, t);
            });
            _animating = false;

            Player.Instance.LookLock.RemoveLock(id_lock);
        }
    }

    private void EndLadder()
    {
        if (!_attached) return;
        if (_animating) return;

        var id_lock = nameof(LadderArea);
        Player.Instance.MovementLock.RemoveLock(id_lock);
        Player.Instance.InteractLock.RemoveLock(id_lock);
        Player.Instance.LookLock.RemoveLock(id_lock);
        Player.Instance.GravityLock.RemoveLock(id_lock);
        Player.Instance.PlayerCollisionShape.Enable();

        _attached = false;
    }

    private Vector3 GetPlayerLadderPosition()
    {
        var player_pos = Player.Instance.GlobalPosition;
        var y = Mathf.Clamp(player_pos.Y, Bottom.Y, Top.Y);
        return Center.Set(y: y);
    }
}
