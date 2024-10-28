using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class WateringCan : Item
{
    [Export]
    public string UseAnimation;

    [NodeType]
    public AnimationPlayer AnimationPlayer;

    [NodeType]
    public Area3D Area;

    private bool _using;
    private bool _slosh_max_speed;
    private List<GodotObject> _bodies = new();


    public override void _Ready()
    {
        base._Ready();

        Area.BodyEntered += v => CallDeferred(nameof(BodyEntered), v);
        Area.BodyExited += v => CallDeferred(nameof(BodyExited), v);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var fdelta = Convert.ToSingle(delta);
        PhysicsProcess_Slosh(fdelta);
    }

    private void PhysicsProcess_Slosh(float delta)
    {
        var max_vel = 8f;
        var min_vel = 2f;
        var vel = RigidBody.LinearVelocity.Length();

        if (_slosh_max_speed)
        {
            if (vel < min_vel)
            {
                PlaySloshSFX();
                _slosh_max_speed = false;
            }
        }
        else
        {
            if (vel > max_vel)
            {
                _slosh_max_speed = true;
            }
        }
    }

    private void PlaySloshSFX()
    {
        var rng = new RandomNumberGenerator();
        var i = rng.RandiRange(1, 3);
        var sfx_name = $"sfx_watering_can_slosh_{i.ToString("000")}";
        SoundController.Instance.Play(sfx_name, new SoundSettings3D
        {
            Bus = SoundBus.SFX,
            Position = GlobalPosition,
            PitchMax = 1.0f,
            PitchMin = 0.9f,
        });
    }

    public override void Use()
    {
        base.Use();

        if (Data.Uses > 0)
        {
            AnimateUse();
        }
        else
        {
            var view = View.Get<GameView>();
            view.DisplayText("The watering can is empty");
        }
    }

    private void AnimateUse()
    {
        if (_using) return;

        StartCoroutine(Cr, "use");
        IEnumerator Cr()
        {
            _using = true;
            var animation = AnimationPlayer.GetAnimation(UseAnimation);
            AnimationPlayer.Play(UseAnimation);
            SetPlayerLocked(true);

            yield return new WaitForSeconds(animation.Length * 0.6f);

            WaterClosest();
            Data.Uses--;

            yield return new WaitForSeconds(animation.Length * 0.2f);

            SetPlayerLocked(false);

            yield return new WaitForSeconds(animation.Length * 0.2f);
            _using = false;
        }
    }

    private void SetPlayerLocked(bool locked)
    {
        var player = FirstPersonController.Instance;

        if (locked)
        {
            player.MovementLock.AddLock(nameof(WateringCan));
            player.LookLock.AddLock(nameof(WateringCan));
            player.InteractLock.AddLock(nameof(WateringCan));
        }
        else
        {
            player.MovementLock.RemoveLock(nameof(WateringCan));
            player.LookLock.RemoveLock(nameof(WateringCan));
            player.InteractLock.RemoveLock(nameof(WateringCan));
        }
    }

    private void WaterClosest()
    {
        var my_position = new Vector3(Area.GlobalPosition.X, 0, Area.GlobalPosition.Y);

        var closest = _bodies
            .Select(x => x as Node)
            .Where(x => IsInstanceValid(x))
            .Select(x => x.GetNodeInParents<Waterable>())
            .Where(x => IsInstanceValid(x))
            .OrderBy(x => my_position.DistanceTo(new Vector3(x.GlobalPosition.X, 0, x.GlobalPosition.Y)))
            .FirstOrDefault();

        if (!IsInstanceValid(closest)) return;

        closest.Water();
    }

    private void BodyEntered(GodotObject go)
    {
        _bodies.Add(go);
    }

    private void BodyExited(GodotObject go)
    {
        _bodies.Remove(go);
    }
}
