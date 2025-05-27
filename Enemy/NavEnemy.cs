using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class NavEnemy : Enemy
{
    [NodeType]
    public NavigationAgent3D Agent;

    [Export]
    public float MoveSpeed = 10;

    [Export]
    public float TurnSpeed = 10;

    protected float CurrentSpeed { get; set; }
    protected bool IsMoving => !Agent.IsNavigationFinished();
    protected float SignedAngleToPlayer => Mathf.RadToDeg(GlobalBasis.Z.SignedAngleTo(DirectionToPlayer, Vector3.Up));
    protected float AngleToPlayer => Mathf.RadToDeg(GlobalBasis.Z.AngleTo(DirectionToPlayer));

    protected const string StateDebugIdle = "DebugIdle";
    protected const string StateDebugFollow = "DebugFollow";

    private bool _facing_player;

    protected override void RegisterDebugActions()
    {
        base.RegisterDebugActions();

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyCategory,
            Text = "Set state: DebugIdle",
            Action = _ => SetState(StateDebugIdle)
        });

        Debug.RegisterAction(new DebugAction
        {
            Id = EnemyId,
            Category = EnemyCategory,
            Text = "Set state: DebugFollow",
            Action = _ => SetState(StateDebugFollow)
        });
    }

    public override void Spawn(bool debug)
    {
        base.Spawn(debug);
        CurrentSpeed = MoveSpeed;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Process_MoveTowardsTarget();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_FacePlayer();
    }

    protected override void RegisterStates()
    {
        base.RegisterStates();
        RegisterState(StateDebugIdle, StateCr_Debug_Idle);
        RegisterState(StateDebugFollow, StateCr_Debug_Follow);
    }

    private IEnumerator StateCr_Debug_Idle()
    {
        while (true)
        {
            if (!Agent.IsNavigationFinished())
            {
                Agent.TargetPosition = GlobalPosition;
            }

            yield return null;
        }
    }

    private IEnumerator StateCr_Debug_Follow()
    {
        while (true)
        {
            if (DistanceToPlayer < 2)
            {
                Agent.TargetPosition = GlobalPosition;
            }
            else if (DistanceToPlayer > 3)
            {
                Agent.TargetPosition = Player.GlobalPosition;
            }

            yield return null;
        }
    }

    private void Process_MoveTowardsTarget()
    {
        if (Agent.IsNavigationFinished())
        {
            OnNavigationFinished();
            return;
        }

        var next_position = Agent.GetNextPathPosition();
        var dir = GlobalPosition.DirectionTo(next_position);

        if (Agent.AvoidanceEnabled)
        {
            Agent.Velocity = dir * CurrentSpeed * GameTime.DeltaTime;
        }
        else
        {
            OnVelocityComputed(dir * CurrentSpeed);
        }
    }

    protected virtual void OnNavigationFinished()
    {

    }

    protected virtual void OnVelocityComputed(Vector3 v)
    {
        MoveAndTurn(v);
    }

    protected void Move(Vector3 v)
    {
        GlobalPosition += v * GameTime.DeltaTime;
    }

    protected void MoveAndTurn(Vector3 v)
    {
        Move(v);
        TurnTowardsDirection(v);
    }

    protected void TurnTowardsDirection(Vector3 direction)
    {
        TurnTowardsDirection(direction, TurnSpeed);
    }

    protected void TurnTowardsDirection(Vector3 direction, float turn_speed)
    {
        var y = Mathf.LerpAngle(GlobalRotation.Y, Mathf.Atan2(direction.X, direction.Z), turn_speed * GameTime.DeltaTime);
        GlobalRotation = GlobalRotation.Set(y: y);
    }

    protected void StopNavigation()
    {
        Agent.TargetPosition = GlobalPosition;
    }

    public BasementRoomElement GetClosestRoomElementToPlayer(Func<BasementRoomElement, bool> validate = null)
    {
        return GetClosestRoomElementsToPlayer(validate)
            .FirstOrDefault();
    }

    public IEnumerable<BasementRoomElement> GetClosestRoomElementsToPlayer(Func<BasementRoomElement, bool> validate = null)
    {
        return GetRooms(validate)
            .OrderBy(x => PlayerPosition.DistanceTo(x.Room.GlobalPosition));
    }

    public BasementRoomElement GetFurthestRoomElementToPlayer(Func<BasementRoomElement, bool> validate = null)
    {
        return GetRooms(validate)
            .OrderByDescending(x => PlayerPosition.DistanceTo(x.Room.GlobalPosition))
            .Take(5)
            .ToList()
            .Random();
    }

    public IEnumerable<BasementRoomElement> GetClosestRoomElements(Func<BasementRoomElement, bool> validate = null)
    {
        return GetRooms(validate)
            .OrderBy(x => GlobalPosition.DistanceTo(x.Room.GlobalPosition));
    }

    public BasementRoomElement GetClosestRoomElement(Func<BasementRoomElement, bool> validate = null)
    {
        return GetClosestRoomElements(validate)
            .FirstOrDefault();
    }

    public IEnumerable<BasementRoomElement> GetConnnectedNeighbours(BasementRoomElement target)
    {
        return target.Connections.Where(x => x.AreaName == target.AreaName && !x.IsStart);
    }

    public Vector3 GetRandomPositionInRoom(BasementRoom room)
    {
        var rng = new RandomNumberGenerator();
        var room_size = BasementRoom.ROOM_SIZE;
        var min = -0.4f;
        var max = 0.4f;
        var x = rng.RandfRange(min, max);
        var z = rng.RandfRange(min, max);
        return room.GlobalPosition + new Vector3(x, 0, z) * room_size;
    }

    protected void StartFacingPlayer() => _facing_player = true;
    protected void StopFacingPlayer() => _facing_player = false;
    private void Process_FacePlayer()
    {
        if (_facing_player)
        {
            var dir = ScreenEffects.View.Camera.GlobalPosition.Set(y: 0) - GlobalPosition.Set(y: 0);
            TurnTowardsDirection(dir);
        }
    }
}
