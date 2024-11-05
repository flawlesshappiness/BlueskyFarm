using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WaterArea : Area3D
{
    [NodeType]
    public CollisionShape3D Shape;

    private List<WaterBody> _bodies = new();
    private float _y_height;

    private const float DRAG = 0.05f;

    private class WaterBody
    {
        public GodotObject GO { get; set; }
        public RigidBody3D Body { get; set; }
        public float TimeLastParticle { get; set; }
        public bool FinalParticle { get; set; }
    }

    public override void _Ready()
    {
        NodeScript.FindNodesFromAttribute(this, GetType());
        base._Ready();

        BodyEntered += v => CallDeferred(nameof(OnBodyEntered), v);
        BodyExited += v => CallDeferred(nameof(OnBodyExited), v);

        InitializeHeight();
    }

    private void InitializeHeight()
    {
        if (Shape.Shape is BoxShape3D box && box != null)
        {
            var size = box.Size;
            var y = Shape.GlobalPosition.Y + size.Y * 0.5f;
            _y_height = y;
        }
        else
        {
            Debug.LogError("WaterArea CollisionShape3D is not a Box");
        }
    }

    private void OnBodyEntered(GodotObject go)
    {
        if (_bodies.Any(x => x.GO == go)) return;
        if (go is RigidBody3D body && body != null)
        {
            var wb = new WaterBody
            {
                GO = go,
                Body = body,
            };

            _bodies.Add(wb);
            SpawnWaterRippleParticle(wb);
        }
    }

    private void OnBodyExited(GodotObject go)
    {
        var wb = _bodies.FirstOrDefault(x => x.GO == go);
        if (wb != null)
        {
            _bodies.Remove(wb);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var fdelta = Convert.ToSingle(delta);
        foreach (var wb in _bodies)
        {
            FloatBody(wb.Body, fdelta);
            SpawnBodyParticleOverTime(wb);
        }
    }

    private void FloatBody(RigidBody3D body, float delta)
    {
        var force = Vector3.Up * 200 * delta;
        var depth = _y_height - body.GlobalPosition.Y;
        body.LinearVelocity += force * depth;

        body.LinearVelocity *= 1 - DRAG;
        body.AngularVelocity *= 1 - DRAG;
    }

    private void SpawnBodyParticleOverTime(WaterBody wb)
    {
        var vel = wb.Body.LinearVelocity.Length();
        if (vel < 0.2f)
        {
            if (!wb.FinalParticle)
            {
                wb.FinalParticle = true;
                SpawnWaterRippleParticle(wb);
            }

            return;
        }

        wb.FinalParticle = false;

        var time_min = 0.05f;
        var time_max = 1f;
        var vel_min = 1f;
        var vel_max = 5f;
        var t_vel = (vel - vel_min) / (vel_max - vel_min);
        var time = Mathf.Lerp(time_max, time_min, t_vel);

        if (GameTime.Time > wb.TimeLastParticle + time)
        {
            SpawnWaterRippleParticle(wb);
        }
    }

    private void SpawnWaterRippleParticle(WaterBody wb)
    {
        var position = wb.Body.GlobalPosition.Set(y: _y_height + 0.01f);
        Particle.PlayOneShot("ps_water_ripple", position);
        wb.TimeLastParticle = GameTime.Time;
    }
}
