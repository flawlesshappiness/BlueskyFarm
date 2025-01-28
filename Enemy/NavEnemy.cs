using Godot;
using System;
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

    public bool Spawned { get; private set; }
    protected float CurrentSpeed { get; set; }

    protected override void Initialize()
    {
        base.Initialize();
        Spawn(IsDebug);
    }

    protected virtual void Spawn(bool debug)
    {
        Spawned = true;
        CurrentSpeed = MoveSpeed;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Process_MoveTowardsTarget();
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
        Move(v);
    }

    protected void Move(Vector3 v)
    {
        GlobalPosition += v * GameTime.DeltaTime;
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
            .FirstOrDefault();
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
}
