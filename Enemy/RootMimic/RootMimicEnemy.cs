using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RootMimicEnemy : Enemy
{
    [NodeType]
    public NavigationAgent3D Agent;

    [NodeName]
    public SkeletonIK3D ik_leg1_R;

    [NodeName]
    public SkeletonIK3D ik_leg2_R;

    [NodeName]
    public SkeletonIK3D ik_leg3_R;

    [NodeName]
    public SkeletonIK3D ik_leg1_L;

    [NodeName]
    public SkeletonIK3D ik_leg2_L;

    [NodeName]
    public SkeletonIK3D ik_leg3_L;

    [NodeName]
    public Node3D Leg1_R;

    [NodeName]
    public Node3D Leg2_R;

    [NodeName]
    public Node3D Leg3_R;

    [NodeName]
    public Node3D Leg1_L;

    [NodeName]
    public Node3D Leg2_L;

    [NodeName]
    public Node3D Leg3_L;

    public List<SkeletonIK3D> IK_Legs => _ik_legs ?? (_ik_legs = new List<SkeletonIK3D> { ik_leg1_R, ik_leg2_R, ik_leg3_R, ik_leg1_L, ik_leg2_L, ik_leg3_L });
    private List<SkeletonIK3D> _ik_legs;

    public List<Node3D> Target_Legs => _target_legs ?? (_target_legs = new List<Node3D> { Leg1_R, Leg2_R, Leg3_R, Leg1_L, Leg2_L, Leg3_L });
    private List<Node3D> _target_legs;

    private List<Limb> _limbs = new();

    private float move_speed = 4;

    private class Limb
    {
        public SkeletonIK3D IK { get; }
        public Node3D Target { get; }
        public Vector3 LocalPosition { get; }
        public Vector3 LocalRotation { get; }
        public Vector3 TargetPosition { get; set; }
        public float MaxDistance { get; set; }
        public float TargetSpeed { get; set; }

        private float _init_max_distance;
        private RandomNumberGenerator _rng = new RandomNumberGenerator();

        public Limb(SkeletonIK3D ik, Node3D target)
        {
            IK = ik;
            Target = target;
            LocalPosition = Target.Position;
            LocalRotation = Target.Rotation;
            MaxDistance = 1f;
            TargetSpeed = 30f;

            _init_max_distance = MaxDistance;
        }

        public void UpdateTargetPosition(float delta)
        {
            Target.GlobalPosition = Target.GlobalPosition.Lerp(TargetPosition, TargetSpeed * delta);
        }

        public void RandomizeMaxDistance()
        {
            var range = 0.1f;
            MaxDistance = _rng.RandfRange(_init_max_distance - range, _init_max_distance + range);
        }
    }

    public override void _Ready()
    {
        base._Ready();

        RegisterDebugActions();
        InitializeLimbs();

        Spawn();
    }

    private void RegisterDebugActions()
    {
        var category = "Enemy (RoomMimic)";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Teleport to",
            Action = _ => Player.GlobalPosition = GlobalPosition

        });
    }

    private void InitializeLimbs()
    {
        _limbs = new List<Limb>
        {
            new Limb(ik_leg1_L, Leg1_L),
            new Limb(ik_leg2_L, Leg2_L),
            new Limb(ik_leg3_L, Leg3_L),
            new Limb(ik_leg1_R, Leg1_R),
            new Limb(ik_leg2_R, Leg2_R),
            new Limb(ik_leg3_R, Leg3_R),
        };

        foreach (var limb in _limbs)
        {
            limb.IK.Start();
            limb.Target.SetParent(GetParent());
        }
    }

    private void Spawn()
    {
        var basement = BasementController.Instance.CurrentBasement;
        var furthest_element = basement.Grid.Elements
            .OrderByDescending(x => PlayerPosition.DistanceTo(x.Room.GlobalPosition))
            .FirstOrDefault();

        Agent.TargetPosition = furthest_element.Room.GlobalPosition;
        GlobalPosition = Agent.GetFinalPosition();
    }

    private void TurnTowardsDirection(Vector3 direction, float delta)
    {
        var y = Mathf.LerpAngle(GlobalRotation.Y, Mathf.Atan2(direction.X, direction.Z), 10 * delta);
        GlobalRotation = GlobalRotation.Set(y: y);
    }

    private void SelectTargetPosition()
    {
        var element = BasementController.Instance.CurrentBasement.Grid.Elements.ToList().Random();
        Agent.TargetPosition = element.Room.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var fdelta = Convert.ToSingle(delta);
        Process_MoveTowardsTarget(fdelta);
        Process_Limbs(fdelta);
    }

    private void Process_MoveTowardsTarget(float delta)
    {
        if (Agent.IsNavigationFinished())
        {
            SelectTargetPosition();
            return;
        }

        var next_position = Agent.GetNextPathPosition();
        var dir = GlobalPosition.DirectionTo(next_position);

        if (Agent.AvoidanceEnabled)
        {
            Agent.Velocity = dir * move_speed * delta;
        }
        else
        {
            VelocityComputed(dir * move_speed, delta);
        }
    }

    private void Process_Limbs(float delta)
    {
        foreach (var limb in _limbs)
        {
            var new_position = GlobalPosition + GlobalBasis * limb.LocalPosition;
            var distance = limb.Target.GlobalPosition.DistanceTo(new_position);

            if (distance > limb.MaxDistance)
            {
                limb.TargetPosition = new_position + (GlobalBasis * Vector3.Back * limb.MaxDistance * 0.5f);
                limb.Target.GlobalRotation = GlobalRotation + GlobalBasis * limb.LocalRotation;
                limb.RandomizeMaxDistance();
            }

            limb.UpdateTargetPosition(delta);
        }
    }

    private void VelocityComputed(Vector3 v, float delta)
    {
        GlobalPosition += v.Set(y: 0) * delta;
        TurnTowardsDirection(v, delta);
    }
}
