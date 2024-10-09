using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class BarrelMimicFlowerEnemy : NavEnemy
{
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
    private BasementRoomElement _last_player_room;

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
        var pose_closed = StateMachine.CreateAnimation("Armature|closed_pose", false);
        var pose_open = StateMachine.CreateAnimation("Armature|open_pose", false);
        var closed_to_open = StateMachine.CreateAnimation("Armature|closed_to_open", false);

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
            Text = "Close pose",
            Action = v => { _param_close.Trigger(); v.SetVisible(false); }
        });

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Debug touched",
            Action = v => { _param_touched.Trigger(); v.SetVisible(false); }
        });
    }

    private void Touched()
    {
        SetState(State.Attacking);
    }

    private bool PlayerCanSeeMe()
    {
        return true;
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

        StartCoroutine(enumerator, "state");
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
        Player.StartLookingAt(this, 0.01f);
        AnimateAttackOverlay();
        yield return null;
    }

    private void AnimateAttackOverlay()
    {
        var animation_length = AnimationPlayer.GetAnimation("Armature|closed_to_open").Length;
        var delay = animation_length * 0.4f;
        var duration = animation_length - delay;
        var a_start = 0f;
        var a_end = 0.5f;

        StartCoroutine(Cr, "overlay");
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);

            var time_start = GameTime.Time;
            var time_end = time_start + duration;
            while (GameTime.Time < time_end)
            {
                var t = (GameTime.Time - time_start) / duration;
                var a = Mathf.Lerp(a_start, a_end, t);
                _overlay.Color = _overlay.Color.SetA(a);
                yield return null;
            }
        }
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
            return;
        }

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
        _last_player_room = room;
        Player.GlobalPosition = nav_position;

        StartCoroutine(LerpEnumerator.Lerp01(0.25f, f =>
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
        var room = basement.Grid.Elements
            .Where(x => IsValidRoomElement(x) && x != _current_room && x != _last_player_room && !x.Info.IsStartRoom)
            .OrderBy(x => x.Room.GlobalPosition.DistanceTo(PlayerPosition))
            .FirstOrDefault();
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
