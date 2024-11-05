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
            PlayWaterRippleParticle(wb);
            PlayWaterImpactEffects(wb);
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
                PlayWaterRippleParticle(wb);
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
            PlayWaterRippleParticle(wb);
        }
    }

    private void PlayWaterRippleParticle(WaterBody wb)
    {
        var position = wb.Body.GlobalPosition.Set(y: _y_height + 0.01f);
        Particle.PlayOneShot("ps_water_ripple", position);
        wb.TimeLastParticle = GameTime.Time;
    }

    private void PlayWaterSplashParticle(WaterBody wb)
    {
        var position = wb.Body.GlobalPosition.Set(y: _y_height + 0.01f);
        Particle.PlayOneShot("ps_water_splash", position);
    }

    private void PlayWaterImpactEffects(WaterBody wb)
    {
        var vel = wb.Body.LinearVelocity.Length();
        var position = wb.Body.GlobalPosition;
        var v_low = 2f;
        var v_med = 4f;
        var v_high = 7f;

        if (vel > v_high)
        {
            var t_vel = (vel - v_high) / (10 - v_high);
            PlayWaterImpactHighSfx(position, t_vel);
            PlayWaterSplashParticle(wb);
        }
        else if (vel > v_med)
        {
            var t_vel = (vel - v_med) / (v_high - v_med);
            PlayWaterImpactMedSfx(position, t_vel);
            PlayWaterSplashParticle(wb);
        }
        else if (vel > v_low)
        {
            var t_vel = (vel - v_low) / (v_med - v_low);
            PlayWaterImpactLowSfx(position, t_vel);
        }
    }

    private void PlayWaterImpactLowSfx(Vector3 position, float t)
    {
        var rng = new RandomNumberGenerator();
        var i = rng.RandiRange(1, 4).ToString("000");
        var sfx_name = $"sfx_impact_water_low_{i}";
        var pitch_base = Mathf.Lerp(1.0f, 0.9f, t);

        SoundController.Instance.Play(sfx_name, new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            PitchMin = pitch_base - 0.05f,
            PitchMax = pitch_base,
            Volume = -30,
            Position = position,
            AttenuationEnabled = false,
        });
    }

    private void PlayWaterImpactMedSfx(Vector3 position, float t)
    {
        var rng = new RandomNumberGenerator();
        var i = rng.RandiRange(1, 3).ToString("000");
        var sfx_name = $"sfx_impact_water_med_{i}";
        var pitch_base = Mathf.Lerp(1.0f, 0.8f, t);

        SoundController.Instance.Play(sfx_name, new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            PitchMin = pitch_base - 0.05f,
            PitchMax = pitch_base,
            Volume = -30,
            Position = position,
            AttenuationEnabled = false,
        });
    }

    private void PlayWaterImpactHighSfx(Vector3 position, float t)
    {
        var rng = new RandomNumberGenerator();
        var i = rng.RandiRange(1, 4).ToString("000");
        var sfx_name = $"sfx_impact_water_high_{i}";
        var pitch_base = Mathf.Lerp(1.0f, 0.7f, t);

        SoundController.Instance.Play(sfx_name, new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            PitchMin = pitch_base - 0.05f,
            PitchMax = pitch_base,
            Volume = -20,
            Position = position,
            AttenuationEnabled = false,
        });
    }
}
