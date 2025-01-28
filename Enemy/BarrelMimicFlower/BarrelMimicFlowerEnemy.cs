using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class BarrelMimicFlowerEnemy : NavEnemy
{
    [Export]
    public string animation_closed;

    [Export]
    public string animation_open;

    [Export]
    public string animation_closed_to_open;

    [Export]
    public Color OverlayColor;

    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public AnimationStateMachine StateMachine;

    [NodeType]
    public Touchable Touchable;

    private TriggerParameter _param_close;
    private TriggerParameter _param_touched;

    private enum State { Waiting, Attacking, Teleporting }
    private State _state;

    private BasementRoomElement _current_room;

    private ColorRect _overlay;

    public override void _Ready()
    {
        base._Ready();
        InitializeAnimations();
        InitializeTouchable();
        InitializePosition();
        RegisterDebugActions();

        _overlay = GameView.Instance.CreateOverlay(OverlayColor.SetA(0));
    }

    public override void _ExitTree()
    {
        _overlay.QueueFree();
        base._ExitTree();
    }

    private void InitializeAnimations()
    {
        var pose_closed = StateMachine.CreateAnimation(animation_closed, false);
        var pose_open = StateMachine.CreateAnimation(animation_open, false);
        var closed_to_open = StateMachine.CreateAnimation(animation_closed_to_open, false);

        _param_close = StateMachine.CreateParameter("close");
        _param_touched = StateMachine.CreateParameter("touched");

        StateMachine.Connect(pose_open.Node, pose_closed.Node, _param_close.WhenTriggered());
        StateMachine.Connect(pose_closed.Node, closed_to_open.Node, _param_touched.WhenTriggered());
        StateMachine.Connect(closed_to_open.Node, pose_open.Node);

        StateMachine.Initialize(AnimationPlayer);
        StateMachine.Start(pose_closed.Node);
    }

    private void InitializeTouchable()
    {
        Touchable.OnTouched += Touched;
        Touchable.SetEnabled(false);
    }

    private void InitializePosition()
    {
        if (BasementController.Instance.CurrentBasement == null) return;

        GlobalPosition = new Vector3(0, -20, 0);
        SetState(State.Teleporting);
    }

    private void RegisterDebugActions()
    {
        var category = $"BarrelMimicFlower ({GetInstanceId()})";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Teleport player",
            Action = v => { Player.GlobalPosition = GlobalPosition; v.Close(); }
        });
    }

    private void Touched()
    {
        SetState(State.Attacking);
    }

    private bool PlayerCanSeeMe()
    {
        return GetLookAtT() > 0.5f;
    }

    private void SetState(State state)
    {
        if (BasementController.Instance.CurrentBasement == null) return;

        _state = state;

        var enumerator = state switch
        {
            State.Waiting => StateCr_Waiting(),
            State.Attacking => StateCr_Attacking(),
            State.Teleporting => StateCr_Teleporting(),
            _ => null
        };

        this.StartCoroutine(enumerator, "state");
    }

    private IEnumerator StateCr_Waiting()
    {
        _param_close.Trigger();
        Touchable.SetEnabled(true);

        var max_dist = BasementRoom.ROOM_SIZE * 2;
        while (true)
        {
            if (DistanceToPlayer > max_dist)
            {
                SetState(State.Teleporting);
            }

            yield return null;
        }
    }

    private IEnumerator StateCr_Attacking()
    {
        _param_touched.Trigger();
        Touchable.SetEnabled(false);
        AnimateAttackOverlay();
        yield return null;
    }

    private void AnimateAttackOverlay()
    {
        var animation_length = AnimationPlayer.GetAnimation(animation_closed_to_open).Length;
        var delay = animation_length * 0.4f;
        var duration = animation_length - delay;
        var a_start = 0f;
        var a_end = 0.5f;

        this.StartCoroutine(Cr, "overlay");
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);

            var time_start = GameTime.Time;
            var time_end = time_start + duration;
            var curve = Curves.EaseInQuad;
            while (GameTime.Time < time_end)
            {
                var t_look_at = GetLookAtT();
                var t_time = (GameTime.Time - time_start) / duration;
                var t = curve.Evaluate(t_time * t_look_at);
                var a = Mathf.Lerp(a_start, a_end, t);
                _overlay.Color = _overlay.Color.SetA(a);
                yield return null;
            }
        }
    }

    private float GetLookAtT()
    {
        var hor = Mathf.Abs(Player.GetHorizontalAngleToPoint(GlobalPosition));
        var ver = Mathf.Abs(Player.GetVerticalAngleToPoint(GlobalPosition));
        var t_hor = (180f - hor) / 180f;
        var t_ver = (180f - ver) / 180f;
        return t_hor * t_ver;
    }

    public void CallMethod_AttackFinished()
    {
        Player.StopLookingAt(this);
        TeleportPlayer();
        SetState(State.Teleporting);
    }

    private void TeleportPlayer()
    {
        if (!PlayerCanSeeMe())
        {
            FadeOutOverlayFromCurrent();

            SoundController.Instance.Play("sfx_barrel_mimic_flower_teleport_self", GlobalPosition);
            return;
        }

        SoundController.Instance.Play("sfx_barrel_mimic_flower_teleport_player");

        _overlay.Color = _overlay.Color.SetA(1);

        var basement = BasementController.Instance.CurrentBasement;
        var room = basement.Grid.Elements
            .Where(x => IsValidRoomElement(x) && x != _current_room && !x.Info.IsStartRoom)
            .ToList().Random();
        var rnd = new RandomNumberGenerator();
        var d = 8;
        var x = rnd.RandfRange(-d, d);
        var z = rnd.RandfRange(-d, d);
        var position = room.Room.GlobalPosition + new Vector3(x, 0, z);
        var nav_position = NavigationServer3D.MapGetClosestPoint(Player.Agent.GetNavigationMap(), position);
        Player.GlobalPosition = nav_position;

        FadeOutOverlayFromFull();
    }

    private void FadeOutOverlayFromCurrent()
    {
        var a_start = _overlay.Color.A;
        this.StartCoroutine(LerpEnumerator.Lerp01(0.25f, f =>
        {
            var a = Mathf.Lerp(a_start, 0f, f);
            _overlay.Color = _overlay.Color.SetA(a);
        }), "overlay");
    }

    private void FadeOutOverlayFromFull()
    {
        this.StartCoroutine(LerpEnumerator.Lerp01(0.25f, f =>
        {
            var a = Mathf.Lerp(1f, 0f, f);
            _overlay.Color = _overlay.Color.SetA(a);
        }), "overlay");
    }

    private bool IsValidRoomElement(BasementRoomElement element)
    {
        var valid_area = element.AreaName == AreaNames.Basement;
        return valid_area;
    }

    private IEnumerator StateCr_Teleporting()
    {
        var basement = BasementController.Instance.CurrentBasement;
        var player_room = GetClosestRoomElementToPlayer();
        var room = GetClosestRoomElementsToPlayer(x => x != player_room).FirstOrDefault();
        var rnd = new RandomNumberGenerator();
        var d = 8;
        var x = rnd.RandfRange(-d, d);
        var z = rnd.RandfRange(-d, d);
        var position = room.Room.GlobalPosition + new Vector3(x, 0, z);
        var nav_position = NavigationServer3D.MapGetClosestPoint(Agent.GetNavigationMap(), position) - new Vector3(0, Agent.PathHeightOffset, 0);

        GlobalPosition = nav_position;
        _current_room = room;

        MoveAwayFromWalls();
        SetState(State.Waiting);

        yield return null;
    }

    private void MoveAwayFromWalls()
    {
        var dir_right = GetDirectionAwayFromWall(Vector3.Right);
        var dir_left = GetDirectionAwayFromWall(Vector3.Left);
        var dir_back = GetDirectionAwayFromWall(Vector3.Back);
        var dir_forward = GetDirectionAwayFromWall(Vector3.Forward);

        var dir_x = dir_right + dir_left;
        var dir_z = dir_forward + dir_back;
        GlobalPosition += dir_x + dir_z;
    }

    private Vector3 GetDirectionAwayFromWall(Vector3 direction)
    {
        var dist = 3f;
        var start = GlobalPosition.Add(y: 0.5f);
        var end = start + direction.Normalized() * dist;
        if (this.TryRaycast(start, end, CollisionMaskHelper.Create(1), out var result))
        {
            var move_dir = -direction * (dist - (start - result.Position).Length());
            return move_dir;
        }

        return Vector3.Zero;
    }
}
