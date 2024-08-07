using Godot;

public partial class NavEnemy : Enemy
{
    [NodeType]
    public NavigationAgent3D Agent;

    [Export]
    public float MoveSpeed = 10;

    [Export]
    public float TurnSpeed = 10;

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
            Agent.Velocity = dir * MoveSpeed * GameTime.DeltaTime;
        }
        else
        {
            OnVelocityComputed(dir * MoveSpeed);
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
        var y = Mathf.LerpAngle(GlobalRotation.Y, Mathf.Atan2(direction.X, direction.Z), TurnSpeed * GameTime.DeltaTime);
        GlobalRotation = GlobalRotation.Set(y: y);
    }

    protected void StopNavigation()
    {
        Agent.TargetPosition = GlobalPosition;
    }
}
