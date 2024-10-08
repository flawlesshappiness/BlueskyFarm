using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Linq;

public partial class BarrelMimicFlowerEnemy : NavEnemy
{
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

    public override void _Ready()
    {
        base._Ready();
        InitializeAnimations();
        InitializeTouchable();
        InitializePosition();
        RegisterDebugActions();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Debug.Log(DistanceToPlayer);
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
        return false;
    }

    private void SetState(State state)
    {
        _state = state;
        Debug.LogMethod(state);

        var enumerator = state switch
        {
            State.Waiting => StateCr_Waiting(),
            State.Attacking => StateCr_Attacking(),
            State.Teleporting => StateCr_Teleporting(),
            _ => null
        };

        StartCoroutine(enumerator, "state");
    }

    IEnumerator StateCr_Waiting()
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

    IEnumerator StateCr_Attacking()
    {
        _param_touched.Trigger();
        yield return null;
    }

    public void CallMethod_AttackFinished()
    {
        TeleportPlayer();
        SetState(State.Teleporting);
    }

    private void TeleportPlayer()
    {
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
    }

    private bool IsValidRoomElement(BasementRoomElement element)
    {
        var valid_area = element.AreaName == AreaNames.Basement;
        return valid_area;
    }

    IEnumerator StateCr_Teleporting()
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
        var nav_position = NavigationServer3D.MapGetClosestPoint(Agent.GetNavigationMap(), position) - new Vector3(0, Agent.PathHeightOffset, 0); ;

        // CHECK FOR WALLS

        GlobalPosition = nav_position;
        _current_room = room;

        SetState(State.Waiting);

        yield return null;
    }
}
